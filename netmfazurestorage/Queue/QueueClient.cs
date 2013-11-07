using System;
using System.IO;
using System.Net;
using System.Text;
using ElzeKool;
using Microsoft.SPOT;
using NetMf.CommonExtensions;

namespace netmfazurestorage.Queue
{
    public class QueueClient
    {
        #region private members

        private string _httpVerb = "PUT";

        #endregion

        #region constants

        internal const string HttpVerbPut = "PUT";
        internal const string HttpVerbDelete = "DELETE";
        internal const string HttpVerbPost = "POST";
        internal const string HttpVerbGet = "GET";
        internal const string VersionHeader = "2011-08-18";

        #endregion

        #region Properties

        public static string AccountName { get; set; }
        public static string AccountKey { get; set; }

        internal string DateHeader { get; set; }

        #endregion

        public QueueClient(string accountName, string accountKey)
        {
            AccountName = accountName;
            AccountKey = accountKey;
            DateHeader = DateTime.UtcNow.ToString("R");
        }

        protected byte[] GetXmlBytesAndLength(string content, out int contentLength)
        {
            var contentBytes = Encoding.UTF8.GetBytes(content);
            contentLength = content.Length;
            return contentBytes;
        }

        private HttpWebRequest PrepareRequest(string url, string authHeader, byte[] fileBytes = null, int contentLength = 0, string httpVerb = "GET")
        {
            var uri = new Uri(url);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = httpVerb;
            request.ContentLength = contentLength;
            request.Headers.Add("x-ms-date", DateHeader);
            request.Headers.Add("x-ms-version", VersionHeader);
            request.Headers.Add("Authorization", authHeader);
            if (contentLength != 0)
            {
                request.GetRequestStream().Write(fileBytes, 0, fileBytes.Length);
            }
            return request;
        }

        protected string SendWebRequest(string url, string authHeader, byte[] fileBytes = null, int contentLength = 0, string httpVerb = "GET")
        {
            string responseBody = "";
            HttpWebRequest request = PrepareRequest(url, authHeader, fileBytes, contentLength, httpVerb);
            try
            {
                HttpWebResponse response;
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        Debug.Print("Queue has been created!");
                    }
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        Debug.Print("Queue action has been completed");
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new WebException("Forbidden", null, WebExceptionStatus.ServerProtocolViolation, response);
                    }

                    using (var responseStream = response.GetResponseStream())
                    using (var reader = new StreamReader(responseStream))
                    {
                        char[] bytes = new char[(int)responseStream.Length];

                        if (bytes.Length > 0)
                        {
                            reader.Read(bytes, 0, bytes.Length);

                            responseBody = new string(bytes);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Conflict)
                {
                    Debug.Print("container or blob already exists!");
                }
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Forbidden)
                {
                    Debug.Print("problem with signature!");
                }
            }

            Debug.Print(responseBody);
            return responseBody;
        }

        protected string CreateAuthorizationHeader(String canResource, string options = "", int contentLength = 0, bool useZero = false, string httpVerb = "GET")
        {
            string contentL = useZero ? "0" : (contentLength == 0 ? "" : contentLength.ToString());
            string toSign = StringUtility.Format("{0}\n\n\n{1}\n\n\n\n\n\n\n\n{5}\nx-ms-date:{2}\nx-ms-version:{3}\n{4}",
                httpVerb, contentL, DateHeader, VersionHeader, canResource, options);

            string signature;
            var hmacBytes = SHA.computeHMAC_SHA256(Convert.FromBase64String(AccountKey), Encoding.UTF8.GetBytes(toSign));
            signature = Convert.ToBase64String(hmacBytes).Replace("!", "+").Replace("*", "/"); ;


            return "SharedKey " + AccountName + ":" + signature;
        }

        public void CreateQueue(string queueName)
        {
            //PUT https://myaccount.queue.core.windows.net/myqueue HTTP/1.1
            var url = StringUtility.Format("http://{0}.queue.core.windows.net/{1}", AccountName, queueName);
            string can = StringUtility.Format("/{0}/{1}", AccountName, queueName);
            var auth = CreateAuthorizationHeader(can, "", 0, true, "PUT");
            SendWebRequest(url, auth, null, 0, "PUT");
        }

        public void CreateQueueMessage(string queueName, string message)
        {
            // POST http://myaccount.queue.core.windows.net/netmfdata/messages?visibilitytimeout=<int-seconds>&messagettl=<int-seconds>
            int length = 0;
            string messageXml = StringUtility.Format("<QueueMessage><MessageText>{0}</MessageText></QueueMessage>",
                                                     Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
            byte[] content = GetXmlBytesAndLength(messageXml, out length);
            string can = StringUtility.Format("/{0}/{1}/messages", AccountName, queueName);
            string auth = CreateAuthorizationHeader(can, "", length, false, "POST");
            string url = StringUtility.Format("http://{0}.queue.core.windows.net/{1}/messages", AccountName, queueName);
            SendWebRequest(url, auth, content, length, "POST");

        }

        public QueueMessageWrapper RetrieveQueueMessage(string queueName)
        {

            // GET http://myaccount.queue.core.windows.net/netmfdata/messages

            string can = StringUtility.Format("/{0}/{1}/messages", AccountName, queueName);
            string auth = CreateAuthorizationHeader(can, "", 0, true);
            string url = StringUtility.Format("http://{0}.queue.core.windows.net/{1}/messages", AccountName, queueName);
            var responseBody = SendWebRequest(url, auth);

            if (responseBody == null)
                return null;

            string retMessage = GetNodeValue(responseBody, "MessageText");
            string popReceipt = GetNodeValue(responseBody, "PopReceipt");
            string messageId = GetNodeValue(responseBody, "MessageId");

            string decoded = new string(Encoding.UTF8.GetChars(Convert.FromBase64String(retMessage)));

            return new QueueMessageWrapper() { Message = decoded, PopReceipt = popReceipt, MessageId = messageId };
        }

        private string GetNodeValue(string responseBody, string nodeName)
        {
            lock (this)
            {
                string ret = "";
                int pos = responseBody.IndexOf(nodeName);
                if (pos > 0)
                {
                    var termPos = responseBody.IndexOf('>', pos);
                    while (responseBody[++termPos] != '<')
                    {
                        ret += responseBody[termPos];
                    }
                }
                return ret;
            }
        }

        public void DeleteMessage(string messageId, string popReceipt)
        {

            // DELETE http://myaccount.queue.core.windows.net/myqueue/messages/messageid?popreceipt=string-value
            string can = StringUtility.Format("/{0}/netmfdata/messages/{2}\npopreceipt:{1}", AccountName, popReceipt,
                                              messageId);
            string auth = CreateAuthorizationHeader(can, "", 0, true, "DELETE");
            string url =
                StringUtility.Format("http://{0}.queue.core.windows.net/netmfdata/messages/{2}?popreceipt={1}",
                                     AccountName, popReceipt, messageId);
            SendWebRequest(url, auth, null, 0, "DELETE");

        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Text;
using ElzeKool;
using Microsoft.SPOT;
using NetMf.CommonExtensions;
using netmfazurestorage.Http;

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

        /// <summary>
        /// Creates a new QueueClient which can create, write to, read from, delete from and delete a Windows
        /// Azure Storage Queue. 
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountKey"></param>
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
            HttpHelper.SendWebRequest(url, auth, DateHeader, VersionHeader, fileBytes: null, contentLength: 0, httpVerb: "PUT");
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
            HttpHelper.SendWebRequest(url, auth, DateHeader, VersionHeader, content, length, "POST");

        }

        public QueueMessageWrapper RetrieveQueueMessage(string queueName)
        {

            // GET http://myaccount.queue.core.windows.net/netmfdata/messages

            string can = StringUtility.Format("/{0}/{1}/messages", AccountName, queueName);
            string auth = CreateAuthorizationHeader(can, "", 0, true);
            string url = StringUtility.Format("http://{0}.queue.core.windows.net/{1}/messages", AccountName, queueName);
            var responseBody = HttpHelper.SendWebRequest(url, auth, DateHeader, VersionHeader);

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

        public void DeleteMessage(string queueName, string messageId, string popReceipt)
        {

            // DELETE http://myaccount.queue.core.windows.net/myqueue/messages/messageid?popreceipt=string-value
            string can = StringUtility.Format("/{0}/{3}/messages/{2}\npopreceipt:{1}", AccountName, popReceipt,
                                              messageId, queueName);
            string auth = CreateAuthorizationHeader(can, "", 0, true, "DELETE");
            string url =
                StringUtility.Format("http://{0}.queue.core.windows.net/{3}/messages/{2}?popreceipt={1}",
                                     AccountName, popReceipt, messageId, queueName);
            HttpHelper.SendWebRequest(url, auth, DateHeader, VersionHeader, null, 0, "DELETE");

        }

        public void DeleteQueue(string queueName)
        {
            //DELETE https://myaccount.queue.core.windows.net/myqueue HTTP/1.1
            var url = StringUtility.Format("http://{0}.queue.core.windows.net/{1}", AccountName, queueName);
            string can = StringUtility.Format("/{0}/{1}", AccountName, queueName);
            var auth = CreateAuthorizationHeader(can, "", 0, true, "DELETE");
            HttpHelper.SendWebRequest(url, auth, DateHeader, VersionHeader, null, 0, "DELETE");
        }
    }
}
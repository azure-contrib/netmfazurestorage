using System;
using System.IO;
using System.Net;
using System.Text;
using ElzeKool;
using Microsoft.SPOT;
using NetMf.CommonExtensions;
using netmfazurestorage.Account;
using netmfazurestorage.Http;
using System.Collections;

namespace netmfazurestorage.Queue
{
    public class QueueClient
    {
        private readonly CloudStorageAccount _account;

        #region constants

        internal const string HttpVerbPut = "PUT";
        internal const string HttpVerbDelete = "DELETE";
        internal const string HttpVerbPost = "POST";
        internal const string HttpVerbGet = "GET";
        internal const string VersionHeader = "2011-08-18";

        #endregion

        #region Properties
        internal string DateHeader { get; set; }

        #endregion

        /// <summary>
        /// Creates a new QueueClient which can create, write to, read from, delete from and delete a Windows
        /// Azure Storage Queue. 
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountKey"></param>
        public QueueClient(CloudStorageAccount account)
        {
            _account = account;
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
            var hmacBytes = SHA.computeHMAC_SHA256(Convert.FromBase64String(_account.AccountKey), Encoding.UTF8.GetBytes(toSign));
            signature = Convert.ToBase64String(hmacBytes).Replace("!", "+").Replace("*", "/"); ;


            return "SharedKey " + _account.AccountName + ":" + signature;
        }

        public void CreateQueue(string queueName)
        {
            //PUT https://myaccount.queue.core.windows.net/myqueue HTTP/1.1
            var url = StringUtility.Format("{0}/{1}", _account.UriEndpoints["Queue"], queueName);
            string can = StringUtility.Format("/{0}/{1}", _account.AccountName, queueName);
            var auth = CreateAuthorizationHeader(can, "", 0, true, "PUT");
            AzureStorageHttpHelper.SendWebRequest(url, auth, DateHeader, VersionHeader, fileBytes: null, contentLength: 0, httpVerb: "PUT");
        }

        public void CreateQueueMessage(string queueName, string message)
        {
            // POST http://myaccount.queue.core.windows.net/netmfdata/messages?visibilitytimeout=<int-seconds>&messagettl=<int-seconds>
            int length = 0;
            string messageXml = StringUtility.Format("<QueueMessage><MessageText>{0}</MessageText></QueueMessage>",
                                                     Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
            byte[] content = GetXmlBytesAndLength(messageXml, out length);
            string can = StringUtility.Format("/{0}/{1}/messages", _account.AccountName, queueName);
            string auth = CreateAuthorizationHeader(can, "", length, false, "POST");
            string url = StringUtility.Format("{0}/{1}/messages", _account.UriEndpoints["Queue"], queueName);
            AzureStorageHttpHelper.SendWebRequest(url, auth, DateHeader, VersionHeader, content, length, "POST");

        }

        public QueueMessageWrapper PeekQueueMessage(string queueName)
        {
            // GET https://myaccount.queue.core.windows.net/myqueue/messages?peekonly=true

            return RetrieveQueueMessage(queueName, true);
        }

        public QueueMessageWrapper RetrieveQueueMessage(string queueName)
        {
            // GET http://myaccount.queue.core.windows.net/netmfdata/messages

            return RetrieveQueueMessage(queueName, false);
        }
        public QueueMessageWrapper RetrieveQueueMessage(string queueName, bool peekOnly)
        {
            string can = StringUtility.Format("/{0}/{1}/messages", _account.AccountName, queueName);
            string auth = CreateAuthorizationHeader(can, "", 0, true);
            string url = StringUtility.Format("{0}/{1}/messages", _account.UriEndpoints["Queue"], queueName);

            if (peekOnly)
            {
                url += "?peekonly=true";
            }
            
            var response = AzureStorageHttpHelper.SendWebRequest(url, auth, DateHeader, VersionHeader);

            if (response.Body == null)
                return null;

            string retMessage = GetNodeValue(response.Body, "MessageText");
            string messageId = GetNodeValue(response.Body, "MessageId");

            string popReceipt = string.Empty;//this will remain empty if we are peeking

            if (!peekOnly)
            {
                popReceipt = GetNodeValue(response.Body, "PopReceipt");
            }

            string decoded = new string(Encoding.UTF8.GetChars(Convert.FromBase64String(retMessage)));

            return new QueueMessageWrapper() { Message = decoded, PopReceipt = popReceipt, MessageId = messageId };
        }

        private string GetNodeValue(string responseBody, string nodeName)
        {
            // why are you locking this? I can't see any multithreading?
            // I love the deserialization strategy BTW :¬)
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
            string can = StringUtility.Format("/{0}/{3}/messages/{2}\npopreceipt:{1}", _account.AccountName, popReceipt,
                                              messageId, queueName);
            string auth = CreateAuthorizationHeader(can, "", 0, true, "DELETE");
            string url =
                StringUtility.Format("{0}/{3}/messages/{2}?popreceipt={1}",
                                     _account.UriEndpoints["Queue"], popReceipt, messageId, queueName);
            AzureStorageHttpHelper.SendWebRequest(url, auth, DateHeader, VersionHeader, null, 0, "DELETE");

        }

        public void DeleteQueue(string queueName)
        {
            //DELETE https://myaccount.queue.core.windows.net/myqueue HTTP/1.1
            var url = StringUtility.Format("{0}/{1}", _account.UriEndpoints["Queue"], queueName);
            string can = StringUtility.Format("/{0}/{1}", _account.AccountName, queueName);
            var auth = CreateAuthorizationHeader(can, "", 0, true, "DELETE");
            AzureStorageHttpHelper.SendWebRequest(url, auth, DateHeader, VersionHeader, null, 0, "DELETE");
        }
    }
}
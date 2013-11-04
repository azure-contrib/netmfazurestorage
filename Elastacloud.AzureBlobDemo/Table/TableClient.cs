using System;
using System.IO;
using System.Net;
using System.Text;
using ElzeKool;
using Microsoft.SPOT;
using NetMf.CommonExtensions;
using System.Collections;

namespace Elastacloud.AzureBlobDemo.Table
{
    public class TableClient
    {
        public static string AccountName;
        public static string AccountKey;

        #region constants

        internal const string VersionHeader = "2011-08-18";
        internal const string ContentType = "application/atom+xml";

        #endregion

        #region Properties

        internal DateTime InstanceDate { get; set; }

        #endregion

        protected byte[] GetBodyBytesAndLength(string body, out int contentLength)
        {
            var content = Encoding.UTF8.GetBytes(body);
            contentLength = content.Length;
            return content;
        }

        public TableClient(string accountName, string accountKey)
        {
            InstanceDate = DateTime.UtcNow;
            AccountName = accountName;
            AccountKey = accountKey;
        }

        public void CreateTable(string tableName)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
            "<entry xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\"  " +
            "xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" " +
            "xmlns=\"http://www.w3.org/2005/Atom\"> " +
            "<id>http://myaccount.table.core.windows.net/Tables('"
                + tableName +
            "')</id>" +
            "<title />" +
            "<updated>2012-11-06T11:48:34.9840639+00:00</updated>" +
            "<author><name/></author> " +
            "<content type=\"application/xml\"><m:properties><d:TableName>" + tableName + "</d:TableName></m:properties></content></entry>";

            int contentLength = 0;
            byte[] payload = GetBodyBytesAndLength(xml, out contentLength);
            string header = CreateAuthorizationHeader(payload, ContentType, "/" + AccountName + "/Tables()");
            SendWebRequest("http://" + AccountName + ".table.core.windows.net/Tables()", header, payload, contentLength);
        }

        public void InsertEntitiy(string tableName, string partitionKey, string rowKey, IDictionary values)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z");

            var sb = new System.Text.StringBuilder();
            foreach (var key in values.Keys)
            {
                var value = values[key];
                sb.Append("<d:"+ key + ">" + value + "</d:" + key + ">");
            }

            string xml =
                StringUtility.Format("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><entry xml:base=\"http://{0}.table.core.windows.net/\" xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\" xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" m:etag=\"W/&quot;datetime'2008-09-18T23%3A46%3A19.4277424Z'&quot;\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "<id>http://{0}.table.core.windows.net/{5}(PartitionKey='{2}',RowKey='{3}')</id>" +
                "<title/><updated>{1}</updated><author><name /></author>" +
                "<link />" +
                "<category term=\"{0}.Tables\" scheme=\"http://schemas.microsoft.com/ado/2007/08/dataservices/scheme\" />" +
                "<content type=\"application/xml\"><m:properties><d:PartitionKey>{2}</d:PartitionKey><d:RowKey>{3}</d:RowKey>" +
                sb.ToString() + 
                "</m:properties>" +
                "</content>" +
                "</entry>", AccountName, timestamp, partitionKey, rowKey, values, tableName);

            int contentLength = 0;
            byte[] payload = GetBodyBytesAndLength(xml, out contentLength);
            string header = CreateAuthorizationHeader(payload, ContentType, StringUtility.Format("/{0}/{1}", AccountName, tableName));
            SendWebRequest(StringUtility.Format("http://{0}.table.core.windows.net/{1}", AccountName, tableName), header, payload, contentLength);
            
        }

        public void AddTableEntityForTemperature(string tablename, string partitionKey, string rowKey, DateTime timeStamp, double temperature, string country)
        {
            var timestamp = timeStamp.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z");

            string xml =
                StringUtility.Format("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><entry xml:base=\"http://{0}.table.core.windows.net/\" xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\" xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" m:etag=\"W/&quot;datetime'2008-09-18T23%3A46%3A19.4277424Z'&quot;\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "<id>http://{0}.table.core.windows.net/{6}(PartitionKey='{2}',RowKey='{3}')</id>" +
                "<title/><updated>{1}</updated><author><name /></author>" +
                "<link />" +
                "<category term=\"{0}.Tables\" scheme=\"http://schemas.microsoft.com/ado/2007/08/dataservices/scheme\" />" +
                "<content type=\"application/xml\"><m:properties><d:PartitionKey>{2}</d:PartitionKey><d:RowKey>{3}</d:RowKey>" +
                "<d:Timestamp m:type=\"Edm.DateTime\">{1}</d:Timestamp>" +
                "<d:Temperature m:type=\"Edm.Double\">{4}</d:Temperature> " +
                "<d:Country>{5}</d:Country>" +
                "</m:properties>" +
                "</content>" +
                "</entry>", AccountName, timestamp, partitionKey, rowKey, temperature.ToString(), country, tablename);

            int contentLength = 0;
            byte[] payload = GetBodyBytesAndLength(xml, out contentLength);
            string header = CreateAuthorizationHeader(payload, ContentType, StringUtility.Format("/{0}/{1}", AccountName, tablename));
            SendWebRequest(StringUtility.Format("http://{0}.table.core.windows.net/{1}", AccountName, tablename), header, payload, contentLength);
        }

        #region Request Handling

        //DataServiceVersion: Set the value of this header to 1.0;NetFx.
        //MaxDataServiceVersion: Set the value of this header to 1.0;NetFx.

        private HttpWebRequest PrepareRequest(string url, string authHeader, byte[] fileBytes = null, int contentLength = 0, string verb = "POST")
        {
            var uri = new Uri(url);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = verb;
            request.Headers.Add("x-ms-date", InstanceDate.ToString("R"));
            request.Headers.Add("DataServiceVersion", "1.0;NetFx");
            request.Headers.Add("MaxDataServiceVersion", "1.0;NetFx");
            request.ContentType = ContentType;
            request.ContentLength = contentLength;
            request.Headers.Add("Date", InstanceDate.ToString("R"));
            request.Headers.Add("x-ms-version", VersionHeader);
            request.Headers.Add("Authorization", authHeader);
            if (contentLength != 0)
            {
                request.GetRequestStream().Write(fileBytes, 0, fileBytes.Length);
            }
            return request;
        }

        protected void SendWebRequest(string url, string authHeader, byte[] fileBytes = null, int contentLength = 0)
        {
            HttpWebRequest request = PrepareRequest(url, authHeader, fileBytes, contentLength);
            try
            {
                HttpWebResponse response;
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        Debug.Print("Resource has been created");
                    }
                    else
                    {
                        Debug.Print("Status was " + response.StatusCode);
                        var ResponseBody = "";
                        using (var responseStream = response.GetResponseStream())
                        using (var reader = new StreamReader(responseStream))
                        {
                            char[] bytes = new char[(int)responseStream.Length];

                            if (bytes.Length > 0)
                            {
                                reader.Read(bytes, 0, bytes.Length);

                                ResponseBody = new string(bytes);
                            }
                        }
                        Debug.Print(ResponseBody);
                    }
                    //if (response.StatusCode == HttpStatusCode.Accepted)
                    //{
                    //    Trace.WriteLine("Container or blob action has been completed");
                    //}
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
        }

        #endregion

        #region Shared Access Signature


        /*StringToSign = VERB + "\n" + 
               Content-MD5 + "\n" + 
               Content-Type + "\n" +
               Date + "\n" +
               CanonicalizedResource;*/
        /*StringToSign = 
               Date + "\n" +
               CanonicalizedResource;*/
        // Signature=Base64(HMAC-SHA256(UTF8(StringToSign)))
        protected string CreateAuthorizationHeader(byte[] content, string contentType, string canonicalResource)
        {
            //string toSign = String.Format("{0}\n{1}\n{2}\n{3}\n{4}",
            //                              HttpVerb, hash, contentType, InstanceDate, canonicalResource);
            string toSign = StringUtility.Format("{0}\n{1}", InstanceDate.ToString("R"), canonicalResource);
            string signature;
            var hmacBytes = SHA.computeHMAC_SHA256(Convert.FromBase64String(AccountKey), Encoding.UTF8.GetBytes(toSign));
            signature = Convert.ToBase64String(hmacBytes).Replace("!", "+").Replace("*", "/"); ;

            return "SharedKeyLite " + AccountName + ":" + signature;
        }

        #endregion
    }
}


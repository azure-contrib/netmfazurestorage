using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using ElzeKool;
using Microsoft.SPOT;
using NetMf.CommonExtensions;
using System.Collections;
using netmfazurestorage.Http;

namespace netmfazurestorage.Table
{
    public class TableClient
    {
        public static string AccountName;
        public static string AccountKey;
        public static bool AttachFiddler;

        #region constants

        internal const string VersionHeader = "2011-08-18";
        internal const string ContentType = "application/atom+xml";
        private string DateHeader { get; set; }

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
            DateHeader = DateTime.UtcNow.ToString("R");
        }

        public void CreateTable(string tableName)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
            "<entry xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\"  " +
            "xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" " +
            "xmlns=\"http://www.w3.org/2005/Atom\"> " +
            "<id>http://" + AccountName + ".table.core.windows.net/Tables('"
                + tableName +
            "')</id>" +
            "<title />" +
            "<updated>" + InstanceDate.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z") + "</updated>" +
            "<author><name/></author> " +
            "<content type=\"application/xml\"><m:properties><d:TableName>" + tableName + "</d:TableName></m:properties></content></entry>";

            int contentLength = 0;
            byte[] payload = GetBodyBytesAndLength(xml, out contentLength);
            string header = CreateAuthorizationHeader(payload, ContentType, "/" + AccountName + "/Tables()");
            AzureStorageHttpHelper.SendWebRequest("http://" + AccountName + ".table.core.windows.net/Tables()", header, DateHeader, VersionHeader, payload, contentLength);
        }

        [Obsolete("Please use the InsertTableEntity method; this AddTableEntityForTemperature method will be removed in a future release.", false)]
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
            AzureStorageHttpHelper.SendWebRequest(StringUtility.Format("http://{0}.table.core.windows.net/{1}", AccountName, tablename), header, DateHeader, VersionHeader, payload, contentLength);
        }

        public void InsertTableEntity(string tablename, string partitionKey, string rowKey, DateTime timeStamp, System.Collections.ArrayList tableEntityProperties)
        {
            var timestamp = timeStamp.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z");

            string xml =
                StringUtility.Format("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><entry xml:base=\"http://{0}.table.core.windows.net/\" xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\" xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" m:etag=\"W/&quot;datetime'2008-09-18T23%3A46%3A19.4277424Z'&quot;\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "<id>http://{0}.table.core.windows.net/{5}(PartitionKey='{2}',RowKey='{3}')</id>" +
                "<title/><updated>{1}</updated><author><name /></author>" +
                "<link />" +
                "<category term=\"{0}.Tables\" scheme=\"http://schemas.microsoft.com/ado/2007/08/dataservices/scheme\" />" +
                "<content type=\"application/xml\"><m:properties><d:PartitionKey>{2}</d:PartitionKey><d:RowKey>{3}</d:RowKey>" +
                "<d:Timestamp m:type=\"Edm.DateTime\">{1}</d:Timestamp>" +
                "{4}" +
                "</m:properties>" +
                "</content>" +
                "</entry>", AccountName, timestamp, partitionKey, rowKey, GetTableXml(tableEntityProperties), tablename);

            int contentLength = 0;
            byte[] payload = GetBodyBytesAndLength(xml, out contentLength);
            string header = CreateAuthorizationHeader(payload, ContentType, StringUtility.Format("/{0}/{1}", AccountName, tablename));
            AzureStorageHttpHelper.SendWebRequest(StringUtility.Format("http://{0}.table.core.windows.net/{1}", AccountName, tablename), header, DateHeader, VersionHeader, payload, contentLength);
        }

        public void InsertTableEntity_Experimental(string tablename, string partitionKey, string rowKey, DateTime timeStamp, Hashtable tableEntityProperties)
        {
            var timestamp = timeStamp.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z");

            string xml =
                StringUtility.Format("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><entry xml:base=\"http://{0}.table.core.windows.net/\" xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\" xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" m:etag=\"W/&quot;datetime'2008-09-18T23%3A46%3A19.4277424Z'&quot;\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "<id>http://{0}.table.core.windows.net/{5}(PartitionKey='{2}',RowKey='{3}')</id>" +
                "<title/><updated>{1}</updated><author><name /></author>" +
                "<link />" +
                "<category term=\"{0}.Tables\" scheme=\"http://schemas.microsoft.com/ado/2007/08/dataservices/scheme\" />" +
                "<content type=\"application/xml\"><m:properties><d:PartitionKey>{2}</d:PartitionKey><d:RowKey>{3}</d:RowKey>" +
                "<d:Timestamp m:type=\"Edm.DateTime\">{1}</d:Timestamp>" +
                "{4}" +
                "</m:properties>" +
                "</content>" +
                "</entry>", AccountName, timestamp, partitionKey, rowKey, GetTableXml(tableEntityProperties), tablename);

            int contentLength = 0;
            byte[] payload = GetBodyBytesAndLength(xml, out contentLength);
            string header = CreateAuthorizationHeader(payload, ContentType, StringUtility.Format("/{0}/{1}", AccountName, tablename));
            AzureStorageHttpHelper.SendWebRequest(StringUtility.Format("http://{0}.table.core.windows.net/{1}", AccountName, tablename), header, DateHeader, VersionHeader, payload, contentLength);
        }

        private string GetTableXml(ArrayList tableEntityProperties)
        {
            string result=string.Empty;
            foreach (var tableEntityProperty in tableEntityProperties)
            {
                var prop = tableEntityProperty as TableEntityProperty;

                if (prop != null)
                {
                    result += prop.ToString();
                }
            }

            return result;
        }

        private static string GetTableXml(Hashtable tableEntityProperties)
        {
            string result = string.Empty;
            foreach (var key in tableEntityProperties.Keys)
            {
                var value = tableEntityProperties[key];
                if (value == null) continue;
                var type = value.GetType().Name;
                switch (type)
                {
                    case "DateTime":
                        value = ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.0000000Z");
                        break;
                    case "Boolean":
                        value = (Boolean) value ? "true" : "false"; // bool is title case when you call ToString()
                        break;
                }
                result += StringUtility.Format("<d:{0} m:type=\"Edm.{2}\">{1}</d:{0}>", key, value, type);
            }
            return result;
        }

        public Hashtable QueryTable(string tablename, string partitionKey, string rowKey)
        {
            var header = CreateAuthorizationHeader(null, ContentType, StringUtility.Format("/{0}/{1}(PartitionKey='{2}',RowKey='{3}')", AccountName, tablename, partitionKey, rowKey));
            var xml = AzureStorageHttpHelper.SendWebRequest(StringUtility.Format("http://{0}.table.core.windows.net/{1}(PartitionKey='{2}',RowKey='{3}')", AccountName, tablename, partitionKey, rowKey), header, DateHeader, VersionHeader, null, 0, "GET");
            string token = null;
            Hashtable results = null;
            var nextStart = 0;
            while (null != (token = NextToken(xml.Body, "<m:properties>", "</m:properties>", nextStart, out nextStart)))
            {
                results = new Hashtable();

                string propertyToken = null;
                int nextPropertyStart = 0;
                while (null != (propertyToken = NextToken(xml.Body, "<d:", "</d", nextPropertyStart, out nextPropertyStart)))
                {
                    var parts = propertyToken.Split('>');
                    if (parts.Length != 2) continue;
                    var rawvalue = parts[1];
                    var propertyName = parts[0].Split(' ')[0];
                    
                    var _ = 0;
                    var type = NextToken(propertyToken, "m:type=\"", "\"", 0, out _);
                    if (null == type)
                    { 
                        type = "Edm.String";
                    }
                    switch (type)
                    {
                        case "Edm.String":
                            results.Add(propertyName, rawvalue);
                            break;
                        case "Edm.DateTime":
                            // not supported
                            break;
                        case "Edm.Int64":
                            results.Add(propertyName, Int64.Parse(rawvalue));
                            break;
                        case "Edm.Int32":
                            results.Add(propertyName, Int32.Parse(rawvalue));
                            break;
                        case "Edm.Double":
                            results.Add(propertyName, Double.Parse(rawvalue));
                            break;
                        case "Edm.Boolean":
                            results.Add(propertyName, rawvalue == "true");
                            break;
                        case "Edm.Guid":
                            // not supported
                            break;
                    }
                }
            }
            return results;
        }

        private string NextToken(string xml, string startToken, string endToken, int startPosition, out int nextStart)
        {
            if (startPosition > xml.Length)
            {
                nextStart = xml.Length;
                return null;
            }
            var start = xml.IndexOf(startToken, startPosition);
            nextStart = 0;
            if (start < 0) return null;
            start += startToken.Length;
            var end = xml.IndexOf(endToken, start);
            nextStart = end + endToken.Length;
            return xml.Substring(start, end - start);           
        }

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


using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using ElzeKool;
using Microsoft.SPOT;
using NetMf.CommonExtensions;
using netmfazurestorage.Account;
using netmfazurestorage.Http;

namespace netmfazurestorage.Table
{
    public class TableClient
    {
        private readonly CloudStorageAccount _account;
        internal const string VersionHeader = "2011-08-18";
        internal const string ContentType = "application/atom+xml";
        private string DateHeader { get; set; }
        private Hashtable additionalHeaders;
        internal DateTime InstanceDate { get; set; }
        
        protected byte[] GetBodyBytesAndLength(string body, out int contentLength)
        {
            var content = Encoding.UTF8.GetBytes(body);
            contentLength = content.Length;
            return content;
        }

        public TableClient(CloudStorageAccount account)
        {
            _account = account;
            InstanceDate = DateTime.UtcNow;
            DateHeader = DateTime.UtcNow.ToString("R");
            additionalHeaders = new Hashtable();
            additionalHeaders.Add("DataServiceVersion", "1.0;NetFx");
            additionalHeaders.Add("MaxDataServiceVersion", "1.0;NetFx");
            additionalHeaders.Add("Content-Type", ContentType);
        }

        public HttpStatusCode CreateTable(string tableName)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
            "<entry xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\"  " +
            "xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" " +
            "xmlns=\"http://www.w3.org/2005/Atom\"> " +
            "<id>http://" + _account.AccountName + ".table.core.windows.net/Tables('"
                + tableName +
            "')</id>" +
            "<title />" +
            "<updated>" + InstanceDate.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z") + "</updated>" +
            "<author><name/></author> " +
            "<content type=\"application/xml\"><m:properties><d:TableName>" + tableName + "</d:TableName></m:properties></content></entry>";

            int contentLength = 0;
            byte[] payload = GetBodyBytesAndLength(xml, out contentLength);
            string header = CreateAuthorizationHeader(payload, "/" + _account.AccountName + "/Tables()");
            return AzureStorageHttpHelper.SendWebRequest(_account.UriEndpoints["Table"] + "/Tables()", header, DateHeader, VersionHeader, payload, contentLength, "POST", false, this.additionalHeaders).StatusCode;
        }


        public HttpStatusCode InsertTableEntity(string tablename, string partitionKey, string rowKey, DateTime timeStamp, System.Collections.ArrayList tableEntityProperties)
        {
            var timestamp = timeStamp.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z");

            string xml =
                StringUtility.Format("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><entry xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\" xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" m:etag=\"W/&quot;datetime'2013-11-11T18%3A46%3A19.4277424Z'&quot;\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "<id>http://{0}.table.core.windows.net/{5}(PartitionKey='{2}',RowKey='{3}')</id>" +
                "<title/><updated>{1}</updated><author><name /></author>" +
                "<link />" +
                //"<category term=\"{0}.Tables\" scheme=\"http://schemas.microsoft.com/ado/2007/08/dataservices/scheme\" />" +
                "<content type=\"application/xml\"><m:properties><d:PartitionKey>{2}</d:PartitionKey><d:RowKey>{3}</d:RowKey>" +
                "<d:Timestamp m:type=\"Edm.DateTime\">{1}</d:Timestamp>" +
                "{4}" +
                "</m:properties>" +
                "</content>" +
                "</entry>", _account.AccountName, timestamp, partitionKey, rowKey, GetTableXml(tableEntityProperties), tablename);

            int contentLength = 0;
            byte[] payload = GetBodyBytesAndLength(xml, out contentLength);
            string header = CreateAuthorizationHeader(payload, StringUtility.Format("/{0}/{1}", _account.AccountName, tablename));
            return AzureStorageHttpHelper.SendWebRequest(StringUtility.Format("{0}/{1}", _account.UriEndpoints["Table"], tablename), header, DateHeader, VersionHeader, payload, contentLength, "POST", false, this.additionalHeaders).StatusCode;
        }

        public HttpStatusCode InsertTableEntity(string tablename, string partitionKey, string rowKey, DateTime timeStamp, Hashtable tableEntityProperties)
        {
            var xml = FormatEntityXml(tablename, partitionKey, rowKey, timeStamp, tableEntityProperties);
            var contentLength = 0;
            var payload = GetBodyBytesAndLength(xml, out contentLength);
            var header = CreateAuthorizationHeader(payload, StringUtility.Format("/{0}/{1}", _account.AccountName, tablename));
            return AzureStorageHttpHelper.SendWebRequest(StringUtility.Format("{0}/{1}", _account.UriEndpoints["Table"], tablename), header, DateHeader, VersionHeader, payload, contentLength, "POST", false, this.additionalHeaders).StatusCode;
        }

        public HttpStatusCode UpdateTableEntity(string tablename, string partitionKey, string rowKey, DateTime timeStamp, Hashtable tableEntityProperties)
        {
            var xml = FormatEntityXml(tablename, partitionKey, rowKey, timeStamp, tableEntityProperties);
            var contentLength = 0;
            var payload = GetBodyBytesAndLength(xml, out contentLength);
            var header = CreateAuthorizationHeader(payload, StringUtility.Format("/{0}/{1}(PartitionKey='{2}',RowKey='{3}')", _account.AccountName, tablename, partitionKey, rowKey));
            return AzureStorageHttpHelper.SendWebRequest(StringUtility.Format("{0}/{1}(PartitionKey='{2}',RowKey='{3}')", _account.UriEndpoints["Table"], tablename, partitionKey, rowKey), header, DateHeader, VersionHeader, payload, contentLength, "PUT", false, this.additionalHeaders).StatusCode;
        }

        private string FormatEntityXml(string tablename, string partitionKey, string rowKey, DateTime timeStamp, Hashtable tableEntityProperties)
        {
            var timestamp = timeStamp.ToString("yyyy-MM-ddTHH:mm:ss.0000000Z");

            string xml =
                StringUtility.Format("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><entry xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\" xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                "<id>http://{0}.table.core.windows.net/{5}(PartitionKey='{2}',RowKey='{3}')</id>" +
                "<title/><updated>{1}</updated><author><name /></author>" +
                "<link />" +
                //"<category term=\"{0}.Tables\" scheme=\"http://schemas.microsoft.com/ado/2007/08/dataservices/scheme\" />" +
                "<content type=\"application/xml\"><m:properties><d:PartitionKey>{2}</d:PartitionKey><d:RowKey>{3}</d:RowKey>" +
                "<d:Timestamp m:type=\"Edm.DateTime\">{1}</d:Timestamp>" +
                "{4}" +
                "</m:properties>" +
                "</content>" +
                "</entry>", _account.AccountName, timestamp, partitionKey, rowKey, GetTableXml(tableEntityProperties), tablename);
            return xml;
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
            var header = CreateAuthorizationHeader(null, StringUtility.Format("/{0}/{1}(PartitionKey='{2}',RowKey='{3}')", _account.AccountName, tablename, partitionKey, rowKey));
            var response = AzureStorageHttpHelper.SendWebRequest(StringUtility.Format("{0}/{1}(PartitionKey='{2}',RowKey='{3}')", _account.UriEndpoints["Table"], tablename, partitionKey, rowKey), header, DateHeader, VersionHeader, null, 0, "GET", false, this.additionalHeaders);
            var entities = ParseResponse(response.Body);
            if (entities.Count == 1)
            {
                return entities[0] as Hashtable;
            }
            return null;
        }

        public ArrayList QueryTable(string tablename, string query)
        {
            if (query.IsNullOrEmpty())
            {
                query = "";
            } 
            else
            {
                query = "$filter=" + query.Replace(" ", "%20");
            }
            var header = CreateAuthorizationHeader(null, StringUtility.Format("/{0}/{1}()", _account.AccountName, tablename));
            var response = AzureStorageHttpHelper.SendWebRequest(StringUtility.Format("{0}/{1}()?{2}", _account.UriEndpoints["Table"], tablename, query), header, DateHeader, VersionHeader, null, 0, "GET", false, this.additionalHeaders);
            return ParseResponse(response.Body);
        }

        private ArrayList ParseResponse(string xml)
        {
            var results = new ArrayList();
            string entityToken = null;
            var nextStart = 0;
            while (null != (entityToken = NextToken(xml, "<m:properties>", "</m:properties>", nextStart, out nextStart)))
            {
                var currentObject = new Hashtable();

                string propertyToken = null;
                int nextPropertyStart = 0;
                while (null != (propertyToken = NextToken(entityToken, "<d:", "</d", nextPropertyStart, out nextPropertyStart)))
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
                    if (currentObject.Contains(propertyName)) continue;
                    switch (type)
                    {
                        case "Edm.String":
                            currentObject.Add(propertyName, rawvalue);
                            break;
                        case "Edm.DateTime":
                            // not supported
                            break;
                        case "Edm.Int64":
                            currentObject.Add(propertyName, Int64.Parse(rawvalue));
                            break;
                        case "Edm.Int32":
                            currentObject.Add(propertyName, Int32.Parse(rawvalue));
                            break;
                        case "Edm.Double":
                            currentObject.Add(propertyName, Double.Parse(rawvalue));
                            break;
                        case "Edm.Boolean":
                            currentObject.Add(propertyName, rawvalue == "true");
                            break;
                        case "Edm.Guid":
                            // not supported
                            break;
                    }
                }
                results.Add(currentObject);
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
            if (end < 0) return null;
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
        protected string CreateAuthorizationHeader(byte[] content, string canonicalResource)
        {
            //string toSign = String.Format("{0}\n{1}\n{2}\n{3}\n{4}",
            //                              HttpVerb, hash, contentType, InstanceDate, canonicalResource);
            string toSign = StringUtility.Format("{0}\n{1}", InstanceDate.ToString("R"), canonicalResource);
            string signature;
            var hmacBytes = SHA.computeHMAC_SHA256(Convert.FromBase64String(_account.AccountKey), Encoding.UTF8.GetBytes(toSign));
            signature = Convert.ToBase64String(hmacBytes).Replace("!", "+").Replace("*", "/"); ;

            return "SharedKeyLite " + _account.AccountName + ":" + signature;
        }

        #endregion
    }


}


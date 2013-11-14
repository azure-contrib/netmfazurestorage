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

namespace netmfazurestorage.Blob
{
    internal class BlobClient
    {
        private readonly CloudStorageAccount _account;

        internal BlobClient(CloudStorageAccount account)
        {
            _account = account;
            HttpVerb = "PUT";
            DateHeader = DateTime.Now.ToString("R");
        }

        internal bool PutBlockBlob(string containerName, string blobName, string fileNamePath)
        {
            try
            {
                string deploymentPath =
                    StringUtility.Format("http://{0}.blob.core.windows.net/{1}/{2}", _account.AccountName, containerName,
                                         blobName);
                int contentLength;
                byte[] ms = GetPackageFileBytesAndLength(fileNamePath, out contentLength);

                string canResource = StringUtility.Format("/{0}/{1}/{2}", _account.AccountName, containerName, blobName);

                string authHeader = CreateAuthorizationHeader(canResource, "\nx-ms-blob-type:BlockBlob", contentLength);

                try
                {
                    var blobTypeHeaders = new Hashtable();
                    blobTypeHeaders.Add("x-ms-blob-type", "BlockBlob");
                    var response = AzureStorageHttpHelper.SendWebRequest(deploymentPath, authHeader, DateHeader, VersionHeader, ms, contentLength, "GET", true, blobTypeHeaders);
                    if (response.StatusCode != HttpStatusCode.Accepted)
                    {
                        Debug.Print("Deployment Path was " + deploymentPath);
                        Debug.Print("Auth Header was " + authHeader);
                        Debug.Print("Ms was " + ms.Length);
                        Debug.Print("Length was " + contentLength);
                    }
                    else
                    {
                        Debug.Print("Success");
                        Debug.Print("Auth Header was " + authHeader);
                    }

                    return response.StatusCode == HttpStatusCode.Accepted;
                }
                catch (WebException wex)
                {
                    Debug.Print(wex.ToString());
                    return false;
                }
            }
            catch(IOException ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }
            catch(Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }

            return true;
        }

        protected byte[] GetPackageFileBytesAndLength(string fileName, out int contentLength)
        {
            byte[] ms = null;
            contentLength = 0;
            if (fileName != null)
            {
                using (StreamReader sr = new StreamReader(File.Open(fileName, FileMode.Open)))
                {
                    string data = sr.ReadToEnd();
                    ms = Encoding.UTF8.GetBytes(data);
                    contentLength = ms.Length;
                }
            }
            return ms;
        }

        protected string CreateAuthorizationHeader(String canResource, string options = "", int contentLength = 0)
        {
            string toSign = StringUtility.Format("{0}\n\n\n{1}\n\n\n\n\n\n\n\n{5}\nx-ms-date:{2}\nx-ms-version:{3}\n{4}",
                                          HttpVerb, contentLength, DateHeader, VersionHeader, canResource, options);

            string signature;

            var hmacBytes = SHA.computeHMAC_SHA256(Convert.FromBase64String(_account.AccountKey), Encoding.UTF8.GetBytes(toSign));
            signature = Convert.ToBase64String(hmacBytes).Replace("!", "+").Replace("*", "/");;
           
            return "SharedKey " + _account.AccountName + ":" + signature;
        }

        internal const string VersionHeader = "2011-08-18";

        protected string DateHeader { get; set; }

        public string HttpVerb { get; set; }
    }
}

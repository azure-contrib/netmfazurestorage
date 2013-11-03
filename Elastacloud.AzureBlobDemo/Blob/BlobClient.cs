using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using ElzeKool;
using Microsoft.SPOT;
using NetMf.CommonExtensions;

namespace netmfazurestorage.Blob
{
    internal class BlobClient
    {
        private string _accountName;
        private string _accountKey;

        internal BlobClient(string accountName, string accountKey)
        {
            HttpVerb = "PUT";
            DateHeader = DateTime.Now.ToString("R");
            _accountName = accountName;
            _accountKey = accountKey;
        }

        internal bool PutBlob(string containerName, string blobName, string fileNamePath)
        {
            try
            {
                string deploymentPath =
                    StringUtility.Format("http://{0}.blob.core.windows.net/{1}/{2}", _accountName, containerName,
                                         blobName);
                int contentLength;
                byte[] ms = GetPackageFileBytesAndLength(fileNamePath, out contentLength);

                string canResource = StringUtility.Format("/{0}/{1}/{2}", _accountName, containerName, blobName);

                string authHeader = CreateAuthorizationHeader(canResource, "\nx-ms-blob-type:BlockBlob", contentLength);

                try
                {
                    var success = SendWebRequest(deploymentPath, authHeader, ms, contentLength);
                    if (!success)
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

                    return success;
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

        private HttpWebRequest PrepareRequest(string url, string authHeader, byte[] fileBytes = null,
                                              int contentLength = 0)
        {
            var uri = new Uri(url);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = HttpVerb;
            request.ContentLength = contentLength;
            request.Headers.Add("x-ms-date", DateHeader);
            request.Headers.Add("x-ms-version", VersionHeader);
            request.Headers.Add("Authorization", authHeader);
            request.Expect = "100-continue";

            if (contentLength != 0)
            {
                request.Headers.Add("x-ms-blob-type", "BlockBlob");
                request.GetRequestStream().Write(fileBytes, 0, fileBytes.Length);
            }
            return request;
        }

        protected bool SendWebRequest(string url, string authHeader, byte[] fileBytes = null, int contentLength = 0)
        {
            HttpWebRequest request = PrepareRequest(url, authHeader, fileBytes, contentLength);
            try
            {
                HttpWebResponse response;
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        Debug.Print("Container or blob has been created! " + url);
                        return true;
                    }
                    else if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        Debug.Print("Container or blob action has been completed");
                        return true;
                    }//this code smell results from the slight difference in throwing between netmf and .net
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new WebException("Forbidden", (WebExceptionStatus) HttpStatusCode.Forbidden);
                    }
                    else
                    {
                        var responseBody = "";
                        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        {
                            responseBody = sr.ReadToEnd();
                        }

                        Debug.Print("Error Status " + response.StatusCode);
                        Debug.Print(responseBody);
                        return false;
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
                return false;
            }
        }

        protected string CreateAuthorizationHeader(String canResource, string options = "", int contentLength = 0)
        {
            string toSign = StringUtility.Format("{0}\n\n\n{1}\n\n\n\n\n\n\n\n{5}\nx-ms-date:{2}\nx-ms-version:{3}\n{4}",
                                          HttpVerb, contentLength, DateHeader, VersionHeader, canResource, options);

            string signature;

            var hmacBytes = SHA.computeHMAC_SHA256(Convert.FromBase64String(_accountKey), Encoding.UTF8.GetBytes(toSign));
            signature = Convert.ToBase64String(hmacBytes).Replace("!", "+").Replace("*", "/");;
           
            return "SharedKey " + _accountName + ":" + signature;
        }

        internal const string VersionHeader = "2011-08-18";

        protected string DateHeader { get; set; }

        public string HttpVerb { get; set; }
    }
}

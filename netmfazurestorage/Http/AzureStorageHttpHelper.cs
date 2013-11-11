using System;
using System.IO;
using System.Net;
using Microsoft.SPOT;

namespace netmfazurestorage.Http
{
    /// <summary>
    /// A common helper class for HTTP access to Windows Azure Storage
    /// </summary>
    public static class AzureStorageHttpHelper
    {
        public static BasicHttpResponse SendWebRequest(string url, string authHeader, string dateHeader, string versionHeader, byte[] fileBytes = null, int contentLength = 0, string httpVerb = "GET")
        {
            string responseBody = "";
            HttpStatusCode responseStatusCode = HttpStatusCode.Ambiguous;
            HttpWebRequest request = PrepareRequest(url, authHeader, dateHeader, versionHeader, fileBytes, contentLength, httpVerb);
            try
            {
                HttpWebResponse response;
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    responseStatusCode = response.StatusCode;
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        Debug.Print("Asset has been created!");
                    }
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        Debug.Print("Action has been completed");
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
                    Debug.Print("Asset already exists!");
                }
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Forbidden)
                {
                    Debug.Print("Problem with signature. Check next debug statement for stack");
                }
            }

            Debug.Print(responseBody);
            return new BasicHttpResponse() {Body = responseBody, StatusCode = responseStatusCode};
        }

        /// <summary>
        /// Prepares a HttpWebRequest with required headers of x-ms-date, x-ms-version and Authorization
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authHeader"></param>
        /// <param name="dateHeader"></param>
        /// <param name="versionHeader"></param>
        /// <param name="fileBytes"></param>
        /// <param name="contentLength"></param>
        /// <param name="httpVerb"></param>
        /// <returns></returns>
        private static HttpWebRequest PrepareRequest(string url, string authHeader, string dateHeader, string versionHeader, byte[] fileBytes = null, int contentLength = 0, string httpVerb = "GET")
        {
            var uri = new Uri(url);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = httpVerb;
            request.ContentLength = contentLength;
            request.Headers.Add("x-ms-date", dateHeader);
            request.Headers.Add("x-ms-version", versionHeader);
            request.Headers.Add("Authorization", authHeader);
            if (contentLength != 0)
            {
                request.GetRequestStream().Write(fileBytes, 0, fileBytes.Length);
            }
            return request;
        }

    }

}

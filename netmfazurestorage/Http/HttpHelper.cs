using System;
using System.IO;
using System.Net;
using Microsoft.SPOT;

namespace netmfazurestorage.Http
{
    /// <summary>
    /// oh boy I hate helpers like this.
    /// </summary>
    public static class HttpHelper
    {
        public static string SendWebRequest(string url, string authHeader, string dateHeader, string versionHeader, byte[] fileBytes = null, int contentLength = 0, string httpVerb = "GET")
        {
            string responseBody = "";
            HttpWebRequest request = PrepareRequest(url, authHeader, dateHeader, versionHeader, fileBytes, contentLength, httpVerb);
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

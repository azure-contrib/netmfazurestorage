using System;
using System.Net;
using Microsoft.SPOT;

namespace netmfazurestorage.Http
{
    public struct BasicHttpResponse
    {
        public string Body { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}

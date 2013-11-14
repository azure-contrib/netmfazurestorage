using System;
using NetMf.CommonExtensions;

namespace netmfazurestorage.Account
{
    public class CloudStorageAccount
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string UriEndpoints { get; set; }

        public CloudStorageAccount(string accountName, string accountKey, string uriEndpoints)
        {
            AccountName = accountName;
            AccountKey = accountKey;
            UriEndpoints = uriEndpoints;
        }

        public CloudStorageAccount(string accountName, string accountKey) : this (accountName,accountKey,StringUtility.Format("http://{0}.blob.core.windows.net/", accountName))
        {
            
        }

        public static CloudStorageAccount Parse(string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}

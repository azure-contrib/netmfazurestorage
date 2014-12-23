using System;
using System.IO;
using System.Text;
using Microsoft.SPOT;
using netmfazurestorage.Account;
using netmfazurestorage.Blob;
using netmfazurestorage.Http;
using netmfazurestorage.Table;

namespace TestRunner.Tests
{
    public class BlobTests
    {
        private BlobClient client;

        public BlobTests(string account, string key)
        {
            this.client = new BlobClient(new CloudStorageAccount(account,key));
        }

        public void Run()
        {
            AzureStorageHttpHelper.AttachFiddler = false;

            this.TestCreate();

            this.TestInsertBlob();

            this.TestDeleteBlob();

            this.TestDeleteContainer();
        }

        private void TestCreate()
        {
            this.client.CreateContainer("blobnetmftest");
        }

        private void TestInsertBlob()
        {
            using (MemoryStream ms = new MemoryStream())
            using (TextWriter tw = new StreamWriter(ms))
            {
                tw.WriteLine("Line 1");
                tw.WriteLine("Line 2");
                tw.WriteLine("Line 3");
                tw.Flush();
                byte[] bytes = ms.ToArray();

                this.client.PutBlockBlob("blobnetmftest", "testfile1.txt", bytes);
            }
        }

        private void TestDeleteBlob()
        {
            this.client.DeleteBlob("blobnetmftest", "testfile1.txt");
        }

        private void TestDeleteContainer()
        {
            this.client.DeleteContainer("blobnetmftest");
        }
    }
}

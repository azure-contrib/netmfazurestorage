using System;
using Microsoft.SPOT;
using Elastacloud.AzureBlobDemo.Table;
using System.Collections;

namespace Elastacloud.AzureBlobDemo.Tests
{
    public class TableTests
    {
        TableClient client;

        public TableTests(string account, string key)
        {
            this.client = new TableClient(account, key);
        }

        public void Run()
        {
            this.TestCreate();
            this.TestInsert();
        }

        private void TestCreate()
        {
            this.client.CreateTable("netmftest");
            
        
        }

        public void TestInsert()
        {
            var values = new System.Collections.Hashtable();
            values.Add("field1", "value1");
            values.Add("field2", "value2");
            this.client.InsertEntitiy("netmftest", "1", Guid.NewGuid().ToString(), values);
        }


    }
}

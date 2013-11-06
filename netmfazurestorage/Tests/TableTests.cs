using System;
using netmfazurestorage.Table;

namespace netmfazurestorage.Tests
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
            var values = new System.Collections.ArrayList();
            var tep = new TableEntityProperty();
            tep.Name = "field1";
            tep.Value = "value1";
            tep.Type = "Edm.String";

            values.Add(tep);

            tep = new TableEntityProperty();
            tep.Name = "field2";
            tep.Value = "value2";
            tep.Type = "Edm.String";
            values.Add(tep);

            this.client.InsertTableEntity("netmftest", "1", Guid.NewGuid().ToString(), DateTime.Now, values);
        }


    }
}
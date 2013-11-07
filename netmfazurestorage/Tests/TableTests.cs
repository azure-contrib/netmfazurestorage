using System;
using netmfazurestorage.Table;
using System.Collections;

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
            this.TestInsertDouble();
            this.TestInsertExperimental();
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

        public void TestInsertDouble()
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

            tep = new TableEntityProperty();
            tep.Name = "field3";
            tep.Value = "5.0";
            tep.Type = "Edm.Double";
            values.Add(tep);

            this.client.InsertTableEntity("netmftest", "1", Guid.NewGuid().ToString(), DateTime.Now, values);
        }


        public void TestInsertExperimental()
        {
            var values = new Hashtable();
            values.Add("guidfield", Guid.NewGuid());
            values.Add("int32field", 32);
            values.Add("stringfield", "string");
            //values.Add("datetimefield", DateTime.Now);
            values.Add("doublefield", (double)123.22);
            values.Add("int64field", (Int64)64);
            values.Add("boolfield", true);
            this.client.InsertTableEntity_Experimental("netmftest", "2", Guid.NewGuid().ToString(), DateTime.Now, values);
        }

    }
}
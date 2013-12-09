using System;
using Microsoft.SPOT;
using netmfazurestorage.Queue;
using netmfazurestorage.Table;
using System.Net;
using System.Collections;
using netmfazurestorage.Helper;

namespace cloudbrew3
{
    public class Program
    {
        private static QueueClient _queueClient;
        private static TableClient _blobClient;
        private const string AccountName = "netmftest";  // please upload pictures of cats and other larger animals to this storage account.
        private const string AccountKey = "gYU/Nf/ib97kHQrVkxhdD3y0lKz6ZOVaR1FjeDpESecGuqZOEq1TE+5+SXfZ/DBzKsXH3m0NDsLxTbTqQxL9yA==";

        public static void Main()
        {
            DeviceAssist.SetupDefault();

            var account = new netmfazurestorage.Account.CloudStorageAccount(AccountName, AccountKey);
            _queueClient = new QueueClient(account);
            _blobClient = new TableClient(account);

            var cloudBrew = "lotsofbeer";

            CreateQueue(cloudBrew);
            CreateQueueMessage(cloudBrew, "Skynet is READY");
            var peeked = PeekQueueMessage(cloudBrew);
            Debug.Print(peeked.MessageId);

            var message = RetrieveQueueMessage(cloudBrew);
            Debug.Print(message.MessageId);

            Debug.Assert(peeked.MessageId == message.MessageId);
            DeleteQueueMessage(cloudBrew, message.MessageId, message.PopReceipt);
            DeleteQueue(cloudBrew);

            //table approach
            TestCreate();

            var startTime = DateTime.Now;
            TestInsert();
            var completeTime = DateTime.Now;
            Debug.Print("Original Time = " + (completeTime - startTime).ToString());

            startTime = DateTime.Now;
            TestInsertDouble();
            completeTime = DateTime.Now;
            Debug.Print("Double Time = " + (completeTime - startTime).ToString());

            startTime = DateTime.Now;
            TestInsertExperimental();
            completeTime = DateTime.Now;
            Debug.Print("Experimental Time = " + (completeTime - startTime).ToString());

            QuerySingleEntity();
            QueryMultipleEntities();
            UpdateTableEntity();

        }
        private static QueueMessageWrapper PeekQueueMessage(string queueName)
        {
            return _queueClient.PeekQueueMessage(queueName);
        }

        private static void DeleteQueue(string queueName)
        {
            _queueClient.DeleteQueue(queueName);
        }

        private static void DeleteQueueMessage(string queueName, string messageId, string popReceipt)
        {
            _queueClient.DeleteMessage(queueName, messageId, popReceipt);
        }

        private static QueueMessageWrapper RetrieveQueueMessage(string queueName)
        {
            var message = _queueClient.RetrieveQueueMessage(queueName);
            Debug.Print(message.Message);

            return message;
        }

        private static void CreateQueue(string queueName)
        {
            _queueClient.CreateQueue(queueName);
        }

        private static void CreateQueueMessage(string queueName, string messageBody)
        {
            _queueClient.CreateQueueMessage(queueName, messageBody);
        }


        private static void TestCreate()
        {
            _blobClient.CreateTable("ranetmftest");

        }

        public static void TestInsert()
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

            var code = _blobClient.InsertTableEntity("ranetmftest", "1", Guid.NewGuid().ToString(), DateTime.UtcNow, values);
            Debug.Assert(HttpStatusCode.Created == code.StatusCode);
        }

        public static void TestInsertDouble()
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

            var code = _blobClient.InsertTableEntity("ranetmftest", "1", Guid.NewGuid().ToString(), DateTime.Now, values);
            Debug.Assert(HttpStatusCode.Created == code.StatusCode);
        }


        public static void TestInsertExperimental()
        {
            var values = new Hashtable();
            values.Add("guidfield", Guid.NewGuid());
            values.Add("int32field", 32);
            values.Add("stringfield", "string");
            //values.Add("datetimefield", DateTime.Now); // not sure why this is appearing as a null in the table
            values.Add("doublefield", (double)123.22);
            values.Add("int64field", (Int64)64);
            values.Add("boolfield", true);
            var code = _blobClient.InsertTableEntity("ranetmftest", "2", Guid.NewGuid().ToString(), DateTime.Now, values);
            Debug.Assert(code == System.Net.HttpStatusCode.Created);
        }

        public static void QuerySingleEntity()
        {
            var output = _blobClient.QueryTable("ranetmftest", "2", "2b331c6e-4d7b-152b-d433-5c1a57988a75");
            Debug.Assert(null != output);
        }

        public static void QueryMultipleEntities()
        {
            var output = _blobClient.QueryTable("ranetmftest", "PartitionKey eq '2'");
            Debug.Assert(null != output);
            Debug.Assert(output.Count > 0);
        }

        public static void UpdateTableEntity()
        {
            var rowKey = Guid.NewGuid();
            var values = new Hashtable();
            values.Add("guidfield", Guid.NewGuid());
            values.Add("int32field", 32);
            values.Add("stringfield", "string");
            //values.Add("datetimefield", DateTime.Now); // not sure why this is appearing as a null in the table
            values.Add("doublefield", (double)123.22);
            values.Add("int64field", (Int64)64);
            values.Add("boolfield", true);
            var code1 = _blobClient.InsertTableEntity("ranetmftest", "3", rowKey.ToString(), DateTime.Now, values);

            values["stringfield"] = "updated string";
            var code2 = _blobClient.UpdateTableEntity("ranetmftest", "3", rowKey.ToString(), DateTime.Now, values);
        }
    }
}

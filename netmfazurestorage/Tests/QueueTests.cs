using System;
using Microsoft.SPOT;
using NetMf.CommonExtensions;
using netmfazurestorage.Account;
using netmfazurestorage.Queue;

namespace netmfazurestorage.Tests
{
    public class QueueTests
    {
        private QueueClient _queueClient;

        public QueueTests(string accountName, string accountKey)
        {
            _queueClient = new QueueClient(new CloudStorageAccount(accountName, accountKey));
        }

        public void Run()
        {
            var testRun = Guid.NewGuid().ToString().Replace("-", "");//your tablename goes here!
            CreateQueue(testRun);
            CreateQueueMessage(testRun, "Skynet is READY");
            var peeked = PeekQueueMessage(testRun);
            Debug.Print(peeked.MessageId);

            var message = RetrieveQueueMessage(testRun);
            Debug.Print(message.MessageId);

            Debug.Assert(peeked.MessageId == message.MessageId);
            DeleteQueueMessage(testRun, message.MessageId, message.PopReceipt);
            DeleteQueue(testRun);
        }

        private QueueMessageWrapper PeekQueueMessage(string queueName)
        {
            return _queueClient.PeekQueueMessage(queueName);
        }

        private void DeleteQueue(string queueName)
        {
            _queueClient.DeleteQueue(queueName);
        }

        private void DeleteQueueMessage(string queueName, string messageId, string popReceipt)
        {
            _queueClient.DeleteMessage(queueName, messageId, popReceipt);
        }

        private QueueMessageWrapper RetrieveQueueMessage(string queueName)
        {
            var message = _queueClient.RetrieveQueueMessage(queueName);
            Debug.Print(message.Message);

            return message;
        }

        private void CreateQueue(string queueName)
        {
            _queueClient.CreateQueue(queueName);
        }

        private void CreateQueueMessage(string queueName, string messageBody)
        {
            _queueClient.CreateQueueMessage(queueName, messageBody);
        }
    }
}

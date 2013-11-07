using System;
using Microsoft.SPOT;
using netmfazurestorage.Queue;

namespace netmfazurestorage.Tests
{
    public class QueueTests
    {
        private QueueClient _queueClient;

        public QueueTests(string accountName, string accountKey)
        {
            _queueClient = new QueueClient(accountName, accountKey);
        }

        public void Run()
        {
            CreateQueue("netmfmessages");
            CreateQueueMessage("netmfmessages", "Skynet is READY");
            RetrieveQueueMessage("netmfmessages");
        }

        private void RetrieveQueueMessage(string queueName)
        {
            var message = _queueClient.RetrieveQueueMessage(queueName);
            Debug.Print(message.Message);
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

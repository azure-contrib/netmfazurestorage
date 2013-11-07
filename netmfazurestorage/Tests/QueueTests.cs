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
        }

        private void CreateQueue(string queueName)
        {
            _queueClient.CreateQueue(queueName);
        }
    }
}

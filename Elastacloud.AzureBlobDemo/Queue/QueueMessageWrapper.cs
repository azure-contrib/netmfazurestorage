namespace netmfazurestorage.Queue
{
    public class QueueMessageWrapper
    {
        public string Message { get; set; }
        public string PopReceipt { get; set; }
        public string MessageId { get; set; }
    }
}
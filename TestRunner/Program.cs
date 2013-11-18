using netmfazurestorage.Tests;

namespace netmfazurestorage.TestRunner
{
    public class Program
    {
        private const string AccountName = "netmftest";  // please upload pictures of cats and other larger animals to this storage account.
        private const string AccountKey = "gYU/Nf/ib97kHQrVkxhdD3y0lKz6ZOVaR1FjeDpESecGuqZOEq1TE+5+SXfZ/DBzKsXH3m0NDsLxTbTqQxL9yA==";

        public static void Main()
        {
            var queueTests = new QueueTests(AccountName, AccountKey);
            queueTests.Run();

            var tableTests = new TableTests(AccountName, AccountKey);
            tableTests.Run();
        }
    }
}

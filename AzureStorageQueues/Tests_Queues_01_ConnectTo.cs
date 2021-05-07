using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace AzureStorageQueues
{
    [TestClass]
    public class Tests_Queues_01_ConnectTo
    {
        [TestMethod]
        public async Task Test_01_ConnectTo()
        {
            var devSA = CloudStorageAccount.DevelopmentStorageAccount;
            var devClient = devSA.CreateCloudQueueClient();

            var myFirstQueueName = "test-q-create";
            var devQ = devClient.GetQueueReference(myFirstQueueName);
            await devQ.CreateIfNotExistsAsync();
        }

        [TestMethod]
        public async Task Test_02_ConnectTo()
        {
            var saName = ConfigurationManager.AzureStorageAccountName;
            var saKey = ConfigurationManager.AzureStorageAccountKey;

            var saCreds = new StorageCredentials(saName, saKey);
            var saConfig = new CloudStorageAccount(saCreds, true);
            var azClient = saConfig.CreateCloudQueueClient();

            var myFirstQueueName = "test-q-create";
            var azQ = azClient.GetQueueReference(myFirstQueueName);
            await azQ.CreateIfNotExistsAsync();
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorageQueues
{
    [TestClass]
    public class Tests_Queues_30_SAS
    {
        private CloudQueueClient azClient = null;
        private string testQname = "test-q-sas";
        private string _sasToken_ProcessingMessages = null;
        private string _sasToken_Add = null;
        private string QueueUrl = null;

        public Tests_Queues_30_SAS()
        {
            var saName = ConfigurationManager.AzureStorageAccountName;
            var saKey = ConfigurationManager.AzureStorageAccountKey;

            var saCreds = new StorageCredentials(saName, saKey);
            var saConfig = new CloudStorageAccount(saCreds, true);

            azClient = saConfig.CreateCloudQueueClient();

            var azQ = azClient.GetQueueReference(testQname);
            azQ.CreateIfNotExistsAsync().Wait();
            azQ.ClearAsync().Wait();

            _sasToken_ProcessingMessages = azQ.GetSharedAccessSignature(
                new SharedAccessQueuePolicy
                {
                    Permissions = SharedAccessQueuePermissions.ProcessMessages,
                    SharedAccessExpiryTime = new DateTimeOffset(DateTime.Now.AddSeconds(90))
                });

            _sasToken_Add = azQ.GetSharedAccessSignature(
                new SharedAccessQueuePolicy
                {
                    Permissions = SharedAccessQueuePermissions.Add,
                    SharedAccessExpiryTime = new DateTimeOffset(DateTime.Now.AddSeconds(90))
                });

            QueueUrl = ConfigurationManager.QueueUrl;
        }

        [TestMethod]
        public async Task Test_31_Add()
        {
            var qClient = new CloudQueueClient(
                new Uri(QueueUrl),
                new StorageCredentials(_sasToken_Add)
                );
            var azQ = qClient.GetQueueReference(testQname);

            //bool myQexists = await azQ.ExistsAsync();

            await azQ.AddMessageAsync(new CloudQueueMessage("data"));
        }

        [TestMethod]
        public async Task Test_32_ProcessMessages()
        {
            var qClient = new CloudQueueClient(
                new Uri(QueueUrl),
                new StorageCredentials(_sasToken_ProcessingMessages)
            );
            var azQ = qClient.GetQueueReference(testQname);

            //bool myQexists = await azQ.ExistsAsync();

            var msg = await azQ.GetMessageAsync();

            if (msg != null)
            {
                await azQ.DeleteMessageAsync(msg);
            }
        }
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorageQueues
{
    [TestClass]
    public class Tests_Queues_20_Metadata
    {
        private CloudQueueClient azClient = null;
        private string testQname = "test-q-meta";

        public Tests_Queues_20_Metadata()
        {
            var saName = ConfigurationManager.AzureStorageAccountName;
            var saKey = ConfigurationManager.AzureStorageAccountKey;

            var saCreds = new StorageCredentials(saName, saKey);
            var saConfig = new CloudStorageAccount(saCreds, true);

            azClient = saConfig.CreateCloudQueueClient();

            var azQ = azClient.GetQueueReference(testQname);
            azQ.CreateIfNotExistsAsync().Wait();

            azQ.Metadata.Add(new KeyValuePair<string, string>("PollingGap", "10"));
            azQ.SetMetadataAsync().Wait();
        }

        [TestMethod]
        public async Task Test_20_Meta()
        {
            var theQ = azClient.GetQueueReference(testQname);
            await theQ.FetchAttributesAsync();
            theQ.Metadata.Should().NotBeNull();
            theQ.Metadata.Count.Should().NotBe(0);

            theQ.Metadata["PollingGap"].Should().NotBeEmpty();

            var pollingGap = int.Parse(theQ.Metadata["PollingGap"]);

            bool cancel = false;

            do
            {
                var msgReceived = await theQ.GetMessageAsync();
                if (msgReceived != null)
                {
                    await theQ.DeleteMessageAsync(msgReceived);
                }
                Thread.Sleep(pollingGap);
                cancel = true;
            } while (!cancel);
        }
    }
}

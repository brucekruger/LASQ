using System;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorageQueues
{
    [TestClass]
    public class Tests_Queues_10_Messages
    {
        private CloudQueueClient azClient = null;

        public Tests_Queues_10_Messages()
        {
            var saName = ConfigurationManager.AzureStorageAccountName;
            var saKey = ConfigurationManager.AzureStorageAccountKey;

            var saCreds = new StorageCredentials(saName, saKey);
            var saConfig = new CloudStorageAccount(saCreds, true);

            azClient = saConfig.CreateCloudQueueClient();
        }

        [TestMethod]
        public async Task Test_10_SendReceiveMessages()
        {
            var testQueueName = "test-q-snr";
            var azQ = azClient.GetQueueReference(testQueueName);
            await azQ.CreateIfNotExistsAsync();
            await azQ.ClearAsync();

            var data = "the quick brown fox jumps over the lazy dog";
            var msgToSend = new CloudQueueMessage(data);

            await azQ.AddMessageAsync(msgToSend);

            var msgReceived = await azQ.GetMessageAsync();

            msgReceived.AsString.Should().Be(data);
            msgReceived.AsString.Should().Be(msgToSend.AsString);

            msgReceived.DequeueCount.Should().Be(1);

            await azQ.DeleteMessageAsync(msgReceived);
        }

        [TestMethod]
        public async Task Test_11_MsgOps()
        {
            var testQname = "test-q-vis";
            var azQ = azClient.GetQueueReference(testQname);

            await azQ.CreateIfNotExistsAsync();
            await azQ.ClearAsync();

            var data = "my data";
            var msgToSend = new CloudQueueMessage(data);

            await azQ.AddMessageAsync(msgToSend);

            var msgReceived = await azQ.GetMessageAsync();

            msgReceived.NextVisibleTime.Should().NotBeNull();
            msgReceived.NextVisibleTime.Should().BeMoreThan(DateTimeOffset.Now.Offset);
            msgReceived.NextVisibleTime.Should().BeLessThan(DateTimeOffset.Now.AddSeconds(30).Offset);

            await azQ.UpdateMessageAsync(msgReceived, TimeSpan.FromSeconds(5), MessageUpdateFields.Visibility);

            await azQ.DeleteMessageAsync(msgReceived);
        }

        [TestMethod]
        public async Task Test_12_ReceiveBatch()
        {
            var testQname = "test-q-batch";
            var azQ = azClient.GetQueueReference(testQname);

            await azQ.CreateIfNotExistsAsync();
            await azQ.ClearAsync();

            var data = "my data";
            var msgToSend = new CloudQueueMessage(data);

            await azQ.AddMessageAsync(msgToSend);
            await azQ.AddMessageAsync(msgToSend);
            await azQ.AddMessageAsync(msgToSend);

            var msgs = await azQ.GetMessagesAsync(3);
            foreach (var msg in msgs)
            {
                // here we process each message
                await azQ.DeleteMessageAsync(msg);
            }
        }

        [TestMethod]
        public async Task Test_13_MaxSize()
        {
            var maxMsgSize = (int) CloudQueueMessage.MaxMessageSize;
            maxMsgSize.Should().Be(64 * 1024);

            var testQname = "test-q-maxmsg";
            var azQ = azClient.GetQueueReference(testQname);

            await azQ.CreateIfNotExistsAsync();

            var rawData = new byte[48 * 1024]; // 48kB
            var bMsg = CloudQueueMessage.CreateCloudQueueMessageFromByteArray(rawData);
            await azQ.AddMessageAsync(bMsg);

            var data = string.Empty.PadLeft(40 * 1024, '*');
            Convert.ToBase64String(Encoding.UTF8.GetBytes(data)).Length.Should().BeLessThan(maxMsgSize);

            var sMsg = new CloudQueueMessage(data);
            await azQ.AddMessageAsync(sMsg);
        }
    }
}

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;

namespace AzureStorageQueues
{
    public static class ConfigurationManager
    {
        public static string AzureStorageAccountName = null;
        public static string AzureStorageAccountKey = null;
        public static string QueueUrl = null;

        static ConfigurationManager()
        {
            var kvUri = "https://learnazure-key-vault.vault.azure.net/";

            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

            var azureStorageAccountNameSecret = client.GetSecret("AzureStorageAccountName");
            AzureStorageAccountName = azureStorageAccountNameSecret.Value.Value;

            var azureStorageAccountKeySecret = client.GetSecret("AzureStorageAccountKey");
            AzureStorageAccountKey = azureStorageAccountKeySecret.Value.Value;

            var queueUrlSecret = client.GetSecret("QueueUrl");
            QueueUrl = queueUrlSecret.Value.Value;
        }
    }
}

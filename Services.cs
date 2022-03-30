using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace IceCreamRatingAPI
{
    public class KeyVaultService
    {
        public async Task<string> GetSecretValue(string keyName)
        {
            string secret = "";
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            var uri = Environment.GetEnvironmentVariable("keyvault");
            var secretBundle = await keyVaultClient.GetSecretAsync(uri, keyName).ConfigureAwait(false);
            secret = secretBundle.Value;
            Console.WriteLine(secret);
            return secret;
        }
    }
}

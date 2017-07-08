// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/azure-keyvault/blob/master/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common.Tooling;

namespace Nuke.Azure.KeyVault
{
    [Serializable]
    [PublicAPI]
    public class KeyVaultTaskSettings : ISettingsEntity
    {
        /// <summary><p>The client id of an AzureAd application with permissions for the required operations.</p></summary>
        public string ClientId { get; internal set; }

        /// <summary><p>The secret of the AzureAd application.</p></summary>
        public string ClientSecret { get; internal set; }

        /// <summary><p>The name of the secret to obtain.</p></summary>
        public string SecretName { get; internal set; }

        /// <summary><p>The base url of the Azure Key Vault.</p></summary>
        public string VaultBaseUrl { get; internal set; }
    }
}

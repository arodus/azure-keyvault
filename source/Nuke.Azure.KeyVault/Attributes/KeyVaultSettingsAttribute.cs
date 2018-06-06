// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/azure-keyvault/blob/master/LICENSE

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Execution;

namespace Nuke.Azure.KeyVault
{
    /// <summary>Defines where the KeyVault login details can be found. Either <see cref="VaultBaseUrl"/> or <see cref="VaultBaseUrlParameterName"/> must be set.</summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class KeyVaultSettingsAttribute : Attribute
    {
        private readonly ParameterService _parameterService = new ParameterService();
        private string _clientId;
        private string _vaultBaseUrl;

        /// <summary>Defines where the KeyVault login details can be found. Either <see cref="VaultBaseUrl"/> or <see cref="VaultBaseUrlParameterName"/> must be set.</summary>
        public KeyVaultSettingsAttribute ()
        {
        }

        /// <summary>Defines where the KeyVault login details can be found. Either <see cref="VaultBaseUrl"/> or <see cref="VaultBaseUrlParameterName"/> must be set.</summary>
        /// <param name="vaultBaseUrlParameterName">The name of the parameter or environment variable which contains the base url to the Azure Key Vault.</param>
        /// <param name="clientIdParameterName">The name of the parameter or environment variable which contains the id of an AzureAd application with permissions for the required operations.</param>
        /// <param name="clientSecretParameterName">The name of the parameter or environment variable which contains the secret of the AzureAd application.</param>
        public KeyVaultSettingsAttribute (string vaultBaseUrlParameterName, string clientIdParameterName, string clientSecretParameterName)
        {
            VaultBaseUrlParameterName = vaultBaseUrlParameterName;
            ClientIdParameterName = clientIdParameterName;
            ClientSecretParameterName = clientSecretParameterName;
        }

        public bool IsValid => ClientId != null && VaultBaseUrl != null && Secret != null;

        /// <summary><p>The base url of the Azure Key Vault. Either <c>VaultBaseUrl</c> or <c>VaultBaseUrlParameterName</c> must be set.</p></summary>
        [CanBeNull]
        public string VaultBaseUrl
        {
            get => _vaultBaseUrl ?? _parameterService.GetParameter<string>(
                           VaultBaseUrlParameterName.NotNull(
                                   $"Either {nameof(VaultBaseUrl)} or {nameof(VaultBaseUrlParameterName)} must be set."));
            set => _vaultBaseUrl = value;
        }

        /// <summary><p>The client id of an AzureAd application with permissions for the required operations. Either <c>ClientId</c> or <c>ClientIdParameterName</c> must be set.</p></summary>
        [CanBeNull]
        public string ClientId
        {
            get => _clientId ?? _parameterService.GetParameter<string>(
                           ClientIdParameterName.NotNull($"Either {nameof(ClientId)} or {nameof(ClientIdParameterName)} must be set."));
            set => _clientId = value;
        }

        /// <summary><p>The secret of the AzureAd application.</p></summary>
        [CanBeNull]
        public string Secret =>
                _parameterService.GetParameter<string>(ClientSecretParameterName.NotNull($"{nameof(ClientSecretParameterName)} must be set."));

        /// <summary><p>The name of the parameter or environment variable which contains the base url to the Azure Key Vault. Either <c>VaultBaseUrl</c> or <c>VaultBaseUrlParametername</c> must be set.</p></summary>
        [CanBeNull] public string VaultBaseUrlParameterName { get; set; }

        /// <summary><p>The name of the parameter or environment variable which contains the id of an AzureAd application with permissions for the required operations. Either <c>ClientId</c> or <c>ClientIdParameterName</c> must be set.</p></summary>
        [CanBeNull] public string ClientIdParameterName { get; set; }

        /// <summary><p>The name of the parameter or environment variable which contains the secret of the AzureAd application.</p></summary>
        [CanBeNull] public string ClientSecretParameterName { get; set; }

        private string EnsureParameter (string parameterName)
        {
            var parameterNameValue = typeof(KeyVaultSettingsAttribute)
                    .GetProperty(parameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                    .NotNull().GetValue(this) as string;

            ControlFlow.Assert(parameterNameValue != null,
                    $"Either {parameterName.Replace("ParameterName", string.Empty)} or {parameterName} must be set.");
            var value = _parameterService.GetParameter<string>(parameterNameValue);
            return value.NotNull($"{parameterName} was set but value could not be retrieved.");
        }
    }
}

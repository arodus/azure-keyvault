// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/azure-keyvault/blob/master/LICENSE

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Execution;

namespace Nuke.Azure.KeyVault
{
    [PublicAPI]
    public static class KeyVaultTasks
    {
        /// <summary> Attribute to obtain a certificates from from the Azure KeyVault defined by <see cref="ParametersAttribute"/>.</summary>
        [PublicAPI]
        public class CertificateAttribute : SecretAttribute
        {
            /// <summary> Attribute to obtain certificates from from the Azure KeyVault defined by <see cref="ParametersAttribute"/>.</summary>
            /// <param name="certificateName">The name of the certificate to obtain.</param>
            /// <param name="includeKey">If set to <c>true</c> the key of the certificate is also obtained.</param>
            public CertificateAttribute (string certificateName = null, bool includeKey = true)
                    : base(certificateName)
            {
                IncludeKey = includeKey;
            }

            /// <summary>If set to true the key of the certificate is also obtained.</summary>
            public bool IncludeKey { get; set; }

            [CanBeNull]
            public override object GetValue (string memberName, Type memberType)
            {
                var parametersAttribute = GetParametersAttribute();
                if (!parametersAttribute.IsValid) return null;

                var secretName = SecretName ?? memberName;

                if (memberType == typeof(KeyVaultCertificate))
                    return GetCertificateBundle(CreateSettings(secretName, parametersAttribute), IncludeKey);
                throw new NotSupportedException();
            }
        }

        /// <summary>Defines where the KeyVault login details can be found. Either <see cref="VaultBaseUrl"/> or <see cref="VaultBaseUrlParameterName"/> must be set.</summary>
        [PublicAPI]
        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
        public class ParametersAttribute : Attribute
        {
            private readonly ParameterService _parameterService = new ParameterService();
            private string _clientId;
            private string _vaultBaseUrl;

            /// <summary>Defines where the KeyVault login details can be found. Either <see cref="VaultBaseUrl"/> or <see cref="VaultBaseUrlParameterName"/> must be set.</summary>
            public ParametersAttribute ()
            {
            }

            /// <summary>Defines where the KeyVault login details can be found. Either <see cref="VaultBaseUrl"/> or <see cref="VaultBaseUrlParameterName"/> must be set.</summary>
            /// <param name="vaultBaseUrlParameterName">The name of the parameter or environment variable which contains the base url to the Azure Key Vault.</param>
            /// <param name="clientIdParameterName">The name of the parameter or environment variable which contains the id of an AzureAd application with permissions for the required operations.</param>
            /// <param name="clientSecretParameterName">The name of the parameter or environment variable which contains the secret of the AzureAd application.</param>
            public ParametersAttribute (string vaultBaseUrlParameterName, string clientIdParameterName, string clientSecretParameterName)
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
            [CanBeNull] public string Secret =>
                    _parameterService.GetParameter<string>(ClientSecretParameterName.NotNull($"{nameof(ClientSecretParameterName)} must be set."));

            /// <summary><p>The name of the parameter or environment variable which contains the base url to the Azure Key Vault. Either <c>VaultBaseUrl</c> or <c>VaultBaseUrlParametername</c> must be set.</p></summary>
            [CanBeNull] public string VaultBaseUrlParameterName { get; set; }

            /// <summary><p>The name of the parameter or environment variable which contains the id of an AzureAd application with permissions for the required operations. Either <c>ClientId</c> or <c>ClientIdParameterName</c> must be set.</p></summary>
            [CanBeNull] public string ClientIdParameterName { get; set; }

            /// <summary><p>The name of the parameter or environment variable which contains the secret of the AzureAd application.</p></summary>
            [CanBeNull] public string ClientSecretParameterName { get; set; }

            private string EnsureParameter (string parameterName)
            {
                var parameterNameValue = typeof(ParametersAttribute)
                        .GetProperty(parameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                        .NotNull().GetValue(this) as string;

                ControlFlow.Assert(parameterNameValue != null,
                        $"Either {parameterName.Replace("ParameterName", string.Empty)} or {parameterName} must be set.");
                var value = _parameterService.GetParameter<string>(parameterNameValue);
                return value.NotNull($"{parameterName} was set but value could not be retrieved.");
            }
        }

        /// <summary>Attribute to obtain a secret from the Azure KeyVault defined by <see cref="T:Nuke.Azure.KeyVault.KeyVaultTasks.ParametersAttribute"/>.</summary>
        [PublicAPI]
        [AttributeUsage(AttributeTargets.Field)]
        [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
        public class SecretAttribute : InjectionAttributeBase
        {
            protected static KeyVaultTaskSettings CreateSettings (string secretName, ParametersAttribute parametersAttribute)
            {
                ControlFlow.Assert(parametersAttribute.IsValid, "attribute.IsValid");
                return new KeyVaultTaskSettings()
                        // ReSharper disable AssignNullToNotNullAttribute
                        .SetClientId(parametersAttribute.ClientId)
                        .SetVaultBaseUrl(parametersAttribute.VaultBaseUrl)
                        .SetClientSecret(parametersAttribute.Secret)
                        .SetSecretName(secretName);
                // ReSharper restore AssignNullToNotNullAttribute
            }

            protected static ParametersAttribute GetParametersAttribute ()
            {
                var secretAttribute =
                        NukeBuild.Instance.GetType().GetCustomAttributes(typeof(ParametersAttribute), inherit: false)
                                .SingleOrDefault() as ParametersAttribute;

                return secretAttribute.NotNull($"{nameof(ParametersAttribute)} must be defined");
            }

            /// <summary>Obtain the secret with the given name from the KeyVault defined by <see cref="ParametersAttribute"/></summary>
            /// <param name="secretName">The name of the secret to obtain. If the name is null the name of the property is used.</param>
            public SecretAttribute (string secretName = null)
            {
                SecretName = secretName;
            }

            /// <summary><p>The name of the secret to obtain.</p></summary>
            [CanBeNull]
            public string SecretName { get; set; }

            [CanBeNull]
            public override object GetValue ([NotNull] string memberName, [NotNull] Type memberType)
            {
                var parametersAttribute = GetParametersAttribute();
                if (!parametersAttribute.IsValid) return null;

                var secretName = SecretName ?? memberName;
                if (memberType == typeof(string))
                    return GetSecret(CreateSettings(secretName, parametersAttribute));
                if (memberType == typeof(KeyVaultKey))
                    return GetKeyBundle(CreateSettings(secretName, parametersAttribute));
                if (memberType == typeof(KeyVaultCertificate))
                    return GetCertificateBundle(CreateSettings(secretName, parametersAttribute));
                if (memberType == typeof(KeyVault))
                    return LoadVault(CreateSettings(secretName, parametersAttribute));

                throw new NotSupportedException();
            }
        }

        /// <summary>Attribute to obtain the KeyVault defined by <see cref="T:Nuke.Azure.KeyVault.KeyVaultTasks.ParametersAttribute"/> to retrieve multiple items.</summary>
        [PublicAPI]
        public class KeyVaultAttribute : SecretAttribute
        {
            public override object GetValue (string memberName, Type memberType)
            {
                if (memberType != typeof(KeyVault))
                    throw new NotSupportedException();
                return base.GetValue(memberName, memberType);
            }
        }

        // <summary> Attribute to obtain a key from from the Azure KeyVault defined by <see cref="ParametersAttribute"/>.</summary>
        [PublicAPI]
        public class KeyAttribute : SecretAttribute
        {
            // <summary> Attribute to obtain a key from from the Azure KeyVault defined by <see cref="ParametersAttribute"/>.</summary>
            /// <param name="keyName">The name of the key to obtain.</param>
            public KeyAttribute (string keyName = null)
                    : base(keyName)
            {
            }

            public override object GetValue ([NotNull] string memberName, [NotNull] Type memberType)
            {
                if (memberType != typeof(KeyAttribute))
                    throw new NotSupportedException();
                return base.GetValue(memberName, memberType);
            }
        }

        /// <summary><p>Load an Azure Key Vault to obtain secrets.</p></summary>
        public static KeyVault LoadVault (KeyVaultTaskSettings settings)
        {
            AssertTaskSettings(settings);
            ControlFlow.Assert(settings.VaultBaseUrl != null, "settings.VaultBaseUrl != null");

            return CreateVault(settings);
        }

        /// <summary><p>Get a secret.</p></summary>
        public static string GetSecret (KeyVaultTaskSettings settings)
        {
            AssertTaskSettings(settings);
            return GetTaskResult(CreateVault(settings).GetSecret(settings.SecretName));
        }

        /// <summary><p>>Get a certificate.</p></summary>
        public static KeyVaultKey GetKeyBundle (KeyVaultTaskSettings settings)
        {
            AssertTaskSettings(settings);
            return GetTaskResult(CreateVault(settings).GetKey(settings.SecretName));
        }

        /// <summary><p>Get a certificate bundle.</p></summary>
        public static KeyVaultCertificate GetCertificateBundle (KeyVaultTaskSettings settings, bool includeKey = true)
        {
            AssertTaskSettings(settings);
            return GetTaskResult(CreateVault(settings).GetCertificate(settings.SecretName, includeKey));
        }

        private static KeyVault CreateVault (KeyVaultTaskSettings settings)
        {
            return new KeyVault(settings.ClientId, settings.ClientSecret, settings.VaultBaseUrl);
        }

        [AssertionMethod]
        private static void AssertTaskSettings (KeyVaultTaskSettings settings)
        {
            ControlFlow.Assert(settings.VaultBaseUrl != null && settings.SecretName != null,
                    "settings.VaultBaseUrl != null && settings.SecretName != null");
            ControlFlow.Assert(settings.ClientSecret != null, "settings.ClientSecret != null");
            ControlFlow.Assert(settings.ClientId != null, "settings.ClientId != null");
        }

        private static T GetTaskResult<T> ([NotNull] Task<T> task)
        {
            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                ControlFlow.Fail($"Could not retrieve KeyVault value. {ex.Message}");
            }

            return task.Result;
        }
    }
}

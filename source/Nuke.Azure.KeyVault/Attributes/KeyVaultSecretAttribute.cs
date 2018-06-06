// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/azure-keyvault/blob/master/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Execution;

namespace Nuke.Azure.KeyVault
{
    /// <summary>Attribute to obtain a secret from the Azure KeyVault defined by <see cref="KeyVaultSettingsAttribute"/>.</summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Field)]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
    public class KeyVaultSecretAttribute : InjectionAttributeBase
    {
        protected static KeyVaultTaskSettings CreateSettings (string secretName, KeyVaultSettingsAttribute parametersAttribute)
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

        protected static KeyVaultSettingsAttribute GetParametersAttribute ()
        {
            var secretAttribute =
                    NukeBuild.Instance.GetType().GetCustomAttributes(typeof(KeyVaultSettingsAttribute), inherit: false)
                            .SingleOrDefault() as KeyVaultSettingsAttribute;

            return secretAttribute.NotNull($"{nameof(KeyVaultSettingsAttribute)} must be defined");
        }

        /// <summary>Obtain the secret with the given name from the KeyVault defined by <see cref="KeyVaultSettingsAttribute"/></summary>
        /// <param name="secretName">The name of the secret to obtain. If the name is null the name of the property is used.</param>
        public KeyVaultSecretAttribute (string secretName = null)
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
                return KeyVaultTasks.GetSecret(CreateSettings(secretName, parametersAttribute));
            if (memberType == typeof(KeyVaultKey))
                return KeyVaultTasks.GetKeyBundle(CreateSettings(secretName, parametersAttribute));
            if (memberType == typeof(KeyVaultCertificate))
                return KeyVaultTasks.GetCertificateBundle(CreateSettings(secretName, parametersAttribute));
            if (memberType == typeof(KeyVault))
                return KeyVaultTasks.LoadVault(CreateSettings(secretName, parametersAttribute));

            throw new NotSupportedException();
        }
    }
}

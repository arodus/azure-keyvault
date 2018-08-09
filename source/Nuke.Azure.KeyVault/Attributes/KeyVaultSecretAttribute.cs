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
    /// <summary>Attribute to obtain a secret from the Azure KeyVault defined by <see cref="KeyVaultSettingsAttribute"/>.</summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Field)]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
    public class KeyVaultSecretAttribute : InjectionAttributeBase
    {
        protected static KeyVaultTaskSettings CreateSettings (string secretName, KeyVaultSettings keyVaultSettings)
        {
            return new KeyVaultTaskSettings()
                    .SetClientId(keyVaultSettings.ClientId)
                    .SetVaultBaseUrl(keyVaultSettings.BaseUrl)
                    .SetClientSecret(keyVaultSettings.Secret)
                    .SetSecretName(secretName);
        }

        /// <summary>Obtain the secret with the given name from the KeyVault defined by <see cref="KeyVaultSettingsAttribute"/></summary>
        public KeyVaultSecretAttribute ()
        {
        }

        /// <summary>Obtain the secret with the given name from the KeyVault defined by <see cref="KeyVaultSettingsAttribute"/></summary>
        /// <param name="secretName">The name of the secret to obtain. If the name is null the name of the property is used.</param>
        public KeyVaultSecretAttribute (string secretName)
        {
            SecretName = secretName;
        }

        /// <summary><p>The name of the secret to obtain.</p></summary>
        [CanBeNull]
        public string SecretName { get; set; }

        [CanBeNull]
        public string SettingFieldName { get; set; }

        [CanBeNull]
        public override object GetValue ([NotNull] string memberName, [NotNull] Type memberType)
        {
            var settings = GetSettings();

            var secretName = SecretName ?? memberName;
            if (memberType == typeof(string))
                return KeyVaultTasks.GetSecret(CreateSettings(secretName, settings));
            if (memberType == typeof(KeyVaultKey))
                return KeyVaultTasks.GetKeyBundle(CreateSettings(secretName, settings));
            if (memberType == typeof(KeyVaultCertificate))
                return KeyVaultTasks.GetCertificateBundle(CreateSettings(secretName, settings));
            if (memberType == typeof(KeyVault))
                return KeyVaultTasks.LoadVault(CreateSettings(secretName, settings));

            throw new NotSupportedException();
        }

        protected KeyVaultSettings GetSettings ()
        {
            var instance = NukeBuild.Instance.NotNull();
            var instanceType = instance.GetType();

            var attributes = instanceType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(x => new { Field = x, Attribute = x.GetCustomAttribute<KeyVaultSettingsAttribute>() })
                    .Where(x => x.Attribute != null)
                    .ToArray();

            ControlFlow.Assert(attributes.Length > 0,
                    "A field of the type `KeyVaultSettings` with the 'KeyVaultSettingsAttribute' has to be defined in the build class when using Azure KeyVault.");

            KeyVaultSettingsAttribute attribute;
            if (attributes.Length > 1)
            {
                ControlFlow.Assert(SettingFieldName != null,
                        "There is more then one KeyVaultSettings field defined. Please specify which one to use by setting 'SettingFieldName'");
                attribute = attributes.FirstOrDefault(x => x.Field.Name == SettingFieldName)
                        .NotNull($"A KeyVaultSetting field with the name {SettingFieldName} does not exist.").Attribute;
            }
            else
            {
                attribute = attributes[0].Attribute;
            }

            return attribute.GetValue();
        }
    }
}

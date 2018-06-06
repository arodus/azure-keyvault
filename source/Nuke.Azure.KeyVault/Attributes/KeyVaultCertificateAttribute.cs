// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/azure-keyvault/blob/master/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Nuke.Azure.KeyVault
{
    /// <summary> Attribute to obtain a certificates from from the Azure KeyVault defined by <see cref="KeyVaultSettingsAttribute"/>.</summary>
    [PublicAPI]
    public class KeyVaultCertificateAttribute : KeyVaultSecretAttribute
    {
        /// <summary> Attribute to obtain certificates from from the Azure KeyVault defined by <see cref="KeyVaultSettingsAttribute"/>.</summary>
        /// <param name="certificateName">The name of the certificate to obtain.</param>
        /// <param name="includeKey">If set to <c>true</c> the key of the certificate is also obtained.</param>
        public KeyVaultCertificateAttribute (string certificateName = null, bool includeKey = true)
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
                return KeyVaultTasks.GetCertificateBundle(CreateSettings(secretName, parametersAttribute), IncludeKey);
            throw new NotSupportedException();
        }
    }
}

// Copyright Sebastian Karasek, Matthias Koch 2018.
// Distributed under the MIT License.
// https://github.com/nuke-build/azure-keyvault/blob/master/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Nuke.Azure.KeyVault
{
    /// <summary>Attribute to obtain the KeyVault defined by <see cref="KeyVaultSettingsAttribute"/> to retrieve multiple items.</summary>
    [PublicAPI]
    public class KeyVaultAttribute : KeyVaultSecretAttribute
    {
        public override object GetValue (string memberName, Type memberType)
        {
            if (memberType != typeof(KeyVault))
                throw new NotSupportedException();
            return base.GetValue(memberName, memberType);
        }
    }
}

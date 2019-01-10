﻿// Copyright Sebastian Karasek, Matthias Koch 2018.
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
    public class KeyVaultSettings
    {
        [CanBeNull]
        public string Secret { get; internal set; }
        [CanBeNull]
        public string ClientId { get; internal set; }
        [CanBeNull]
        public string BaseUrl { get; internal set; }

        public bool IsValid ([CanBeNull]out string error)
        {
            error = null;
            if (!IsPropertyValid(Secret, nameof(Secret), out var msg)) error += msg;
            if (!IsPropertyValid(ClientId, nameof(ClientId), out msg)) error += msg;
            if (!IsPropertyValid(BaseUrl, nameof(BaseUrl), out msg)) error += msg;
            return error == null;
        }

        private bool IsPropertyValid ([CanBeNull] string value, string name, [CanBeNull] out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                error = $"The value of {name} is invalid{EnvironmentInfo.NewLine}";
                return false;
            }

            return true;
        }
    }

    /// <summary>Defines where the KeyVault login details can be found.</summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Field)]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
    public class KeyVaultSettingsAttribute : InjectionAttributeBase
    {
        private readonly ParameterService _parameterService = new ParameterService();

        /// <summary><p>The base url of the Azure Key Vault. Either <see cref="BaseUrl"/> or <see cref="BaseUrlParameterName"/> must be set.</p></summary>
        [CanBeNull] public string BaseUrl { get; set; }

        /// <summary><p>The client id of an AzureAd application with permissions for the required operations. Either <see cref="ClientId"/> or <see cref="ClientIdParameterName"/> must be set.</p></summary>

        [CanBeNull] public string ClientId { get; set; }

        /// <summary><p>The name of the parameter or environment variable which contains the base url to the Azure Key Vault. Either <see cref='BaseUrl'/> or  <see cref='BaseUrlParameterName'/> must be set.</p></summary>
        [CanBeNull] public string BaseUrlParameterName { get; set; }

        /// <summary><p>The name of the parameter or environment variable which contains the id of an AzureAd application with permissions for the required operations. Either <see cref='ClientId'/> or <see cref='ClientIdParameterName'/> must be set.</p></summary>
        [CanBeNull] public string ClientIdParameterName { get; set; }

        /// <summary><p>The name of the parameter or environment variable which contains the secret of the AzureAd application.</p></summary>
        [CanBeNull] public string ClientSecretParameterName { get; set; }

        /// <summary>Defines where the KeyVault login details can be found.</summary>
        public KeyVaultSettingsAttribute()
        {
        }

        /// <summary>Defines where the KeyVault login details can be found.</summary>
        /// <param name="baseUrlParameterName">The name of the parameter, commandline argument or environment variable which contains the base url to the Azure Key Vault.</param>
        /// <param name="clientIdParameterName">The name of the parameter, commandline argument or environment variable which contains the id of an AzureAd application with permissions for the required operations.</param>
        /// <param name="clientSecretParameterName">The name of the parameter, commandline argument or environment variable which contains the secret of the AzureAd application.</param>
        public KeyVaultSettingsAttribute(string baseUrlParameterName, string clientIdParameterName, string clientSecretParameterName)
        {
            BaseUrlParameterName = baseUrlParameterName;
            ClientIdParameterName = clientIdParameterName;
            ClientSecretParameterName = clientSecretParameterName;
        }

        [NotNull]
        public override object GetValue(MemberInfo member, object instance)
        {
            if (instance is NukeBuild build)
            {
                var memberType = (member as FieldInfo)?.FieldType ?? ((PropertyInfo)member)?.PropertyType;
                ControlFlow.Assert(memberType == typeof(KeyVaultSettings), "memberType == typeof(KeyVaultConfiguration)");
                AssertIsValid();

                return new KeyVaultSettings
                {
                    ClientId = string.IsNullOrWhiteSpace(ClientId) ? GetParameter(ClientIdParameterName, build) : ClientId,
                    BaseUrl = string.IsNullOrWhiteSpace(BaseUrl) ? GetParameter(BaseUrlParameterName, build) : BaseUrl,
                    Secret = GetParameter(ClientSecretParameterName, build)
                };
            }
            return default;
        }

        public KeyVaultSettings GetValue(NukeBuild build)
        {
            return (KeyVaultSettings) GetValue(member: null, build);
        }

        private void AssertIsValid()
        {
            var error = string.Empty;
            if (string.IsNullOrWhiteSpace(BaseUrl) && string.IsNullOrWhiteSpace(BaseUrlParameterName))
                error += EnvironmentInfo.NewLine + $"Either '{nameof(BaseUrl)}' or '{nameof(BaseUrlParameterName)}' must be defined";
            if (string.IsNullOrWhiteSpace(ClientId) && string.IsNullOrWhiteSpace(ClientIdParameterName))
                error += EnvironmentInfo.NewLine + $"Either '{nameof(ClientId)}' or '{nameof(ClientIdParameterName)}' must be defined";
            if (string.IsNullOrWhiteSpace(ClientSecretParameterName))
                error += EnvironmentInfo.NewLine + $"'{nameof(ClientSecretParameterName)}' must be defined";
            ControlFlow.Assert(error == string.Empty, error);
        }

        private string GetParameter(string memberName, NukeBuild build)
        {
            string result = null;
            var fieldInfo = build.GetType().GetField(memberName, ReflectionService.Instance);
            if (fieldInfo != null)
            {
                var parameterAttribute = fieldInfo.GetCustomAttribute<ParameterAttribute>();
                if (parameterAttribute != null)
                {
                    var member = build.GetType().GetMember(memberName, ReflectionService.Instance).Single();
                    result = (string) parameterAttribute.GetValue(member, build);
                    if (string.IsNullOrEmpty(result))
                        result = (string) fieldInfo.GetValue(build);
                }
            }

            if (string.IsNullOrWhiteSpace(result))
                result = _parameterService.GetParameter<string>(memberName);

            return result;
        }
    }
}

# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [vNext]
- Deprecated all attributes in `AzureKeyVaultTasks`. The attributes are now located in `Nuke.Azure.KeyVault`.
- Deprecated `AzureKeyVaultTasks.ParametersAttribute`. Use `Nuke.Azure.KeyVault.KeyVaultSettingsAttribute` instead.

## [0.1.0] / 2018-05-22
- First release. Simply fetch items from the Azure KeyVault for your build.

[vNext]: https://github.com/nuke-build/azure-keyvault/compare/0.1.0...HEAD
[0.1.0]: https://github.com/nuke-build/azure-keyvault/tree/0.1.0

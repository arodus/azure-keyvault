# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [vNext]

## [0.4.0] / 2018-08-15
- Changed `KeyVaultSettingsAttribute` now has to be defined on a field of the type `KeyVaultSettings`.
- Added possibility to get secrets from multiple Key Vaults.
- Added using the value of a filed marked with the `ParameterAttribute` as setting when the name of the field is passed to the `...ParamterName` property of the `KeyVaultSettingsAttribute`.
## [0.3.0] / 2018-08-05
- Removed deprecated members.
- Changed minmum required Nuke version to 0.6.0.
## [0.2.0] / 2018-06-06
- Deprecated all attributes in `AzureKeyVaultTasks`. The attributes are now located in `Nuke.Azure.KeyVault`.
- Deprecated `AzureKeyVaultTasks.ParametersAttribute`. Use `Nuke.Azure.KeyVault.KeyVaultSettingsAttribute` instead.

## [0.1.0] / 2018-05-22
- First release. Simply fetch items from the Azure KeyVault for your build.

[vNext]: https://github.com/nuke-build/azure-keyvault/compare/0.4.0...HEAD
[0.4.0]: https://github.com/nuke-build/azure-keyvault/compare/0.3.0...0.4.0
[0.3.0]: https://github.com/nuke-build/azure-keyvault/compare/0.2.0...0.3.0
[0.2.0]: https://github.com/nuke-build/azure-keyvault/compare/0.1.0...0.2.0
[0.1.0]: https://github.com/nuke-build/azure-keyvault/tree/0.1.0


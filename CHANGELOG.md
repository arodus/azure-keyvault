# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [vNext]

## [0.6.0] / 2019-10-21
- Changed `Nuke.Common` version to 0.22.0

## [0.5.0] / 2019-06-25
- Changed `Nuke.Common` version to 0.20.1
- Added resolving of string secrets from parameters
- Fixed getting the value of `KeyVaultSettingsAttribute`

## [0.4.3] / 2019-02-23
- Changed `Nuke.Common` version to 0.17.0

## [0.4.1] / 2018-08-16
- Fixed build failure when parameters for `KeyVaultSettings` were not found
- Fixed that default values of `[Parameter]` fields were ignored when populating the `KeyVaultSettings`
 
## [0.4.0] / 2018-08-15
- Changed `KeyVaultSettingsAttribute` now has to be defined on a field of the type `KeyVaultSettings`
- Added possibility to get secrets from multiple Key Vaults
- Added using the value of a field marked with the `ParameterAttribute` as setting when the name of the field is passed to the `...ParameterName` property of the `KeyVaultSettingsAttribute`

## [0.3.0] / 2018-08-05
- Removed deprecated members
- Changed minimum required Nuke version to 0.6.0

## [0.2.0] / 2018-06-06
- Deprecated attributes in `AzureKeyVaultTasks`. The attributes are now located in `Nuke.Azure.KeyVault`
- Deprecated `AzureKeyVaultTasks.ParametersAttribute` in favor of `Nuke.Azure.KeyVault.KeyVaultSettingsAttribute`

## [0.1.0] / 2018-05-22
- Initial release

[vNext]: https://github.com/nuke-build/azure-keyvault/compare/0.6.0...HEAD
[0.6.0]: https://github.com/nuke-build/azure-keyvault/compare/0.5.0...0.6.0
[0.5.0]: https://github.com/nuke-build/azure-keyvault/compare/0.4.3...0.5.0
[0.4.3]: https://github.com/nuke-build/azure-keyvault/compare/0.4.1...0.4.3
[0.4.1]: https://github.com/nuke-build/azure-keyvault/compare/0.4.0...0.4.1
[0.4.0]: https://github.com/nuke-build/azure-keyvault/compare/0.3.0...0.4.0
[0.3.0]: https://github.com/nuke-build/azure-keyvault/compare/0.2.0...0.3.0
[0.2.0]: https://github.com/nuke-build/azure-keyvault/compare/0.1.0...0.2.0
[0.1.0]: https://github.com/nuke-build/azure-keyvault/tree/0.1.0


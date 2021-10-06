# Microsoft.Extensions.Hosting for Unity

## [1.2.4] - 2021-09-17
### Fixed version issues

## [1.2.1] - 2021-09-17
### Added
- HostManager events changed from private to protected
### Fixed
- Trace output formatting for unity logger

## [1.2.0] - 2021-07-23
### Fixeds
- Removed reload on change for FileConfigurationSources (caused UnityEditor to enter play mode with long delay)

### Added
- ScriptableObject registration
- MonoBehaviour HostedService registration
- Possibility to pass args to the host
- Host log level parameter in the host Inspector
- Host graceful shutduwn timeout parameter in the host Inspector

## [1.1.0] - 2021-05-27
### Fixed
- Dependency resolving for MonoBehaviours

## [1.0.4] - 2021-05-27
### Fixed
- Dependency resolving for MonoBehaviours

## [1.0.3] - 2021-05-13
### Fixed
- Minor fixes

## [1.0.2] - 2021-04-22
### Added
- MonoBeaviour component registration from existing GameObject

## [1.0.1] - 2021-04-22
### Added
- Host lifetime Unity Events for HostManager

## [1.0.0] - 2021-04-19
### Initial release
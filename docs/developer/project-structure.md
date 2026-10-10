# Project structure

:movie_camera: You can find the code dive session recorded video [here](https://youtu.be/_adhRj19ESY).

```
|_ docs
|_ eng
|_ samples
|_ src
    |_ WinSW
    |   |_ Program.cs
    |   |_ ServiceControllerExtension.cs
    |   |_ Logging
    |_ WinSW.Core
    |   |_ Configuration
    |   |_ Extensions
    |   |_ Logging
    |   |_ Native
    |   |_ Util
    |   |_ WrapperService.cs
    |_ WinSW.Plugins
    |_ WinSW.Tasks
    |_ WinSW.Tests
```

The solution is `src/WinSW.sln`. Shared build settings live in `Directory.Build.props` at the repository root.

## :open_file_folder: samples

This folder contains templates for configuration files. *minimal.xml* contains a template for mandatory configurations and *complete.xml* contains all possible configurations with documentation.

## :open_file_folder: src

### :notebook: WinSW

This is the main executable project. It contains the entry point of the program and the command-line interface.

#### :page_facing_up: Program.cs

This file contains the entry point of the program (`Main` method). It defines the command-line commands (see [CLI commands](../cli-commands.md)), handles elevation (UAC), and sets up logging.

#### :page_facing_up: ServiceControllerExtension.cs

Extension methods for `ServiceController` used by the command-line commands.

#### :open_file_folder: Logging

Custom log4net appenders for console and Windows event log output.

### :notebook: WinSW.Core

This is the main component of the project. It contains the most important logics of the project such as the service wrapper, configuration parsing, the extension API, and logging.

#### :open_file_folder: Configuration

This contains the configuration model. `XmlServiceConfig.cs` parses the XML configuration file, `ServiceConfig.cs` holds the resolved settings, and `SettingNames.cs` centralizes the configuration element names.

#### :open_file_folder: Extensions

This contains the extension engine. `IWinSWExtension.cs` defines the extension interface, `AbstractWinSWExtension.cs` is the convenience base class, and `WinSWExtensionManager.cs` loads and dispatches extensions. See [WinSW extensions](../extensions/extensions.md).

#### :open_file_folder: Native

This contains the Windows API interop (P/Invoke) wrappers: service control, process and job management, security, registry, and resources.

#### :open_file_folder: Util

Helper utilities such as process extensions and XML helpers.

#### :page_facing_up: WrapperService.cs

This is the service base class. It implements the service lifecycle: starting and stopping the child process, monitoring it, and invoking the extensions.

### :notebook: WinSW.Plugins

Placeholder project for future plugins. It references `WinSW.Core`.

### :notebook: WinSW.Tasks

MSBuild tasks used by the build (e.g. trimming).

### :notebook: WinSW.Tests

The automated test suite (xUnit). It covers the command-line interface, configuration parsing, logging, and the extensions.

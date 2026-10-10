# WinSW extensions

Starting from WinSW 2.0, the wrapper provides an internal extension engine and several extensions.
These extensions allow to alter the behavior of the Windows service in order to setup the required service environment.

## Available extensions

* [Shared Directory Mapper](../xml-config-file.md#shareddirectorymapping) - Allows mapping shared drives before starting the executable

## Developer guide

In the current versions of WinSW the extension engine does not support inclusion of external extension DLLs.
Extensions are located by class name within the executable assembly.

### Adding external extensions

The only way to create an external extension is to create a new extension assembly and
  then to merge it into the executable using tools like `ILMerge`.

Generic extension creation guideline:

* The extension assembly should reference the `WinSW.Core` library.
* The extension should implement the `IWinSWExtension` interface, or extend the `AbstractWinSWExtension` base class
  (both located in `src/WinSW.Core/Extensions/`).
* The extension then can override the event handlers offered by the base class.
* The extension should implement the configuration parsing from the `XmlNode` in `Configure`.
* The extension should support disabling from the configuration file.

WinSW engine will automatically locate your extension using the class name in the [XML configuration file](../xml-config-file.md).
See configuration samples provided for the extensions in the core.
For extensions from external assemblies, the `className` field should also specify the assembly name.
It can be done via fully qualified class name or just by the `${CLASS_NAME}, ${ASSEMBLY_NAME}` declaration.

Please note that in the current versions of WinSW the binary compatibility of extension APIs **is not guaranteed**.

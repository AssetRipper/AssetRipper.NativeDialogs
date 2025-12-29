# AssetRipper.NativeDialogs

A cross-platform library for picking files and folders in .NET applications.

## CETCompat requirement (Windows)

On Windows, downstream projects must disable `CETCompat`. Leaving it enabled may cause crashes when opening native file dialogs.

Add the following to the consuming project’s `.csproj`:

```xml
<PropertyGroup>
  <CETCompat>false</CETCompat>
</PropertyGroup>
```
### Why?

Native file dialogs can load .NET Framework components that do not support CET mitigations. When CETCompat is enabled, this may lead to shadow stack violations and runtime crashes. CET affects the entire process, so it must be disabled by the application that uses this library.
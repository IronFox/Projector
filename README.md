# Projector

Projector is a simplified VS C++ solution/project assembler, adhering to the respective format specifications.
It scans source code files automatically and maintains a global project registry to automatically resolve project dependencies.
Projector is firmly geared towards Windows development at this stage, enabling the inclusion of installed libraries from fixed or Registry-identifiable paths.

The application can currently build solutions for Visual Studio 2013, 2015, 2017, 2019, 2022, and 2026.
Installed Visual Studio versions are detected automatically and offered as toolset versions (v120, v140, v141, v142, v143, v145).
Generated projects loaded by Visual Studio will compile as parallel as possible

 

## Download

Ready-to-run builds are published on the [Releases page](https://github.com/IronFox/Projector/releases).
Each release contains `Projector-<version>-win-x64.zip`, a self-contained bundle that includes the .NET runtime, so no separate .NET installation is needed.
Unzip it anywhere and run `Projector/Projector.exe`.

To publish a new release, push a version tag (for example `git tag v1.5.0 && git push origin v1.5.0`) or run the *Release* workflow from the Actions tab.

## Building Projector

Projector is a Windows Forms application targeting .NET 10.
Building it requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0); running it requires the .NET 10 Desktop Runtime.

```
dotnet build Projector.sln -c Release
```

The executable is written to `Projector/bin/Release/net10.0-windows/Projector.exe`.
To produce a single self-contained executable that runs without an installed runtime:

```
dotnet publish Projector/Projector.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

Alternatively, open `Projector.sln` in Visual Studio 2022 (17.14 or newer) or Visual Studio 2026.

## General Usage

In order to use Projector, one XML file needs to be created per project (ending with .project) and a separate one to outline the solution (ending with .solution).
Projector allows to open/review the solution file and present options to generate Visual Studio solutions as well as immediately open them in Visual Studio.

Please see the respective documentation files in this folder for details.

## Restrictions and Behavior

Projector has certain behaviorial characteristics hardcoded, which may require source code modification if undesired:

* All generated projects will statically link to default libraries. This behavior implies that C++ Redistributables need not be shipped with the compiled product
* Solution and project files are updated only if found to be different. When rebuilding a solution but nothing changed, then Visual Studio will not detect the rebuild and not ask to reload its projects
* Multiple instances of Visual Studio are always started with a time delay to avoid undesirable behavior during startup
* A reference to each started Visual Studio instance is maintained during the runtime of Projector. A new instance of Visual Studio will be opened for a solution only if the previous instance is found to have closed. Closing Projector purges these references, enforcing new instances when solutions are being opened by Visual Studio
* Projector assumes referenced projects, solutions, and libraries are compatible to Windows development. Although macros can be specified, no multi-platform behavior has been implemented
* Projector supports x86 and x64 machine targets only
* Projector enables parallel release bulk-build with specific rebuild heuristics. Leaf projects (projects not referenced by other projects) are always selected for x86 and x64, and force-rebuilt. Intermediate projects (projectes referenced by other projects) are not selected by default and only built as found to be different. Projects not part of any selected project dependency path are not built. There is currently no way to memorize this selection
* CUDA support is highly experimental and assumes the CUDA SDK is installed

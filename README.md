# Projector

Projector is a simplified VS C++ solution/project assembler, adhering to the respective format specifications.
It scans source code files automatically and maintains a global project registry to automatically resolve project dependencies.
Projector is firmly geared towards Windows development at this stage, enabling the inclusion of installed libraries from fixed or Registry-identifiable paths.

The application can currently build solutions for Visual Studio 2013, 2015, 2017, and 2019.
Generated projects loaded by Visual Studio will compile as parallel as possible

 

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

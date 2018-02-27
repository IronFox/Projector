# Projector .project files

.project files declare an individual project using XML syntax. 
The file name doubles as the project name.
Since Projector maintains a mapping of names to .project paths, you should use unique project file names across all of your code repositories.

Projector cannot load .project files directly, but requires them to be referenced by a .solution file.

## General syntax

A minimal .project file is declared as following:
```xml
<project type="[type]">
	<!-- ... -->
</project>
```
where ```[type]``` may be:
* ```Application:Console``` (console application)
* ```Application:Windows``` (windows application)
* ```StaticLibrary``` (statically linked library)
* ```DLL``` (dynamically linked library)


## Sources

Assuming your project is supposed to compile some sources, those are declared using any number of ```source``` sections:
```xml
<project type="[type]">
	<source path="[path]" />
</project>
```
This simple example recursively searches for all files (starting at ```[path]```, relative to the .project file) matching any of the known file extensions:
* ```.h .hpp .h++ .hh``` (header file, excluded from compilation)
* ```.c``` (C file)
* ```.cpp .c++ .cc``` (C++ file)
* ```.ico``` (icon file)
* ```.rc .rc2``` (Resource files)
* ```.hlsl .hlsli``` (HLSL shader files, excluded from compilation)

Each source section receives its own filter (project sub-directory) in the final Visual Studio project file.

### Header Inclusion
Adding ```include="true"``` to ```project``` sections may be used to declare that the respective headers should be accessible using ```#include <...>```.
```xml
<project type="[type]">
	<source path="[path]" include="true" />
</project>
```
The respective source root folders are added as _Include Directories_ to the Visual Studio project.


### File/Directory Exclusion
By default, the above declaration will search for all matching files.
If certain files or folders should be excluded, they can be inserted into the ```source``` tag in the form of ```exclude``` sections:
```xml
<project type="[type]">
	<source path="[path]">
	    <exclude find="[needle]" />
	    <exclude dir="[directory]" />
	    <exclude file="[file]" />
	</source>
</project>
```
* ```find```: Files (and folders) are excluded if the specified ```[needle]``` was found to occur in their path at least once.
* ```dir```: Excludes sub-folders whose name matches the given ```[directory]``` string.
* ```file```: Excludes files whose name matches the given ```[file]``` string.


## Macros

Macros can be defined by adding any number of ```macro``` sections to the project declaration:
```xml
<project type="[type]">
	<macro name="[name]" />
	<macro name="[name]">[value]</macro>
</project>
```

## References

Project references may be declared by adding one or more ```referenceProject``` sections:
```xml
<project type="[type]">
	<referenceProject name="[project name]" />
</project>
```
The source root folder(s) of referenced projects are automatically added to the _Include Directories_ of the local Visual Studio project. Note that project header inclusion is not transitive. If you wish to include the headers of a project referenced by some other referenced project, you must re-reference that project locally.

For any given ```[project name]``` a file ```[project name].project``` following this syntax must exist somewhere on your drives.
Projector asks for projects only until located once, and remember their location in a user-local registry file.

## Manifest Files

Custom manifest files can be included by adding one or more ```manifest``` sections:
```xml
<project type="[type]">
	<manifest>[path]</manifest>
</project>
```

## Referencing Compiled, Installed Libraries

Precompiled libraries may be included by adding one or more ```includeLibrary``` sections:
```xml
<project type="[type]">
	<includeLibrary name="[name]">
	    <rootRegistryHint>[registry path]</rootRegistryHint>
	    <include>[path]</include>
	    <linkDirectory>[path]</linkDirectory>
	    <link>[lib name].lib</link>
	</includeLibrary>
</project>
```
### ```<rootRegistryHint>```
Specifies a registry path to look up the library installation folder. The last backslash separates the value name: ```folder[\folder[\folder[...]]]```\\```valueName```.
You can specify any number of registry hints (including none at all).
The first successful lookup will determine the library installation path to match against.

### ```<include>```
Declares a sub-directory (relative to the found installation folder) to include header files from.
You can specify any number of include paths (including none at all), all of which are added to the _Include Directories_ of the local Visual Studio project.
May include **conditions**.

### ```<linkDirectory>```
Declares a sub-directory (relative to the found installation folder) to link .lib files from.
You can specify any number of link directories (including none at all), all of which are added to the _Library Directories_ of the local Visual Studio project.
May include **conditions**.

### ```<link>```
Declares a .lib file (including extension) to link into the local project.
You can specify any number of links, each declaring an additional .lib file to link to the local project.
May include **conditions**.

### Conditions
```include```, ```linkDirectory```, and ```link``` sections support platform/config conditions:
```xml
<project type="[type]">
	<includeLibrary name="[name]">
		<linkDirectory if_platform="[platform]">[path]</linkDirectory>
		<linkDirectory if_platform="[platform]" if_config="[config]">[path]</linkDirectory>
		<linkDirectory if_config="[config]">[path]</linkDirectory>
	</includeLibrary>
</project>
```
All declared conditions must be true for the respective section to be implemented.
If any evaluates to false, the entire section is ignored for the given constellation.
* ```if_platform``` Evaluates whether the targeted platform matches. 
```[platform]``` currently supports:
    * ```x32```: 32 Bit compilation target (x86)
    * ```x64```: 64 Bit compilation target (x64)
* ```if_config``` Evaluates whether the targeted configuration matches. 
```[config]``` currently supports:
    * ```Debug```: Debug configuration
    * ```OptimizedDebug```: Release configuration with debug symbols
    * ```Release```: Release configuration (without debug symbols)

### Example 1: OpenAL 1.1, v3.03 and v3.05
```xml
<project type="[type]">
	<includeLibrary name="OpenAL 1.1 SDK">
		<rootRegistryHint>HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Creative Labs\OpenAL 1.1 Software Development Kit\3.05\InstallDir</rootRegistryHint>
		<rootRegistryHint>HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Creative Labs\OpenAL 1.1 Software Development Kit\3.03\InstallDir</rootRegistryHint>
		<include>./include</include>
		<linkDirectory if_platform="x32">./libs/Win32</linkDirectory>
		<linkDirectory if_platform="x64">./libs/Win64</linkDirectory>
		<link>OpenAL32.lib</link>
	</includeLibrary>
</project>
```

### Example 2: Rpcrt4 (Windows Library, no library paths needed)
```xml
<project type="[type]">
	<includeLibrary name="Rpcrt4">
		<link>Rpcrt4.lib</link>
	</includeLibrary>
</project>
```









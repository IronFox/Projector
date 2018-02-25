# Projector .solution files

.solution files declare each one Visual Studio solution, which should include one or more projects.
The file name doubles as the solution name.
Solution/file names should be unique across all of your code repositories.

Projector opens .solution files directly, implicitly including all referenced .project files.

## General syntax

A minimal .solution file is declared as following:
```xml
<solution>
	<project name="[project name]" />
	<project name="[project name]" />
	<project name="[project name]" />
	<!-- ... -->
</solution>
```
The given projects are loaded explicitely by name.
More projects may be loaded implicitly, by inter-project references.

## Domains

Solutions may be grouped in domains:
```xml
<solution domain="[domain name]">
	<!-- ... -->
</solution>
```
This allows Projector to group them together in the view of all previous solutions.
All members of a domain may be reloaded with a single click.



# Ws2

Common utilities for .net projects.

## Usage

1. Clone:

```bash
git submodule add https://github.com/hexog/ws2.git submodules/ws2

SolutionPath='./'

dotnet sln $SolutionPath add submodules/ws2/Ws2.Data -s /ws2
dotnet sln $SolutionPath add submodules/ws2/Ws2.DependencyInjection -s /ws2
dotnet sln $SolutionPath add submodules/ws2/Ws2.Hosting -s /ws2
```
2. Add project references:

```bash
ProjectPath='./'

dotnet add $ProjectPath reference submodules/ws2/Ws2.Data
dotnet add $ProjectPath reference submodules/ws2/Ws2.DependencyInjection
dotnet add $ProjectPath reference submodules/ws2/Ws2.Hosting
```

3. Update:

```bash
git submodule update --remote --recursive
```

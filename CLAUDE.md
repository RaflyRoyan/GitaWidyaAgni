# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project state

This is a freshly scaffolded .NET 8 solution with no real code yet. Both projects contain only the template `Class1.cs`. There is no README, no test project, and no git repository.

## Structure

- `GitaWidyaAgni.sln`: the Visual Studio 2022 (v17) solution.
- `GWAShared/`: a net8.0 class library with no dependencies. It is meant for shared types.
- `GWALib/`: a net8.0 class library that references `GWAShared` through a `ProjectReference`.

Dependencies go one way only: `GWALib` → `GWAShared`. Keep `GWAShared` free of references to other projects in this solution.

Both projects set `ImplicitUsings` and `Nullable` to `enable`, so nullable reference warnings apply.

**Gotcha:** `GWAShared` is **not listed in `GitaWidyaAgni.sln`**. Building the solution still compiles it through the project reference, but it won't appear in Solution Explorer. To add it:

```
dotnet sln GitaWidyaAgni.sln add GWAShared/GWAShared.csproj
```

## Commands

```
dotnet build GitaWidyaAgni.sln            # build everything
dotnet build GWALib/GWALib.csproj         # build one project (also builds GWAShared)
dotnet build -c Release GitaWidyaAgni.sln
```

There are no tests yet. If you add an xUnit/NUnit/MSTest project, run a single test with:

```
dotnet test <TestProject>.csproj --filter "FullyQualifiedName~Namespace.Class.Method"
```

# Repository Guidelines

## Project Structure & Module Organization

`Ws2.slnx` groups reusable .NET libraries under `src/`, NUnit projects under `test/`, and BenchmarkDotNet executables under `benchmark/`. Libraries cover asynchronous locks, distributed locks, hosting, equality comparison, and dependency injection. Keep changes within the relevant `Ws2.*` module and matching test project. Source-generator snapshots (`*.verified.cs`) live beside their tests.

Shared build settings are in `Directory.Build.props`; dependency versions are centralized in `Directory.Packages.props`, with test additions under `test/`.

## Build, Test, and Development Commands

Run from the repository root:

- `dotnet restore Ws2.slnx` — restore solution dependencies.
- `dotnet build Ws2.slnx --no-restore` — compile libraries, tests, and benchmarks.
- `dotnet test Ws2.slnx --no-build` — run all tests after building.
- `dotnet test test/Ws2.Async.Tests/Ws2.Async.Tests.csproj` — build and test one module.
- `dotnet run -c Release --project benchmark/Ws2.Async.Benchmarks` — select and run performance benchmarks.

Use the SDK pinned in `global.json`. Preserve library compatibility with LTS .NET releases to support a broad range of consuming projects. Choose target frameworks, dependencies, and APIs accordingly; a newer build SDK alone is not a reason to raise minimum runtime requirements. Keep CI aligned with the required SDK and validate supported targets when changing compatibility.

## Coding Style & Naming Conventions

Preserve the style of surrounding code, including its use of `var` or explicit types. For new code without an established local pattern, follow `.editorconfig`: four spaces for C#, two for XML/project files, file-scoped namespaces, braces on new lines, and `System` imports first. Use PascalCase for types and methods, `I`-prefixed interfaces, and camelCase for parameters, locals, and private fields. Match namespaces to folders, respect analyzer diagnostics, and avoid unrelated reformatting.

## Testing Guidelines

Use NUnit `[Test]` methods and descriptive names following existing examples such as `TestPooledLockOnSameKeyWaits`. Add regression coverage in the corresponding `test/Ws2.*.Tests` project; use the dependency-injection integration project for generated registration behavior. Review Verify snapshot changes before accepting updated `*.verified.cs` files.

Collect coverage with `dotnet test Ws2.slnx --collect:"XPlat Code Coverage"`. No minimum coverage threshold is configured.

## Commit & Pull Request Guidelines

History uses short, imperative subjects such as `Fix sharing detection` and `Upgrade packages`. Follow that style and keep each commit focused. In pull requests, describe behavior changes, link relevant issues, list validation commands and results, and explain public API or snapshot changes.

# Ws2

Common utilities for .NET projects.

## Install

```sh
dotnet add package Ws2.DependencyInjection
dotnet add package Ws2.Hosting
dotnet add package Ws2.Async
dotnet add package Ws2.Data
```

## Usage

### Ws2.DependencyInjection

> Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServicesByAttributes(typeof(Program).Assembly);
```

> MyService.cs

```csharp
public interface IMyInterface
{
}

[ScopedService<IMyInterface>]
public class MyService : IMyInterface
{
}

[SingletonService]
public class SomeService
{
}

[ScopedService]
public class SomeScopedService
{
    public SomeScopedService(IMyInterface myService, SomeService someService)
    {
    }
}
```

Available attributes ([source](Ws2.DependencyInjection/ServiceAttribute.cs)):

- `[ScopedService]`
- `[SingletonService]`
- `[TransientService]`

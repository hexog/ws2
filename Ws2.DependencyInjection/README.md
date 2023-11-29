# Ws2.DependencyInjection

Automated dependency injection through attributes.

## Usage

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

Available attributes:

- `[ScopedService]`
- `[SingletonService]`
- `[TransientService]`


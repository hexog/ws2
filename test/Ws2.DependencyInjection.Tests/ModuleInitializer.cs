using System.Runtime.CompilerServices;

namespace Ws2.DependencyInjection.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}
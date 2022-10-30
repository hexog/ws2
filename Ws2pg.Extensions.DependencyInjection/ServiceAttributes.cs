namespace Ws2pg.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Class)]
public class ScopedServiceAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class SingletonServiceAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class IgnoreServiceAttribute : Attribute
{
}

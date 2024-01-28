using Ws2.DependencyInjection.LifetimeAttributes.Abstract;

namespace Ws2.DependencyInjection.Registrars;

internal static class TypeAttributeHelper
{
    public static TAttribute[] GetTypeAttributes<TAttribute>(Type type)
        where TAttribute : ServiceAttribute
    {
        var attributes = type.GetCustomAttributes(typeof(TAttribute), true);
        return Array.ConvertAll(attributes, static a => (TAttribute)a);
    }
}
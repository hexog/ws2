namespace Ws2.DependencyInjection;

public class AttributeServiceCollectionPopulationException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public static void ThrowMultipleTypesWithSameFullNameExist(string typeFullName)
    {
        throw new AttributeServiceCollectionPopulationException($"Multiple types with same full name exist: {typeFullName}");
    }

    public static void ThrowMultipleTypesWithSameNameExist(string typeName)
    {
        throw new AttributeServiceCollectionPopulationException($"Multiple types with same name exist: {typeName}");
    }
}
namespace Ws2.DependencyInjection;

public class AttributeServiceCollectionPopulationException : Exception
{
    public AttributeServiceCollectionPopulationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    public static void ThrowMultipleTypesWithSameFullNameExist(string typeFullName)
    {
        throw new AttributeServiceCollectionPopulationException($"Multiple types with same full name exist: {typeFullName}");
    }

    public static void ThrowMultipleTypesWithSameNameExist(string typeName)
    {
        throw new AttributeServiceCollectionPopulationException($"Multiple types with same name exist: {typeName}");
    }

    public static void ThrowAttributeServiceNotRegistered(Type attributeServiceType)
    {
        throw new AttributeServiceCollectionPopulationException(
            $"Type with attribute service {attributeServiceType.Name} could not be registered"
        );
    }
}
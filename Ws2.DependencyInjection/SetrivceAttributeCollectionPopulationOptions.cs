namespace Ws2.DependencyInjection;

public class ServiceAttributeCollectionPopulationOptions
{
    public static ServiceAttributeCollectionPopulationOptions Global { get; set; } = new();

    public bool ThrowOnMultipleTypesWithSameFullName { get; set; } = false;

    public bool ThrowOnMultipleTypesWithSameName { get; set; } = false;
}
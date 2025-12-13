namespace Ws2.DependencyInjection;

internal class RegisteredSingleton()
{
    public readonly List<Registration> Registrations = new(1);

    public bool AllSeparateInstances = true;

    public void AddRegistration(string? serviceTypeName, bool sharedInstance, object? key)
    {
        Registrations.Add(new Registration(serviceTypeName, sharedInstance, key));
        AllSeparateInstances = AllSeparateInstances && !sharedInstance;
    }

    public readonly struct Registration(
        string? serviceTypeName,
        bool sharedInstance,
        object? key
    )
    {
        public readonly string? ServiceTypeName = serviceTypeName;
        public readonly bool SharedInstance = sharedInstance;
        public readonly object? Key = key;
    }
}
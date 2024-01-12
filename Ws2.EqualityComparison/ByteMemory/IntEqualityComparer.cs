namespace Ws2.EqualityComparison.ByteMemory;

public class IntEqualityComparer : IEqualityComparer<int>
{
	public static readonly IntEqualityComparer Instance = new();

	public bool Equals(int x, int y)
	{
		return x == y;
	}

	public int GetHashCode(int obj)
	{
		return obj;
	}
}
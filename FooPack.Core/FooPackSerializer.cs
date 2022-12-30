namespace FooPack;

public static class FooPackSerializer
{
	private static readonly Dictionary<Type, IFooPackFormatter> _registry = new();

	public static void Register<T>(IFooPackFormatter formatter)
	{
		_registry[typeof(T)] = formatter;
	}

	public static IFooPackFormatter GetFormatter<T>()
	{
		if (_registry.TryGetValue(typeof(T), out var formatter))
		{
			return formatter;
		}

		throw new InvalidOperationException($"No formatter has been found for type {typeof(T).FullName}");
	}

	public static string? Serialize<T>(T obj)
	{
		var f = GetFormatter<T>();

#if NET7_0_OR_GREATER
		// SPECIAL-CASE: .NET 7
		if (f is IFooPackFormatterNet7 f7)
		{
			return f7.SerializeOptimizedNet7(obj);
		}
#endif

		// SPECIAL-CASE: .NET STANDARD 2.1
		return f.Serialize(obj);
	}
}

namespace FooPack
{
	public interface IFooPackFormatter
	{
		string? Serialize(object? obj);
	}

#if NET7_0_OR_GREATER

	public interface IFooPackFormatterNet7 : IFooPackFormatter
	{
		string? SerializeOptimizedNet7(object? obj);
	}

#endif

}

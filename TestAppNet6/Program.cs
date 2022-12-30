using FooPack;
using MyLibNetStandard21;
using MyLibMultiTarget;

Test(new MyClassNet6());
Test(new MyClassInExtLibNetStandard21());
Test(new MyClassInExtLibMultiTarget());

static void Test<T>(T obj)
{
	Console.WriteLine($"TYPE  : {typeof(T).Name}");
	Console.WriteLine($"RESULT: {FooPackSerializer.Serialize<T>(obj)}");
	Console.WriteLine();
}

[FooPackable]
public partial class MyClassNet6
{
	public string? Prop1 { get; set; }
}
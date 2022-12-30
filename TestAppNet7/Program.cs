using FooPack;
using MyLibNetStandard21;
using MyLibNet7;
using MyLibMultiTarget;

Test(new MyClassNet7());
Test(new MyClassInExtLibNetStandard21());
Test(new MyClassInExtLibNet7());
Test(new MyClassInExtLibMultiTarget());

static void Test<T>(T obj)
{
	Console.WriteLine($"TYPE  : {typeof(T).Name}");
	Console.WriteLine($"RESULT: {FooPackSerializer.Serialize<T>(obj)}");
	Console.WriteLine();
}

[FooPackable]
public partial class MyClassNet7
{
	public string? Prop1 { get; set; }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace FooPack;

[Generator]
public class FooPackGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		//// Add the marker attribute
		//context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
		//	"FooSerializableAttribute.g.cs",
		//	SourceText.From(FooSerializerGeneratorHelper.Attribute, Encoding.UTF8))
		//);

		// Do a simple filter for classes
		IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select classes with attributes
				transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx) // select the class with the [FooSerializable] attribute
			)
			.Where(static m => m is not null)!;

		// Combine the selected classes with the `Compilation`
		IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
			= context.CompilationProvider.Combine(classDeclarations.Collect());

		// Generate the source using the compilation and classes
		context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Item1, source.Item2, spc));
	}

	static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is ClassDeclarationSyntax m && m.AttributeLists.Count > 0;

	static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
	{
		// we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
		var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

		// loop through all the attributes on the method
		foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
		{
			foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
			{
				if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
				{
					// weird, we couldn't get the symbol, ignore it
					continue;
				}

				INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
				string fullName = attributeContainingTypeSymbol.ToDisplayString();

				// Is the attribute the [FooSerializable] attribute?
				if (fullName == "FooPack.FooPackableAttribute")
				{
					// return the enum
					return classDeclarationSyntax;
				}
			}
		}

		// we didn't find the attribute we were looking for
		return null;
	}

	static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
	{
		if (classes.IsDefaultOrEmpty)
		{
			// nothing to do yet
			return;
		}

		// I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
		var distinctClasses = classes.Distinct().ToArray();

		if (distinctClasses.Length > 0)
		{
			// generate the source code and add it to the output
			var result = FooPackGeneratorHelper.GenerateSupportClasses(distinctClasses);
			if (string.IsNullOrWhiteSpace(result) == false)
				context.AddSource("FooPackSupportClasses.g.cs", SourceText.From(result!, Encoding.UTF8));
		}
	}
}
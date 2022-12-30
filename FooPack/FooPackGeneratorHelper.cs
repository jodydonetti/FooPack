using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace FooPack;

internal static class FooPackGeneratorHelper
{
	public static string? GenerateSupportClasses(IEnumerable<ClassDeclarationSyntax> classes)
	{
		var sb = new StringBuilder();

		sb.AppendLine("#nullable enable");
		sb.AppendLine();
		sb.AppendLine("using FooPack;");
		sb.AppendLine();

		foreach (var c in classes)
		{
			var cName = c.Identifier.Text;
			var cNamespace = GetNamespace(c);
			var fName = cName + "FooPackFormatter";

			if (string.IsNullOrWhiteSpace(cNamespace) == false)
			{
				sb.AppendLine($"namespace {cNamespace} {{");
			}

			// SERIALIZER
			sb.AppendLine($"public class {fName}");
			sb.AppendLine("  : IFooPackFormatter");
			sb.AppendLine(@"
#if NET7_0_OR_GREATER
, IFooPackFormatterNet7
#endif");
			sb.AppendLine("  {");

			// IMPLEMENT IFooPackFormatter
			sb.AppendLine(@"
public string? Serialize(object? obj) {
	return ""executing normal .NET Standard code..."";
}");

			// IMPLEMENT IFooPackFormatterNet7
			sb.AppendLine(@"
#if NET7_0_OR_GREATER
public string? SerializeOptimizedNet7(object? obj) {
	return ""executing optimized .NET 7 code..."";
}
#endif");

			sb.AppendLine("  }");

			sb.AppendLine();

			// PARTIAL CLASS STATIC CTOR (AUTO-REGISTER)
			sb.AppendLine($"partial class {cName} {{");
			sb.AppendLine($"static {cName}() {{ FooPackSerializer.Register<{cName}>(new {fName}()); }}");
			sb.AppendLine($"}}");

			if (string.IsNullOrWhiteSpace(cNamespace) == false)
			{
				sb.Append("}");
			}
		}

		sb.AppendLine();

		return sb.ToString();
	}

	// determine the namespace the class/enum/struct is declared in, if any
	static string GetNamespace(BaseTypeDeclarationSyntax syntax)
	{
		// If we don't have a namespace at all we'll return an empty string
		// This accounts for the "default namespace" case
		string nameSpace = string.Empty;

		// Get the containing syntax node for the type declaration
		// (could be a nested type, for example)
		SyntaxNode? potentialNamespaceParent = syntax.Parent;

		// Keep moving "out" of nested classes etc until we get to a namespace
		// or until we run out of parents
		while (potentialNamespaceParent != null &&
				potentialNamespaceParent is not NamespaceDeclarationSyntax
				&& potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
		{
			potentialNamespaceParent = potentialNamespaceParent.Parent;
		}

		// Build up the final namespace by looping until we no longer have a namespace declaration
		if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
		{
			// We have a namespace. Use that as the type
			nameSpace = namespaceParent.Name.ToString();

			// Keep moving "out" of the namespace declarations until we 
			// run out of nested namespace declarations
			while (true)
			{
				if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
				{
					break;
				}

				// Add the outer namespace as a prefix to the final namespace
				nameSpace = $"{namespaceParent.Name}.{nameSpace}";
				namespaceParent = parent;
			}
		}

		// return the final namespace
		return nameSpace;
	}
}
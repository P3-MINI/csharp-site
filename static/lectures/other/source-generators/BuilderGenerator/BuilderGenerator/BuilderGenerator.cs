using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Threading;

namespace BuilderGenerator
{
    [Generator]
    public class BuilderGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Add the marker attribute source code
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("GenerateBuilderAttribute.g.cs", SourceText.From(
                    """
                    using System;

                    namespace BuilderGenerator
                    {
                        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
                        public sealed class GenerateBuilderAttribute : Attribute { }
                    }
                    """, Encoding.UTF8));
            });

            // Create the pipeline to find and transform classes
            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                "BuilderGenerator.GenerateBuilderAttribute",
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: (ctx, ct) => GetClassToGenerate(ctx, ct))
                .Where(m => m != null);

            // Register the source output
            context.RegisterSourceOutput(pipeline, (ctx, data) => GenerateCode(ctx, data));
        }

        private static ClassModel? GetClassToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken ct)
        {
            if (context.TargetSymbol is not INamedTypeSymbol symbol) return null;

            ct.ThrowIfCancellationRequested();

            var properties = symbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsReadOnly && p.DeclaredAccessibility == Accessibility.Public && p.SetMethod != null)
                .Select(p => new PropertyModel(
                    p.Name, 
                    p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) 
                ))
                .ToList();

            string ns = symbol.ContainingNamespace.IsGlobalNamespace 
                ? string.Empty 
                : symbol.ContainingNamespace.ToDisplayString();

            return new ClassModel(symbol.Name, ns, properties);
        }

        private static void GenerateCode(SourceProductionContext context, ClassModel? model)
        {
            if (model == null) return;

            var code = new CodeWriter();

            // Handle Namespace
            if (!string.IsNullOrEmpty(model.Namespace))
            {
                code.AppendLine($"namespace {model.Namespace}");
                code.StartBlock();
            }

            // Generate Class Partial
            code.AppendLine($"public partial class {model.ClassName}");
            using (code.Block())
            {
                code.AppendLine($"public static Builder CreateBuilder() => new Builder();");
                code.AppendLine();

                code.AppendLine($"public class Builder");
                using (code.Block())
                {
                    code.AppendLine($"private readonly {model.ClassName} _target = new {model.ClassName}();");
                    code.AppendLine();

                    foreach (var prop in model.Properties)
                    {
                        context.CancellationToken.ThrowIfCancellationRequested();

                        code.AppendLine($"public Builder With{prop.Name}({prop.Type} value)");
                        using (code.Block())
                        {
                            code.AppendLine($"_target.{prop.Name} = value;");
                            code.AppendLine("return this;");
                        }
                        code.AppendLine();
                    }

                    code.AppendLine($"public {model.ClassName} Build() => _target;");
                }
            }

            if (!string.IsNullOrEmpty(model.Namespace))
            {
                code.EndBlock();
            }

            context.AddSource($"{model.ClassName}.Builder.g.cs", SourceText.From(code.ToString(), Encoding.UTF8));
        }
    }
}
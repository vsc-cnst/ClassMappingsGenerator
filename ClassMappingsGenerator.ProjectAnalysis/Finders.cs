using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vsc.Mappings
{
    public static class Finders
    {
        public static Assembly LoadAssembly(Compilation compilation) {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            Assembly assembly;

            using (var ms = new MemoryStream())
            {
                var emitResult = compilation.Emit(ms);

                if (emitResult.Success)
                {
                    // Load the emitted assembly
                    ms.Seek(0, SeekOrigin.Begin);
                    assembly = Assembly.Load(ms.ToArray());
                }
                else
                {
                    throw new Exception("No Assembly!");
                }
            }

            Console.WriteLine($"Assembly loaded in {sw.ElapsedMilliseconds / 1000m}s");
            sw.Stop();

            return assembly;
        }

        /// <summary>
        /// Finds all classes in the specified namespaces of a document.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="targetNamespaces">namespaces to search for classes in. If null or empty, all namespaces will be searched.</param>
        /// <returns></returns>
        public static IEnumerable<ClassDeclarationSyntax> GetClasses(this Document document, string[] targetNamespaces)
        {

            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var root = document.GetSyntaxRootAsync().Result;

            List<SyntaxNode> namespaces;

            if (targetNamespaces == null || targetNamespaces.Length == 0)
            {
                namespaces = root!.ChildNodes().ToList();
            }
            else
            {
                namespaces = root!
                    .ChildNodes()
                    .OfType<NamespaceDeclarationSyntax>()
                    .Where(n => targetNamespaces.Contains(n.Name.ToString()))
                    .SelectMany(n => n.ChildNodes())
                    .ToList();
            }

            var classes = namespaces
                .SelectMany(n => n.ChildNodes())
                .OfType<ClassDeclarationSyntax>()
                .Where(n => n is ClassDeclarationSyntax)
                .ToList();

            return classes;
        }
        

        public static IEnumerable<Type> ToTypes(this IEnumerable<ClassDeclarationSyntax> classDeclarationSyntaxes,
                Assembly assembly, SemanticModel semanticModel
            )
        {
            return classDeclarationSyntaxes
                .Select(classDeclaration => { 

                    var classDeclaredSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                    var classFullyQualifiedName = classDeclaredSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    
                    return assembly.GetType(classFullyQualifiedName?.Replace("global::", "") ?? string.Empty);
                })
                .Where(type => type != null)
                .Cast<Type>()
                .Distinct()
                ;
        }


        public static IEnumerable<Type> AllClasses(Project project, Compilation compilation, string[] targetNamespaces)
        {
            Assembly assembly = LoadAssembly(compilation);

            var types = project.Documents
                        .AsParallel()
                        .Select(d => (document: d, semanticModel: d.GetSemanticModelAsync().Result))
                        .Where(i => i.semanticModel != null)
                        .Cast<(Document document, SemanticModel semanticModel)>()
                        .SelectMany(i => ToTypes(GetClasses(i.document, targetNamespaces), assembly, i.semanticModel))
                        .Distinct()
                        .ToList();
            
            return types!;
        }
    }
}


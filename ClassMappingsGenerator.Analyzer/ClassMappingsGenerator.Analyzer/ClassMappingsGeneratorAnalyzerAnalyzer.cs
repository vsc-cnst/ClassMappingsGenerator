using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace ClassMappingsGenerator.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClassMappingsGeneratorAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ClassMappingsGeneratorAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            if (context.Symbol is not INamedTypeSymbol namedType || namedType.TypeKind != TypeKind.Class)
                return;


            var mapAttributes = namedTypeSymbol.GetAttributes()
                .Where(attr => attr.AttributeClass.Arity == 1) // attribute is generic with one type parameter. e.g. Map<T>
                .Where(attr => attr.AttributeClass?.Name == "MapAttribute")
                .ToList();

            if(mapAttributes.Count == 0)
                return; // No MapAttribute found, nothing to analyze

            // Case 1: generic [Map<T>]
            var genericAttrs = mapAttributes
                .Select(attr => attr.AttributeClass.TypeArguments.FirstOrDefault()) // get the ITypeSymbol representing 'T' (or 'List<int>', etc)
                .Where(t => t != null)
                .Distinct(SymbolEqualityComparer.Default)
                .ToList();

            if (genericAttrs.Count == 0)
                return; // No generic MapAttribute found, nothing to analyze

            // Case 2: non-generic [Map(typeof(T))]
            var nonGenericAttrs = mapAttributes
                .Where(attr => attr.ConstructorArguments.Length >= 1 &&
                    attr.ConstructorArguments[0].Kind == TypedConstantKind.Type &&
                    attr.ConstructorArguments[0].Value is ITypeSymbol typeArg
                )
                .Select(attr => attr.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol) // get the ITypeSymbol representing 'T' (or 'List<int>', etc)
                .Where(t => t != null)
                .Distinct(SymbolEqualityComparer.Default)
                .ToList()
                ;

            if (nonGenericAttrs.Count == 0)
                return; // No generic MapAttribute found, nothing to analyze

            //var mapAttributes = namedTypeSymbol.GetAttributes()
            //    .Where(attr => attr.AttributeClass.Arity == 1) // attribute is generic with one type parameter. e.g. Map<T>
            //    .Where(attr => attr.AttributeClass?.Name == "MapAttribute")
            //    .Select(attr => attr.AttributeClass.TypeArguments.FirstOrDefault()) // get the ITypeSymbol representing 'T' (or 'List<int>', etc)
            //    .Where(t => t != null)
            //    .Distinct(SymbolEqualityComparer.Default)
            //    .ToList();

            // For all such symbols, produce a diagnostic.
            var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}

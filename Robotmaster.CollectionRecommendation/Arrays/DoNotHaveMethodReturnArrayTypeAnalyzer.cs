namespace Robotmaster.CollectionRecommendation.Arrays
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Robotmaster.CollectionRecommendation.Helpers;
    using Robotmaster.CollectionRecommendation.Helpers.Collections;

    /// <summary>
    ///     This is used to analyze and detect for situations where methods return arrays instead of <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotHaveMethodReturnArrayTypeAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        ///     This is the complete diagnostic ID for this analyzer.
        /// </summary>
        public static readonly string DiagnosticId = AnalyzerHelper.GetCompleteAnalyzerId(IdNumber);

        /// <summary>
        ///     The message format to use for diagnostics generated by this analyzer.
        /// </summary>
        internal const string MessageFormat = "The {0} method returned an Array instead of an IReadOnlyList.";

        /// <summary>
        ///     The description to use for diagnostics generated by this analyzer.
        /// </summary>
        private const string Description = "This is when an array is returned by a method instead of an IReadOnlyList.";

        /// <summary>
        ///     The category to use for diagnostics generated by this analyzer.
        /// </summary>
        private const string Category = "Arrays";

        /// <summary>
        ///     The number portion of the analyzer's <see cref="DiagnosticId"/>.
        /// </summary>
        private const int IdNumber = 1;

#pragma warning disable RS1017 // DiagnosticId for analyzers must be a non-null constant.
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, AnalyzerHelper.AnalyzerTitle, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
#pragma warning restore RS1017 // DiagnosticId for analyzers must be a non-null constant.

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // Cast down to a IMethodSymbol.
            var methodSymbol = (IMethodSymbol)context.Symbol;

            // If the method returns void.
            if (methodSymbol.ReturnsVoid)
            {
                // Nothing more needs to be done here; just return null.
                return;
            }

            // If the method starts with the special prefix reserved for property getters.
            if (methodSymbol.Name.StartsWith(CollectionHelper.PropertyGetterPrefix, StringComparison.Ordinal))
            {
                // This is a getter.

                // Any diagnostic will be raised by another analyzer to avoid duplication.
                return;
            }

            // If the method returns an array type.
            if (methodSymbol.ReturnType is IArrayTypeSymbol)
            {
                // For every location where the method is defined.
                foreach (var location in methodSymbol.Locations)
                {
                    // Report a diagnostic that IReadOnlyList should be the type instead.
                    context.ReportDiagnostic(Diagnostic.Create(Rule, location, methodSymbol.Name));
                }
            }
        }
    }
}

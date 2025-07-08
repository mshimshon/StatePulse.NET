using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace StatePulse.Net.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FactSecondParamAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SP0001";

        private static readonly LocalizableString Title = "The onStateChanged parameter of StateOf should not be a lambda";
        private static readonly LocalizableString MessageFormat = "The onStateChanged argument to 'StateOf' cannot be a lambda expression";
        private static readonly LocalizableString Description = "The onStateChanged argument must be a real method.";
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation.Expression, context.CancellationToken);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

            if (methodSymbol == null)
                return;

            // Only proceed if method name is "StateOf"
            if (methodSymbol.Name != "StateOf")
                return;

            // Check method's containing type full name: must be "StatePulse.Net.IStatePulse"
            var containingType = methodSymbol.ContainingType;
            if (containingType == null)
                return;

            var fullTypeName = containingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (fullTypeName != "global::StatePulse.Net.IStatePulse")
                return;

            // Check parameter count
            if (methodSymbol.Parameters.Length < 2)
                return;

            var args = invocation.ArgumentList.Arguments;
            if (args.Count < 2)
                return;

            var secondArg = args[1].Expression;

            if (secondArg.IsKind(SyntaxKind.SimpleLambdaExpression) ||
                secondArg.IsKind(SyntaxKind.ParenthesizedLambdaExpression))
            {
                var diagnostic = Diagnostic.Create(Rule, secondArg.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }

    }
}

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
        private static readonly LocalizableString Description = "The onStateChanged argument must be a method group, not an inline lambda.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            // Analyze generated code (Blazor .razor -> generated C#) and report diagnostics there
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = context.Node as InvocationExpressionSyntax;
            if (invocation == null)
            {
                return;
            }

            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation.Expression, context.CancellationToken);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            // Ensure it's a method named StateOf
            if (methodSymbol.Name != "StateOf")
                return;

            // Ensure it belongs to the correct interface
            var containingType = methodSymbol.ContainingType;
            var fullTypeName = containingType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (fullTypeName != "global::StatePulse.Net.IStatePulse")
                return;

            // Check argument count
            if (invocation.ArgumentList == null)
                return;

            var args = invocation.ArgumentList.Arguments;
            if (args.Count < 2)
                return;

            var secondArg = args[1].Expression;

            if (secondArg is LambdaExpressionSyntax lambda)
            {
                var diagnostic = Diagnostic.Create(Rule, lambda.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}

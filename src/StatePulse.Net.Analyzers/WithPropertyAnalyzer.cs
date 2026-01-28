using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class WithPropertyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "SP002";

    private static readonly DiagnosticDescriptor Rule =
        new DiagnosticDescriptor(
            DiagnosticId,
            "Invalid With property",
            "Property '{0}' must be get; set;. init-only or non-public setters are forbidden.",
            "StatePulse",
            DiagnosticSeverity.Error,
            true
        );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (symbol == null || symbol.Name != "With")
            return;

        if (!symbol.IsExtensionMethod)
            return;

        if (symbol.ContainingNamespace.ToDisplayString() != "StatePulse.Net")
            return;

        if (invocation.ArgumentList.Arguments.Count < 1)
            return;

        var arg = invocation.ArgumentList.Arguments[0].Expression;

        if (arg is not LambdaExpressionSyntax lambda)
            return;

        ExpressionSyntax body = lambda switch
        {
            SimpleLambdaExpressionSyntax s => s.Body as ExpressionSyntax,
            ParenthesizedLambdaExpressionSyntax p => p.Body as ExpressionSyntax,
            _ => null
        };

        if (body is not MemberAccessExpressionSyntax member)
            return;

        var prop = context.SemanticModel.GetSymbolInfo(member).Symbol as IPropertySymbol;
        if (prop == null)
            return;

        var setter = prop.SetMethod;

        if (setter == null || setter.IsInitOnly || setter.DeclaredAccessibility != Accessibility.Public)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                member.Name.GetLocation(),
                prop.Name));
        }
    }
}

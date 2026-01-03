using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Threading.Tasks;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StatePulseStateOfAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "SP001";

    private static readonly DiagnosticDescriptor Rule =
        new DiagnosticDescriptor(
            DiagnosticId,
            "Invalid StateOf binding",
            "StateOf requires first argument '() => this' and second argument a method group returning Task",
            "StatePulse",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
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
        if (symbol == null)
            return;

        if (symbol.Name != "StateOf")
            return;

        if (symbol.ContainingType.Name != "IStatePulse")
            return;

        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 2)
            return;

        var arg0 = args[0].Expression;
        var arg1 = args[1].Expression;

        if (!IsLambdaThis(arg0, context))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, arg0.GetLocation()));
            return;
        }

        if (arg1 is LambdaExpressionSyntax)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, arg1.GetLocation()));
            return;
        }

        var methodSymbol = context.SemanticModel.GetSymbolInfo(arg1).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, arg1.GetLocation()));
            return;
        }

        if (!IsTask(methodSymbol.ReturnType))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, arg1.GetLocation()));
        }
    }

    private static bool IsLambdaThis(ExpressionSyntax expr, SyntaxNodeAnalysisContext context)
    {
        if (expr is not LambdaExpressionSyntax lambda)
            return false;

        ExpressionSyntax body = lambda switch
        {
            SimpleLambdaExpressionSyntax s => s.Body as ExpressionSyntax,
            ParenthesizedLambdaExpressionSyntax p => p.Body as ExpressionSyntax,
            _ => null
        };

        if (body is not ThisExpressionSyntax)
            return false;

        var typeInfo = context.SemanticModel.GetTypeInfo(body).Type;
        return typeInfo != null;
    }

    private static bool IsTask(ITypeSymbol type)
        => type.ToDisplayString() == typeof(Task).FullName;
}

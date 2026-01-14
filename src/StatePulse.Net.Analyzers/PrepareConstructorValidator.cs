using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PrepareCtorAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "SP003";

    private static readonly DiagnosticDescriptor Rule =
        new DiagnosticDescriptor(
            DiagnosticId,
            "Invalid Prepare arguments",
            "Arguments passed to Prepare<{0}> do not match any constructor of {0}",
            "StatePulse",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

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

        var method = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (method is null)
            return;

        if (method.Name != "Prepare" || !method.IsGenericMethod)
            return;

        var reduced = method.ReducedFrom ?? method;

        if (reduced.ContainingType.Name != "IDispatcher")
            return;

        var actionType = reduced.TypeArguments[0];

        var args = invocation.ArgumentList.Arguments;
        var argTypes = args
            .Select(a => context.SemanticModel.GetTypeInfo(a.Expression).Type)
            .ToArray();

        var ctors = actionType
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor);

        bool match = ctors.Any(ctor =>
        {
            if (ctor.Parameters.Length != argTypes.Length)
                return false;

            for (int i = 0; i < argTypes.Length; i++)
            {
                if (!SymbolEqualityComparer.Default.Equals(
                        ctor.Parameters[i].Type, argTypes[i]))
                    return false;
            }

            return true;
        });

        if (!match)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                invocation.GetLocation(),
                actionType.Name));
        }
    }
}

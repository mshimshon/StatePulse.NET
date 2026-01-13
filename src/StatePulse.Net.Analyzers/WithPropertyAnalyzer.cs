using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace StatePulse.Net.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class WithPropertyAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor InvalidPropertyRule =
        new DiagnosticDescriptor(
            id: "SP002",
            title: "Property cannot be used with .With",
            messageFormat: "Property '{0}' must be get; set;. It is get-only or init-only.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(InvalidPropertyRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Must be something like: .With(...)
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        if (memberAccess.Name.Identifier.Text != "With")
            return;

        // Resolve the invoked method symbol
        var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;
        if (symbol is null)
            return;

        // --- FILTER: Only our specific extension method ---
        if (!symbol.IsExtensionMethod)
            return;

        if (symbol.Name != "With")
            return;

        // Namespace + class must match your extension class
        if (symbol.ContainingNamespace.ToDisplayString() != "StatePulse.Net")
            return;

        if (symbol.ContainingType.Name != "DispatcherPrepperExtensions")
            return;

        // Must have at least one argument (the lambda)
        if (invocation.ArgumentList.Arguments.Count == 0)
            return;

        var lambdaExpr = invocation.ArgumentList.Arguments[0].Expression;

        if (lambdaExpr is not LambdaExpressionSyntax lambda)
            return;

        // Lambda must be: p => p.Prop
        if (lambda.Body is not MemberAccessExpressionSyntax propAccess)
            return;

        // Resolve the property symbol
        var propSymbol = context.SemanticModel.GetSymbolInfo(propAccess).Symbol as IPropertySymbol;
        if (propSymbol is null)
            return;

        var setter = propSymbol.SetMethod;

        // --- PROPERTY VALIDATION ---
        bool invalid =
            setter is null ||                  // get-only
            setter.IsInitOnly ||               // init-only
            setter.DeclaredAccessibility != Accessibility.Public; // private/protected/internal setter

        if (invalid)
        {
            var diagnostic = Diagnostic.Create(
                InvalidPropertyRule,
                propAccess.Name.GetLocation(),
                propSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
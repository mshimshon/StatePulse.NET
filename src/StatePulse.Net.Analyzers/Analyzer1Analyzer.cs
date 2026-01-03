using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Analyzer1
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CapitalClassNameAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DEMO001";

        private static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                DiagnosticId,
                "Class name starts with a capital letter",
                "Class name '{0}' starts with a capital letter",
                "Naming",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(
                AnalyzeClass,
                SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;
            var name = classDecl.Identifier.Text;

            if (string.IsNullOrEmpty(name))
                return;

            if (char.IsUpper(name[0]))
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    classDecl.Identifier.GetLocation(),
                    name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}

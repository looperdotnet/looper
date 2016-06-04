using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LooperAnalyzer.Analysis
{
    internal static class SyntaxUtils
    {
        public static StatementSyntax GetParentStatement(this SyntaxNode node) => 
            node.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
    }
}

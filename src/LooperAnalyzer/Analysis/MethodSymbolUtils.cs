using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LooperAnalyzer.Analysis
{
    internal static class MethodSymbolUtils
    {
        private static HashSet<IMethodSymbol> _whitelist;

        public static void InitializeFromCompilation(Microsoft.CodeAnalysis.Compilation compilation)
        {
            if (_whitelist != null) return;

            var whitelistNames = new HashSet<string>
            {
                "Sum",
                "First"
            };

            var filtered = compilation.GetTypeByMetadataName("System.Linq.Enumerable")
                .GetMembers()
                .Where(s => whitelistNames.Contains(s.Name))
                .OfType<IMethodSymbol>();
            _whitelist = new HashSet<IMethodSymbol>(filtered);
        }

        public static bool IsOptimizableConsumerMethod(this IMethodSymbol method) => _whitelist.Contains(method);
    }
}

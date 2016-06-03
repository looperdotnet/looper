using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LooperAnalyzer.Analysis
{
    internal static class SymbolUtils
    {
        private static HashSet<IMethodSymbol> _whitelist;
        private static ITypeSymbol _genericIEnumerableType;

        public static void InitializeFromCompilation(Microsoft.CodeAnalysis.Compilation compilation)
        {
            if (_whitelist != null && _genericIEnumerableType != null) return;

            var whitelistNames = new HashSet<string>
            {
                nameof(Enumerable.First),
                nameof(Enumerable.FirstOrDefault),
                nameof(Enumerable.Single),
                nameof(Enumerable.SingleOrDefault),
                nameof(Enumerable.Any),
                nameof(Enumerable.Average),
                nameof(Enumerable.Count),
                nameof(Enumerable.ElementAt),
                nameof(Enumerable.ElementAtOrDefault),
                nameof(Enumerable.Last),
                nameof(Enumerable.LastOrDefault),
                nameof(Enumerable.Max),
                nameof(Enumerable.Min),
                nameof(Enumerable.Single),
                nameof(Enumerable.SingleOrDefault),
                nameof(Enumerable.Sum),
                nameof(Enumerable.ToArray),
                nameof(Enumerable.ToList),
            };

            var filtered = compilation.GetTypeByMetadataName("System.Linq.Enumerable")
                .GetMembers()
                .Where(s => whitelistNames.Contains(s.Name))
                .OfType<IMethodSymbol>();

            _whitelist = new HashSet<IMethodSymbol>(filtered);
            
            _genericIEnumerableType = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
        }

        public static bool IsOptimizableConsumerMethod(this IMethodSymbol method) => 
            _whitelist.Contains(method.IsExtensionMethod ? method.ReducedFrom : method);

        public static bool IsArrayType(this ITypeSymbol type) => type is IArrayTypeSymbol;

        public static bool IsIEnumerableType(this ITypeSymbol type) =>
            type is INamedTypeSymbol
            && (type.OriginalDefinition == _genericIEnumerableType 
                || type.OriginalDefinition.AllInterfaces.Contains(_genericIEnumerableType));

        public static bool IsOptimizableSourceType(this ITypeSymbol type) => type.IsArrayType() || type.IsIEnumerableType();


    }
}

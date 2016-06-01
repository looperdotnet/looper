using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LooperAnalyzer.Analysis
{
    internal static class SymbolInfoUtils
    {
        private static ITypeSymbol _genericIEnumerableType;

        public static void InitializeFromCompilation(Microsoft.CodeAnalysis.Compilation compilation)
        {
            if (_genericIEnumerableType != null) return;

            _genericIEnumerableType = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
        }

        public static bool IsArrayType(this ITypeSymbol type) => type is IArrayTypeSymbol;

        public static bool IsIEnumerableType(this ITypeSymbol type) =>
            type is INamedTypeSymbol
            && (type.OriginalDefinition == _genericIEnumerableType || type.OriginalDefinition.AllInterfaces.Contains(_genericIEnumerableType));

        public static bool IsOptimizableSourceType(this ITypeSymbol type) => type.IsArrayType() || type.IsIEnumerableType();
    }
}

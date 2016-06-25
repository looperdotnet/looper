using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading.Tasks;

namespace LooperAnalyzer.Test.Helpers
{
    static class ScriptRunningExtensions
    {
        public static async Task<ValueOrException<T>> RunProtectedAsync<T>(this Script<T> script)
        {
            try {
                var result = await script.RunAsync();
                return new ValueOrException<T>(result.ReturnValue);
            }
            catch (Exception e) when (e.StackTrace.Contains("Submission")) {
                return new ValueOrException<T>(e);
            }
        }

        public static ValueOrException<T> RunProtected<T>(this ScriptRunner<T> runner, object globals)
        {
            try {
                var result = runner(globals).Result;
                return new ValueOrException<T>(result);
            }
            catch (Exception e) when (e.StackTrace.Contains("Submission")) {
                return new ValueOrException<T>(e);
            }
        }

    }

    struct ValueOrException<T> : IEquatable<ValueOrException<T>>
    {
        public T Value { get; private set; }
        public Exception Exception { get; private set; }

        public ValueOrException(T value)
        {
            Value = value;
            Exception = null;
        }

        public ValueOrException(Exception e)
        {
            Exception = e;
            Value = default(T);
        }

        public override string ToString() => Exception?.Message ?? Value?.ToString();

        public bool Equals(ValueOrException<T> other) => Value.Equals(other.Value) && Exception?.Message == other.Exception?.Message;
    }
}

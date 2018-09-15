using System;
using System.Threading.Tasks;

namespace ResponsibilityChain
{
    /// <summary>
    /// A handler that returns a completed task with default value of type <typeparamref name="TOut"/>. This is usually set as the last handler in the chain.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    public sealed class ReturnCompletedTaskFromDefaultValueHandler<TIn, TOut> : IHandler<TIn, Task<TOut>>
    {
        static ReturnCompletedTaskFromDefaultValueHandler()
        {
        }

        private ReturnCompletedTaskFromDefaultValueHandler()
        {
        }

        /// <summary>
        /// Singleton instance of this handler.
        /// </summary>
        public static ReturnCompletedTaskFromDefaultValueHandler<TIn, TOut> Instance { get; } =
            new ReturnCompletedTaskFromDefaultValueHandler<TIn, TOut>();

        /// <summary>
        /// Returns a completed task with default value of <typeparamref name="TOut"/> on invocation.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public Task<TOut> Handle(TIn input, Func<TIn, Task<TOut>> next) => Task.FromResult(default(TOut));
    }
}
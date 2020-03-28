using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResponsibilityChain
{
    /// <summary>
    /// A handler that returns a completed task with default value of type <typeparamref name="TOut"/>. This is usually set as the last handler in the chain.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    public class ReturnCompletedTaskWithDefaultValue<TIn, TOut> : IHandler<TIn, Task<TOut>>, IAsyncHandler<TIn, TOut>
    {
        /// <summary>
        /// Returns a completed task with default value of <typeparamref name="TOut"/> on invocation.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="next">The next handler in the chain.</param>
        /// <returns></returns>
        public virtual Task<TOut> Handle(TIn input, Func<TIn, Task<TOut>> next) => Task.FromResult(default(TOut));

        /// <summary>
        /// Returns a completed task with default value of <typeparamref name="TOut"/> on invocation.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="next">The next handler in the chain.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task<TOut> HandleAsync(TIn input, Func<TIn, CancellationToken, Task<TOut>> next, CancellationToken cancellationToken) =>
            Task.FromResult(default(TOut));
    }
}
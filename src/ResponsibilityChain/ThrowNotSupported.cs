using System;
using System.Threading.Tasks;

namespace ResponsibilityChain
{
    /// <summary>
    /// A handler that throws <see cref="NotSupportedException"/>. This is usually set as the last handler in the chain.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    public sealed class ThrowNotSupported<TIn, TOut> : IHandler<TIn, TOut>, IAsyncHandler<TIn, TOut>
    {
        /// <summary>
        /// Throws <see cref="NotSupportedException"/> on invocation.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="next">The next handler in the chain.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public TOut Handle(TIn input, Func<TIn, TOut> next)
        {
            throw new NotSupportedException($"Cannot handle this input. Input information: {typeof(TIn)}");
        }

        /// <summary>
        /// Throws <see cref="NotSupportedException"/> on invocation.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="next">The next handler in the chain.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public Task<TOut> HandleAsync(TIn input, Func<TIn, Task<TOut>> next)
        {
            throw new NotSupportedException($"Cannot handle this input. Input information: {typeof(TIn)}");
        }
    }
}
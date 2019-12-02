using System;
using System.Threading.Tasks;

namespace ResponsibilityChain
{
    /// <summary>
    /// Represents an asynchronous handler (processing element) in the chain.
    /// </summary>
    /// <typeparam name="TIn">The type of the input.</typeparam>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    public interface IAsyncHandler<TIn, TOut> : IHandler
    {
        /// <summary>
        /// Either processes the input then returns result to its caller or passes on the input to the next handler in the chain for further processing.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="next">The next handler in the chain.</param>
        /// <returns></returns>
        Task<TOut> HandleAsync(TIn input, Func<TIn, Task<TOut>> next);
    }
}
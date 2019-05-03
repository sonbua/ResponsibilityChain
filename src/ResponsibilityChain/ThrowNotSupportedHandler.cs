using System;

namespace ResponsibilityChain
{
    /// <summary>
    /// A handler that throws <see cref="NotSupportedException"/>. This is usually set as the last handler in the chain.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    public class ThrowNotSupportedHandler<TIn, TOut> : IHandler<TIn, TOut>
    {
        /// <summary>
        /// Throws <see cref="NotSupportedException"/> on invocation.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public virtual TOut Handle(TIn input, Func<TIn, TOut> next)
        {
            throw new NotSupportedException($"Cannot handle this input. Input information: {typeof(TIn)}");
        }
    }
}
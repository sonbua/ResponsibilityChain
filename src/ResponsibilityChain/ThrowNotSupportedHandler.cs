using System;

namespace ResponsibilityChain
{
    /// <summary>
    /// A handler that throws <see cref="NotSupportedException"/>. This is usually set as the last handler in the chain.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    public sealed class ThrowNotSupportedHandler<TIn, TOut> : IHandler<TIn, TOut>
    {
        static ThrowNotSupportedHandler()
        {
        }

        private ThrowNotSupportedHandler()
        {
        }

        /// <summary>
        /// Singleton instance of this handler.
        /// </summary>
        public static ThrowNotSupportedHandler<TIn, TOut> Instance { get; } =
            new ThrowNotSupportedHandler<TIn, TOut>();

        /// <summary>
        /// Throws <see cref="NotSupportedException"/> on invocation.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public TOut Handle(TIn input, Func<TIn, TOut> next)
        {
            throw new NotSupportedException($"Cannot handle this input. Input information: {typeof(TIn)}");
        }
    }
}
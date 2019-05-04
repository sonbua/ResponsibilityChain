using System;

namespace ResponsibilityChain
{
    /// <summary>
    /// A handler that returns default value of type <typeparamref name="TOut"/>. This is usually set as the last handler in the chain.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    public class ReturnDefaultValue<TIn, TOut> : IHandler<TIn, TOut>
    {
        /// <summary>
        /// Returns default value of <typeparamref name="TOut"/> on invocation.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public virtual TOut Handle(TIn input, Func<TIn, TOut> next) => default(TOut);
    }
}
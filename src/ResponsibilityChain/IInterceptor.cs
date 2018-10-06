using System;

namespace ResponsibilityChain
{
    /// <summary>
    /// Provides a mean to intercept a <typeparamref name="THandler"/>'s execution.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler.</typeparam>
    /// <typeparam name="TIn">The type of input.</typeparam>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    public interface IInterceptor<THandler, TIn, TOut>
        where THandler : IHandler<TIn, TOut>
    {
        /// <summary>
        /// <para>The interceptor is in control, either do something before/after the <paramref name="handler"/>'s execution, or just bypass the <paramref name="handler"/> and move on to the <paramref name="next"/>.</para>
        /// <para>This could be use to implement cross-cutting concerns, such as auditing, logging, transaction, opting-out a feature, etc.</para>
        /// </summary>
        /// <param name="handler">The handler object, which is going to be intercepted.</param>
        /// <param name="input">The input object.</param>
        /// <param name="next">The next handler in the chain.</param>
        /// <returns></returns>
        TOut Intercept(IHandler<TIn, TOut> handler, TIn input, Func<TIn, TOut> next);
    }
}
using System;

namespace ResponsibilityChain
{
    /// <summary>
    /// Defines a common interface for all interception strategies, which are used to intercept a resolved handler.
    /// </summary>
    public interface IInterceptionStrategy
    {
        /// <summary>
        /// Intercepts the given <paramref name="handler"/> object with all interceptors found by the <paramref name="serviceProvider"/>.
        /// </summary>
        /// <param name="handler">The handler object, which is going to be intercepted.</param>
        /// <param name="serviceProvider">The service provider object, which is used to resolve all interceptors of the given <paramref name="handler"/> object.</param>
        /// <typeparam name="THandler"></typeparam>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        IHandler<TIn, TOut> Intercept<THandler, TIn, TOut>(IHandler<TIn, TOut> handler, IServiceProvider serviceProvider)
            where THandler : IHandler<TIn, TOut>;
    }
}
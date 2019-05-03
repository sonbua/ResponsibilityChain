using System.Collections.Generic;
using System.Linq;

namespace ResponsibilityChain.Interception
{
    /// <summary>
    /// Provides a helper method to intercept a handler, given a list of interceptors which implement
    /// <see cref="IInterceptor{THandler,TIn,TOut}"/> interface. See <see cref="Intercept{THandler,TIn,TOut}"/> method
    /// for details.
    /// </summary>
    public static class DefaultInterceptionStrategyHelper
    {
        /// <summary>
        /// Creates multiple wrappers to wrap the <paramref name="handler"/> object in the order the given
        /// <paramref name="interceptors"/>, i.e. the first interceptor will be the innermost wrapper,
        /// and the last interceptor will be the outermost wrapper to the handler.
        /// </summary>
        /// <param name="handler">The handler object to be intercepted.</param>
        /// <param name="interceptors">The interceptors that applied to the given <paramref name="handler"/></param>
        /// <typeparam name="THandler">The type of the handler.</typeparam>
        /// <typeparam name="TIn">The type of the input.</typeparam>
        /// <typeparam name="TOut">The type of the output.</typeparam>
        /// <returns></returns>
        public static IHandler<TIn, TOut> Intercept<THandler, TIn, TOut>(
            THandler handler,
            IEnumerable<IInterceptor<THandler, TIn, TOut>> interceptors)
            where THandler : class, IHandler<TIn, TOut>
        {
            var interceptorArray = interceptors?.ToArray() ?? new IInterceptor<THandler, TIn, TOut>[0];

            return InterceptImpl(handler, interceptorArray);
        }

        private static IHandler<TIn, TOut> InterceptImpl<THandler, TIn, TOut>(
            THandler handler,
            IInterceptor<THandler, TIn, TOut>[] interceptors)
            where THandler : IHandler<TIn, TOut>
        {
            IHandler<TIn, TOut> intercepted = handler;

            foreach (var interceptor in interceptors)
            {
                intercepted = new InterceptedHandler<THandler, TIn, TOut>(intercepted, interceptor);
            }

            return intercepted;
        }
    }
}
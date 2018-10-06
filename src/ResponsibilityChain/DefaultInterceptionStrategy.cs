using System;
using System.Collections.Generic;
using System.Linq;

namespace ResponsibilityChain
{
    /// <summary>
    /// The default interception strategy.
    /// </summary>
    internal class DefaultInterceptionStrategy : IInterceptionStrategy
    {
        static DefaultInterceptionStrategy()
        {
        }

        private DefaultInterceptionStrategy()
        {
        }

        /// <summary>
        /// Singleton instance of this interception strategy.
        /// </summary>
        public static DefaultInterceptionStrategy Instance { get; } = new DefaultInterceptionStrategy();

        /// <summary>
        /// <para>Resolves all possible interceptors of the given <paramref name="handler"/>, which implement <see cref="IInterceptor{THandler,TIn,TOut}"/>.</para>
        /// <para>Then creates multiple wrapped handlers to wrap the given <paramref name="handler"/> in the order they were resolved, i.e. the first interceptor will be the outermost wrapper, and the last interceptor will be the closest wrapper to the wrapped handler.</para>
        /// </summary>
        /// <param name="handler">The handler object to be intercepted.</param>
        /// <param name="serviceProvider">The service provider used to resolve interceptor instances.</param>
        /// <typeparam name="THandler">The type of the handler.</typeparam>
        /// <typeparam name="TIn">The type of the input.</typeparam>
        /// <typeparam name="TOut">The type of the output.</typeparam>
        /// <returns></returns>
        public IHandler<TIn, TOut> Intercept<THandler, TIn, TOut>(
            IHandler<TIn, TOut> handler,
            IServiceProvider serviceProvider
        )
            where THandler : IHandler<TIn, TOut>
        {
            var interceptorEnumerable =
                (IEnumerable<IInterceptor<THandler, TIn, TOut>>)
                serviceProvider.GetService(typeof(IEnumerable<IInterceptor<THandler, TIn, TOut>>));

            var interceptors = interceptorEnumerable?.ToArray() ?? new IInterceptor<THandler, TIn, TOut>[0];

            if (!interceptors.Any())
            {
                return handler;
            }

            var intercepted = handler;

            for (var i = interceptors.Length - 1; i >= 0; i--)
            {
                intercepted = new InterceptedHandler<THandler, TIn, TOut>(intercepted, interceptors[i]);
            }

            return intercepted;
        }
    }
}
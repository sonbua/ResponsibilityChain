namespace ResponsibilityChain
{
    /// <summary>
    /// The interception strategy which just returns the original handler to the caller.
    /// </summary>
    public class NoopInterceptionStrategy : IInterceptionStrategy
    {
        /// <summary>
        /// Returns the original handler to the caller.
        /// </summary>
        /// <param name="handler">The handler object to be intercepted.</param>
        /// <typeparam name="THandler">The handler type.</typeparam>
        /// <typeparam name="TIn">The input type.</typeparam>
        /// <typeparam name="TOut">The output type.</typeparam>
        /// <returns></returns>
        public IHandler<TIn, TOut> Intercept<THandler, TIn, TOut>(THandler handler)
            where THandler : class, IHandler<TIn, TOut>
        {
            // do nothing
            return handler;
        }
    }
}
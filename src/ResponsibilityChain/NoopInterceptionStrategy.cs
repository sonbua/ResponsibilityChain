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
        /// <typeparam name="TIn">The type of the input.</typeparam>
        /// <typeparam name="TOut">The type of the output.</typeparam>
        /// <returns></returns>
        public IHandler<TIn, TOut> InterceptHandler<THandler, TIn, TOut>(THandler handler)
            where THandler : class, IHandler<TIn, TOut>
        {
            // do nothing
            return handler;
        }

        /// <summary>
        /// Returns the original handler to the caller.
        /// </summary>
        /// <param name="asyncHandler">The handler object to be intercepted.</param>
        /// <typeparam name="TAsyncHandler">The type of the handler.</typeparam>
        /// <typeparam name="TIn">The type of the input.</typeparam>
        /// <typeparam name="TOut">The type of the output.</typeparam>
        /// <returns></returns>
        public IAsyncHandler<TIn, TOut> InterceptAsyncHandler<TAsyncHandler, TIn, TOut>(TAsyncHandler asyncHandler)
            where TAsyncHandler : class, IAsyncHandler<TIn, TOut>
        {
            // do nothing
            return asyncHandler;
        }
    }
}
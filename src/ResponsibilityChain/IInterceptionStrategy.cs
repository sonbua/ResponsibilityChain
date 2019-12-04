namespace ResponsibilityChain
{
    /// <summary>
    /// Represents a common interface for all interception strategies, which are used to intercept a given handler.
    /// </summary>
    public interface IInterceptionStrategy
    {
        /// <summary>
        /// <para>Intercepts the given <paramref name="handler"/> object and returns an intercepted handler of type
        /// <see cref="IHandler{TIn,TOut}"/>.</para>
        /// 
        /// <para>Be noted that the input is a <typeparamref name="THandler"/>, yet the return object is an
        /// <see cref="IHandler{TIn,TOut}"/>. This is by design.</para>
        /// 
        /// <para>Most of the time, the <see cref="IHandler{TIn,TOut}.Handle"/> method of the <paramref name="handler"/>
        /// object is not virtual, i.e. not overrideable, so that creating a callback hook is not possible
        /// (think about aspect-oriented programming).</para>
        ///
        /// <para>To realize callback hooks, it needs to be intercepted by the interceptors and then cast the 
        /// intercepted handler to the generic interface version <see cref="IHandler{TIn,TOut}"/>.</para>
        /// </summary>
        /// <param name="handler">The handler object, which is going to be intercepted.</param>
        /// <typeparam name="THandler">The type of the handler.</typeparam>
        /// <typeparam name="TIn">The type of the input.</typeparam>
        /// <typeparam name="TOut">The type of the output.</typeparam>
        /// <returns></returns>
        IHandler<TIn, TOut> InterceptHandler<THandler, TIn, TOut>(THandler handler)
            where THandler : class, IHandler<TIn, TOut>;

        /// <summary>
        /// <para>Intercepts the given <paramref name="asyncHandler"/> object and returns an intercepted handler of type
        /// <see cref="IAsyncHandler{TIn,TOut}"/>.</para>
        /// 
        /// <para>Be noted that the input is a <typeparamref name="TAsyncHandler"/>, yet the return object is an
        /// <see cref="IAsyncHandler{TIn,TOut}"/>. This is by design.</para>
        /// 
        /// <para>Most of the time, the <see cref="IAsyncHandler{TIn,TOut}.HandleAsync"/> method of the
        /// <paramref name="asyncHandler"/> object is not virtual, i.e. not overrideable, so that creating a callback
        /// hook is not possible (think about aspect-oriented programming).</para>
        ///
        /// <para>To realize callback hooks, it needs to be intercepted by the interceptors and then cast the 
        /// intercepted handler to the generic interface version <see cref="IAsyncHandler{TIn,TOut}"/>.</para>
        /// </summary>
        /// <param name="asyncHandler">The handler object, which is going to be intercepted.</param>
        /// <typeparam name="TAsyncHandler">The type of the handler.</typeparam>
        /// <typeparam name="TIn">The type of the input.</typeparam>
        /// <typeparam name="TOut">The type of the output.</typeparam>
        /// <returns></returns>
        IAsyncHandler<TIn, TOut> InterceptAsyncHandler<TAsyncHandler, TIn, TOut>(TAsyncHandler asyncHandler)
            where TAsyncHandler : class, IAsyncHandler<TIn, TOut>;
    }
}
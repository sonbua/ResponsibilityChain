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
        /// <para>Be noted that the input is a <c>THandler</c>, yet the return object is
        /// an <see cref="IHandler{TIn,TOut}"/>. This is by design.</para>
        /// 
        /// <para>Most of the time, the <see cref="IHandler{TIn,TOut}.Handle"/> method of the <paramref name="handler"/>
        /// object is not virtual, i.e. not overrideable, so that creating a callback hook is not possible
        /// (think about aspect-oriented programming).</para>
        ///
        /// <para>To realize callback hooks, it needs to be intercepted by the interceptors and then cast the 
        /// intercepted handler to the generic interface version <see cref="IHandler{TIn,TOut}"/>.</para>
        /// </summary>
        /// <param name="handler">The handler object, which is going to be intercepted.</param>
        /// <typeparam name="THandler">The handler type.</typeparam>
        /// <typeparam name="TIn">The input type.</typeparam>
        /// <typeparam name="TOut">The output type.</typeparam>
        /// <returns></returns>
        IHandler<TIn, TOut> Intercept<THandler, TIn, TOut>(THandler handler)
            where THandler : class, IHandler<TIn, TOut>;
    }
}
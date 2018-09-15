using System;
using System.Threading.Tasks;

namespace ResponsibilityChain
{
    /// <summary>
    /// <para>Serves as a marker for all handlers (processing elements) in the chain, e.g. for dependency registrations, etc.</para>
    /// <para>For most cases, the generic version <see cref="IHandler{TIn,TOut}"/> should be the one to be implemented, and not this one.</para>
    /// </summary>
    public interface IHandler
    {
    }

    /// <summary>
    /// <para>Represents a handler (processing element) in the chain. This could be used in several fashion</para>
    /// <para>1. Synchronous input/output model.</para>
    /// <para>2. Asynchronous input/output model by setting the output type <typeparamref name="TOut"/> to be <see cref="Task"/> or <see cref="Task{TResult}"/>.</para>
    /// <para>3. Asynchronous, OWIN-like model by setting the input type <typeparamref name="TIn"/> to be a context (including input and output objects), and the output type <typeparamref name="TOut"/> to be a <see cref="Task"/>.</para>
    /// </summary>
    /// <typeparam name="TIn">The type of the input.</typeparam>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    public interface IHandler<TIn, TOut> : IHandler
    {
        /// <summary>
        /// Either processes the input then returns result to its caller or passes on the input to the next handler in the chain for further processing.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="next">The next handler in the chain.</param>
        /// <returns></returns>
        TOut Handle(TIn input, Func<TIn, TOut> next);
    }
}
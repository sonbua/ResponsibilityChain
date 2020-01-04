using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;

namespace ResponsibilityChain
{
    /// <summary>
    /// Represents an asynchronous composite handler, which comprises of multiple asynchronous handlers in order to handle a more complicate input.
    /// </summary>
    /// <typeparam name="TIn">The type of the input.</typeparam>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    public class AsyncHandler<TIn, TOut> : IAsyncHandler<TIn, TOut>
    {
        private readonly IInterceptionStrategy _interceptionStrategy;
        private readonly List<IAsyncHandler<TIn, TOut>> _handlers;
        private Func<Func<TIn, Task<TOut>>, Func<TIn, Task<TOut>>> _chainedDelegate;

        /// <summary>
        /// Constructs the asynchronous composite handler.
        /// </summary>
        /// <param name="interceptionStrategy">The interception strategy, which is used to intercept all child handlers. If it is null, the <see cref="InterceptionStrategy.Default"/> will be used.</param>
        protected AsyncHandler(IInterceptionStrategy interceptionStrategy = null)
        {
            _interceptionStrategy = interceptionStrategy ?? InterceptionStrategy.Default;
            _handlers = new List<IAsyncHandler<TIn, TOut>>();
        }

        private Func<Func<TIn, Task<TOut>>, Func<TIn, Task<TOut>>> ChainedDelegate
        {
            get
            {
                if (_chainedDelegate != null)
                {
                    return _chainedDelegate;
                }

                Func<Func<TIn, Task<TOut>>, Func<TIn, Task<TOut>>> chainedDelegate =
                    next => input => _handlers.Last().HandleAsync(input, next);

                for (var index = _handlers.Count - 2; index >= 0; index--)
                {
                    var handler = _handlers[index];
                    var chainedDelegateCloned = chainedDelegate;

                    chainedDelegate =
                        next => input => handler.HandleAsync(input, chainedDelegateCloned(next));
                }

                return _chainedDelegate = chainedDelegate;
            }
        }

        /// <summary>
        /// <para>Asynchronously invokes handlers one by one until the <paramref name="input"/> has been processed by a handler and returns output, ignoring
        /// the rest of the handlers.</para>
        /// <para>It is done by first creating a pipeline execution delegate from existing handlers then invoking that delegate against the input.</para>
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="next">The next handler in the chain.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown if none of the handlers is able to handle the <paramref name="input"/>.</exception>
        public virtual Task<TOut> HandleAsync(TIn input, Func<TIn, Task<TOut>> next)
        {
            if (next == null)
            {
                next = _ => throw new NotSupportedException(
                    $"Cannot handle this input. Input information: {typeof(TIn)}"
                );
            }

            if (!_handlers.Any())
            {
                return next(input);
            }

            return ChainedDelegate.Invoke(next).Invoke(input);
        }

        /// <summary>
        /// Asynchronously invokes handlers one by one until the <paramref name="input"/> has been processed by a handler and returns output, ignoring the rest
        /// of the handlers. If there is no handler, which can handle the <paramref name="input"/>, it will raise an exception of type
        /// <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown if none of the handlers is able to handle the <paramref name="input"/>.</exception>
        public virtual Task<TOut> HandleAsync(TIn input)
        {
            return HandleAsync(input, null);
        }

        /// <summary>
        /// <para>Performs interception to the given <paramref name="asyncHandler"/> object.</para>
        /// <para>Then adds the intercepted handler to the last position in the chain.</para>
        /// </summary>
        /// <param name="asyncHandler">The handler object.</param>
        /// <typeparam name="TAsyncHandler"></typeparam>
        protected void AddHandler<TAsyncHandler>(TAsyncHandler asyncHandler)
            where TAsyncHandler : class, IAsyncHandler<TIn, TOut>
        {
            EnsureArg.IsNotNull(asyncHandler, nameof(asyncHandler));

            var intercepted = InterceptAsyncHandler(asyncHandler);

            _handlers.Add(intercepted);
        }

        private IAsyncHandler<TIn, TOut> InterceptAsyncHandler<TAsyncHandler>(TAsyncHandler asyncHandler)
            where TAsyncHandler : class, IAsyncHandler<TIn, TOut>
        {
            return _interceptionStrategy.InterceptAsyncHandler<TAsyncHandler, TIn, TOut>(asyncHandler);
        }
    }
}
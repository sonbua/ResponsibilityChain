using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private Func<Func<TIn, CancellationToken, Task<TOut>>, Func<TIn, CancellationToken, Task<TOut>>> _chainedDelegate;

        /// <summary>
        /// Constructs the asynchronous composite handler.
        /// </summary>
        /// <param name="interceptionStrategy">The interception strategy, which is used to intercept all child handlers. If it is null, the <see cref="InterceptionStrategy.Default"/> will be used.</param>
        protected AsyncHandler(IInterceptionStrategy interceptionStrategy = null)
        {
            _interceptionStrategy = interceptionStrategy ?? InterceptionStrategy.Default;
            _handlers = new List<IAsyncHandler<TIn, TOut>>();
        }

        /// <summary>
        /// Builds a chained delegate from the list of handlers.
        /// </summary>
        private Func<Func<TIn, CancellationToken, Task<TOut>>, Func<TIn, CancellationToken, Task<TOut>>> ChainedDelegate
        {
            get
            {
                if (_chainedDelegate != null)
                {
                    return _chainedDelegate;
                }

                Func<Func<TIn, CancellationToken, Task<TOut>>, Func<TIn, CancellationToken, Task<TOut>>> chainedDelegate =
                    next => (input, token) => _handlers.Last().HandleAsync(input, next, token);

                for (var index = _handlers.Count - 2; index >= 0; index--)
                {
                    var handler = _handlers[index];
                    var chainedDelegateCloned = chainedDelegate;

                    chainedDelegate =
                        next => (input, token) => handler.HandleAsync(input, chainedDelegateCloned(next), token);
                }

                return _chainedDelegate = chainedDelegate;
            }
        }

        /// <summary>
        /// <para>Asynchronously invokes child handlers one by one until the <paramref name="input"/> has been processed by a child handler and returns output,
        /// ignoring the rest of the handlers.</para>
        /// <para>If none of the child handlers is able to handle the <paramref name="input"/>, it will be passed on to the <paramref name="next"/>
        /// delegate. And if <paramref name="next"/> is <c>null</c>, <see cref="NotSupportedException"/> will be thrown.</para>
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="next">The next handler in the chain.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public virtual Task<TOut> HandleAsync(TIn input, Func<TIn, CancellationToken, Task<TOut>> next, CancellationToken cancellationToken)
        {
            if (next == null)
            {
                next = (_, __) => throw new NotSupportedException(
                    $"Cannot handle this input. Input information: {typeof(TIn)}"
                );
            }

            if (_handlers.Count == 0)
            {
                return next(input, cancellationToken);
            }

            return ChainedDelegate.Invoke(next).Invoke(input, cancellationToken);
        }

        /// <summary>
        /// <para>Asynchronously invokes handlers one by one until the <paramref name="input"/> has been processed by a handler and returns output, ignoring
        /// the rest of the handlers.</para>
        /// <para>If none of the child handlers is able to handle the <paramref name="input"/>, <see cref="NotSupportedException"/> will be thrown.</para>
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <returns></returns>
        public virtual Task<TOut> HandleAsync(TIn input)
        {
            return HandleAsync(input, null, CancellationToken.None);
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
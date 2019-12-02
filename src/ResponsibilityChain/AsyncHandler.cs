﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;

namespace ResponsibilityChain
{
    /// <summary>
    /// Represents a composite handler, which comprises of multiple handlers in order to handle a more complicate input.
    /// </summary>
    /// <typeparam name="TIn">The type of the input.</typeparam>
    /// <typeparam name="TOut">The type of the output.</typeparam>
    public class AsyncHandler<TIn, TOut> : IAsyncHandler<TIn, TOut>
    {
        private readonly List<IAsyncHandler<TIn, TOut>> _handlers;
        private Func<Func<TIn, Task<TOut>>, Func<TIn, Task<TOut>>> _chainedDelegate;

        /// <summary>
        /// </summary>
        protected AsyncHandler()
        {
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
        /// <para>Asynchronously invokes handlers one by one until the input has been processed by a handler and returns output, ignoring the rest of the handlers.</para>
        /// <para>It is done by first creating a pipeline execution delegate from existing handlers then invoking that delegate against the input.</para>
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="next">The next handler in the chain.</param>
        /// <returns></returns>
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
        /// <para>Performs interception to the given <paramref name="handler"/> object.</para>
        /// <para>Then adds the intercepted handler to the last position in the chain.</para>
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="THandler"></typeparam>
        protected void AddHandler<THandler>(THandler handler)
            where THandler : class, IAsyncHandler<TIn, TOut>
        {
            EnsureArg.IsNotNull(handler, nameof(handler));

            _handlers.Add(handler);
        }
    }
}
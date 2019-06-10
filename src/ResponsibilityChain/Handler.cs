﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnsureThat;

namespace ResponsibilityChain
{
    /// <summary>
    /// <para>Represents a composite handler, which comprises of multiple handlers in order to handle a more complicate input.</para>
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    public abstract class Handler<TIn, TOut> : IHandler<TIn, TOut>
    {
        private readonly List<IHandler<TIn, TOut>> _handlers;
        private Func<Func<TIn, TOut>, Func<TIn, TOut>> _chainedDelegate;

        /// <summary>
        /// </summary>
        protected Handler()
        {
            _handlers = new List<IHandler<TIn, TOut>>();
        }

        /// <summary>
        /// Builds a chained delegate from the list of handlers.
        /// </summary>
        private Func<Func<TIn, TOut>, Func<TIn, TOut>> ChainedDelegate
        {
            get
            {
                if (_chainedDelegate != null)
                {
                    return _chainedDelegate;
                }

                Func<Func<TIn, TOut>, Func<TIn, TOut>> chainedDelegate =
                    next => input => _handlers.Last().Handle(input, next);

                for (var index = _handlers.Count - 2; index >= 0; index--)
                {
                    var handler = _handlers[index];
                    var chainedDelegateCloned = chainedDelegate;

                    chainedDelegate = next => input => handler.Handle(input, chainedDelegateCloned(next));
                }

                return _chainedDelegate = chainedDelegate;
            }
        }

        /// <summary>
        /// <para>Invokes handlers one by one until the input has been processed by a handler and returns output, ignoring the rest of the handlers.</para>
        /// <para>It is done by first creating a pipeline execution delegate from existing handlers then invoking that delegate against the input.</para>
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="next">The next handler in the chain. If null is provided, <see cref="ThrowNotSupported{TIn,TOut}"/> will be set as the end of the chain.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if handler list is null.</exception>
        /// <exception cref="ArgumentException">Thrown if handler list is empty.</exception>
        public virtual TOut Handle(TIn input, Func<TIn, TOut> next)
        {
            EnsureArg.HasItems(_handlers, nameof(_handlers));

            if (next == null)
            {
                next = _ => new ThrowNotSupported<TIn, TOut>().Handle(_, null);
            }

            return ChainedDelegate.Invoke(next).Invoke(input);
        }

        /// <summary>
        /// <para>Performs interception to the given <paramref name="handler"/> object.</para>
        /// <para>Then adds the intercepted handler to the last position in the chain.</para>
        /// </summary>
        /// <param name="handler">The handler object.</param>
        protected void AddHandler<THandler>(THandler handler)
            where THandler : class, IHandler<TIn, TOut>
        {
            EnsureArg.IsNotNull(handler, nameof(handler));

            var intercepted = InterceptHandler(handler);

            _handlers.Add(intercepted);
        }

        private static IHandler<TIn, TOut> InterceptHandler<THandler>(THandler handler)
            where THandler : class, IHandler<TIn, TOut>
        {
            return InterceptionStrategy.Current.Intercept<THandler, TIn, TOut>(handler);
        }

        /// <summary>
        /// Returns a hierarchical view of the chain's structure.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var node in AsNodes())
            {
                builder.AppendLine(node.ToString());
            }

            return builder.ToString().TrimEnd();
        }

        /// <summary>
        /// Returns current handler as a node along with its child handlers. 
        /// </summary>
        /// <param name="indentLevel">The indent level of this handler.</param>
        /// <returns></returns>
        private IEnumerable<Node> AsNodes(int indentLevel = 0)
        {
            yield return new Node(GetType().ToString(), indentLevel);

            var nextIndentLevel = indentLevel + 1;

            foreach (var handler in _handlers)
            {
                if (handler is Handler<TIn, TOut> compositeHandler)
                {
                    foreach (var node in compositeHandler.AsNodes(nextIndentLevel))
                    {
                        yield return node;
                    }
                }
                else
                {
                    yield return new Node(handler.ToString(), nextIndentLevel);
                }
            }
        }

        /// <summary>
        /// Represent a node in the tree.
        /// </summary>
        private struct Node
        {
            private readonly string _name;
            private readonly int _indentLevel;

            public Node(string name, int indentLevel)
            {
                _name = name;
                _indentLevel = indentLevel;
            }

            public override string ToString()
            {
                var builder = new StringBuilder();

                for (var i = 0; i < _indentLevel; i++)
                {
                    builder.Append("  ");
                }

                builder.Append(_name);

                return builder.ToString();
            }
        }
    }
}
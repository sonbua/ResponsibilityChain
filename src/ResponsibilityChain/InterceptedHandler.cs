using System;

namespace ResponsibilityChain
{
    internal class InterceptedHandler<THandler, TIn, TOut> : IHandler<TIn, TOut>
        where THandler : IHandler<TIn, TOut>
    {
        private readonly IHandler<TIn, TOut> _handler;
        private readonly IInterceptor<THandler, TIn, TOut> _interceptor;

        public InterceptedHandler(IHandler<TIn, TOut> handler, IInterceptor<THandler, TIn, TOut> interceptor)
        {
            _handler = handler;
            _interceptor = interceptor;
        }

        public TOut Handle(TIn input, Func<TIn, TOut> next) => _interceptor.Intercept(_handler, input, next);

        public override string ToString() => $"{_handler} # intercepted by {_interceptor.GetType().Name}";
    }
}
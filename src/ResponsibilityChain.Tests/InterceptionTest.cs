using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class InterceptionTest
    {
        private class MockServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                if (typeof(IEnumerable<IInterceptor<CoreBusinessHandler, int, string>>).IsAssignableFrom(serviceType))
                {
                    return new IInterceptor<CoreBusinessHandler, int, string>[]
                    {
                        new StopwatchInterceptor<CoreBusinessHandler, int, string>(),
                        new DebugInterceptor<CoreBusinessHandler, int, string>(),
                        new FeatureOptedOutInterceptor<CoreBusinessHandler, int, string>()
                    };
                }

                if (serviceType.IsAbstract || serviceType.IsInterface)
                {
                    return null;
                }

                return Activator.CreateInstance(serviceType);
            }
        }

        private class StopwatchInterceptor<THandler, TIn, TOut> : IInterceptor<THandler, TIn, TOut>
            where THandler : IHandler<TIn, TOut>
        {
            public StopwatchInterceptor()
            {
                LogMessages = new List<string>();
            }

            internal static List<string> LogMessages { get; set; }

            public TOut Intercept(IHandler<TIn, TOut> handler, TIn input, Func<TIn, TOut> next)
            {
                var stopwatch = Stopwatch.StartNew();

                LogMessages.Add($"DEBUG {typeof(THandler)} started at {DateTime.Now.ToLongTimeString()}");
                stopwatch.Start();

                var output = handler.Handle(input, next);

                stopwatch.Stop();
                LogMessages.Add($"DEBUG {typeof(THandler)} completed at {DateTime.Now.ToLongTimeString()}");
                LogMessages.Add($"DEBUG {typeof(THandler)} elapsed {stopwatch.ElapsedMilliseconds} ms");

                return output;
            }
        }

        private class DebugInterceptor<THandler, TIn, TOut> : IInterceptor<THandler, TIn, TOut>
            where THandler : IHandler<TIn, TOut>
        {
            public DebugInterceptor()
            {
                LogMessages = new List<string>();
            }

            internal static List<string> LogMessages { get; private set; }

            public TOut Intercept(IHandler<TIn, TOut> handler, TIn input, Func<TIn, TOut> next)
            {
                LogMessages.Add($"DEBUG {typeof(THandler)} input = {input}");
                TOut output;

                try
                {
                    output = handler.Handle(input, next);
                }
                catch (Exception exception)
                {
                    LogMessages.Add($"error: {exception.Message}");
                    throw;
                }

                LogMessages.Add($"DEBUG {typeof(THandler)} > {output}");

                return output;
            }
        }

        private class FeatureOptedOutInterceptor<THandler, TIn, TOut> : IInterceptor<THandler, TIn, TOut>
            where THandler : IHandler<TIn, TOut>
        {
            public TOut Intercept(IHandler<TIn, TOut> handler, TIn input, Func<TIn, TOut> next)
            {
                // user chooses to ignore this feature
                return next(input);
            }
        }

        private class CoreBusinessHandler : IHandler<int, string>
        {
            public string Handle(int input, Func<int, string> next)
            {
                Thread.Sleep(100);

                return "business handled";
            }
        }

        private class FallbackHandler : IHandler<int, string>
        {
            public string Handle(int input, Func<int, string> next)
            {
                return "unhandled";
            }
        }

        private class CompositeHandler : Handler<int, string>
        {
            public CompositeHandler(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
                AddHandler<CoreBusinessHandler>();
                AddHandler<FallbackHandler>();
            }
        }

        [Fact]
        public void GivenBusinessHandlerWasInvoked_InterceptorIsInvokedToo()
        {
            // arrange
            var serviceProvider = new MockServiceProvider();
            var handler = new CompositeHandler(serviceProvider);

            // act
            handler.Handle(111, null);

            // assert
            Assert.NotEmpty(StopwatchInterceptor<CoreBusinessHandler, int, string>.LogMessages);
            Assert.NotEmpty(DebugInterceptor<CoreBusinessHandler, int, string>.LogMessages);
        }
    }
}
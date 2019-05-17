using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using Xunit;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace ResponsibilityChain.Interception.Tests
{
    public class InterceptionTest
    {
        public class given_an_interception_strategy : InterceptionTest, IDisposable
        {
            public given_an_interception_strategy()
            {
                InterceptionStrategy.SetStrategy(new FakeInterceptionStrategy(new MockServiceProvider()));
            }

            private class MockServiceProvider : IServiceProvider
            {
                public object GetService(Type serviceType)
                {
                    if (typeof(IEnumerable<IInterceptor<CoreBusinessHandler, int, string>>).IsAssignableFrom(
                        serviceType
                    ))
                    {
                        return new IInterceptor<CoreBusinessHandler, int, string>[]
                        {
                            new FeatureOptedOutInterceptor<CoreBusinessHandler, int, string>(),
                            new StopwatchInterceptor(),
                            new DebugInterceptor()
                        };
                    }

                    return ActivatorServiceProvider.Instance.GetService(serviceType);
                }
            }

            [Fact]
            public void then_invokes_eligible_interceptors_before_invoking_the_original_handler()
            {
                // arrange
                StopwatchInterceptor.LogMessages?.Clear();
                DebugInterceptor.LogMessages?.Clear();
                var handler = new CompositeHandler(new CoreBusinessHandler(), new FallbackHandler());

                // act
                var result = handler.Handle(111, null);

                // assert
                StopwatchInterceptor.LogMessages.Should().NotBeNullOrEmpty();
                DebugInterceptor.LogMessages.Should().NotBeNullOrEmpty();
                result.Should().Be("unhandled");
            }

            public void Dispose()
            {
                InterceptionStrategy.SetStrategy(new NoopInterceptionStrategy());
            }

            private class StopwatchInterceptor : IInterceptor<CoreBusinessHandler, int, string>
            {
                static StopwatchInterceptor()
                {
                    LogMessages = new List<string>();
                }

                public static List<string> LogMessages { get; }

                public string Intercept(IHandler<int, string> handler, int input, Func<int, string> next)
                {
                    var stopwatch = Stopwatch.StartNew();

                    LogMessages.Add(
                        $"DEBUG {typeof(CoreBusinessHandler)} started at {DateTime.Now.ToLongTimeString()}"
                    );
                    stopwatch.Start();

                    var output = handler.Handle(input, next);

                    stopwatch.Stop();
                    LogMessages.Add(
                        $"DEBUG {typeof(CoreBusinessHandler)} completed at {DateTime.Now.ToLongTimeString()}"
                    );
                    LogMessages.Add($"DEBUG {typeof(CoreBusinessHandler)} elapsed {stopwatch.ElapsedMilliseconds} ms");

                    return output;
                }
            }

            private class DebugInterceptor : IInterceptor<CoreBusinessHandler, int, string>
            {
                static DebugInterceptor()
                {
                    LogMessages = new List<string>();
                }

                public static List<string> LogMessages { get; }

                public string Intercept(IHandler<int, string> handler, int input, Func<int, string> next)
                {
                    LogMessages.Add($"DEBUG {typeof(CoreBusinessHandler)} input = {input}");
                    string output;

                    try
                    {
                        output = handler.Handle(input, next);
                    }
                    catch (Exception exception)
                    {
                        LogMessages.Add($"error: {exception.Message}");
                        throw;
                    }

                    LogMessages.Add($"DEBUG {typeof(CoreBusinessHandler)} > {output}");

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
        }

        public class given_no_configuration_to_the_framework : InterceptionTest
        {
            [Fact]
            public void then_ignores_all_interceptors()
            {
                // arrange
                StopwatchInterceptor.LogMessages?.Clear();
                DebugInterceptor.LogMessages?.Clear();
                var handler = new CompositeHandler(new CoreBusinessHandler(), new FallbackHandler());

                // act
                var result = handler.Handle(112, null);

                // assert
                StopwatchInterceptor.LogMessages.Should().BeNullOrEmpty();
                DebugInterceptor.LogMessages.Should().BeNullOrEmpty();
                result.Should().Be("business handled");
            }
        }

        private class FakeInterceptionStrategy : IInterceptionStrategy
        {
            private readonly IServiceProvider _serviceProvider;

            public FakeInterceptionStrategy(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public IHandler<TIn, TOut> Intercept<THandler, TIn, TOut>(THandler handler)
                where THandler : class, IHandler<TIn, TOut>
            {
                var interceptors =
                    (IEnumerable<IInterceptor<THandler, TIn, TOut>>) _serviceProvider.GetService(
                        typeof(IEnumerable<IInterceptor<THandler, TIn, TOut>>)
                    );

                return DefaultInterceptionStrategyHelper.Intercept(handler, interceptors);
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
            public CompositeHandler(CoreBusinessHandler coreBusinessHandler, FallbackHandler fallbackHandler)
            {
                AddHandler(coreBusinessHandler);
                AddHandler(fallbackHandler);
            }
        }
    }
}
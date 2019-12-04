using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ResponsibilityChain.Interception.Tests
{
    public class InterceptionTest
    {
        public class given_an_interception_strategy : InterceptionTest
        {
            private readonly FakeInterceptionStrategy _fakeInterceptionStrategy;

            public given_an_interception_strategy()
            {
                _fakeInterceptionStrategy = new FakeInterceptionStrategy(new MockServiceProvider());
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
                var handler = new CompositeHandler(
                    new CoreBusinessHandler(),
                    new FallbackHandler(),
                    _fakeInterceptionStrategy
                );

                // act
                var result = handler.Handle(111, null);

                // assert
                StopwatchInterceptor.LogMessages.Should().NotBeNullOrEmpty();
                DebugInterceptor.LogMessages.Should().NotBeNullOrEmpty();
                result.Should().Be("unhandled");
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

        public class given_an_interception_strategy_to_measure_execution_time : InterceptionTest
        {
            private readonly FakeInterceptionStrategy _interceptionStrategy;

            public given_an_interception_strategy_to_measure_execution_time(ITestOutputHelper testOutputHelper)
            {
                _interceptionStrategy = new FakeInterceptionStrategy(new MockServiceProvider(testOutputHelper));
            }

            private class MockServiceProvider : IServiceProvider
            {
                private readonly ITestOutputHelper _testOutputHelper;

                public MockServiceProvider(ITestOutputHelper testOutputHelper)
                {
                    _testOutputHelper = testOutputHelper;
                }

                public object GetService(Type serviceType)
                {
                    if (typeof(IEnumerable<IInterceptor<CoreBusinessHandler, int, string>>).IsAssignableFrom(
                        serviceType
                    ))
                    {
                        return new IInterceptor<CoreBusinessHandler, int, string>[]
                        {
                            new StopwatchInterceptor(_testOutputHelper)
                        };
                    }

                    return ActivatorServiceProvider.Instance.GetService(serviceType);
                }
            }

            [Fact]
            public void then_prints_out_execution_time_of_each_handler()
            {
                // arrange
                var handler = new CompositeHandler(
                    new CoreBusinessHandler(),
                    new FallbackHandler(),
                    _interceptionStrategy
                );

                // act
                var result = handler.Handle(111, null);

                // assert
                result.Should().Be("business handled");
            }

            private class StopwatchInterceptor : IInterceptor<CoreBusinessHandler, int, string>
            {
                private readonly ITestOutputHelper _testOutputHelper;

                public StopwatchInterceptor(ITestOutputHelper testOutputHelper)
                {
                    _testOutputHelper = testOutputHelper;
                }

                public string Intercept(IHandler<int, string> handler, int input, Func<int, string> next)
                {
                    var stopwatch = new Stopwatch();

                    Func<int, string> interceptedNext = i =>
                    {
                        stopwatch.Stop();

                        _testOutputHelper.WriteLine(stopwatch.ElapsedMilliseconds.ToString() + " ms");

                        stopwatch.Start();

                        return next(i);
                    };

                    stopwatch.Start();

                    var result = handler.Handle(input, interceptedNext);

                    if (stopwatch.IsRunning)
                    {
                        stopwatch.Stop();

                        _testOutputHelper.WriteLine(stopwatch.ElapsedMilliseconds + " ms");
                    }

                    return result;
                }
            }
        }

        public class given_no_configuration_to_the_framework : InterceptionTest
        {
            [Fact]
            public void then_ignores_all_interceptors()
            {
                // arrange
                var handler = new CompositeHandler(
                    new CoreBusinessHandler(),
                    new FallbackHandler(),
                    InterceptionStrategy.Default
                );

                // act
                var result = handler.Handle(112, null);

                // assert
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

            public IHandler<TIn, TOut> InterceptHandler<THandler, TIn, TOut>(THandler handler)
                where THandler : class, IHandler<TIn, TOut>
            {
                var interceptors =
                    (IEnumerable<IInterceptor<THandler, TIn, TOut>>) _serviceProvider.GetService(
                        typeof(IEnumerable<IInterceptor<THandler, TIn, TOut>>)
                    );

                return DefaultInterceptionStrategyHelper.Intercept(handler, interceptors);
            }

            public IAsyncHandler<TIn, TOut> InterceptAsyncHandler<TAsyncHandler, TIn, TOut>(TAsyncHandler asyncHandler)
                where TAsyncHandler : class, IAsyncHandler<TIn, TOut>
            {
                throw new NotSupportedException();
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
            public CompositeHandler(
                CoreBusinessHandler coreBusinessHandler,
                FallbackHandler fallbackHandler,
                IInterceptionStrategy interceptionStrategy)
                : base(interceptionStrategy)
            {
                AddHandler(coreBusinessHandler);
                AddHandler(fallbackHandler);
            }
        }

        /// <summary>
        /// Represents a singleton service provider which uses <see cref="Activator"/> to create instance of given service type.
        /// </summary>
        private class ActivatorServiceProvider : IServiceProvider
        {
            static ActivatorServiceProvider()
            {
            }

            private ActivatorServiceProvider()
            {
            }

            /// <summary>
            /// Singleton instance of this service provider.
            /// </summary>
            public static ActivatorServiceProvider Instance { get; } = new ActivatorServiceProvider();

            /// <summary>
            /// Uses <see cref="Activator"/> to create instance of the given <paramref name="serviceType"/>.
            /// </summary>
            /// <param name="serviceType"></param>
            /// <returns></returns>
            public object GetService(Type serviceType)
            {
                if (serviceType.IsAbstract)
                {
                    return null;
                }

                return Activator.CreateInstance(serviceType);
            }
        }
    }
}
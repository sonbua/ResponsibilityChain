using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class AsyncHandlerTest
    {
        public class given_a_composite_handler_with_no_child : AsyncHandlerTest
        {
            private readonly CompositeHandlerWithNoChild _handler;

            public given_a_composite_handler_with_no_child()
            {
                _handler = new CompositeHandlerWithNoChild();
            }

            [Fact]
            public async Task then_passes_through_to_the_next_handler()
            {
                // act
                var result = await _handler.HandleAsync(string.Empty, x => Task.FromResult(0));

                // assert
                result.Should().Be(0);
            }

            private class CompositeHandlerWithNoChild : AsyncHandler<string, int>
            {
            }
        }

        public class given_a_composite_coin_detector : AsyncHandlerTest
        {
            private readonly IAsyncHandler<string, int> _handler;

            public given_a_composite_coin_detector()
            {
                _handler = new CoinDetector(
                    new OnePennyHandler(),
                    new TwoPennyHandler(),
                    new FivePennyHandler()
                );
            }

            [Theory]
            [InlineData("1", 1)]
            [InlineData("2", 2)]
            [InlineData("5", 5)]
            public async Task then_is_able_to_detect_a_single_coin(string coin, int expected)
            {
                // arrange

                // act
                var actual = await _handler.HandleAsync(coin, null);

                // assert
                actual.Should().Be(expected);
            }

            [Fact]
            public void then_throws_exception_on_unknown_coin()
            {
                // arrange
                const string coin = "3";

                // act
                Func<Task> action = async () => await _handler.HandleAsync(coin, null);

                // assert
                action.Should().Throw<NotSupportedException>();
            }

            [Theory]
            [InlineData("1 2 5", 8)]
            public async Task then_is_able_to_detect_multiple_coins_at_once(string coins, int expected)
            {
                // arrange

                // act
                var actual = await _handler.HandleAsync(coins, null);

                // assert
                actual.Should().Be(expected);
            }

            private class CoinDetector : AsyncHandler<string, int>
            {
                public CoinDetector(
                    OnePennyHandler onePennyHandler,
                    TwoPennyHandler twoPennyHandler,
                    FivePennyHandler fivePennyHandler)
                {
                    AddHandler(onePennyHandler);
                    AddHandler(twoPennyHandler);
                    AddHandler(fivePennyHandler);
                }

                public override async Task<int> HandleAsync(string input, Func<string, Task<int>> next)
                {
                    var coinStrings = input.Split(' ');
                    var detectionTasks = coinStrings.Select(coin => base.HandleAsync(coin, next)).ToList();
                    var coins = await Task.WhenAll(detectionTasks);

                    return coins.Sum();
                }
            }

            private class OnePennyHandler : IAsyncHandler<string, int>
            {
                public async Task<int> HandleAsync(string input, Func<string, Task<int>> next)
                {
                    if (input != "1")
                    {
                        return await next(input);
                    }

                    await Task.Delay(100);

                    return 1;
                }
            }

            private class TwoPennyHandler : IAsyncHandler<string, int>
            {
                public async Task<int> HandleAsync(string input, Func<string, Task<int>> next)
                {
                    if (input != "2")
                    {
                        return await next(input);
                    }

                    await Task.Delay(200);

                    return 2;
                }
            }

            private class FivePennyHandler : IAsyncHandler<string, int>
            {
                public async Task<int> HandleAsync(string input, Func<string, Task<int>> next)
                {
                    if (input != "5")
                    {
                        return await next(input);
                    }

                    await Task.Delay(500);

                    return 5;
                }
            }
        }

        public class given_a_custom_interception_strategy : AsyncHandlerTest
        {
            private readonly SuppressExceptionInterceptionStrategy _suppressExceptionInterceptionStrategy;

            protected given_a_custom_interception_strategy()
            {
                _suppressExceptionInterceptionStrategy = new SuppressExceptionInterceptionStrategy();
            }

            public class and_a_composite_handler : given_a_custom_interception_strategy
            {
                private readonly CompositeHandlerThatThrows _handlerThatThrows;

                public and_a_composite_handler()
                {
                    _handlerThatThrows = new CompositeHandlerThatThrows(
                        new ThrowNotSupported<string, int>(),
                        _suppressExceptionInterceptionStrategy
                    );
                }

                [Fact]
                public async Task then_composite_handler_is_intercepted()
                {
                    // act
                    var result = await _handlerThatThrows.HandleAsync("any", null);

                    // assert
                    result.Should().Be(default);
                }
            }

            private class SuppressExceptionInterceptionStrategy : IInterceptionStrategy
            {
                public IHandler<TIn, TOut> InterceptHandler<THandler, TIn, TOut>(THandler handler)
                    where THandler : class, IHandler<TIn, TOut>
                {
                    throw new NotSupportedException();
                }

                public IAsyncHandler<TIn, TOut> InterceptAsyncHandler<TAsyncHandler, TIn, TOut>(
                    TAsyncHandler asyncHandler)
                    where TAsyncHandler : class, IAsyncHandler<TIn, TOut>
                {
                    return new SuppressExceptionAsyncHandler<TAsyncHandler, TIn, TOut>(asyncHandler);
                }

                private class SuppressExceptionAsyncHandler<TAsyncHandler, TIn, TOut> : IAsyncHandler<TIn, TOut>
                    where TAsyncHandler : IAsyncHandler<TIn, TOut>
                {
                    private readonly TAsyncHandler _asyncHandler;

                    public SuppressExceptionAsyncHandler(TAsyncHandler asyncHandler)
                    {
                        _asyncHandler = asyncHandler;
                    }

                    public async Task<TOut> HandleAsync(TIn input, Func<TIn, Task<TOut>> next)
                    {
                        try
                        {
                            return await _asyncHandler.HandleAsync(input, next);
                        }
                        catch
                        {
                            return default;
                        }
                    }
                }
            }

            private class CompositeHandlerThatThrows : AsyncHandler<string, int>
            {
                public CompositeHandlerThatThrows(
                    ThrowNotSupported<string, int> child,
                    IInterceptionStrategy interceptionStrategy)
                    : base(interceptionStrategy)
                {
                    AddHandler(child);
                }
            }
        }
    }
}
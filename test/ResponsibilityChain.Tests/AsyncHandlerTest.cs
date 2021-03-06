﻿using System;
using System.Linq;
using System.Threading;
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

            public class when_invoking_with_a_next_delegate : given_a_composite_handler_with_no_child
            {
                private readonly int _result;

                public when_invoking_with_a_next_delegate()
                {
                    _result = _handler.HandleAsync(string.Empty, (_, __) => Task.FromResult(0), CancellationToken.None).GetAwaiter().GetResult();
                }

                [Fact]
                public void then_passes_through_to_the_next_handler()
                {
                    // arrange

                    // act

                    // assert
                    _result.Should().Be(0);
                }
            }

            public class when_invoking_without_next_delegate_argument : given_a_composite_handler_with_no_child
            {
                private readonly Func<Task> _action;

                public when_invoking_without_next_delegate_argument()
                {
                    _action = () => _handler.HandleAsync(string.Empty);
                }

                [Fact]
                public async Task then_throws_NotSupportedException()
                {
                    // arrange

                    // act

                    // assert
                    await _action.Should().ThrowAsync<NotSupportedException>().ConfigureAwait(false);
                }
            }

            private class CompositeHandlerWithNoChild : AsyncHandler<string, int>
            {
            }
        }

        public class given_a_composite_coin_detector : AsyncHandlerTest
        {
            private readonly AsyncHandler<string, int> _handler;

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
                var actual = await _handler.HandleAsync(coin).ConfigureAwait(false);

                // assert
                actual.Should().Be(expected);
            }

            [Fact]
            public void then_throws_exception_on_unknown_coin()
            {
                // arrange
                const string coin = "3";

                // act
                Func<Task> action = async () => await _handler.HandleAsync(coin).ConfigureAwait(false);

                // assert
                action.Should().Throw<NotSupportedException>();
            }

            [Theory]
            [InlineData("1 2 5", 8)]
            public async Task then_is_able_to_detect_multiple_coins_at_once(string coins, int expected)
            {
                // arrange

                // act
                var actual = await _handler.HandleAsync(coins).ConfigureAwait(false);

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

                public override async Task<int> HandleAsync(string input, Func<string, CancellationToken, Task<int>> next, CancellationToken cancellationToken)
                {
                    var coinStrings = input.Split(' ');
                    var detectionTasks = coinStrings.Select(coin => base.HandleAsync(coin, next, cancellationToken)).ToList();
                    var coins = await Task.WhenAll(detectionTasks).ConfigureAwait(false);

                    return coins.Sum();
                }
            }

            private class OnePennyHandler : IAsyncHandler<string, int>
            {
                public async Task<int> HandleAsync(string input, Func<string, CancellationToken, Task<int>> next, CancellationToken cancellationToken)
                {
                    if (input != "1")
                    {
                        return await next(input, cancellationToken).ConfigureAwait(false);
                    }

                    await Task.Delay(100, cancellationToken).ConfigureAwait(false);

                    return 1;
                }
            }

            private class TwoPennyHandler : IAsyncHandler<string, int>
            {
                public async Task<int> HandleAsync(string input, Func<string, CancellationToken, Task<int>> next, CancellationToken cancellationToken)
                {
                    if (input != "2")
                    {
                        return await next(input, cancellationToken).ConfigureAwait(false);
                    }

                    await Task.Delay(200, cancellationToken).ConfigureAwait(false);

                    return 2;
                }
            }

            private class FivePennyHandler : IAsyncHandler<string, int>
            {
                public async Task<int> HandleAsync(string input, Func<string, CancellationToken, Task<int>> next, CancellationToken cancellationToken)
                {
                    if (input != "5")
                    {
                        return await next(input, cancellationToken).ConfigureAwait(false);
                    }

                    await Task.Delay(500, cancellationToken).ConfigureAwait(false);

                    return 5;
                }
            }
        }

        public class given_a_composite_handler_which_take_3_seconds_to_complete : AsyncHandlerTest
        {
            private readonly SlowHandler _handler;

            public given_a_composite_handler_which_take_3_seconds_to_complete()
            {
                _handler = new SlowHandler(new SlowHandlerImpl());
            }

            public class when_starting_operation : given_a_composite_handler_which_take_3_seconds_to_complete, IDisposable
            {
                private readonly Func<Task> _testDelegate;
                private readonly CancellationTokenSource _cts;

                public when_starting_operation()
                {
                    _cts = new CancellationTokenSource();

                    _testDelegate = async () => await _handler.HandleAsync("any", null, _cts.Token).ConfigureAwait(false);
                }

                [Fact]
                public async Task then_it_can_be_cancelled()
                {
                    // arrange
                    _cts.CancelAfter(TimeSpan.FromMilliseconds(500));

                    // act

                    // assert
                    await _testDelegate.Should().ThrowAsync<TaskCanceledException>().ConfigureAwait(false);
                }

                public void Dispose()
                {
                    _cts?.Dispose();
                }
            }

            private class SlowHandler : AsyncHandler<string, int>
            {
                public SlowHandler(SlowHandlerImpl slowHandlerImpl)
                {
                    AddHandler(slowHandlerImpl);
                }
            }

            private class SlowHandlerImpl : IAsyncHandler<string, int>
            {
                public async Task<int> HandleAsync(string input, Func<string, CancellationToken, Task<int>> next, CancellationToken cancellationToken)
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken).ConfigureAwait(false);

                    return int.MaxValue;
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
                    var result = await _handlerThatThrows.HandleAsync("any").ConfigureAwait(false);

                    // assert
                    result.Should().Be(default(int));
                }
            }

            private class SuppressExceptionInterceptionStrategy : IInterceptionStrategy
            {
                public IHandler<TIn, TOut> InterceptHandler<THandler, TIn, TOut>(THandler handler)
                    where THandler : class, IHandler<TIn, TOut>
                {
                    throw new NotSupportedException();
                }

                public IAsyncHandler<TIn, TOut> InterceptAsyncHandler<TAsyncHandler, TIn, TOut>(TAsyncHandler asyncHandler)
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

                    public async Task<TOut> HandleAsync(TIn input, Func<TIn, CancellationToken, Task<TOut>> next, CancellationToken cancellationToken)
                    {
                        try
                        {
                            return await _asyncHandler.HandleAsync(input, next, cancellationToken).ConfigureAwait(false);
                        }
                        catch
                        {
                            return default(TOut);
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
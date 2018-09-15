using System;
using System.Threading.Tasks;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class ReturnCompletedTaskFromDefaultValueHandlerTest
    {
        private class CompositeFooAsyncHandler : Handler<int, Task<string>>
        {
            public CompositeFooAsyncHandler()
                : base(ActivatorServiceProvider.Instance)
            {
                AddHandler(new BarHandler());
                AddHandler(ReturnCompletedTaskFromDefaultValueHandler<int, string>.Instance);
                AddHandler(ThrowNotSupportedHandler<int, Task<string>>.Instance);
            }

            private class BarHandler : IHandler<int, Task<string>>
            {
                public async Task<string> Handle(int input, Func<int, Task<string>> next)
                {
                    await Task.Delay(100);

                    return await next(input);
                }
            }
        }

        [Fact]
        public async Task
            GivenThisShortCircuitHandlerPlacedInTheMiddleOfTheChain_ReturnsCompletedTaskWithDefaultValueWithoutInvokingNextHandler()
        {
            // arrange
            var handler = new CompositeFooAsyncHandler();

            // act
            var result = await handler.Handle(111, null);

            // assert
            Assert.Null(result);
        }
    }
}
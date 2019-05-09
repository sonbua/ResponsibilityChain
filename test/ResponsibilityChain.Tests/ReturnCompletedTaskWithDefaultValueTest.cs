using System;
using System.Threading.Tasks;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class ReturnCompletedTaskWithDefaultValueTest
    {
        private class CompositeFooAsyncHandler : Handler<int, Task<string>>
        {
            public CompositeFooAsyncHandler(
                BarHandler barHandler,
                ReturnCompletedTaskWithDefaultValue<int, string> returnCompletedTaskWithDefaultValue)
            {
                AddHandler(barHandler);
                AddHandler(returnCompletedTaskWithDefaultValue);
                AddHandler(new ThrowNotSupported<int, Task<string>>());
            }
        }

        private class BarHandler : IHandler<int, Task<string>>
        {
            public async Task<string> Handle(int input, Func<int, Task<string>> next)
            {
                await Task.Delay(100);

                return await next(input);
            }
        }

        [Fact]
        public async Task
            GivenThisShortCircuitHandlerPlacedInTheMiddleOfTheChain_ReturnsCompletedTaskWithDefaultValueWithoutInvokingNextHandler()
        {
            // arrange
            var handler = new CompositeFooAsyncHandler(
                new BarHandler(),
                new ReturnCompletedTaskWithDefaultValue<int, string>()
            );

            // act
            var result = await handler.Handle(111, null);

            // assert
            Assert.Null(result);
        }
    }
}
using System;
using System.Threading.Tasks;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class ReturnCompletedTaskHandlerTest
    {
        private class CompositeFooAsyncHandler : Handler<int, Task>
        {
            public CompositeFooAsyncHandler(BarHandler barHandler)
            {
                AddHandler(barHandler);
                AddHandler(new ReturnCompletedTaskHandler<int>());
                AddHandler(new ThrowNotSupportedHandler<int, Task>());
            }
        }

        [Fact]
        public async Task
            GivenThisShortCircuitHandlerPlacedInTheMiddleOfTheChain_ReturnsCompletedTaskWithoutThrowingException()
        {
            // arrange
            var handler = new CompositeFooAsyncHandler(new BarHandler());

            // act
            await handler.Handle(111, null);

            // assert
        }

        private class BarHandler : IHandler<int, Task>
        {
            public async Task Handle(int input, Func<int, Task> next)
            {
                await Task.Delay(100);

                await next(input);
            }
        }
    }
}
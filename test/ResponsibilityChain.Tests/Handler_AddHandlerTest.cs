using System;
using Xunit;

namespace ResponsibilityChain.Tests
{
    // ReSharper disable once InconsistentNaming
    public class Handler_AddHandlerTest
    {
        [Fact]
        public void AddNullHandlerToTheChain_ThrowsException()
        {
            // arrange

            // act
            Action action = () =>
            {
                var _ = new CompositeHandler(null);
            };

            // assert
            Assert.Throws<ArgumentNullException>(action);
        }

        private class CompositeHandler : Handler<int, int>
        {
            public CompositeHandler(DummyHandler dummyHandler)
            {
                AddHandler(dummyHandler);
            }
        }

        private class DummyHandler : IHandler<int, int>
        {
            public int Handle(int input, Func<int, int> next) => throw new NotSupportedException();
        }
    }
}
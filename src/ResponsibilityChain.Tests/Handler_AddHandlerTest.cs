using System;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class Handler_AddHandlerTest
    {
        [Fact]
        public void AddNullHandlerToTheChain_ThrowsException()
        {
            // arrange

            // act
            Action action = () =>
            {
                var _ = new CompositeHandler(new DummyHandler());
            };

            // assert
            Assert.Throws<ArgumentNullException>(action);
        }

        private class CompositeHandler : Handler<int, int>
        {
            public CompositeHandler(DummyHandler dummyHandler)
            {
                AddHandler<DummyHandler>(null);
            }
        }

        private class DummyHandler : IHandler<int, int>
        {
            public int Handle(int input, Func<int, int> next) => throw new NotSupportedException();
        }
    }
}
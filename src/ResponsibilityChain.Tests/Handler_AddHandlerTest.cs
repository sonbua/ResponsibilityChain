using System;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class Handler_AddHandlerTest
    {
        [Fact]
        public void AddUnregisteredHandlerToTheChain_ThrowsException()
        {
            // arrange

            // act
            Action action = () =>
            {
                var handler = new CompositeHandler();
            };
            
            // assert
            Assert.Throws<ArgumentNullException>(action);
        }
        
        private class CompositeHandler : Handler<int, int>
        {
            public CompositeHandler()
                : base(new NullServiceProvider())
            {
                AddHandler<DummyHandler>();
            }
        }

        private class NullServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }

        private class DummyHandler : IHandler<int, int>
        {
            public int Handle(int input, Func<int, int> next) => throw new NotSupportedException();
        }
    }
}
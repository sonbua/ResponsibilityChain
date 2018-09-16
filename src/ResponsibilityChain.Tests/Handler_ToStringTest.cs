using System;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class Handler_ToStringTest
    {
        private class CompositeWithNoNestedHandler : Handler<int, int>
        {
            public CompositeWithNoNestedHandler()
                : base(new MockedServiceProvider())
            {
                AddHandler<SimpleHandler1>();
                AddHandler<SimpleHandler2>();
            }
        }

        private class SimpleHandler1 : IHandler<int, int>
        {
            public int Handle(int input, Func<int, int> next) => throw new NotSupportedException();
        }

        private class SimpleHandler2 : IHandler<int, int>
        {
            public int Handle(int input, Func<int, int> next) => throw new NotSupportedException();
        }

        private class CompositeWithOneLevelNestedHandler : Handler<int, int>
        {
            public CompositeWithOneLevelNestedHandler()
                : base(new MockedServiceProvider())
            {
                AddHandler<CompositeWithNoNestedHandler>();
                AddHandler<SimpleHandler1>();
            }
        }

        private class CompositeWithTwoLevelNestedHandler : Handler<int, int>
        {
            public CompositeWithTwoLevelNestedHandler()
                : base(new MockedServiceProvider())
            {
                AddHandler<CompositeWithOneLevelNestedHandler>();
                AddHandler<SimpleHandler1>();
            }
        }

        private class MockedServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                return serviceType.IsAbstract || serviceType.IsInterface
                    ? null
                    : Activator.CreateInstance(serviceType);
            }
        }

        [Fact]
        public void GivenCompositeWithNoNestedHandler_ReturnsCorrectHierarchy()
        {
            // arrange
            var handler = new CompositeWithNoNestedHandler();

            // act
            var handlerDescription = handler.ToString();

            // assert
            Assert.Equal(
                $"{typeof(CompositeWithNoNestedHandler)}" + Environment.NewLine +
                $"  {typeof(SimpleHandler1)}" + Environment.NewLine +
                $"  {typeof(SimpleHandler2)}",
                handlerDescription
            );
        }

        [Fact]
        public void GivenCompositeWithOneLevelNestedHandler_ReturnsCorrectHierarchy()
        {
            // arrange
            var handler = new CompositeWithOneLevelNestedHandler();

            // act
            var handlerDescription = handler.ToString();

            // assert
            Assert.Equal(
                $"{typeof(CompositeWithOneLevelNestedHandler)}" + Environment.NewLine +
                $"  {typeof(CompositeWithNoNestedHandler)}" + Environment.NewLine +
                $"    {typeof(SimpleHandler1)}" + Environment.NewLine +
                $"    {typeof(SimpleHandler2)}" + Environment.NewLine +
                $"  {typeof(SimpleHandler1)}",
                handlerDescription
            );
        }

        [Fact]
        public void GivenCompositeWithTwoLevelNestedHandler_ReturnsCorrectHierarchy()
        {
            // arrange
            var handler = new CompositeWithTwoLevelNestedHandler();

            // act
            var handlerDescription = handler.ToString();

            // assert
            Assert.Equal(
                $"{typeof(CompositeWithTwoLevelNestedHandler)}" + Environment.NewLine +
                $"  {typeof(CompositeWithOneLevelNestedHandler)}" + Environment.NewLine +
                $"    {typeof(CompositeWithNoNestedHandler)}" + Environment.NewLine +
                $"      {typeof(SimpleHandler1)}" + Environment.NewLine +
                $"      {typeof(SimpleHandler2)}" + Environment.NewLine +
                $"    {typeof(SimpleHandler1)}" + Environment.NewLine +
                $"  {typeof(SimpleHandler1)}",
                handlerDescription
            );
        }
    }
}
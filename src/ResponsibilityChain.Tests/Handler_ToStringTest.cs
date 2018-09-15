using System;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class Handler_ToStringTest
    {
        private class CompositeWithNoNestedHandler : Handler<int, int>
        {
            public CompositeWithNoNestedHandler()
                : base(ActivatorServiceProvider.Instance)
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
                : base(ActivatorServiceProvider.Instance)
            {
                AddHandler<CompositeWithNoNestedHandler>();
                AddHandler<SimpleHandler1>();
            }
        }

        private class CompositeWithTwoLevelNestedHandler : Handler<int, int>
        {
            public CompositeWithTwoLevelNestedHandler()
                : base(ActivatorServiceProvider.Instance)
            {
                AddHandler<CompositeWithOneLevelNestedHandler>();
                AddHandler<SimpleHandler1>();
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
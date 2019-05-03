using System;
using Xunit;

namespace ResponsibilityChain.Tests
{
    // ReSharper disable once InconsistentNaming
    public class Handler_ToStringTest
    {
        private class CompositeWithNoNestedHandler : Handler<int, int>
        {
            public CompositeWithNoNestedHandler(SimpleHandler1 simpleHandler1, SimpleHandler2 simpleHandler2)
            {
                AddHandler(simpleHandler1);
                AddHandler(simpleHandler2);
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
            public CompositeWithOneLevelNestedHandler(
                CompositeWithNoNestedHandler compositeWithNoNestedHandler,
                SimpleHandler1 simpleHandler1)
            {
                AddHandler(compositeWithNoNestedHandler);
                AddHandler(simpleHandler1);
            }
        }

        private class CompositeWithTwoLevelNestedHandler : Handler<int, int>
        {
            public CompositeWithTwoLevelNestedHandler(
                CompositeWithOneLevelNestedHandler compositeWithOneLevelNestedHandler,
                SimpleHandler1 simpleHandler1)
            {
                AddHandler(compositeWithOneLevelNestedHandler);
                AddHandler(simpleHandler1);
            }
        }

        [Fact]
        public void GivenCompositeWithNoNestedHandler_ReturnsCorrectHierarchy()
        {
            // arrange
            var handler = new CompositeWithNoNestedHandler(new SimpleHandler1(), new SimpleHandler2());

            // act
            var handlerDescription = handler.ToString();

            // assert
            Assert.Equal(
                $"{typeof(CompositeWithNoNestedHandler)}" +
                Environment.NewLine +
                $"  {typeof(SimpleHandler1)}" +
                Environment.NewLine +
                $"  {typeof(SimpleHandler2)}",
                handlerDescription
            );
        }

        [Fact]
        public void GivenCompositeWithOneLevelNestedHandler_ReturnsCorrectHierarchy()
        {
            // arrange
            var handler = new CompositeWithOneLevelNestedHandler(
                new CompositeWithNoNestedHandler(
                    new SimpleHandler1(),
                    new SimpleHandler2()
                ),
                new SimpleHandler1()
            );

            // act
            var handlerDescription = handler.ToString();

            // assert
            Assert.Equal(
                $"{typeof(CompositeWithOneLevelNestedHandler)}" +
                Environment.NewLine +
                $"  {typeof(CompositeWithNoNestedHandler)}" +
                Environment.NewLine +
                $"    {typeof(SimpleHandler1)}" +
                Environment.NewLine +
                $"    {typeof(SimpleHandler2)}" +
                Environment.NewLine +
                $"  {typeof(SimpleHandler1)}",
                handlerDescription
            );
        }

        [Fact]
        public void GivenCompositeWithTwoLevelNestedHandler_ReturnsCorrectHierarchy()
        {
            // arrange
            var handler = new CompositeWithTwoLevelNestedHandler(
                new CompositeWithOneLevelNestedHandler(
                    new CompositeWithNoNestedHandler(
                        new SimpleHandler1(),
                        new SimpleHandler2()
                    ),
                    new SimpleHandler1()
                ),
                new SimpleHandler1()
            );

            // act
            var handlerDescription = handler.ToString();

            // assert
            Assert.Equal(
                $"{typeof(CompositeWithTwoLevelNestedHandler)}" +
                Environment.NewLine +
                $"  {typeof(CompositeWithOneLevelNestedHandler)}" +
                Environment.NewLine +
                $"    {typeof(CompositeWithNoNestedHandler)}" +
                Environment.NewLine +
                $"      {typeof(SimpleHandler1)}" +
                Environment.NewLine +
                $"      {typeof(SimpleHandler2)}" +
                Environment.NewLine +
                $"    {typeof(SimpleHandler1)}" +
                Environment.NewLine +
                $"  {typeof(SimpleHandler1)}",
                handlerDescription
            );
        }
    }
}
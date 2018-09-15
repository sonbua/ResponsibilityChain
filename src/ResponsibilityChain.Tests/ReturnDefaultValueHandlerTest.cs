using System;
using System.Text;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class ReturnDefaultValueHandlerTest
    {
        [Fact]
        public void IntegerOutputExpected_ReturnsZero()
        {
            // arrange
            var handler = ReturnDefaultValueHandler<string, int>.Instance;

            // act
            var output = handler.Handle("some input", next: null);

            // assert
            Assert.Equal(0, output);
        }

        [Fact]
        public void StringOutputExpected_ReturnsNull()
        {
            // arrange
            var handler = ReturnDefaultValueHandler<string, string>.Instance;

            // act
            var output = handler.Handle("some input", next: null);

            // assert
            Assert.Null(output);
        }

        [Fact]
        public void DateTimeOutputExpected_ReturnsDateTimeMin()
        {
            // arrange
            var handler = ReturnDefaultValueHandler<string, DateTime>.Instance;

            // act
            var output = handler.Handle("some input", next: null);

            // assert
            Assert.Equal(DateTime.MinValue, output);
        }

        [Fact]
        public void ReferenceTypeObjectOutputExpected_ReturnsNull()
        {
            // arrange
            var handler = ReturnDefaultValueHandler<string, StringBuilder>.Instance;

            // act
            var output = handler.Handle("some input", next: null);

            // assert
            Assert.Null(output);
        }
    }
}
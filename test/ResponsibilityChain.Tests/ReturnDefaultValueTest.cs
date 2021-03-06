using System;
using System.Text;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class ReturnDefaultValueTest
    {
        [Fact]
        public void IntegerOutputExpected_ReturnsZero()
        {
            // arrange
            var handler = new ReturnDefaultValue<string, int>();

            // act
            var output = handler.Handle("some input", next: null);

            // assert
            Assert.Equal(0, output);
        }

        [Fact]
        public void StringOutputExpected_ReturnsNull()
        {
            // arrange
            var handler = new ReturnDefaultValue<string, string>();

            // act
            var output = handler.Handle("some input", next: null);

            // assert
            Assert.Null(output);
        }

        [Fact]
        public void DateTimeOutputExpected_ReturnsDateTimeMin()
        {
            // arrange
            var handler = new ReturnDefaultValue<string, DateTime>();

            // act
            var output = handler.Handle("some input", next: null);

            // assert
            Assert.Equal(DateTime.MinValue, output);
        }

        [Fact]
        public void ReferenceTypeObjectOutputExpected_ReturnsNull()
        {
            // arrange
            var handler = new ReturnDefaultValue<string, StringBuilder>();

            // act
            var output = handler.Handle("some input", next: null);

            // assert
            Assert.Null(output);
        }
    }
}
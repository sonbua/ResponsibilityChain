using System;
using System.Text;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class ThrowNotSupportedTest
    {
        [Fact]
        public void IntegerOutputExpected_ThrowsNotSupportedException()
        {
            // arrange
            var handler = new ThrowNotSupported<string, int>();

            // act
            Func<object> testDelegate = () => handler.Handle("some input", null);

            // assert
            Assert.Throws<NotSupportedException>(testDelegate);
        }

        [Fact]
        public void StringOutputExpected_ThrowsNotSupportedException()
        {
            // arrange
            var handler = new ThrowNotSupported<string, string>();

            // act
            Func<object> testDelegate = () => handler.Handle("some input", null);

            // assert
            Assert.Throws<NotSupportedException>(testDelegate);
        }

        [Fact]
        public void ReferenceTypeOutputExpected_ThrowsNotSupportedException()
        {
            // arrange
            var handler = new ThrowNotSupported<string, StringBuilder>();

            // act
            Func<object> testDelegate = () => handler.Handle("some input", null);

            // assert
            Assert.Throws<NotSupportedException>(testDelegate);
        }
    }
}
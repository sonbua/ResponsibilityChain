using System;
using System.Text;
using Xunit;

namespace ResponsibilityChain.Tests
{
    public class ThrowNotSupportedChainHandlerTest
    {
        [Fact]
        public void IntegerOutputExpected_ThrowsNotSupportedException()
        {
            // arrange
            var handler = ThrowNotSupportedHandler<string, int>.Instance;

            // act
            Func<object> testDelegate = () => handler.Handle("some input", null);

            // assert
            Assert.Throws<NotSupportedException>(testDelegate);
        }

        [Fact]
        public void StringOutputExpected_ThrowsNotSupportedException()
        {
            // arrange
            var handler = ThrowNotSupportedHandler<string, string>.Instance;

            // act
            Func<object> testDelegate = () => handler.Handle("some input", null);

            // assert
            Assert.Throws<NotSupportedException>(testDelegate);
        }

        [Fact]
        public void ReferenceTypeOutputExpected_ThrowsNotSupportedException()
        {
            // arrange
            var handler = ThrowNotSupportedHandler<string, StringBuilder>.Instance;

            // act
            Func<object> testDelegate = () => handler.Handle("some input", null);

            // assert
            Assert.Throws<NotSupportedException>(testDelegate);
        }
    }
}
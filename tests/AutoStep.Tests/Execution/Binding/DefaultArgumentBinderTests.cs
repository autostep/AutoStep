using System.Globalization;
using System.Threading;
using AutoStep.Execution.Binding;
using FluentAssertions;
using Xunit;

namespace AutoStep.Tests.Execution.Binding
{
    public class DefaultArgumentBinderTests
    {
        [Fact]
        public void CanConvertDecimal()
        {
            var binder = new DefaultArgumentBinder();
            var result = binder.Bind("123", typeof(decimal));

            result.Should().Be(123M);
        }

        [Fact]
        public void StringToString()
        {
            var binder = new DefaultArgumentBinder();
            var result = binder.Bind("123", typeof(string));

            result.Should().Be("123");
        }

        [Fact]
        public void UsesCurrentCulture()
        {
            var binder = new DefaultArgumentBinder();
            var defaultCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr");
                
                var result = binder.Bind("123,05", typeof(double));

                result.Should().Be(123.05);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = defaultCulture;
            }
        }
    }
}

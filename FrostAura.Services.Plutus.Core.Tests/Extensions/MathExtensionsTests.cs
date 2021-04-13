using FrostAura.Services.Plutus.Core.Extensions;
using System;
using Xunit;

namespace FrostAura.Services.Plutus.Core.Tests.Extensions
{
  public class MathExtensionsTests
  {
    [Theory]
    [InlineData(100, 50, -50)]
    [InlineData(50, 100, 100)]
    [InlineData(0.035, 0.070, 100)]
    [InlineData(0.070, 0.035, -50)]
    [InlineData(0.02257600, 0.02301300, 1.9356839121190644932671863900)]
    public void GetPercentageDelta_WithValidInput_ShouldReturnValidOutput(decimal value1, decimal value2, decimal expected)
    {
      var actual = MathExtensions.GetPercentageDelta(value1, value2);

      Assert.Equal(Math.Round(expected, 3), Math.Round(actual, 3));
    }
  }
}

using System;

namespace FrostAura.Services.Plutus.Core.Extensions
{
  /// <summary>
  /// Extensions for the math class.
  /// </summary>
  public static class MathExtensions
  {
    /// <summary>
    /// Calculate the percentage delta between two numbers.
    /// </summary>
    /// <param name="value1">Number 1</param>
    /// <param name="value2">Number 2</param>
    /// <returns>Percentage delta between two numbers.</returns>
    public static decimal GetPercentageDelta(decimal value1, decimal value2)
    {
      var delta = Math.Abs(value1 - value2) / value1;

      if (value1 > value2) delta *= -1;

      return delta * 100;
    }
  }
}
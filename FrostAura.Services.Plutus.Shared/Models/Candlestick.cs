using Microsoft.ML.Data;
using System;
using System.Diagnostics;

namespace FrostAura.Services.Plutus.Shared.Models
{
  /// <summary>
  /// Model representing a candlestick.
  /// </summary>
  [DebuggerDisplay("Open: {Open}, Close: {Close}, High: {High}, Low: {Low}, Time: {CloseTime}")]
  public class Candlestick
  {
    /// <summary>
    /// The time this candlestick opened.
    /// </summary>
    [LoadColumn(7)]
    public DateTime OpenTime { get; set; }
    /// <summary>
    /// The price at which this candlestick opened.
    /// </summary>
    [LoadColumn(8)]
    public float Open { get; set; }
    /// <summary>
    /// The highest price in this candlestick.
    /// </summary>
    [LoadColumn(9)]
    public float High { get; set; }
    /// <summary>
    /// The lowest price in this candlestick.
    /// </summary>
    [LoadColumn(10)]
    public float Low { get; set; }
    /// <summary>
    /// The price at which this candlestick closed.
    /// </summary>
    [LoadColumn(11)]
    public float Close { get; set; }
    /// <summary>
    /// The volume traded during this candlestick.
    /// </summary>
    [LoadColumn(12)]
    public float Volume { get; set; }
    /// <summary>
    /// The close time of this candlestick
    /// </summary>
    [LoadColumn(13)]
    public DateTime CloseTime { get; set; }
    /// <summary>
    /// The amount of trades in this candlestick
    /// </summary>
    [LoadColumn(14)]
    public float TradeCount { get; set; }
  }
}

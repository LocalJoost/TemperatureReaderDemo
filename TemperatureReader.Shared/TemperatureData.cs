using System;

namespace TemperatureReader.Shared
{
  public class TemperatureData
  {
    public TemperatureData()
    {
    }

    public TemperatureData(double temperature, bool isValid = true )
    {
      Temperature = temperature;
      IsValid = isValid;
      Timestamp = DateTimeOffset.UtcNow;
    }

    public DateTimeOffset Timestamp { get; set; }

    public double Temperature { get; set; }

    public bool IsValid { get; set; }

    public FanStatus FanStatus { get; set; }

  }
}
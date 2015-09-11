using System;

namespace TemperatureReader.ClientApp.Models
{
  public static class DateTimeExtensions
  {
    public static bool IsSecondsAgo(this DateTimeOffset time, int seconds)
    {
      return (DateTimeOffset.UtcNow - time).TotalSeconds > seconds;
    }
  }
}

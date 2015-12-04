using System.Diagnostics;

namespace TemperatureReader.Logic.Utilities
{
  class SynchronousWaiter
  {
    readonly Stopwatch _stopwatch;
    public SynchronousWaiter()
    {
      _stopwatch = Stopwatch.StartNew();
    }

    public void Wait(double milliseconds)
    {
      var initialTick = _stopwatch.ElapsedTicks;
      var desiredTicks = milliseconds / 1000.0 * Stopwatch.Frequency;
      var finalTick = initialTick + desiredTicks;
      while (_stopwatch.ElapsedTicks < finalTick)
      {

      }
    }
  }
}

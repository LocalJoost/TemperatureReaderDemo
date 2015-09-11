using System;
using TemperatureReader.Shared;

namespace TemperatureReader.Logic.Devices
{
  public interface IAnalogTemperatureSensorController
  {
    bool IsMeasuring { get; }
    event EventHandler<TemperatureData> OnTemperatureMeasured;
    bool Start();
    void Stop();
  }
}
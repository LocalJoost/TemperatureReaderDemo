using System;
using System.Threading.Tasks;
using TemperatureReader.Shared;

namespace TemperatureReader.ClientApp.Models
{
  public interface ITemperatureListener
  {
    Task Start();

    void Stop();

    bool IsRunning { get; }

    event EventHandler<TemperatureData> OnTemperatureDataReceived;
  }
}

using Windows.Devices.Gpio;

namespace TemperatureReader.Logic.Devices
{
  public interface IGpioService
  {
    GpioController Controller { get; }
  }
}
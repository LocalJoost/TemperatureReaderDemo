using Windows.Devices.Gpio;

namespace TemperatureReader.Logic.Devices
{
  public class GpioService : IGpioService
  {
    public GpioService()
    {

      if (Windows.Foundation.Metadata.ApiInformation‏.IsTypePresent(
        "Windows.Devices.Gpio.GpioController"))
      {
        if (Controller == null)
        {
          Controller = GpioController.GetDefault();
        }
      }
    }
    public GpioController Controller { get; private set; }
  }
}

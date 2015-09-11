using System.Threading.Tasks;

namespace TemperatureReader.Logic.Devices
{
  public class StatusLed : SwitchDevice
  {
    public StatusLed(IGpioService gpioCtrl, int pinId) : base(gpioCtrl,pinId)
    {
    }

    public async Task Flash(int milliseconds)
    {
      SwitchOn(true);
      await Task.Delay(milliseconds);
      SwitchOn(false);
    }
  }
}
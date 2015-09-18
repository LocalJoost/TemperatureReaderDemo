using Windows.Devices.Gpio;

namespace TemperatureReader.Logic.Devices
{
  public class SwitchDevice
  {
    private readonly int _pinId;
    private readonly IGpioService _gpioCtrl;
    private GpioPin _pin;

    public SwitchDevice(IGpioService gpioCtrl, int pinId)
    {
      _pinId = pinId;
      _gpioCtrl = gpioCtrl;
      SwitchOn(false);
    }

    public void SwitchOn(bool @on)
    {
      GpioPin pin;
      if ((pin = GetPin()) != null)
      {
        pin.Write(@on? GpioPinValue.High : GpioPinValue.Low);
      }
    }

    public void Toggle()
    {
      SwitchOn(!IsOn);
    }
    
    public bool IsOn
    {
      get
      {
        GpioPin pin;
        if ((pin = GetPin()) != null)
        {
          var currentPinValue = pin.Read();
          return currentPinValue == GpioPinValue.High;
        }
        return false;
      }
    }
    
    protected GpioPin GetPin()
    {
      if (_pin == null)
      {
        if (_gpioCtrl?.Controller != null)
        {
          _pin = _gpioCtrl.Controller.OpenPin(_pinId);
          _pin.SetDriveMode(GpioPinDriveMode.Output);
        }
      }
      return _pin;
    }
  }
}
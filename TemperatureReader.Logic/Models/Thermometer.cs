using System;
using System.Threading.Tasks;
using TemperatureReader.Logic.Devices;
using TemperatureReader.Shared;

namespace TemperatureReader.Logic.Models
{
  public class Thermometer
  {
    private const int WaitTimeBeforeStartup = 5000;

    private readonly int _errorPinId;
    private readonly int _dataPinId;

    private readonly int _longFlashTime;
    private readonly int _shortFlashTime;

    private StatusLed _errorLed;
    private StatusLed _dataPinLed;

    private readonly IGpioService _gpioService;
    private readonly IAnalogTemperatureSensorController _temperatureSensorController ;

    public Thermometer(IGpioService gpioService, 
      IAnalogTemperatureSensorController temperatureSensorController, 
      int errorPinId = Settings.ErrorPinId, int dataPinId = Settings.DataPinId, 
      int longFlashTime = Settings.LongFlashTime, int shortFlashTime = Settings.ShortFlashTime)
    {
      _gpioService = gpioService;
      _errorPinId = errorPinId;
      _dataPinId = dataPinId;
      _longFlashTime = longFlashTime;
      _shortFlashTime = shortFlashTime;
      _temperatureSensorController = temperatureSensorController;
    }

    public async Task Start()
    {
      _errorLed = new StatusLed(_gpioService, _errorPinId);
      _dataPinLed = new StatusLed(_gpioService, _dataPinId);

      await ShowStartup();
      await Task.Delay(WaitTimeBeforeStartup);

      _temperatureSensorController.OnTemperatureMeasured += HandleTemperatureMeasured;
      _temperatureSensorController.Start();
    }

    public void Stop()
    {
      //TODO!!!
    }

    private async void HandleTemperatureMeasured(object sender, TemperatureData e)
    {
      if (e.IsValid)
      {
        await _dataPinLed.Flash(_longFlashTime);
        OnTemperatureMeasured?.Invoke(this, e);
      }
      else
      {
        await _errorLed.Flash(_longFlashTime);
      }
    }

    private async Task ShowStartup()
    {
      for (var c = 0; c < 3; c++)
      {
        _errorLed.SwitchOn(true);
        await Task.Delay(_shortFlashTime);
        _dataPinLed.SwitchOn(true);
        _errorLed.SwitchOn(false);
        await Task.Delay(_shortFlashTime);
        _dataPinLed.SwitchOn(false);
        await Task.Delay(_shortFlashTime);
      }
    }

    public event EventHandler<TemperatureData> OnTemperatureMeasured;
  }
}

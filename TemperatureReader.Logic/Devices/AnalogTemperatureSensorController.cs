using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using TemperatureReader.Logic.Utilities;
using TemperatureReader.Shared;

namespace TemperatureReader.Logic.Devices
{
  public class AnalogTemperatureSensorController : IAnalogTemperatureSensorController
  {
    private readonly int _adcCsPinId;
    private readonly int _adcClkPinId;
    private readonly int _adcDigitalIoPinId;

    private readonly IGpioService _gpioCtrl;
    private GpioPin _adcCsPin;
    private GpioPin _adcClkPin;
    private GpioPin _adcDigitalIoPin;

    private Task _task;
    private CancellationTokenSource _cancellationTokenSource;
    private DateTimeOffset _lastExecutionTime = DateTimeOffset.MinValue;
    private readonly int _delayMilliSeconds;
    private readonly int _maxRetries;


    public AnalogTemperatureSensorController(
      IGpioService gpioCtrl, 
      int adcCsPinId = Settings.AdcCsPinId, 
      int adcClkPinId = Settings.AdcClkPinId, 
      int adcDigitalIoPinId = Settings.AdcDigitalIoPinId, 
      int delayMilliSeconds = Settings.DefaultTemperaturePostingDelay, 
      int maxRetries = Settings.MaxReadRetry)
    {
      _gpioCtrl = gpioCtrl;
      _adcCsPinId = adcCsPinId;
      _adcClkPinId = adcClkPinId;
      _adcDigitalIoPinId = adcDigitalIoPinId;
      _delayMilliSeconds = delayMilliSeconds;
      _maxRetries = maxRetries;
      IsMeasuring = false;
    }

    public bool IsMeasuring { get; private set; }

    public bool Start()
    {
      if (_gpioCtrl?.Controller != null)
      {
        IsMeasuring = true;

        if (_adcDigitalIoPin == null)
        {
          _adcCsPin = _gpioCtrl.Controller.OpenPin(_adcCsPinId);
          _adcClkPin = _gpioCtrl.Controller.OpenPin(_adcClkPinId);
          _adcDigitalIoPin = _gpioCtrl.Controller.OpenPin(_adcDigitalIoPinId);
        }

        if (_task == null)
        {
          _cancellationTokenSource = new CancellationTokenSource();
          InitReadSession();
          _task = new Task(async () => await ExecuteMeasuring(_cancellationTokenSource.Token));
          _task.Start();
        }
      }
      return IsMeasuring;
    }

    public void Stop()
    {
      _cancellationTokenSource.Cancel();
      _adcCsPin.Dispose();
      _adcCsPin = null;
      _adcClkPin.Dispose();
      _adcClkPin = null;
      _adcDigitalIoPin.Dispose();
      _adcDigitalIoPin = null;
      IsMeasuring = false;
    }

    private async Task ExecuteMeasuring(CancellationToken cancellationToken)
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        var timePassed = DateTimeOffset.UtcNow - _lastExecutionTime;
        if (timePassed > TimeSpan.FromMilliseconds(_delayMilliSeconds))
        {
          var retries = 0;
          var readStatus = false;

          while (!readStatus && retries++ < _maxRetries)
          {
            readStatus = ReadData();
            _lastExecutionTime = DateTimeOffset.UtcNow;
          }

          if (retries >= _maxRetries)
          {
            OnTemperatureMeasured?.Invoke(this, new TemperatureData {IsValid = false});
          }
          _lastExecutionTime = DateTimeOffset.UtcNow;
        }
        else
        {
          var waitTime = _delayMilliSeconds - timePassed.TotalMilliseconds;

          if (waitTime > 0)
          {
            await Task.Delay(Convert.ToInt32(waitTime), cancellationToken);
          }
        }
      }
    }

    private void InitReadSession()
    {
      _adcClkPin.SetDriveMode(GpioPinDriveMode.Output);
      _adcCsPin.SetDriveMode(GpioPinDriveMode.Output);
      _adcDigitalIoPin.SetDriveMode(GpioPinDriveMode.Output);
    }

    private void InitAdConverter(SynchronousWaiter waiter)
    {
      _adcCsPin.Write(GpioPinValue.Low);
      _adcClkPin.Write(GpioPinValue.Low);
      _adcDigitalIoPin.Write(GpioPinValue.High); waiter.Wait(2);
      _adcClkPin.Write(GpioPinValue.High); waiter.Wait(2);

      _adcClkPin.Write(GpioPinValue.Low);
      _adcDigitalIoPin.Write(GpioPinValue.High); waiter.Wait(2);
      _adcClkPin.Write(GpioPinValue.High); waiter.Wait(2);

      _adcClkPin.Write(GpioPinValue.Low);
      _adcDigitalIoPin.Write(GpioPinValue.Low); waiter.Wait(2);
      _adcClkPin.Write(GpioPinValue.High);
      _adcDigitalIoPin.Write(GpioPinValue.High); waiter.Wait(2);
      _adcClkPin.Write(GpioPinValue.Low);
      _adcDigitalIoPin.Write(GpioPinValue.High); waiter.Wait(2);

      _adcDigitalIoPin.SetDriveMode(GpioPinDriveMode.Input);
    }

    private bool ReadData()
    {
      int sequence1 = 0, sequence2 = 0;
      _adcCsPin.Write(GpioPinValue.Low);

      InitReadSession();
      var waiter = new SynchronousWaiter();
      InitAdConverter(waiter);

      //Read the first sequence
      for (var i = 0; i < 8; i++)
      {
        _adcClkPin.Write(GpioPinValue.High);
        waiter.Wait(2);
        _adcClkPin.Write(GpioPinValue.Low);
        waiter.Wait(2);
        sequence1 = sequence1 << 1 | (int)_adcDigitalIoPin.Read();
      }

      //Read the seconds sequence
      for (var i = 0; i < 8; i++)
      {
        sequence2 = sequence2 | (int)_adcDigitalIoPin.Read() << i;

        _adcClkPin.Write(GpioPinValue.High);
        waiter.Wait(2);
        _adcClkPin.Write(GpioPinValue.Low);
        waiter.Wait(2);
      }

      _adcCsPin.Write(GpioPinValue.High);

      if (sequence1 == sequence2)
      {
        OnTemperatureMeasured?.Invoke(this,
          new TemperatureData { IsValid = true, Temperature = 
          Math.Round(((255 - sequence1) - 121) * 0.21875,1) + 21.8, Timestamp = DateTimeOffset.UtcNow});
        return true;
      }

      return false;
    }

    public event EventHandler<TemperatureData> OnTemperatureMeasured;
  }
}

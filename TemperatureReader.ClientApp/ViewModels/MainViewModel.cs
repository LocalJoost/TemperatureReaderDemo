using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Devices.Geolocation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using TemperatureReader.ClientApp.Helpers;
using TemperatureReader.ClientApp.Messages;
using TemperatureReader.ClientApp.Models;
using TemperatureReader.ServiceBus;

namespace TemperatureReader.ClientApp.ViewModels
{
  public class MainViewModel : ViewModelBase
  {
    private readonly ITemperatureListener _listener;
    private readonly IBandOperator _bandOperator;
    private readonly IMessageDisplayer _messageDisplayer;
    private readonly IErrorLogger _errorLogger;


    public MainViewModel(ITemperatureListener listener, IBandOperator bandOperator, 
      IMessageDisplayer messageDisplayer, IErrorLogger errorLogger)
    {
      _listener = listener;
      _bandOperator = bandOperator;
      _messageDisplayer = messageDisplayer;
      _errorLogger = errorLogger;
    }

    public void Init()
    {
      Messenger.Default.Register<ResumeMessage>(this, async msg => await OnResume());
    }

    public async Task RemoveTile()
    {
      IsBusy = true;
      await Task.Delay(1);
      await _bandOperator.RemoveTile();
      IsBusy = false;
    }

    private void Listener_OnTemperatureDataReceived(object sender, Shared.TemperatureData e)
    {
      if (e.IsValid)
      {
        DispatcherHelper.CheckBeginInvokeOnUI(() =>
        {
          Messenger.Default.Send(new DataReceivedMessage());
          Temperature = e.Temperature.ToString(CultureInfo.InvariantCulture);
          LastDateTimeReceived = e.Timestamp.ToLocalTime().ToString("HH:mm:ss   dd-MM-yyyy");
          FanStatus = e.FanStatus == Shared.FanStatus.On ? "on" : "off";
        });
      }
    }

    public async Task OnSuspend()
    {
      if (_bandOperator != null && _bandOperator.IsRunning)
      {
        await _bandOperator.Stop();
        await _messageDisplayer.ShowMessage("Suspended");
      }
    }


    private async Task Start()
    {
      IsBusy = true;
      await Task.Delay(1);
      _listener.OnTemperatureDataReceived += Listener_OnTemperatureDataReceived;
      _listener.OnTemperatureDataReceived += _bandOperator.HandleNewTemperature;
      await _listener.Start();
      await StartBackgroundSession();
      await _bandOperator.Start();
      await _bandOperator.SendVibrate();
      IsBusy = false;
    }

    private async Task Stop()
    {
      IsBusy = true;
      await Task.Delay(1);
      _listener.OnTemperatureDataReceived -= Listener_OnTemperatureDataReceived;
      _listener.OnTemperatureDataReceived -= _bandOperator.HandleNewTemperature;
      _listener.Stop();
      await _bandOperator.Stop();
      _session.Dispose();
      _session = null;
      IsBusy = false;
    }

    private async Task Toggle()
    {
      if (_listener.IsRunning)
      {
        await Stop();
      }
      else
      {
        await Start();
      }
      RaisePropertyChanged(() => IsListening);
    }

    public async Task OnResume()
    {
      if ( IsListening && _bandOperator != null)
      {
        try
        {
          IsBusy = true;
          await Task.Delay(1);
          await StartBackgroundSession();
          await _bandOperator.Start(true);
          await _bandOperator.SendVibrate();
          IsBusy = false;

        }
        catch (Exception ex)
        {
          await _errorLogger.LogException(ex);
          await _messageDisplayer.ShowMessage($"Error restarting Band {ex.Message}");
        }
      }
    }

    public bool IsListening
    {
      get
      {
        return _listener?.IsRunning ?? false;
      }
      set
      {
        if (_listener != null)
        {
          if (value != _listener.IsRunning)
          {
            Toggle();
          }
        }
      }
    }

    private string _temperature = "--.-";
    public string Temperature
    {
      get { return _temperature; }
      set { Set(() => Temperature, ref _temperature, value); }
    }

    private string _lastDateTimeReceived = "--:--:--   ----------";
    public string LastDateTimeReceived
    {
      get { return _lastDateTimeReceived; }
      set { Set(() => LastDateTimeReceived, ref _lastDateTimeReceived, value); }
    }

    private string _fanStatus = "???";
    public string FanStatus
    {
      get { return _fanStatus; }
      set { Set(() => FanStatus, ref _fanStatus, value); }
    }

    private bool _isBusy;
    public bool IsBusy
    {
      get { return _isBusy; }
      set { Set(() => IsBusy, ref _isBusy, value); }
    }

    private ExtendedExecutionSession _session;
    private async Task<bool> StartBackgroundSession()
    {
      if (_session != null)
      {
        try
        {
          _session.Dispose();
        }
        catch (Exception){}
      }
      _session = null;
      {
        _session = new ExtendedExecutionSession
        {
          Description = "Temperature tracking",
          Reason = ExtendedExecutionReason.LocationTracking
        };
        StartFakeGeoLocator();

        _session.Revoked += async (p, q) => { await OnRevoke(); };

        var result = await _session.RequestExtensionAsync();
        return result != ExtendedExecutionResult.Denied;
      }
      return false;
    }

    private Geolocator _locator;

    private void StartFakeGeoLocator()
    {
      _locator = new Geolocator
      {
        DesiredAccuracy = PositionAccuracy.Default,
        DesiredAccuracyInMeters = 1,
        MovementThreshold = 1,
        ReportInterval = 5000
      };
      _locator.PositionChanged += LocatorPositionChanged;
    }

    private void LocatorPositionChanged(Geolocator sender, PositionChangedEventArgs args)
    {
    }

    private async Task OnRevoke()
    {
      await StartBackgroundSession();
    }

    private static MainViewModel _instance;

    public static MainViewModel Instance
    {
      get { return _instance ?? (_instance = CreateNew()); }
      set { _instance = value; }
    }

    public static MainViewModel CreateNew()
    {
      var fanStatusPoster = new FanSwitchQueueClient(QueueMode.Send);
      fanStatusPoster.Start();
      var listener = new TemperatureListener();
      var errorLogger = SimpleIoc.Default.GetInstance<IErrorLogger>();
      var messageDisplayer = SimpleIoc.Default.GetInstance<IMessageDisplayer>();
      var bandOperator = new BandOperator(fanStatusPoster);
      return (new MainViewModel(
        listener, bandOperator, 
        messageDisplayer, errorLogger));
    }
  }
}

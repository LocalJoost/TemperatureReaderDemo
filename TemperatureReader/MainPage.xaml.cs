using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using TemperatureReader.Logic.Devices;
using TemperatureReader.Logic.Models;
using TemperatureReader.ServiceBus;
using TemperatureReader.Shared;

namespace TemperatureReader
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
      this.Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
      var gpioService = new GpioService();
      var fanSwitch = new SwitchDevice(gpioService, Settings.SwitchPinId);
      var controller = new AnalogTemperatureSensorController(gpioService);
      var thermometer = new Thermometer(gpioService, controller);
      var poster = new TemperatureQueueClient(QueueMode.Send);
      var fanCommandListener = new FanSwitchQueueClient(QueueMode.Listen);
      await fanCommandListener.Start();

      fanCommandListener.OnDataReceived += (cs, cmd) =>
      {
        Debug.WriteLine($"Fanswitchcommand received {cmd.Status}; current status fan on = {fanSwitch.IsOn} at {DateTime.Now}");

        var newStatus = cmd.Status == FanStatus.On;
        if (newStatus != fanSwitch.IsOn)
        {
          fanSwitch.Toggle();
        }
      };

      thermometer.OnTemperatureMeasured += OnTemperatureMeasured;
      thermometer.OnTemperatureMeasured += async (thObject, data) =>
      {
        data.FanStatus = fanSwitch.IsOn ? FanStatus.On : FanStatus.Off;
        await poster.PostData(data);
      };

      await poster.Start();
      await thermometer.Start();

    }

    private async void OnTemperatureMeasured(object sender, TemperatureData e)
    {
      Debug.WriteLine($"MainPage.OnTemperatureMeasured {e.Temperature}: {e.IsValid} at {DateTime.Now}");
      if (e.IsValid)
      {
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
        {
          var brush = TemperatureText.Foreground;
          TemperatureText.Text = $"{e.Temperature}°C";
          TemperatureText.Foreground = new SolidColorBrush(Colors.Red);
          await Task.Delay(250);
          TemperatureText.Foreground = brush;
        });
      }
    }
  }
}

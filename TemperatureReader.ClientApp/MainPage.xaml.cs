using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using TemperatureReader.ClientApp.ViewModels;

namespace TemperatureReader.ClientApp
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class MainPage : Page
  {
    public MainPage()
    {
      this.InitializeComponent();
    }

    public MainViewModel ViewModel
    {
      get { return MainViewModel.Instance; }
    }

    public CrashLoggerViewModel CrashViewModel
    {
      get { return CrashLoggerViewModel.Instance; }
    }
  }
}

using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Xaml.Interactivity;
using TemperatureReader.ClientApp.Messages;

namespace TemperatureReader.ClientApp.Behaviors
{
  public class BlinkBehavior : DependencyObject, IBehavior
  {
    private Shape _shape;
    private Brush _originalFillBrush;
    private readonly Brush _blinkBrush = Application.Current.Resources["SystemControlHighlightAccentBrush"] as SolidColorBrush;

    public void Attach(DependencyObject associatedObject)
    {
      AssociatedObject = associatedObject;
      _shape = associatedObject as Shape;
      if (_shape != null)
      {
        _originalFillBrush = _shape.Fill;
        Messenger.Default.Register<DataReceivedMessage>(this, OnDateReceivedMessage);
      }
    }

    private async void OnDateReceivedMessage(DataReceivedMessage mes)
    {
      _shape.Fill = _blinkBrush;

      await Task.Delay(500);
      _shape.Fill = _originalFillBrush;
    }

    public void Detach()
    {
      Messenger.Default.Unregister(this);
      if (_shape != null)
      {
        _shape.Fill = _originalFillBrush;
      }
    }

    public DependencyObject AssociatedObject { get; private set; }
  }
}

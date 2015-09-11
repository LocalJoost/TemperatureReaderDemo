using System.Threading.Tasks;
using Windows.UI.Notifications;
using NotificationsExtensions.Toasts;

namespace TemperatureReader.ClientApp.Helpers
{
  public class Toaster : IMessageDisplayer
  {
    public async Task ShowMessage(string text)
    {
      var content = new ToastContent()
      {

        Visual = new ToastVisual()
        {
          TitleText = new ToastText()
          {
            Text = "Temperature Listener"
          },

          BodyTextLine1 = new ToastText()
          {
            Text = text
          }
        }
      };

      var toast = new ToastNotification(content.GetXml());

      var toastNotifier = ToastNotificationManager.CreateToastNotifier();
      toastNotifier.Show(toast);
    }
  }
}

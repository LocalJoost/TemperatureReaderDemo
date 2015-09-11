using System.Threading.Tasks;

namespace TemperatureReader.ClientApp.Helpers
{
  public interface IMessageDisplayer
  {
    Task ShowMessage(string text);
  }
}
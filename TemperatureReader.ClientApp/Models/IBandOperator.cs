using System.Threading.Tasks;
using TemperatureReader.Shared;

namespace TemperatureReader.ClientApp.Models
{
  public interface IBandOperator
  {
    Task<bool> Start(bool forceFreshClient = false);

    Task SendVibrate();

    Task Stop ();

    Task RemoveTile();

    void HandleNewTemperature(object sender, TemperatureData data);

    bool IsRunning { get; }
  }
}

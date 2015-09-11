using TemperatureReader.Shared;

namespace TemperatureReader.ServiceBus
{
  public class FanSwitchQueueClient : QueueClient<FanSwitchCommand>
  {
    public FanSwitchQueueClient(QueueMode mode = QueueMode.Listen) : base(Settings.FanSwitchQueue, Settings.TemperatureBusConnectionString, mode, Settings.FanSwitchQueueTtl)
    {
    }

  }
}

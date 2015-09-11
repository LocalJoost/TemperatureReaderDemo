using TemperatureReader.Shared;

namespace TemperatureReader.ServiceBus
{
  public class TemperatureQueueClient : QueueClient<TemperatureData>
  {
    public TemperatureQueueClient(QueueMode mode = QueueMode.Listen) :
      base(Settings.TemperatureQueue, 
        Settings.TemperatureBusConnectionString, mode, 
        Settings.TemperatureQueueTtl)
    {
    }
  }
}

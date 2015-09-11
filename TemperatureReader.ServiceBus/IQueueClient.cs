using System;
using System.Threading.Tasks;

namespace TemperatureReader.ServiceBus
{
  public interface IQueueClient<T>
  {
    event EventHandler<T> OnDataReceived;

    Task PostData(T tData);
    Task Start();
    void Stop();
  }
}
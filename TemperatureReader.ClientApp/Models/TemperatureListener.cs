using System;
using System.Threading.Tasks;
using TemperatureReader.ServiceBus;
using TemperatureReader.Shared;

namespace TemperatureReader.ClientApp.Models
{
  public class TemperatureListener : ITemperatureListener
  {
    private readonly TemperatureQueueClient _client;
    public TemperatureListener()
    {
      _client = new TemperatureQueueClient(QueueMode.Listen);
      _client.OnDataReceived += ProcessTemperatureData;
   }

    private void ProcessTemperatureData(object sender, 
      TemperatureData temperatureData)
    {
      OnTemperatureDataReceived?.Invoke(this, temperatureData);
    }

    public async Task Start()
    {
      await _client.Start();
      IsRunning = true;
    }

    public bool IsRunning { get; private set; }

    public void Stop()
    {
      _client.Stop();
      IsRunning = false;
    }

    public event EventHandler<TemperatureData> OnTemperatureDataReceived;
  }
}

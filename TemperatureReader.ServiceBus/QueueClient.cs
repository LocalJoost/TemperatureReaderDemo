using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Messaging;

namespace TemperatureReader.ServiceBus
{
  public class QueueClient<T> : IQueueClient<T>
  {
    private readonly string _queueName;
    private readonly string _connectionString;
    private readonly int _messageTimeToLive;
    private readonly QueueMode _queueMode;
    private Queue _queue;
    
    public QueueClient(string queueName, string connectionString, 
      QueueMode mode = QueueMode.Send, int messageTimeToLive = 10)
    {
      _queueName = queueName;
      _connectionString = connectionString;
      _queueMode = mode;
      _messageTimeToLive = messageTimeToLive;
    }


    public async virtual Task Start()
    {
      try
      {
        var settings = new QueueSettings { DefaultMessageTimeToLive = TimeSpan.FromSeconds(_messageTimeToLive) };
        await Queue.CreateAsync(_queueName, _connectionString, settings);
        Debug.WriteLine($"Queue {_queueName} created");
      }
      catch (Exception)
      {
        Debug.WriteLine($"Queue {_queueName} already exists");
      }

      _queue = new Queue(_queueName, _connectionString);
      if (_queueMode == QueueMode.Listen)
      {
        _queue.OnMessage(message =>
        {
          var data = message.GetBody<T>();
          OnDataReceived?.Invoke(this, data);
        });
      }
    }

    public void Stop()
    {
      if (_queue != null)
      {
        _queue.Dispose();
        _queue = null;
      }
    }

    public virtual async Task PostData(T tData)
    {
      if (this._queueMode == QueueMode.Send)
      {
        await _queue.SendAsync(tData);
      }
      else
      {
        throw new ArgumentException("Cannot send data using a QueueMode.Listen client");
      }
    }

    public event EventHandler<T> OnDataReceived;
  }
}

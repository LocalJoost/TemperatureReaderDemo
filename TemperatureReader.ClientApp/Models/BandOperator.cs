using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Band;
using Microsoft.Band.Notifications;
using Microsoft.Band.Tiles;
using TemperatureReader.ServiceBus;
using TemperatureReader.Shared;

namespace TemperatureReader.ClientApp.Models
{
  public class BandOperator : IBandOperator
  {
    private TemperatureData _lastTemperatureData;
    private DateTimeOffset _lastToggleUse = DateTimeOffset.MinValue;
    private FanStatus _lastFanStatus = FanStatus.Off;
    private bool _isProcessing = false;

    private IBandClient _bandClient;
    private readonly IQueueClient<FanSwitchCommand> _fanStatusPoster;

    public BandOperator(IQueueClient<FanSwitchCommand> fanStatusPoster)
    {
      _fanStatusPoster = fanStatusPoster;
    }

    private async Task<IBandClient> GetBandClient(bool forceFreshClient = false)
    {
      if (_bandClient == null || forceFreshClient)
      {
        TryClearExistingBand();
        _bandClient = await GetNewBandClient();
      }
      try
      {
        var t = _bandClient.NotificationManager;
        Debug.WriteLine($"Notifcation manager succesfull accessed {t}");
      }
      catch (Exception)
      {
        TryClearExistingBand();
        _bandClient = await GetNewBandClient();
      }
      return _bandClient;
    }

    private bool TryClearExistingBand()
    {
      try
      {
        if (_bandClient != null)
        {
          _bandClient.Dispose();
          _bandClient = null;
        }
      }
      catch (Exception)
      {
        return false;
      }
      return true;
    }

    private async Task<IBandClient> GetNewBandClient()
    {
      var retries = 0;
      var waitTime = 500;
      while (retries++ < 10)
      {
        try
        {
          var pairedBands = await BandClientManager.Instance.GetBandsAsync();
          if (pairedBands != null && pairedBands.Any())
          {
            return await BandClientManager.Instance.ConnectAsync(pairedBands.First());
          }
        }
        catch (Exception)
        {
          await Task.Delay(waitTime += 100);
        }
      }
      return null;

    }

    public bool IsRunning { get; private set; }


    public async Task<bool> Start(bool forceFreshClient = false)
    {
      var tilePresent = false;
      var bandClient = await GetBandClient(forceFreshClient);
      if (bandClient != null)
      {
        var currentTiles = await bandClient.TileManager.GetTilesAsync();
        var temperatureTile = currentTiles.FirstOrDefault(p => p.TileId == BandUiDefinitions.TileId);
        if (temperatureTile == null)
        {
          var buc = new BandUiController(bandClient);
          tilePresent = await buc.BuildTile();
        }
        else
        {
          tilePresent = true;
        }

        if (tilePresent)
        {
          await bandClient.TileManager.StartReadingsAsync();
          bandClient.TileManager.TileOpened += TileManager_TileOpened;
          bandClient.TileManager.TileButtonPressed += TileManager_TileButtonPressed;
        }
      }
      IsRunning = tilePresent;
      return tilePresent;
    }

    private async void TileManager_TileButtonPressed(object sender, BandTileEventArgs<IBandTileButtonPressedEvent> e)
    {
      var te = e.TileEvent;
      if (te.TileId == BandUiDefinitions.TileId && te.PageId == BandUiDefinitions.Page1Id && te.ElementId == BandUiDefinitions.ButtonToggleFanId)
      {
        if (!_isProcessing)
        {
          _lastToggleUse = DateTime.UtcNow;
          _isProcessing = true;
          var cmd = new FanSwitchCommand(_lastFanStatus, true);
          Debug.WriteLine($"Sending fan command {cmd.Status}");
          await UpdateFirstPageStatus();
          await _fanStatusPoster.PostData(cmd);
        }
      }
    }

    private async Task UpdateFirstPageStatus()
    {
      var bandClient = await GetBandClient();
      if (bandClient != null)
      {
        var text = GetFanStatusText();
        var buc = new BandUiController(bandClient);
        await buc.SetUiValues($"{_lastTemperatureData.Temperature}°C", text);
      }
    }

    public async Task Stop()
    {
      var bandClient = await GetBandClient();
      if (bandClient != null)
      {
        await bandClient.TileManager.StopReadingsAsync();
        bandClient.TileManager.TileOpened -= TileManager_TileOpened;
        bandClient.TileManager.TileButtonPressed -= TileManager_TileButtonPressed;
        TryClearExistingBand();
      }
      IsRunning = false;
    }

    public async Task RemoveTile()
    {
      await Stop();
      var bandClient = await GetBandClient();
      if (bandClient != null)
      {
        var buc = new BandUiController(_bandClient);
        await buc.RemoveTile();
      }
    }

    public async void HandleNewTemperature(object sender, TemperatureData data)
    {
      Debug.WriteLine($"New temperature data received {data.Temperature} fanstatus = {data.FanStatus}");
      _lastTemperatureData = data;
      _lastTemperatureData.Timestamp = DateTimeOffset.UtcNow;
      if (_lastFanStatus != _lastTemperatureData.FanStatus && _isProcessing)
      {
        _isProcessing = false;
        _lastFanStatus = _lastTemperatureData.FanStatus;
        await UpdateFirstPageStatus();
      }
      else if (_lastToggleUse.IsSecondsAgo(Settings.FanSwitchQueueTtl) && _isProcessing)
      {
        _isProcessing = false;
        _lastFanStatus = _lastTemperatureData.FanStatus;
        await UpdateFirstPageStatus();
      }
      else if (!_isProcessing)
      {
        _lastFanStatus = _lastTemperatureData.FanStatus;
      }
    }

    private async void TileManager_TileOpened(object sender, BandTileEventArgs<IBandTileOpenedEvent> e)
    {
      var bandClient = await GetBandClient();
      if (bandClient != null)
      {
        if (e.TileEvent.TileId == BandUiDefinitions.TileId && _lastTemperatureData != null)
        {
          var buc = new BandUiController(bandClient);
          await buc.SetUiValues(
            _lastTemperatureData.Timestamp.ToLocalTime().ToString("HH:mm:ss"),
            _lastTemperatureData.Timestamp.ToLocalTime().ToString("dd-MM-yyyy"),
             $"{_lastTemperatureData.Temperature}°C",
             GetFanStatusText());
          await bandClient.NotificationManager.VibrateAsync(VibrationType.NotificationOneTone);
        }
      }
    }

    public async Task SendVibrate()
    {
      var bandClient = await GetBandClient();
      if (bandClient != null)
      {
        await bandClient.NotificationManager.VibrateAsync(VibrationType.ThreeToneHigh);
      }
    }

    private string GetFanStatusText()
    {
      return _isProcessing ? "Processing" : _lastTemperatureData.FanStatus == FanStatus.On ? "Stop fan" : "Start fan";
    }
  }
}

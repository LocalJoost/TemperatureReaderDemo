using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Band;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;

namespace TemperatureReader.ClientApp.Models
{
  public class BandUiController
  {
    private readonly IBandClient _bandClient;

    public BandUiController(IBandClient bandClient)
    {
      _bandClient = bandClient;
    }

    public async Task<bool> BuildTile()
    {
      if (_bandClient != null)
      {
        var cap = await _bandClient.TileManager.GetRemainingTileCapacityAsync();
        if (cap > 0)
        {
          var tile = new BandTile(BandUiDefinitions.TileId)
          {
            Name = "Temperature reader",
            TileIcon = await LoadIcon("ms-appx:///Assets/TileIconLarge.png"),
            SmallIcon = await LoadIcon("ms-appx:///Assets/TileIconSmall.png"),
          };

          foreach (var page in BuildTileUi())
          {
            tile.PageLayouts.Add(page);
          }
          await _bandClient.TileManager.AddTileAsync(tile);
          await _bandClient.TileManager.RemovePagesAsync(BandUiDefinitions.TileId);
          await _bandClient.TileManager.SetPagesAsync(BandUiDefinitions.TileId, BuildIntialTileData());
          return true;
        }
      }
      return false;
    }

    public async Task SetUiValues(string timeText, string dateText, string temperature, string buttonText)
    {
      var pageData = BuildTileData(timeText, dateText, temperature, buttonText);
      await _bandClient.TileManager.SetPagesAsync(BandUiDefinitions.TileId, pageData);
    }

    public async Task SetUiValues(string temperature, string buttonText)
    {
      await
        _bandClient.TileManager.SetPagesAsync(BandUiDefinitions.TileId,
           BuildTileDataPage1(temperature, buttonText));
    }

    public async Task RemoveTile()
    {
      var currentTiles = await _bandClient.TileManager.GetTilesAsync();
      var temperatureTile = currentTiles.FirstOrDefault(p => p.TileId == BandUiDefinitions.TileId);
      if (temperatureTile != null)
      {
        await _bandClient.TileManager.RemoveTileAsync(temperatureTile);
      }

    }

    private IEnumerable<PageLayout> BuildTileUi()
    {
      var bandUi = new List<PageLayout>();
      var page1Elements = new List<PageElement>
      {
        new Icon {ElementId = BandUiDefinitions.IconId, Rect = new PageRect(60,10,24,24)},
        new TextBlock  {ElementId = BandUiDefinitions.TextTemperatureId, Rect = new PageRect(90, 10, 50, 40)},
        new TextButton {ElementId = BandUiDefinitions.ButtonToggleFanId, Rect = new PageRect(10, 50, 220, 40), HorizontalAlignment = HorizontalAlignment.Center}
      };
      var firstPanel = new FilledPanel(page1Elements) { Rect = new PageRect(0, 0, 240, 150) };

      var page2Elements = new List<PageElement>
      {
        new TextBlock {ElementId = BandUiDefinitions.TextTimeId, Rect = new PageRect(10, 10, 220, 40)},
        new TextBlock {ElementId = BandUiDefinitions.TextDateId, Rect = new PageRect(10, 58, 220, 40)}
      };
      var secondPanel = new FilledPanel(page2Elements) { Rect = new PageRect(0, 0, 240, 150) };

      bandUi.Add(new PageLayout(firstPanel));
      bandUi.Add(new PageLayout(secondPanel));

      return bandUi;
    }


    private List<PageData> BuildIntialTileData()
    {
      return BuildTileData(
          "not yet set",
          "not yet set",
          "--.-°C",
          "not yet set");
    }

    private async Task<BandIcon> LoadIcon(string uri)
    {
      var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

      using (var fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
      {
        var bitmap = new WriteableBitmap(1, 1);
        await bitmap.SetSourceAsync(fileStream);
        return bitmap.ToBandIcon();
      }
    }

    private List<PageData> BuildTileData(string timeText, string dateText, string temperature, string buttonText)
    {
      var result = new List<PageData>
      {
        BuildTileDataPage2(timeText, dateText),
        BuildTileDataPage1(temperature, buttonText)
      };
      return result;
    }

    private PageData BuildTileDataPage1(string temperature, string buttonText)
    {
      return new PageData(BandUiDefinitions.Page1Id, 0, new IconData(BandUiDefinitions.IconId, 1),
        new TextButtonData(BandUiDefinitions.ButtonToggleFanId, buttonText),
        new TextBlockData(BandUiDefinitions.TextTemperatureId, $": {temperature}"));
    }

    private PageData BuildTileDataPage2(string timeText, string dateText)
    {
      return new PageData(BandUiDefinitions.Page2Id, 1,
        new TextBlockData(BandUiDefinitions.TextTimeId, $"Time: {timeText}"), new TextBlockData(BandUiDefinitions.TextDateId, $"Date: {dateText}"));
    }
  }
}

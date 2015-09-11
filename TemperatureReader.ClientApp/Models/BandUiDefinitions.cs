using System;

namespace TemperatureReader.ClientApp.Models
{
  public static class BandUiDefinitions
  {
    public static readonly Guid TileId = new Guid("567FF10C-E373-4AEC-85B4-EF30EE294174");
    public static readonly Guid Page1Id = new Guid("7E494E17-B498-4610-A6A6-3D0C3AF20226");
    public static readonly Guid Page2Id = new Guid("BB4EB700-A57B-4B8E-983B-72974A98D19E");
    public const short IconId = 1;

    public const short ButtonToggleFanId = 2;
    public const short TextTemperatureId = 3;
    public const short TextTimeId = 4;
    public const short TextDateId = 5;
  }
}

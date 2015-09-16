namespace TemperatureReader.Shared
{
  public static class Settings
  {

    public static readonly string TemperatureBusConnectionString =
         "Endpoint=sb://yournamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=XXXXXXXXXXXXXXXXXYOURKEYHEREXXXXXXXXXXXXXXXX";

    public static readonly string TemperatureQueue = "temperaturedatabus";
    public static readonly int TemperatureQueueTtl = 10;

    public static readonly string FanSwitchQueue = "fanswitchcommandbus";
    public static readonly int FanSwitchQueueTtl = 10;

    public const int ErrorPinId = 12;
    public const int DataPinId = 16;
    public const int LongFlashTime = 250;
    public const int ShortFlashTime = 125;

    public const int AdcCsPinId = 5;
    public const int AdcClkPinId = 6;
    public const int AdcDigitalIoPinId = 13;
    public const int DefaultTemperaturePostingDelay = 5000;
    public const int MaxReadRetry = 10;

    public const int SwitchPinId = 18;
  }
}

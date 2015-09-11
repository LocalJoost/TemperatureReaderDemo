namespace TemperatureReader.Shared
{
  public class FanSwitchCommand
  {

    public FanSwitchCommand()
    {
    }

    public FanSwitchCommand(FanStatus status, bool toggle = false)
    {
      if (toggle)
      {
        Status = status == FanStatus.Off ? FanStatus.On: FanStatus.Off;
      }
      else
      {
        Status = status;
      }
    }
    public FanStatus Status { get; set; }
  }
}

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TemperatureReader.ClientApp.Helpers;

namespace TemperatureReader.ClientApp.ViewModels
{
  public class CrashLoggerViewModel : ViewModelBase
  {
    private readonly IErrorLogger _crashLogger;

    public CrashLoggerViewModel(IErrorLogger crashLogger)
    {
      _crashLogger = crashLogger;
      ErrorLogLines = new ObservableCollection<string>();
    }

    public ObservableCollection<string> ErrorLogLines { get; }


    private bool _isLoggerVisible;

    public bool IsLoggerVisible
    {
      get { return _isLoggerVisible; }
      set { Set(() => IsLoggerVisible, ref _isLoggerVisible, value); }
    }

    public async Task ToggleLog()
    {
      if (!IsLoggerVisible)
      {
        try
        {
          var lines = await _crashLogger.GetLogContents();
          foreach (var line in lines)
          {
            ErrorLogLines.Add(line);
          }
        }
        catch (Exception ex)
        {
          var a = ex;
        }

      }
      else
      {
        ErrorLogLines.Clear();
      }
      IsLoggerVisible = !IsLoggerVisible;
    }

    public async Task ClearLog()
    {
      ErrorLogLines.Clear();
      await _crashLogger.DeleteLog();
    }

    private static CrashLoggerViewModel _instance;

    public static CrashLoggerViewModel Instance
    {
      get { return _instance ?? (_instance = CreateNew()); }
      set { _instance = value; }
    }

    public static CrashLoggerViewModel CreateNew()
    {
      var crashLogger = new ErrorLogger();
      return new CrashLoggerViewModel(crashLogger);
    }
  }
}

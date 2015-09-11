using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace TemperatureReader.ClientApp.Helpers
{
  public class ErrorLogger : IErrorLogger
  {
    private const string LogFile = "crashlog.txt";
    public async void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      await LogException(e.Exception);
    }

    public async Task LogException(Exception ex)
    {
      try
      {
        var folder = ApplicationData.Current.LocalFolder;
        var logFile = await folder.CreateFileAsync(LogFile, CreationCollisionOption.OpenIfExists);
        var lines = new List<string>
        {
          "------------------------",
          "Error at " + DateTimeOffset.Now.ToString("yy-MM-yyyy  HH:mm:ss"),
        };
        if (!string.IsNullOrWhiteSpace(ex.Message)) lines.Add(ex.Message);
        if (!string.IsNullOrWhiteSpace(ex.StackTrace)) lines.Add(ex.StackTrace);
        await FileIO.AppendLinesAsync(logFile, lines);
      }
      catch (Exception)
      {
      }
    }

    public async Task<IList<string>> GetLogContents()
    {
      var result = new List<string>();
      var logFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync(LogFile);
      var file = logFile as StorageFile;
      if (file != null)
      {
        var lines = await FileIO.ReadLinesAsync(file);
        result.AddRange(lines);
      }
      return result;
    }

    public async Task DeleteLog()
    {
      var logFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync(LogFile);
      var file = logFile as StorageFile;
      if (file != null)
      {
        await file.DeleteAsync();
      }
    }
  }
}

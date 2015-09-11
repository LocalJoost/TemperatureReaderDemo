using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace TemperatureReader.ClientApp.Helpers
{
  public interface IErrorLogger
  {
    Task DeleteLog();
    Task<IList<string>> GetLogContents();
    void LogUnhandledException(object sender, UnhandledExceptionEventArgs e);
    Task LogException(Exception ex);
  }
}
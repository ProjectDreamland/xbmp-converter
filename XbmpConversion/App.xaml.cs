using System.Windows;
using XbmpConversion.Models;

namespace XbmpConversion
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void AppStartup(object sender, StartupEventArgs args)
        {
            //  if (!Debugger.IsAttached)
            //    ExceptionHandler.AddGlobalHandlers();

            var mainWindow = new Windows.MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            mainWindow.Show();

            //  Tools.CheckForUpdates();
        }
    }
}
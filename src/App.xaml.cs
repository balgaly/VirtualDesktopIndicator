using System.Windows;
using VirtualDesktopIndicator.Services;

namespace VirtualDesktopIndicator
{
    public partial class App : System.Windows.Application
    {
        private TrayService? _trayService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            if (MainWindow != null)
            {
                MainWindow.WindowState = WindowState.Minimized;
                MainWindow.ShowInTaskbar = false;
                MainWindow.Visibility = Visibility.Hidden;
            }

            _trayService = new TrayService();
            _trayService.Initialize();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayService?.Dispose();
            base.OnExit(e);
        }
    }
}
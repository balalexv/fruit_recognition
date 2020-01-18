using System.Windows;
using SerialPortMonitor.Views;

namespace SerialPortMonitor
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new AppWindow();
            MainWindow.Show();
        }
    }
}

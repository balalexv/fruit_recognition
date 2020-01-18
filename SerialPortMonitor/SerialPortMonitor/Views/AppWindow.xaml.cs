using System;
using System.Windows;
using SerialPortMonitor.ViewModels;

namespace SerialPortMonitor.Views
{
    public partial class AppWindow
    {
        private readonly AppViewModel _context;

        public AppWindow()
        {
            InitializeComponent();

            DataContext = _context = new AppViewModel();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await _context.InitAsync();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _context.Clean();
        }
    }
}

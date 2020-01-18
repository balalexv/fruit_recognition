namespace SerialPortMonitor.ViewModels
{
    internal class SerialPortDataViewModel : ViewModelBase
    {
        public int Number { get; private set; }
        public string FilePath { get; private set; }

        public void Init(int number, string filePath = null)
        {
            Number = number;
            FilePath = filePath ?? "/error.png";
        }
    }
}

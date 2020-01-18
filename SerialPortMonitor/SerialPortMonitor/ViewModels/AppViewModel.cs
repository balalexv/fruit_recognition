using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SerialPortMonitor.Commands;
using SerialPortMonitor.Data;

namespace SerialPortMonitor.ViewModels
{
    internal class AppViewModel : ViewModelBase
    {
        private Thread _availablePortsMonitorThread;
        private Thread _portMonitorThread;
        private ImageData[] _imageDataCollection = new ImageData[0];

        private string[] _availablePorts;
        private string _selectedPort;
        private int _countOfNumbers = 5;
        private int[] _portData;
        private SerialPortDataViewModel[] _dataCollection;
        

        SerialPort port = null;

        int _weight = 0;
        int message_type;
        private int[] answers;
        private bool answer_sended;
        private ImageData _selectedImageData;

        public AppViewModel()
        {
            ClickToItemCommand = new RelayCommand<object>(ClickToItemCommandHandler);
        }

        public ImageData[] ImageDataCollection
        {
            get
            {
                return _imageDataCollection;
            }
            set
            {
                _imageDataCollection = value;
                OnPropertyChanged();
            }
        }

        public ImageData SelectedImageData
        {
            get
            {
                return _selectedImageData;
            }
            set
            {
                _selectedImageData = value;
                OnPropertyChanged();
            }
        }

        public int Weight
        {
            get {
                return _weight;
            }
            set {
                _weight = value;
                OnPropertyChanged();
            }
        }

        public string[] AvailablePorts
        {
            get { return _availablePorts; }
            private set
            {
                if (_availablePorts == value)
                    return;

                if (_availablePorts != null && value != null && _availablePorts.SequenceEqual(value))
                    return;

                _availablePorts = value;
                OnPropertyChanged();

                SelectedPort = _availablePorts.FirstOrDefault();
            }
        }

        public string SelectedPort
        {
            get { return _selectedPort; }
            set
            {
                if (_selectedPort == value)
                    return;

                _selectedPort = value;
                OnPropertyChanged();
                RestartPortMonitorThread();
            }
        }

        public int CountOfNumbers
        {
            get { return _countOfNumbers; }
            set
            {
                if (_countOfNumbers == value)
                    return;

                _countOfNumbers = value;
                OnPropertyChanged();
            }
        }

        public int[] PortData
        {
            get { return _portData; }
            set
            {
                if (_portData == value)
                    return;

                if (_portData != null && value != null && _portData.SequenceEqual(value))
                    return;

                _portData = value;
                OnPropertyChanged();

                GenerateImages();
            }
        }

        public SerialPortDataViewModel[] DataCollection
        {
            get { return _dataCollection; }
            set
            {
                if (_dataCollection == value)
                    return;

                if (_dataCollection != null && value != null && _dataCollection.SequenceEqual(value))
                    return;

                _dataCollection = value;
                OnPropertyChanged();
            }
        }

        public ICommand ClickToItemCommand { get; }

        public Task InitAsync()
        {
            return Task.Run(() =>
            {
                ParseDatabaseFile();

                _availablePortsMonitorThread = new Thread(AvailablePortsMonitorThread) { IsBackground = true };
                _availablePortsMonitorThread.Start();
            });
        }

        public void Clean()
        {
            _availablePortsMonitorThread.Abort();
        }

        private void RestartPortMonitorThread()
        {
            _portMonitorThread?.Abort();
            _portMonitorThread = new Thread(PortMonitorThread) { IsBackground = true };
            _portMonitorThread.Start();
        }

        private void AvailablePortsMonitorThread()
        {
            while (true)
            {
                try
                {
                    AvailablePorts = SerialPort.GetPortNames();
                }
                catch { }
                finally
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void PortMonitorThread()
        {
            
            var isConnected = false;
            while (true)
            {

                try
                {
                    if (SelectedPort == null)
                        continue;

                    if (!isConnected)
                    {
                        port = new SerialPort(SelectedPort, 9600, Parity.None, 8, StopBits.One);
                        port.Open();
                        port.ReadExisting();
                        isConnected = true;
                        continue;
                    }

                    //port.DtrEnable = true;
                    //port.RtsEnable = true;

                    var line = port.ReadLine();
                    Debug.WriteLine(line);
                    ProcessPortData(line);

                }
                catch { }
                finally
                {
                    //port?.Close();

                    Thread.Sleep(100);
                }
            }
        }

        private int generationCounter;
        
        private void ProcessPortData(string data)
        {
            generationCounter++;


            var values = data.Replace("\\r", "").Replace("\\n", "")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(byte.Parse)
                .ToArray();

            if (values.Length == 0)
            {
                PortData = null;
                return;
            }

            if (values.Length == 0 && values[0] == 0)
            {
                PortData = null;
                return;
            }

            message_type = values[0];
            List<int> temp_answers = new List<int>();

            if (message_type == 1 || message_type == 2) 
            {

                answer_sended = false;
                SelectedImageData = null;

                Weight = values[1] * 128 + values[2];

                var portData = new List<int>();
                for (int i = 3, j = 0; i < values.Length - 1; i += 2, j++)
                {

                    var item1 = values[i];
                    var item2 = values[i + 1];

                    portData.Add(item1 * 128 + item2);
                    temp_answers.Add(item1 * 128 + item2);
                }

                answers = temp_answers.ToArray();

                PortData = portData.ToArray();
            }
            else if (message_type == 3)
            {
                if (answers.Length > 0)
                {
                    var imageData = _imageDataCollection.SingleOrDefault(x => x.Number == answers[0]);

                    var item = new SerialPortDataViewModel();

                    if (File.Exists(imageData.Filepath))
                    {
                        item.Init(imageData.Number, imageData.Filepath);
                    }
                    else
                    {
                        item.Init(imageData.Number);
                    }

                    var itemArray = new SerialPortDataViewModel[1];
                    itemArray[0] = item;

                    DataCollection = itemArray;

                }
            }
        }

        private void ParseDatabaseFile()
        {
            var lines = File
                .ReadAllLines("db.txt")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            var imageDataCollection = new List<ImageData>();

            foreach (var line in lines)
            {
                try
                {
                    var line_values = line.Split('\t');
                    var number = Convert.ToInt32(line_values[0]);
                    var filepath = line_values[1];
                    var name = line_values[2];

                    if (Path.GetExtension(filepath) != ".jpg")
                        continue;

                    var imageData = new ImageData
                    {
                        Number = number,
                        Filepath = filepath,
                        Name = name
                    };

                    imageDataCollection.Add(imageData);
                }
                catch { }
            }

            ImageDataCollection = imageDataCollection.ToArray();
        }

        private void GenerateImages()
        {
            if (PortData == null)
            {
                DataCollection = new SerialPortDataViewModel[0];
                return;
            }

            var dataCollection = new List<SerialPortDataViewModel>();
            foreach (var data in PortData.Take(CountOfNumbers))
            {
                var imageData = _imageDataCollection.SingleOrDefault(x => x.Number == data);

                if (imageData == null)
                    continue;

                var item = new SerialPortDataViewModel();

                if (File.Exists(imageData.Filepath))
                {
                    item.Init(imageData.Number, imageData.Filepath);
                }
                else
                {
                    item.Init(imageData.Number);
                }

                dataCollection.Add(item);
            }

            DataCollection = dataCollection.ToArray();
        }

        private void ClickToItemCommandHandler(object parameter)
        {
            Task.Run(() =>
            {

                if (answer_sended == true)
                    return;

                var item = (SerialPortDataViewModel)parameter;
                var itemArray = new SerialPortDataViewModel[1];
                itemArray[0] = item;

                DataCollection = itemArray;

                
                //SerialPort port = null;

                try
                {
                    
                    byte[] a = new byte[2];
                    a[0] = Convert.ToByte(item.Number / 128);
                    a[1] = Convert.ToByte(item.Number - 128 * a[0]);
                    port.Write(a, 0, 2);
                    answer_sended = true;
                }
                catch { }
                finally
                {
                    //port?.Close();
                }
            });
        }

    }
}

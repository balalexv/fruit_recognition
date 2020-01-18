using System.Runtime.Serialization;

namespace SerialPortMonitor.Data
{
    [DataContract]
    public class ImageData
    {
        [DataMember]
        public int Number { get; set; }

        [DataMember]
        public string Filepath { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}

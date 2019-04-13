using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;


namespace MifareModules
{
    public class SerialModuleConfig
    {
        public string PortName = "COM3";
        public int BaudRate = 9600;
        public Parity Parity = Parity.None;
        public int DataBits = 8;
        public StopBits StopBits = StopBits.One;
        public Handshake Handshake = Handshake.XOnXOff;
        public int ReadTimeout = 200;
        public int WriteTimeout = 200;
        public String NewLine = "\r\n";
    }
}

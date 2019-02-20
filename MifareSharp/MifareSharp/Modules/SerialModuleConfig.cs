using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;


namespace MifareSharp
{
    static class SerialModuleConfig
    {
        public static string PortName = "COM3";
        public static int BaudRate = 9600;
        public static Parity Parity = Parity.None;
        public static int DataBits = 8;
        public static StopBits StopBits = StopBits.One;
        public static Handshake Handshake = Handshake.XOnXOff;
        public static int ReadTimeout = 200;
        public static int WriteTimeout = 200;
        public static String NewLine = "\r\n";
    }
}

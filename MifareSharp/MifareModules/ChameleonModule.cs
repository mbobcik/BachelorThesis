using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MifareModules
{
    public class ChameleonModule
    {
        private SerialModule serial;
        public Boolean Verbose { get; set; } = true;
        private const string sendCommand = "SEND";

        public ChameleonModule(SerialModule serial)
        {
            this.serial = serial;
        }

        public void GetCommands()
        {
            serial.WriteLine("HELP");
            var lines = serial.ReadLines(2);
            Console.WriteLine($"**Commands for Chameleon\n{lines[1]}");
        }

        public List<string> ReadToEnd()
        {
            var result = new List<string>();
            var line = "";
            while (!line.Contains("PARITY OK") && !line.Contains("NO DATA"))
            {
                line = serial.ReadLine();
                result.Add(line);
            }
            return result;
        }

        public string GetUid()
        {
            return serial.WriteAndGetResult("GETUID", 2)[1];
        }

        public void Send(string message)
        {
            string length = CalculateMessageLength(message);
            serial.WriteLine(String.Format("{0} {1} {2}", sendCommand, length, message));
        }

        public void TurnElectromagnetic(Field value)
        {
            serial.WriteAndTrashResult($"FIELD={(int)value}");
        }

        private string CalculateMessageLength(string message)
        {
            var lengthInBits = message.Length * 4;
            if (lengthInBits == 8)
                return 7.ToString("X4");
            return lengthInBits.ToString("X4");
        }
    }

    public enum Field
    {
        Off = 0,
        On = 1
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MifareModules
{
    public class ChameleonModule
    {
        //TODO add logging!!
        private SerialModule serial;
        public Boolean Verbose { get; set; } = true;
        public string Role = "";
        private const string sendCommand = "SEND";

        public ChameleonModule(SerialModule serial)
        {
            this.serial = serial;
        }

        public ChameleonModule(SerialModule serial, string role)
        {
            this.serial = serial;
            this.Role = role;
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
            string toSend = String.Format("{0} {1} {2}", sendCommand, length, message);
            serial.WriteLine(toSend);
            Log(toSend);
        }

        public string SendWithAnswer(string message)
        {

            this.Send(message);
            string answer = this.ReadToEnd()[1];
            Log("< Received " + answer);
            return answer;
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

        private void Log(string message)
        {
            if (Verbose)
            {
                Console.WriteLine(Role + " - " + message);
            }
        }

    }

    public enum Field
    {
        Off = 0,
        On = 1
    }
}

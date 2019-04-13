using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace MifareModules
{
    public class SerialModule
    {
        private SerialPort serialPort;
        public Boolean Verbose { get; set; } = true;

        public SerialModule(SerialModuleConfig config)
        {
            serialPort = new SerialPort()
            {
                PortName = config.PortName,
                BaudRate = config.BaudRate,
                Parity = config.Parity,
                DataBits = config.DataBits,
                StopBits = config.StopBits,
                Handshake = config.Handshake,
                ReadTimeout = config.ReadTimeout,
                WriteTimeout = config.WriteTimeout,
                NewLine = config.NewLine
            };

            serialPort.Open();
        }

        public void Open()
        {
            serialPort.Open();
        }

        public void WriteLine(string message)
        {
            if (Verbose)
            {
                Console.WriteLine($"** Sending: {message} **");
            }
            serialPort.WriteLine(message);
        }

        public string ReadLine()
        {
            string result ="";
            try
            {
                result = serialPort.ReadLine();
                
                return result;
            }
            catch (TimeoutException)
            {
                return result;
            }
            catch
            {
                throw;
            }
        }

        public List<string> ReadLines(int numberOfLines)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < numberOfLines; i++)
            {
                var line = ReadLine();
                if (Verbose)
                {
                    Console.WriteLine($"\t{line}");
                }
                result.Add(line);
            }
            return result;
        }

        public List<string> WriteAndGetResult(string message, int numberOfLines = 1)
        {
            WriteLine(message);
            return ReadLines(numberOfLines);
        }

        public void WriteAndTrashResult(string message, int numberOfLines = 1)
        {
            WriteLine(message);
            for (int i = 0; i < numberOfLines; i++)
            {
                ReadLine();
            }
        }

        public void Close()
        {
            serialPort.Close();
        }
    }
}

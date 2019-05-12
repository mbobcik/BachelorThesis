using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using MifareModules;

namespace CryptoAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var parsedArgs = argParse(args);

            int repeat = int.Parse(parsedArgs["repeat"]);
            int waitTime = int.Parse(parsedArgs["waitTime"]);

            Stopwatch sw = new Stopwatch();

            SerialModuleConfig chameleon1Config = new SerialModuleConfig()
            {
                PortName = parsedArgs["portName"]
            };
            SerialModule serial1 = new SerialModule(chameleon1Config)
            {
                Verbose = false
            };

            ChameleonModule chameleon1 = new ChameleonModule(serial1)
            {
                Verbose = false,
            };
            serial1.WriteAndGetResult("Config=ISO14443A_READER"); // config chameleon as reader
            CardModule card = new CardModule(chameleon1)
            {
                Verbose = false
            };

            List<string> nonces = new List<string>(repeat);
            for (int i = 0; i < repeat; i++)
            {
                sw.Start();
                chameleon1.TurnElectromagnetic(Field.On);
                Sleep(waitTime + 2);

                card.ReqA();
                Sleep(waitTime);
                try
                {
                    string uid = card.Select();
                }
                catch (NoDataException)
                {
                    Console.WriteLine("NO DATA");
                    chameleon1.TurnElectromagnetic(Field.Off);
                    continue;
                }
                Sleep(waitTime);
                card.SelectUid(card.UID);
                string nonce = card.AuthenticateForBlock("00");


                chameleon1.TurnElectromagnetic(Field.Off);
                sw.Stop();
                if (!string.Equals(nonce, "NO DATA"))
                {
                    nonces.Add(nonce);
                }
                Console.WriteLine("nonce: {0} Elapsed: {1}ms ({2} ticks)", nonce, sw.ElapsedMilliseconds, sw.ElapsedTicks);
                sw.Reset();
                Sleep(waitTime + 2);
            }

            var duplicateNonces = nonces.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key);
            if (duplicateNonces.Count() > 0)
            {
                Console.WriteLine("Duplicate nonces:");
                foreach (string nonce in duplicateNonces)
                {
                    Console.WriteLine("\t" + nonce);
                }
            }
            else
            {
                Console.WriteLine("There were no duplicate nonces.");
            }
           

            if (Debugger.IsAttached)
                Console.ReadLine();
        }

        private static void Sleep(int ms)
        {
            if (ms > 0)
            {
                Thread.Sleep(ms);
            }
        }

        private static Dictionary<string, string> argParse(string[] args)
        {
            var result = new Dictionary<string, string>()
            {
                {"portName", "COM3"},
                {"waitTime", "2" },
                {"repeat", "100" }
            };

            if (args == null || args.Count() <= 0)
                return result;

            if (Array.IndexOf(args, "-c") >= 0)
            {
                result["portName"] = args[Array.IndexOf(args, "-c") + 1];
            }
            if (Array.IndexOf(args, "-w") >= 0)
            {
                result["waitTime"] = args[Array.IndexOf(args, "-w") + 1];
            }
            if (Array.IndexOf(args, "-r") >= 0)
            {
                result["repeat"] = args[Array.IndexOf(args, "-r") + 1];
            }
            if (args.Contains("-h") || args.Contains("\\?"))
            {
                Console.WriteLine(@"This is Help...
-p portName default:COM3
-w waiting Time between messages default:2
-r number of attempts default:100");
                Environment.Exit(0);
            }
            return result;
        }

    }
}

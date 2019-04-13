using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using MifareModules;

namespace CryptoAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            int repeat = 200;
            int waitTime = 3;

            Stopwatch sw = new Stopwatch();

            SerialModuleConfig chameleon1Config = new SerialModuleConfig();
            SerialModule serial1 = new SerialModule(chameleon1Config)
            {
                Verbose = false
            };

            ChameleonModule chameleon1 = new ChameleonModule(serial1);
            CardModule card = new CardModule(chameleon1) { Verbose = true };

            List<string> nonces = new List<string>(repeat);

            for (int i = 0; i < repeat; i++)
            {
                sw.Start();
                chameleon1.TurnElectromagnetic(Field.On);
                Thread.Sleep(waitTime + 2);

                card.ReqA();
                Thread.Sleep(waitTime);
                try
                {
                    string uid = card.Select();
                }
                catch (NoDataException)
                {
                    Console.WriteLine("NO DATA");
                    continue;
                }
                Thread.Sleep(waitTime);
                card.SelectUid(card.UID);
                string nonce = card.AuthenticateForBlock("00");


                chameleon1.TurnElectromagnetic(Field.Off);
                sw.Stop();
                nonces.Add(nonce);
                Console.WriteLine("nonce: {0} Elapsed: {1}ms", nonce, sw.ElapsedMilliseconds);
                sw.Reset();
                Thread.Sleep(waitTime + 2);
            }

            var duplicateNonces = nonces.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key);
            if(duplicateNonces.Count() > 0)
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
        }
    }
}

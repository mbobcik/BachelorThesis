using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace MifareSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            SerialModule s = new SerialModule()
            {
                Verbose = false
            };
            ChameleonModule m = new ChameleonModule(s);
            CardModule card = new CardModule(m) { Verbose = true};
            int waitTime = 3;
            for (int i = 0; i < 20; i++)
            {
                sw.Start();
                m.TurnElectromagnetic(Field.On);
                Thread.Sleep(waitTime+2);

                card.ReqA();
                Thread.Sleep(waitTime);
                try
                {
                    string uid = card.Select();
                } catch (NoDataException)
                {
                    Console.WriteLine("NO DATA");
                    continue;
                }
                Thread.Sleep(waitTime);
                card.SelectUid(card.UID);
                string nonce = card.AuthenticateForBlock("00");


                m.TurnElectromagnetic(Field.Off);
                sw.Stop();
                Console.WriteLine("nonce: {0} Elapsed: {1}ms", nonce, sw.ElapsedMilliseconds);
                sw.Reset();
                Thread.Sleep(waitTime+2);
            }

            Console.ReadLine();

        }
    }
}

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
            Stopwatch sw = new Stopwatch();

            SerialModuleConfig chameleon1Config = new SerialModuleConfig();
            SerialModule serial1 = new SerialModule(chameleon1Config)
            {
                Verbose = false
            };

            ChameleonModule chameleon1 = new ChameleonModule(serial1);
            CardModule card = new CardModule(chameleon1) { Verbose = true };


            SerialModuleConfig chameleon2config = new SerialModuleConfig()
            {
                PortName = "COM4"
            };
            SerialModule serial2 = new SerialModule(chameleon2config);
            ChameleonModule chameleon2 = new ChameleonModule(serial2);


            Console.ReadLine();

        }
    }
}

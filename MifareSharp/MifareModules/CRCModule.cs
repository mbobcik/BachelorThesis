using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.KitchenSink.CRC;


namespace MifareModules
{
    public class CRCModule
    {
        private CRC16 crc;
        public string LastHash { get; set; }
        private string lastHashed;

        public CRCModule()
        {
            crc = new CRC16(new CRC16.Definition()
            {
                TruncatedPolynomial = 0x1021,
                Initializer = 0xc6c6,
                FinalXorValue = 0x0,
                ReverseDataBytes = true,
                ReverseResultBeforeFinalXor = true
            });
            crc.Initialize();
            lastHashed = string.Empty;
        }

        public string Hash(string value)
        {
            if (lastHashed == String.Empty || lastHashed != value)
            {
                lastHashed = value;
                var resultBytes = crc.ComputeHash(HexToByteArray.Convert(lastHashed));
                LastHash = HexToByteArray.BytesToHex(resultBytes);
            }
            return LastHash;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MifareModules
{
    public class CardModule
    {
        private ChameleonModule cm;
        private CRCModule crc;
        public string UID { get; set; }
        public Boolean Verbose { get; set; } = false;

        public CardModule(ChameleonModule cm)
        {
            this.cm = cm;
            crc = new CRCModule();
            UID = "";
        }

        public string ReqA()
        {
            Log(">ReqA");
            cm.Send(Get(ISOCode.ReqA));
            var result = cm.ReadToEnd()[1];
            Log($"<{result}");
            return result;
        }

        public string Select()
        {
            Log(">Select");
            cm.Send(Get(ISOCode.Select));
            var result = cm.ReadToEnd()[1];
            Log($"<{result}");
            if (result == "NO DATA")
            {
                throw new NoDataException();
            }
            if (UID == "")
            {
                UID = result;
            }


            return UID; // UID of Card
        }

        public string SelectUid(string uid)
        {
            string message = Get(ISOCode.SelectUid) + uid;
            message = message + crc.Hash(message);
            Log($">Select({uid})");
            cm.Send(message);
            var result = cm.ReadToEnd()[1];
            Log("<"+ (result.Equals("08B6DD") ? "Ack" : "Nack" )+ $" ({result})");
            return result;// ACK code
        }

        /// <summary>
        /// Sends authentication request for specific block on card
        /// </summary>
        /// <param name="blockNumber">block hex number in string</param>
        /// <returns>nonce sent by card</returns>
        public string AuthenticateForBlock(string blockNumber)
        {
            Log($">Auth(block 0x{blockNumber})");
            string message = Get(ISOCode.Auth) + blockNumber;
            message = message + crc.Hash(message);
            cm.Send(message);
            var result = cm.ReadToEnd()[1];
            Log($"<nT = {result}");
            return result; // nonce
        }

        private void Log(string Message)
        {
            if (Verbose)
                Console.WriteLine(Message);
        }

        private string Get(ISOCode code)
        {
            return ((int)code).ToString();
        }
    }

    public enum ISOCode
    {
        ReqA = 26,
        Select = 9320,
        SelectUid = 9370,
        Auth = 60
    }


    [Serializable]
    public class NoDataException : Exception
    {
        public NoDataException() { }
        public NoDataException(string message) : base(message) { }
        public NoDataException(string message, Exception inner) : base(message, inner) { }
        protected NoDataException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

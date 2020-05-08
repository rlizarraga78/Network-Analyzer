using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Network_Analyzer.PacketData
{
    public class PacketUdp
    {
        private ushort _SourcePort;
        private ushort _DestinationPort;
        private ushort _Length;
        private short _Checksum;
                                                    
        private byte[] _Data = new byte[4096];  

        public PacketUdp(byte [] byBuffer, int nReceived)
        {
                MemoryStream memoryStream = null;
                BinaryReader binaryReader = null;

                try
                {
                    memoryStream = new MemoryStream(byBuffer, 0, nReceived);
                    binaryReader = new BinaryReader(memoryStream);

                    _SourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    _DestinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    _Length = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    _Checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                    Array.Copy(byBuffer, 8, _Data, 0, nReceived - 8);
                }
                catch (Exception) { }

                finally
                {
                    binaryReader.Close();
                    memoryStream.Close();
                }
        }

        public string SourcePort
        {
            get {return _SourcePort.ToString(); }
        }

        public string DestinationPort
        {
            get { return _DestinationPort.ToString(); }
        }

        public string Length
        {
            get { return _Length.ToString(); }
        }

        public string Checksum
        {
            get { return "0x" + _Checksum.ToString("x"); }
        }

        public byte[] Data
        {
            get { return _Data; }
        }
    }
}

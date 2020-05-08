using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Network_Analyzer.PacketData
{
    public class PacketIcmp
    {
        private byte _Type;
        private byte _Code;
        private short _Checksum;
        private ushort _Identifier;
        private ushort _SequenceNUmber;
        private int _AddressMask;

        public PacketIcmp(byte[] buffer, int iReceived)
        {
            MemoryStream memoryStream = null;
            BinaryReader binaryReader = null;

            try
            {

                memoryStream = new MemoryStream(buffer, 0, iReceived);
                binaryReader = new BinaryReader(memoryStream);
                _Type = binaryReader.ReadByte();

                _Code = binaryReader.ReadByte();

                _Checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                _Identifier = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                _SequenceNUmber = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                _AddressMask = IPAddress.NetworkToHostOrder(binaryReader.ReadInt32());
            }
            //catch (IOException e)
            //{
                //temp
            //}
            finally
            {
                binaryReader.Close();
                memoryStream.Close();
            }
        }

        public string Type
        {
            get{return _Type.ToString();}
        }

        public string Code
        {
            get { return _Code.ToString(); }
        }
        public string Checksum
        {
            get { return "0x" + _Checksum.ToString("X"); }
        }
        public string Identifier
        {
            get { return _Identifier.ToString(); }
        }
        public string SequenceNUmber
        {
            get { return _SequenceNUmber.ToString(); }
        }
        public string AddressMask
        {
            get { return "0x" + _AddressMask.ToString("X"); }
        }
    }
}

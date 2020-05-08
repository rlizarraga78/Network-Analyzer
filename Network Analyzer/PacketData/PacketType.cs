using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network_Analyzer.PacketData
{
    class PacketType
    {
        PacketIP _ip;
        PacketTcp _tcp;
        PacketUdp _udp;
        PacketIcmp _icmp;

        public PacketType(PacketIP ip)
        {
            _ip = ip;
        }
        public PacketType(PacketIP ip, PacketTcp tcp)
        {
            _ip = ip;
            _tcp = tcp;
        }
        public PacketType(PacketIP ip, PacketUdp udp)
        {
            _ip = ip;
            _udp = udp;
        }
        public PacketType(PacketIP ip, PacketIcmp icmp)
        {
            _ip = ip;
            _icmp = icmp;
        }

        public PacketIP IP
        {
            get { return _ip; }
        }
        public PacketTcp TCP
        {
            get { return _tcp; }
        }
        public PacketUdp UDP
        {
            get { return _udp; }
        }
        public PacketIcmp ICMP
        {
            get { return _icmp; }
        }
    }
}

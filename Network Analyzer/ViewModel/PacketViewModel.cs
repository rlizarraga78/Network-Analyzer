using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network_Analyzer.ViewModel
{
    public class PacketViewModel
    {
        private ObservableCollection<PacketMessages> _messages = new ObservableCollection<PacketMessages>() { };

        public ObservableCollection<PacketMessages> messages { get { return _messages; } }
    }
}

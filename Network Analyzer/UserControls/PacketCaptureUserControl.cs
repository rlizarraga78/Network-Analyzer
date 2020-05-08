using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Network_Analyzer.ViewModel;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Network_Analyzer.PacketData;
using System.IO;
using System.Windows.Threading;

namespace Network_Analyzer.UserControls
{
    public partial class PacketCaptureUserControl : UserControl
    {
        PacketViewModel PacketTracker = new PacketViewModel();
        IPAddress[] _ip;
        Socket _socket;
        Thread _processReceived = null;
        byte[] _in = new byte[4] { 1, 0, 0, 0 };
        byte[] _out = new byte[4];
        byte[] _buffer = new byte[8192];

        private bool Stopped = false;
        private int Mbyte = 100000;
        private decimal DataUsage;

        //Creat path to data
        private string datapath = System.IO.Path.Combine(Application.UserAppDataPath, "Data");
        private string filepath;
        private string path;
        private string generalDataPath;
        
        //Hour and data usage for a particular hour
        private int currentHour;
        private decimal currentHourDataUsage;

        public PacketCaptureUserControl()
        {
            InitializeComponent();

            lvMessages.DoubleBuffered(true);

            this.chart1.Text = "chart1";
            this.chart1.Series["MB"].Points.AddXY("12am", 0);
            this.chart1.Series["MB"].Points.AddXY("1am", 0);
            this.chart1.Series["MB"].Points.AddXY("2am", 0);
            this.chart1.Series["MB"].Points.AddXY("3am", 0);
            this.chart1.Series["MB"].Points.AddXY("4am", 0);
            this.chart1.Series["MB"].Points.AddXY("5am", 0);
            this.chart1.Series["MB"].Points.AddXY("6am", 0);
            this.chart1.Series["MB"].Points.AddXY("7am", 0);
            this.chart1.Series["MB"].Points.AddXY("8am", 0);
            this.chart1.Series["MB"].Points.AddXY("9am", 0);
            this.chart1.Series["MB"].Points.AddXY("10am", 0);
            this.chart1.Series["MB"].Points.AddXY("11am", 0);
            this.chart1.Series["MB"].Points.AddXY("12pm", 0);
            this.chart1.Series["MB"].Points.AddXY("1pm", 0);
            this.chart1.Series["MB"].Points.AddXY("2pm", 0);
            this.chart1.Series["MB"].Points.AddXY("3pm", 0);
            this.chart1.Series["MB"].Points.AddXY("4pm", 0);
            this.chart1.Series["MB"].Points.AddXY("5pm", 0);
            this.chart1.Series["MB"].Points.AddXY("6pm", 0);
            this.chart1.Series["MB"].Points.AddXY("7pm", 0);
            this.chart1.Series["MB"].Points.AddXY("8pm", 0);
            this.chart1.Series["MB"].Points.AddXY("9pm", 0);
            this.chart1.Series["MB"].Points.AddXY("10pm", 0);
            this.chart1.Series["MB"].Points.AddXY("11pm", 0);
            this.chart1.ChartAreas[0].AxisX.LabelStyle.Angle = -90;
            this.chart1.ChartAreas[0].AxisX.LabelStyle.Interval = 1;

            button1.Enabled = true;

            currentHour = DateTime.Now.Hour;
            currentHourDataUsage = 0;
            createGeneralFolder();
            updateDataUsage();
            createAndUpdateDataFolder(0);
            updateChart();

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            

            if (Stopped)
            {
                Stopped = false;
                lblDataUsage.Text = "0";
                lvMessages.Items.Clear();
                System.Diagnostics.Process.Start("ipconfig", "/renew");
            }
            StartCapturing();
            btnStart.Enabled = false;
            btnStop.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            currentHourDataUsage = 0;
            DataUsage = 0;
            File.WriteAllText(generalDataPath + "\\totaldata.txt", "0.0");
            lblDataUsage.Text = "0.0";            

            for (int i = 0; i < 24; i++)
            {
                string tempPath = path + "\\" + i.ToString() + ".txt";
                if (File.Exists(tempPath))
                {
                    File.WriteAllText(tempPath, "0.0");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            updateChart();
            chart1.Refresh();
            chart1.ResetAutoValues();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Stopped = true;
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            updateChart();
            chart1.Refresh();
            chart1.ResetAutoValues();
        }

        private void StartCapturing()
        {
            _ip = Dns.GetHostAddresses(Dns.GetHostName());

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            _ip[1] = endPoint.Address;

            lblUserIP.Text = _ip[1].ToString();

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
                _socket.Bind(new IPEndPoint(_ip[1], 0));
                _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
                _socket.IOControl(IOControlCode.ReceiveAll, _in, _out);

                
                _processReceived = new Thread(ProcessReceived);
                _processReceived.Start();

         
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
        }

        private void ProcessReceived()
        {
            while (!Stopped)
            {
                int _bytes = _socket.Receive(_buffer, 0, _buffer.Length, SocketFlags.None);


                if (_bytes > 0 && _buffer.Length > 0)
                {
                    ConvertData(_buffer, _bytes);
                }
                Array.Clear(_buffer, 0, _buffer.Length);
                
            }
        }

        private void ConvertData(byte[] bfr, int rcvd)
        {
            PacketType pkgData;
            PacketIP packet = new PacketIP(bfr, rcvd);
            ListViewItem item = new ListViewItem();

            this.BeginInvoke(new Action(() =>
            {
                decimal newDataUsage = decimal.Parse(packet.TotalLength) / Mbyte;
                DataUsage += newDataUsage;
                lblDataUsage.Text = DataUsage.ToString("F2");
                File.WriteAllText(generalDataPath + "\\totaldata.txt", DataUsage.ToString());
                createAndUpdateDataFolder(newDataUsage);
            }));

            
            item.Text = DateTime.Now.ToString("HH:mm:ss:") + DateTime.Now.Millisecond.ToString();
            item.SubItems.Add(packet.SourceAddress.ToString());
            item.SubItems.Add("0");
            item.SubItems.Add(packet.DestinationAddress.ToString());
            item.SubItems.Add("0");
            item.SubItems.Add(packet.Protocol);
            item.SubItems.Add(packet.TotalLength);

            switch (packet.Protocol)
            {
                case "TCP":
                    PacketTcp tcp = new PacketTcp(packet.Data, packet.MessageLength);
                    item.SubItems[2].Text = tcp.SourcePort;
                    item.SubItems[4].Text = tcp.DestinationPort;

                    pkgData = new PacketType(packet, tcp);
                    break;
                case "UDP":
                    PacketUdp udp = new PacketUdp(packet.Data, packet.MessageLength);
                    item.SubItems[2].Text = udp.SourcePort;
                    item.SubItems[4].Text = udp.DestinationPort;

                    pkgData = new PacketType(packet, udp);
                    break;
                case "ICMP":
                    PacketIcmp icmp = new PacketIcmp(packet.Data, packet.MessageLength);
                    item.SubItems.Add("N/A");

                    pkgData = new PacketType(packet, icmp);
                    break;
                case "Unknown":
                    pkgData = new PacketType(packet);
                    break;
            }

            if (decimal.Parse(lblDataUsage.Text) > nudDataLimit.Value)
            {
                System.Diagnostics.Process.Start("ipconfig", "/release");
                Stopped = true;
                MessageBox.Show("You've reached your data limit!");
            }
            else
            {
                //_pkgBuffer.Add(strKey, pkgInfo);
                this.BeginInvoke(new Action(() =>
                {
                    lvMessages.Items.Add(item);
                }));
            }
        }

        private void createGeneralFolder()
        {
            generalDataPath = datapath + "\\general";
            if (!File.Exists(generalDataPath))
            {
                //Create directory for general info
                Directory.CreateDirectory(generalDataPath);
            }

            if (!File.Exists(generalDataPath + "\\totaldata.txt"))
            {
                //Create filename for total data usage
                string totalFile = Path.Combine(generalDataPath, "totaldata.txt");
                var myFile = File.CreateText(totalFile);
                myFile.Close();
            }
        }

        private void updateDataUsage()
        {
            //Update hourly data usage
            string totalDatapath = datapath + "\\general\\totaldata.txt";
            string tempTxt = File.ReadAllText(totalDatapath);     //Get current data length from txt file
            if (string.IsNullOrEmpty(tempTxt))
            {
                tempTxt = "0.0";
                File.WriteAllText(totalDatapath, tempTxt);
            }
            DataUsage = decimal.Parse(tempTxt);
            lblDataUsage.Text = DataUsage.ToString("F2");
        }

        private void createAndUpdateDataFolder(decimal newDataUsage)
        {
            //Create directory if needed
            string date = DateTime.Now.ToString("MM-dd-yyyy");
            path = datapath + "\\" + date;

            if (!File.Exists(path))
            {
                Directory.CreateDirectory(path);            //Create directory for the day if it doesnt exist
            }

            //Create filename based on hour and write data
            int hour = DateTime.Now.Hour;
            string filename = hour.ToString() + ".txt";
            filepath = Path.Combine(path, filename);

            if (!File.Exists(filepath))
            {
                var myFile = File.CreateText(filepath);
                myFile.Close();
            }
            
            string tem = File.ReadAllText(filepath);
            if (string.IsNullOrEmpty(tem)) { tem = "0.0"; }
            currentHourDataUsage = decimal.Parse(tem);

            if (hour == currentHour)
            {
                currentHourDataUsage += newDataUsage;
            }
            else
            {
                currentHourDataUsage = 0;
                currentHour = DateTime.Now.Hour;

                //Update hourly data usage
                string tempTxt = File.ReadAllText(filepath);     //Get current data length from txt file
                if (string.IsNullOrEmpty(tempTxt)) tempTxt = "0.0";
                currentHourDataUsage = decimal.Parse(tempTxt);

            }
            //Write to file
            File.WriteAllText(filepath, currentHourDataUsage.ToString());

        }

        private void updateChart()
        {
            for (int i = 0; i < 24; i++)
            {
                string tempPath = path + "\\" + i.ToString() + ".txt";
                if (File.Exists(tempPath))
                {
                    string tempTxt = File.ReadAllText(tempPath);
                    chart1.Series["MB"].Points[i].SetValueY(decimal.Parse(tempTxt));
                }
                else
                    chart1.Series["MB"].Points[i].SetValueY(0);
            }
        }

        
    }
}

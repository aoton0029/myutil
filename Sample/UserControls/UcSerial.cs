using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sample
{
    public partial class UcSerial : UserControl
    {
        public UcSerial()
        {
            InitializeComponent();
        }

        //public void SetSerialPort(string portName, int baudRate)
        //{
        //    Console.WriteLine($"SetSerialPort {portName} {baudRate}");
        //    SerialComm serialComm = new SerialComm(portName, baudRate);

        //    txtRecv.AppendText(serialComm.OpenSend(txtSend.Text) + Environment.NewLine);
        //}

        //private void UcSerial_Load(object sender, EventArgs e)
        //{
        //    Console.WriteLine("UcSerial_Load");
        //    cmbPort.DisplayMember = nameof(PortInfo.DeviceName);
        //    cmbPort.ValueMember = nameof(PortInfo.PortName);
        //    cmbPort.DataSource = SerialComm.GetPortInfos();
        //}

        //private void btnSend_Click(object sender, EventArgs e)
        //{
        //    try 
        //    {
        //        SetSerialPort(cmbPort.SelectedValue.ToString(), 9600);
        //    }
        //    catch(Exception ex)
        //    {
        //        txtRecv.AppendText(ex.Message + Environment.NewLine);
        //    }
        //}
    }
}

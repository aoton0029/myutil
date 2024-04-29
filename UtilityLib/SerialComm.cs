using System.IO.Ports;
using System.Management;

namespace UtilityLib
{
    public class PortInfo
    {
        public string PortName { get; set; } = "";
        public string DeviceName { get; set; } = "";
    }

    public class SerialComm
    {
        private SerialPort serialPort;

        public delegate void DataReceivedEventHandler(string message);
        public event DataReceivedEventHandler DataReceived;

        public SerialComm(string portName, int baudRate, bool useReceiveEvent = false)
        {
            serialPort = new SerialPort(portName, baudRate);
            if (useReceiveEvent)
            {
                serialPort.DataReceived += SerialPort_DataReceived;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("Data received event fired.");
            SerialPort obj = (SerialPort)sender;
            if (obj.IsOpen)
            {
                string data = obj.ReadLine();
                DataReceived?.Invoke(data);
            }
        }

        public bool Open()
        {
            try
            {
                serialPort.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening serial port: " + ex.Message);
                return false;
            }
        }

        public bool SendData(string data)
        {
            try
            {
                serialPort.WriteLine(data);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending data: " + ex.Message);
                return false;
            }
        }

        public string OpenSend(string data)
        {
            try
            {
                serialPort.Open();
                serialPort.ReadTimeout = 1000; // 1秒の読み取りタイムアウトを設定
                serialPort.WriteTimeout = 1000; // 1秒の書き込みタイムアウトを設定

                serialPort.WriteLine(data);

                string response = serialPort.ReadLine();
                return response;
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine("Timeout occurred: " + ex.Message);
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return "";
            }
            finally
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }

        }


        public void Close()
        {
            serialPort.Close();
        }

        public static List<PortInfo> GetPortInfos()
        {
            List<PortInfo> portInfoList = new List<PortInfo>();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                string deviceName = "";
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(" + port + @")'"))
                {
                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        deviceName = queryObj["Caption"].ToString();
                        break; // 最初に見つかったデバイス名を取得するためループを終了
                    }
                }

                PortInfo portInfo = new PortInfo
                {
                    PortName = port,
                    DeviceName = deviceName
                };
                portInfoList.Add(portInfo);
            }
            return portInfoList;
        }

    }
}

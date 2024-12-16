using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Input
{
    public class RawInputHandler : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public ushort UsagePage;
            public ushort Usage;
            public uint Flags;
            public IntPtr Target;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTHEADER
        {
            public uint Type;
            public uint Size;
            public IntPtr Device;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RAWINPUT
        {
            [FieldOffset(0)]
            public RAWINPUTHEADER Header;
        }

        private const int RIM_TYPEHID = 2;
        private const int WM_INPUT = 0x00FF;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        public event Action<string> OnRawInputReceived;

        public RawInputHandler(IntPtr windowHandle)
        {
            RegisterDevices(windowHandle);
        }

        private void RegisterDevices(IntPtr windowHandle)
        {
            RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[1];
            devices[0].UsagePage = 0x0D; // HID_USAGE_PAGE_DIGITIZER
            devices[0].Usage = 0x04;     // HID_USAGE_DIGITIZER_TOUCH_SCREEN
            devices[0].Flags = 0x00000000; // No special flags
            devices[0].Target = windowHandle;

            if (!RegisterRawInputDevices(devices, (uint)devices.Length, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                throw new Exception("Failed to register raw input devices.");
            }
        }

        public void ProcessMessage(IntPtr lParam)
        {
            uint size = 0;
            GetRawInputData(lParam, 0x10000003, IntPtr.Zero, ref size, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));

            IntPtr buffer = Marshal.AllocHGlobal((int)size);
            try
            {
                if (GetRawInputData(lParam, 0x10000003, buffer, ref size, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) == size)
                {
                    RAWINPUT rawInput = Marshal.PtrToStructure<RAWINPUT>(buffer);
                    ProcessRawInput(rawInput);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private void ProcessRawInput(RAWINPUT rawInput)
        {
            if (rawInput.Header.Type == RIM_TYPEHID)
            {
                OnRawInputReceived?.Invoke("Touch event detected!");
            }
        }

        public void Dispose()
        {
            // No resources to release
        }
    }

    public class MainForm : Form
    {
        private RawInputHandler _rawInputHandler;

        public MainForm()
        {
            Text = "Raw Input Monitor";
            Width = 800;
            Height = 600;

            // RawInputHandlerの初期化
            _rawInputHandler = new RawInputHandler(this.Handle);
            _rawInputHandler.OnRawInputReceived += HandleRawInput;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_INPUT = 0x00FF;

            if (m.Msg == WM_INPUT)
            {
                // RawInputHandlerにメッセージを渡す
                _rawInputHandler.ProcessMessage(m.LParam);
            }

            base.WndProc(ref m);
        }

        private void HandleRawInput(string message)
        {
            Console.WriteLine(message);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _rawInputHandler.Dispose();
        }
    }
}

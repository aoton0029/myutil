using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNet45
{
    public partial class Form1 : Form
    {
        // Raw Input structures
        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public ushort UsagePage;
            public ushort Usage;
            public uint Flags;
            public IntPtr Target;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

        const int RIM_TYPEHID = 2;
        const int WM_INPUT = 0x00FF;

        public Form1()
        {
            // Register Raw Input for touch devices
            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];
            rid[0].UsagePage = 0x0D; // HID_USAGE_PAGE_DIGITIZER
            rid[0].Usage = 0x04;     // HID_USAGE_DIGITIZER_TOUCH_SCREEN
            rid[0].Flags = 0x00000000; // RIDEV_INPUTSINK
            rid[0].Target = Handle;

            if (!RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                throw new Exception("Failed to register raw input device.");
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_INPUT)
            {
                uint dwSize = 0;
                GetRawInputData(m.LParam, 0x10000003, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE)));

                IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
                try
                {
                    if (GetRawInputData(m.LParam, 0x10000003, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))) == dwSize)
                    {
                        // Process touch input data here
                        Debug.WriteLine("Touch event detected!");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }

            base.WndProc(ref m);
        }
    }
}

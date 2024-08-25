using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilityLib.Input
{
    public partial class TouchForm : Form
    {
        // Constants
        private const int WM_TOUCH = 0x0240;
        private const int TOUCHEVENTF_MOVE = 0x0001;
        private const int TOUCHEVENTF_DOWN = 0x0002;
        private const int TOUCHEVENTF_UP = 0x0004;

        // Structures
        [StructLayout(LayoutKind.Sequential)]
        private struct TOUCHINPUT
        {
            public int x;
            public int y;
            public IntPtr hSource;
            public int dwID;
            public int dwFlags;
            public int dwMask;
            public int dwTime;
            public IntPtr dwExtraInfo;
            public int cxContact;
            public int cyContact;
        }

        // DLL Imports
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterTouchWindow(IntPtr hWnd, uint ulFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetTouchInputInfo(IntPtr hTouchInput, int cInputs, [In, Out] TOUCHINPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void CloseTouchInputHandle(IntPtr lParam);

        public TouchForm()
        {
            this.Load += new EventHandler(TouchForm_Load);
        }

        private void TouchForm_Load(object sender, EventArgs e)
        {
            // Register the window to receive touch events
            RegisterTouchWindow(this.Handle, 0);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_TOUCH)
            {
                int inputCount = m.WParam.ToInt32();
                TOUCHINPUT[] inputs = new TOUCHINPUT[inputCount];

                if (GetTouchInputInfo(m.LParam, inputCount, inputs, Marshal.SizeOf(typeof(TOUCHINPUT))))
                {
                    for (int i = 0; i < inputCount; i++)
                    {
                        TOUCHINPUT ti = inputs[i];

                        // Convert the coordinates to pixels
                        int x = ti.x / 100;
                        int y = ti.y / 100;

                        // Handle different touch events
                        if ((ti.dwFlags & TOUCHEVENTF_DOWN) != 0)
                        {
                            OnTouchDown(x, y);
                        }
                        else if ((ti.dwFlags & TOUCHEVENTF_MOVE) != 0)
                        {
                            OnTouchMove(x, y);
                        }
                        else if ((ti.dwFlags & TOUCHEVENTF_UP) != 0)
                        {
                            OnTouchUp(x, y);
                        }
                    }

                    CloseTouchInputHandle(m.LParam);
                }
            }
            base.WndProc(ref m);
        }

        private void OnTouchDown(int x, int y)
        {
            // タッチダウンイベントの処理
            Console.WriteLine($"Touch down at ({x}, {y})");
        }

        private void OnTouchMove(int x, int y)
        {
            // タッチムーブイベントの処理
            Console.WriteLine($"Touch move at ({x}, {y})");
        }

        private void OnTouchUp(int x, int y)
        {
            // タッチアップイベントの処理
            Console.WriteLine($"Touch up at ({x}, {y})");
        }
    }
}

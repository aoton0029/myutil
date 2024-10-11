using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class TouchManager
{
    // 定数定義
    private const int WM_TOUCH = 0x0240;
    private const int TOUCHEVENTF_DOWN = 0x0001;
    private const int TOUCHEVENTF_UP = 0x0002;
    private const int TOUCHEVENTF_MOVE = 0x0004;

    // タッチイベント構造体
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

    // Windows APIの宣言
    [DllImport("user32.dll")]
    private static extern bool RegisterTouchWindow(IntPtr hwnd, uint ulFlags);

    [DllImport("user32.dll")]
    private static extern bool GetTouchInputInfo(IntPtr hTouchInput, int cInputs, [In, Out] TOUCHINPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern void CloseTouchInputHandle(IntPtr lParam);

    private Control _control;

    public TouchManager(Control control)
    {
        _control = control;
        RegisterTouchWindow(_control.Handle, 0);
        _control.HandleCreated += (s, e) => RegisterTouchWindow(_control.Handle, 0);
        _control.HandleDestroyed += (s, e) => UnregisterTouchWindow(_control.Handle);
        //_control.WndProc += new WndProcDelegate(WndProc);
    }

    private void WndProc(ref Message m)
    {
        if (m.Msg == WM_TOUCH)
        {
            int inputCount = m.WParam.ToInt32();
            TOUCHINPUT[] inputs = new TOUCHINPUT[inputCount];
            if (GetTouchInputInfo(m.LParam, inputCount, inputs, Marshal.SizeOf(typeof(TOUCHINPUT))))
            {
                foreach (var input in inputs)
                {
                    var point = _control.PointToClient(new System.Drawing.Point(input.x / 100, input.y / 100));

                    if ((input.dwFlags & TOUCHEVENTF_DOWN) != 0)
                    {
                        OnTouchDown(point);
                    }
                    else if ((input.dwFlags & TOUCHEVENTF_MOVE) != 0)
                    {
                        OnTouchMove(point);
                    }
                    else if ((input.dwFlags & TOUCHEVENTF_UP) != 0)
                    {
                        OnTouchUp(point);
                    }
                }
                CloseTouchInputHandle(m.LParam);
            }
        }
    }

    protected virtual void OnTouchDown(System.Drawing.Point point)
    {
        // タッチダウンイベント処理
    }

    protected virtual void OnTouchMove(System.Drawing.Point point)
    {
        // タッチムーブイベント処理
    }

    protected virtual void OnTouchUp(System.Drawing.Point point)
    {
        // タッチアップイベント処理
    }

    [DllImport("user32.dll")]
    private static extern bool UnregisterTouchWindow(IntPtr hwnd);

    private delegate void WndProcDelegate(ref Message m);
}

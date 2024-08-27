using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class SwipeForm : Form
{
    private Panel panel;

    private const int WM_GESTURE = 0x0119;

    [StructLayout(LayoutKind.Sequential)]
    private struct GESTUREINFO
    {
        public uint cbSize;
        public uint dwFlags;
        public uint dwID;
        public IntPtr hwndTarget;
        public POINTS ptsLocation;
        public uint dwInstanceID;
        public uint dwSequenceID;
        public ulong ullArguments;
        public uint cbExtraArgs;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINTS
    {
        public short x;
        public short y;
    }

    [DllImport("user32")]
    private static extern bool GetGestureInfo(IntPtr hGestureInfo, ref GESTUREINFO pGestureInfo);

    [DllImport("user32")]
    private static extern bool CloseGestureInfoHandle(IntPtr hGestureInfo);

    private const uint GID_PAN = 4;

    public SwipeForm()
    {
        this.Text = "Swipe Example with Panel";
        this.Width = 800;
        this.Height = 600;

        // パネルを作成してフォームに追加
        panel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };

        // 複数のユーザーコントロールをパネルに追加
        for (int i = 0; i < 10; i++)
        {
            var userControl = new UserControl
            {
                BackColor = i % 2 == 0 ? System.Drawing.Color.LightBlue : System.Drawing.Color.LightGreen,
                Height = 100,
                Dock = DockStyle.Top
            };
            panel.Controls.Add(userControl);
        }

        this.Controls.Add(panel);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_GESTURE)
        {
            HandleGesture(m.LParam);
        }
        base.WndProc(ref m);
    }

    private void HandleGesture(IntPtr lParam)
    {
        GESTUREINFO gi = new GESTUREINFO();
        gi.cbSize = (uint)Marshal.SizeOf(gi);

        if (GetGestureInfo(lParam, ref gi))
        {
            if (gi.dwID == GID_PAN)
            {
                HandlePanGesture(gi);
            }
            CloseGestureInfoHandle(lParam);
        }
    }

    private void HandlePanGesture(GESTUREINFO gestureInfo)
    {
        int deltaX = gestureInfo.ptsLocation.x;
        int deltaY = gestureInfo.ptsLocation.y;

        // スワイプの方向に基づいてパネルをスクロール
        if (Math.Abs(deltaX) < Math.Abs(deltaY))
        {
            if (deltaY > 0)
            {
                // 下スワイプ - 上にスクロール
                panel.VerticalScroll.Value = Math.Max(panel.VerticalScroll.Value - deltaY, panel.VerticalScroll.Minimum);
            }
            else
            {
                // 上スワイプ - 下にスクロール
                panel.VerticalScroll.Value = Math.Min(panel.VerticalScroll.Value - deltaY, panel.VerticalScroll.Maximum);
            }
        }
        else
        {
            if (deltaX > 0)
            {
                // 右スワイプ - 左にスクロール
                panel.HorizontalScroll.Value = Math.Max(panel.HorizontalScroll.Value - deltaX, panel.HorizontalScroll.Minimum);
            }
            else
            {
                // 左スワイプ - 右にスクロール
                panel.HorizontalScroll.Value = Math.Min(panel.HorizontalScroll.Value - deltaX, panel.HorizontalScroll.Maximum);
            }
        }
        panel.PerformLayout();
    }
}

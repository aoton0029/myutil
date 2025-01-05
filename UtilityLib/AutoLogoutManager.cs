using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class AutoLogoutManager : IMessageFilter
    {
        private readonly System.Windows.Forms.Timer _timer;
        private readonly Action _onLogoutAction;
        private readonly int _timeout; // タイムアウト時間（ミリ秒）
        private DateTime _lastActivityTime;

        public AutoLogoutManager(int timeoutInSeconds, Action onLogoutAction)
        {
            _timeout = timeoutInSeconds * 1000; // ミリ秒に変換
            _onLogoutAction = onLogoutAction;
            _lastActivityTime = DateTime.Now;

            // タイマーの設定
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 1000; // 1秒ごとにチェック
            _timer.Tick += Timer_Tick;

            // MessageFilterの登録
            Application.AddMessageFilter(this);
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            Application.RemoveMessageFilter(this);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((DateTime.Now - _lastActivityTime).TotalMilliseconds >= _timeout)
            {
                _timer.Stop();
                _onLogoutAction?.Invoke();
            }
        }

        public bool PreFilterMessage(ref Message m)
        {
            // マウスやキーボードの操作を検出
            if (m.Msg == 0x0200 || m.Msg == 0x0100) // WM_MOUSEMOVE または WM_KEYDOWN
            {
                _lastActivityTime = DateTime.Now;
            }
            return false;
        }
    }
}

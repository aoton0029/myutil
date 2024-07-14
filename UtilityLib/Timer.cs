using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    // 汎用タイマークラス。
    public class Timer
    {
        // タイマーのインターバル。
        private int interval = 1000;
        // タイマーのイベントハンドラ。
        public event EventHandler Tick;
        // タイマーの状態。
        public bool Enabled { get; private set; } = false;

        // タイマーのインターバルを取得または設定します。
        public int Interval
        {
            get { return interval; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("Interval", "0より大きい値を設定してください。");
                }
                interval = value;
            }
        }

        public Timer()
        {

        }

        // タイマーを開始します。
        public void Start()
        {
            if (Enabled)
            {
                return;
            }
            Enabled = true;
            Task.Run(async () =>
            {
                while (Enabled)
                {
                    await Task.Delay(interval);
                    Tick?.Invoke(this, EventArgs.Empty);
                }
            });
        }

        // タイマーを停止します。
        public void Stop()
        {
            Enabled = false;
        }
    }



}

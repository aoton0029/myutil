using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Controls
{
    public class HoverControlManager
    {
        private Control parentControl;
        private UserControl hoverControl;

        public HoverControlManager(Control parent, UserControl controlToShow)
        {
            parentControl = parent;
            hoverControl = controlToShow;

            // 初期設定
            hoverControl.Visible = false;
            hoverControl.AutoSize = true;
            hoverControl.BackColor = Color.LightYellow;
            parentControl.Controls.Add(hoverControl);

            // イベントハンドラを設定
            parentControl.MouseMove += ParentControl_MouseMove;
            parentControl.MouseLeave += ParentControl_MouseLeave;
        }

        private void ParentControl_MouseMove(object sender, MouseEventArgs e)
        {
            // カーソル位置にUserControlを表示
            hoverControl.Location = new Point(e.X + 10, e.Y + 10); // カーソル位置の少し下
            hoverControl.Visible = true;
        }

        private void ParentControl_MouseLeave(object sender, EventArgs e)
        {
            // マウスが外れたら非表示
            hoverControl.Visible = false;
        }

        public void Dispose()
        {
            // イベントハンドラを解除
            parentControl.MouseMove -= ParentControl_MouseMove;
            parentControl.MouseLeave -= ParentControl_MouseLeave;

            // コントロールを削除
            parentControl.Controls.Remove(hoverControl);
            hoverControl.Dispose();
        }
    }
}

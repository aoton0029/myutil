using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Controls
{
    public class LineControl : Control
    {
        public Color LineColor { get; set; } = Color.Black;
        public float LineWidth { get; set; } = 2f;
        public Point StartPoint { get; set; } = new Point(0, 0);
        public Point EndPoint { get; set; } = new Point(100, 100);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (Pen pen = new Pen(LineColor, LineWidth))
            {
                e.Graphics.DrawLine(pen, StartPoint, EndPoint);
            }
        }
    }
}

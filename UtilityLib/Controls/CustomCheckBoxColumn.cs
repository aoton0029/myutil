using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Controls
{
    [ToolboxItem(true)] // ツールボックスに表示する
    public class CustomCheckBoxColumn : DataGridViewCheckBoxColumn
    {
        public CustomCheckBoxColumn()
        {
            // セルテンプレートをカスタムセルに設定
            this.CellTemplate = new CustomCheckBoxCell();
        }

        // デザイナーで表示される列の名前
        public override string ToString()
        {
            return "CustomCheckBoxColumn";
        }
    }

    public class CustomCheckBoxCell : DataGridViewCheckBoxCell
    {
        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds,
            int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue,
            string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            // 背景を描画
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue,
                errorText, cellStyle, advancedBorderStyle, paintParts & ~DataGridViewPaintParts.ContentForeground);

            // チェックボックスの状態を取得
            bool isChecked = value != null && (bool)value;

            // チェックボックスの描画領域を計算
            int size = Math.Min(cellBounds.Width, cellBounds.Height) - 8;
            Rectangle checkBoxRect = new Rectangle(
                cellBounds.X + (cellBounds.Width - size) / 2,
                cellBounds.Y + (cellBounds.Height - size) / 2,
                size,
                size
            );

            // チェックボックスの枠を描画
            ControlPaint.DrawCheckBox(graphics, checkBoxRect, ButtonState.Normal);

            // チェックされている場合、チェックマークを描画
            if (isChecked)
            {
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    Point p1 = new Point(checkBoxRect.Left + (int)(size * 0.2), checkBoxRect.Top + (int)(size * 0.5));
                    Point p2 = new Point(checkBoxRect.Left + (int)(size * 0.4), checkBoxRect.Bottom - (int)(size * 0.2));
                    Point p3 = new Point(checkBoxRect.Right - (int)(size * 0.2), checkBoxRect.Top + (int)(size * 0.2));

                    graphics.DrawLines(pen, new[] { p1, p2, p3 });
                }
            }
        }
    }
}

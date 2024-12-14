using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNet45
{
    public class CustomCheckBoxCell : DataGridViewCheckBoxCell
    {
        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseClick(e);

            // クリック位置に関わらずチェック状態をトグル
            if (DataGridView != null && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                bool currentValue = this.Value != null && (bool)this.Value;
                this.Value = !currentValue;

                // セルをリフレッシュして状態を更新
                DataGridView.InvalidateCell(this);
            }
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds,
            int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue,
            string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            // 背景を描画
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue,
                errorText, cellStyle, advancedBorderStyle, paintParts & ~DataGridViewPaintParts.ContentForeground);

            // チェック状態
            bool isChecked = value != null && (bool)value;

            // セル全体にチェックの見た目を描画
            string displayText = isChecked ? "✔" : "";
            TextRenderer.DrawText(graphics, displayText, cellStyle.Font, cellBounds,
                cellStyle.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    [ToolboxItem(true)]
    public class CustomCheckBoxColumn : DataGridViewCheckBoxColumn
    {
        public CustomCheckBoxColumn()
        {
            this.CellTemplate = new CustomCheckBoxCell();
        }
    }
}

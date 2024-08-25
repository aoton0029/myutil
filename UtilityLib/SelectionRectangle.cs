using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib
{
    public class SelectionRectangle : Control
    {
        private Point startPoint;
        private Point currentPoint;
        private bool isSelecting;

        public Rectangle SelectedRectangle { get; private set; }

        public SelectionRectangle()
        {
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Cross;
            this.isSelecting = false;
            this.MouseDown += new MouseEventHandler(OnMouseDown);
            this.MouseMove += new MouseEventHandler(OnMouseMove);
            this.MouseUp += new MouseEventHandler(OnMouseUp);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                startPoint = e.Location;
                isSelecting = true;
                SelectedRectangle = new Rectangle();
                Invalidate(); // 再描画を促す
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                currentPoint = e.Location;
                SelectedRectangle = new Rectangle(
                    Math.Min(startPoint.X, currentPoint.X),
                    Math.Min(startPoint.Y, currentPoint.Y),
                    Math.Abs(startPoint.X - currentPoint.X),
                    Math.Abs(startPoint.Y - currentPoint.Y));
                Invalidate(); // 再描画を促す
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isSelecting)
            {
                isSelecting = false;
                Invalidate(); // 再描画を促す
                              // 選択が完了した後に必要な処理をここで行う
                OnSelectionCompleted(EventArgs.Empty);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (isSelecting)
            {
                using (var pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, SelectedRectangle);
                }
            }
        }

        // 選択が完了したときのイベント
        public event EventHandler SelectionCompleted;

        protected virtual void OnSelectionCompleted(EventArgs e)
        {
            SelectionCompleted?.Invoke(this, e);
        }
    }

    public partial class MainForm : Form
    {
        private SelectionRectangle selectionRectangle;

        public MainForm()
        {
            //InitializeComponent();
            InitializeSelectionRectangle();
        }

        private void InitializeSelectionRectangle()
        {
            selectionRectangle = new SelectionRectangle
            {
                Dock = DockStyle.Fill // フォーム全体で選択できるようにする
            };
            selectionRectangle.SelectionCompleted += SelectionRectangle_SelectionCompleted;
            this.Controls.Add(selectionRectangle);
        }

        private void SelectionRectangle_SelectionCompleted(object sender, EventArgs e)
        {
            Rectangle selectedArea = selectionRectangle.SelectedRectangle;
            MessageBox.Show($"Selected Area: {selectedArea}", "Selection Completed");
            // 必要に応じて、選択した範囲を使用する処理をここに追加
        }
    }
}

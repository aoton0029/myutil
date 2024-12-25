private void c1FlexGrid1_OwnerDrawCell(object sender, C1.Win.C1FlexGrid.OwnerDrawCellEventArgs e)
{
    var grid = sender as C1.Win.C1FlexGrid.C1FlexGrid;

    // マージされたセルの範囲を取得
    var range = grid.GetMergedRange(e.Row, e.Col);

    if (range != null) // マージされている場合
    {
        // 範囲の最初の行を基準に背景色を設定
        int baseRow = range.TopRow;

        // 背景色を変更するロジック（例：行ごとの色分け）
        if (baseRow % 2 == 0)
        {
            e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds);
        }
        else
        {
            e.Graphics.FillRectangle(Brushes.LightGreen, e.Bounds);
        }

        // 通常のテキスト描画
        e.Graphics.DrawString(
            grid[e.Row, e.Col]?.ToString(),
            e.Style.Font,
            Brushes.Black,
            e.Bounds,
            StringFormat.GenericDefault
        );

        e.Handled = true; // 標準描画を無効化
    }
    else // マージされていない場合
    {
        // 通常の背景色変更処理
        if (e.Row % 2 == 0)
        {
            e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);
        }
        else
        {
            e.Graphics.FillRectangle(Brushes.White, e.Bounds);
        }

        // 通常のテキスト描画
        e.Graphics.DrawString(
            grid[e.Row, e.Col]?.ToString(),
            e.Style.Font,
            Brushes.Black,
            e.Bounds,
            StringFormat.GenericDefault
        );

        e.Handled = true; // 標準描画を無効化
    }
}





using System;
using System.Drawing;
using System.Windows.Forms;

public class CustomForm : Form
{
    private Rectangle closeButtonRect;

    public CustomForm()
    {
        // フォームのスタイルを設定
        this.FormBorderStyle = FormBorderStyle.None;
        this.DoubleBuffered = true; // スムーズな描画
        this.Padding = new Padding(1);

        // ウィンドウのサイズ変更やドラッグを有効化
        this.MouseDown += CustomForm_MouseDown;
        this.Paint += CustomForm_Paint;
        this.MouseClick += CustomForm_MouseClick;

        // ボタンの位置を設定
        closeButtonRect = new Rectangle(this.ClientSize.Width - 30, 5, 25, 25);
    }

    private void CustomForm_Paint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        // タイトルバーの背景を描画
        g.FillRectangle(Brushes.DarkBlue, 0, 0, this.ClientSize.Width, 30);

        // タイトルを描画
        g.DrawString("カスタムウィンドウ", this.Font, Brushes.White, 10, 5);

        // 閉じるボタンを描画
        g.FillRectangle(Brushes.Red, closeButtonRect);
        g.DrawString("X", this.Font, Brushes.White, closeButtonRect.Location);
    }

    private void CustomForm_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            // ウィンドウをドラッグ可能にする
            ReleaseCapture();
            SendMessage(this.Handle, 0xA1, 0x2, 0);
        }
    }

    private void CustomForm_MouseClick(object sender, MouseEventArgs e)
    {
        if (closeButtonRect.Contains(e.Location))
        {
            // 閉じるボタンがクリックされた場合
            this.Close();
        }
    }

    // Windows API のインポート
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    // エントリポイント
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new CustomForm());
    }
}
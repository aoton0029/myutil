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



using System;
using System.Drawing;
using System.Windows.Forms;

public class CustomForm : Form
{
    private Rectangle minimizeButtonRect;
    private Rectangle maximizeButtonRect;
    private Rectangle pinButtonRect;
    private Rectangle searchBoxRect;

    private bool isPinned = false;
    private bool isMaximized = false;
    private TextBox searchTextBox;

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
        this.Resize += CustomForm_Resize;

        // ボタンの位置を初期化
        UpdateButtonPositions();

        // 検索ボックスの設定
        searchTextBox = new TextBox
        {
            Location = new Point(searchBoxRect.X + 5, searchBoxRect.Y + 5),
            Width = 150,
            Height = 20,
            BorderStyle = BorderStyle.None
        };
        this.Controls.Add(searchTextBox);
    }

    private void UpdateButtonPositions()
    {
        int titleBarHeight = 30;

        minimizeButtonRect = new Rectangle(this.ClientSize.Width - 90, 5, 25, 25);
        maximizeButtonRect = new Rectangle(this.ClientSize.Width - 60, 5, 25, 25);
        pinButtonRect = new Rectangle(this.ClientSize.Width - 120, 5, 25, 25);
        searchBoxRect = new Rectangle(10, 5, 160, titleBarHeight - 10);
    }

    private void CustomForm_Paint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        // タイトルバーの背景を描画
        g.FillRectangle(Brushes.DarkBlue, 0, 0, this.ClientSize.Width, 30);

        // タイトルを描画
        g.DrawString("カスタムウィンドウ", this.Font, Brushes.White, new Point(180, 5));

        // 各ボタンを描画
        // 最小化ボタン
        g.FillRectangle(Brushes.Gray, minimizeButtonRect);
        g.DrawString("_", this.Font, Brushes.White, minimizeButtonRect.Location);

        // 最大化ボタン
        g.FillRectangle(Brushes.Gray, maximizeButtonRect);
        g.DrawString(isMaximized ? "□" : "☐", this.Font, Brushes.White, maximizeButtonRect.Location);

        // ピンボタン
        g.FillRectangle(isPinned ? Brushes.Green : Brushes.Gray, pinButtonRect);
        g.DrawString("📌", this.Font, Brushes.White, pinButtonRect.Location);

        // 検索ボックスの枠
        g.DrawRectangle(Pens.White, searchBoxRect);
    }

    private void CustomForm_MouseClick(object sender, MouseEventArgs e)
    {
        if (minimizeButtonRect.Contains(e.Location))
        {
            // 最小化ボタン
            this.WindowState = FormWindowState.Minimized;
        }
        else if (maximizeButtonRect.Contains(e.Location))
        {
            // 最大化ボタン
            if (isMaximized)
            {
                this.WindowState = FormWindowState.Normal;
                isMaximized = false;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                isMaximized = true;
            }
            UpdateButtonPositions();
            this.Invalidate();
        }
        else if (pinButtonRect.Contains(e.Location))
        {
            // ピン固定ボタン
            isPinned = !isPinned;
            this.TopMost = isPinned; // ピン固定をトグル
            this.Invalidate();
        }
    }

    private void CustomForm_Resize(object sender, EventArgs e)
    {
        UpdateButtonPositions();
        this.Invalidate();
    }

    private void CustomForm_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && e.Y <= 30)
        {
            // ウィンドウをドラッグ可能にする
            ReleaseCapture();
            SendMessage(this.Handle, 0xA1, 0x2, 0);
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


using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

public class CustomColorComponent : Component
{
    private Color customColor = Color.FromArgb(255, 128, 0); // 初期色

    [Category("Custom Colors")]
    [Description("Select a custom color.")]
    [TypeConverter(typeof(CustomColorConverter))]
    public Color CustomColor
    {
        get { return customColor; }
        set { customColor = value; }
    }
}

public class CustomColorConverter : ColorConverter
{
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        // 標準色にカスタム色を追加
        var standardColors = new[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.FromArgb(255, 128, 0), // カスタム色
            Color.FromArgb(128, 0, 255)  // 別のカスタム色
        };
        return new StandardValuesCollection(standardColors);
    }

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        return true; // 標準値のサポート
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        return false; // カスタム値も許可
    }
}
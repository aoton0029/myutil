手動でページを切り替えた場合に、定期実行（自動ページ切り替え）を一時停止し、一定時間後に再開 するように改良します。

改良点

1. 手動操作時に自動切り替えを一時停止

手動でページを切り替えたら、自動切り替えを一時停止

一定時間（例: 10秒）経過後に再開



2. 定期実行の再開

一定時間が経過するごとに、次のページへ自動遷移





---

修正コード

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Timers;

public class PaginatedTableLayoutPanel : UserControl
{
    private TableLayoutPanel tableLayoutPanel;
    private Button btnPrev, btnNext;
    private Label lblPageInfo;
    private List<PageModel> pages = new List<PageModel>();
    private int currentPageIndex = 0;
    private System.Timers.Timer pageSwitchTimer;
    private System.Timers.Timer resumeTimer;
    private bool isManualOperation = false;

    public PaginatedTableLayoutPanel()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // メインのTableLayoutPanel
        tableLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true
        };

        // ページ制御ボタン
        btnPrev = new Button { Text = "←", Width = 50 };
        btnNext = new Button { Text = "→", Width = 50 };
        lblPageInfo = new Label { AutoSize = true, TextAlign = ContentAlignment.MiddleCenter };

        btnPrev.Click += (s, e) => ManualChangePage(-1);
        btnNext.Click += (s, e) => ManualChangePage(1);

        // ページ制御エリア
        FlowLayoutPanel controlPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight
        };
        controlPanel.Controls.Add(btnPrev);
        controlPanel.Controls.Add(lblPageInfo);
        controlPanel.Controls.Add(btnNext);

        // タイマー設定（ページ自動切り替え）
        pageSwitchTimer = new System.Timers.Timer(5000); // 5秒ごとに切り替え
        pageSwitchTimer.Elapsed += (s, e) => this.Invoke(new Action(() => ChangePage(1)));
        pageSwitchTimer.Start();

        // 手動操作後の再開タイマー（10秒後に再開）
        resumeTimer = new System.Timers.Timer(10000);
        resumeTimer.Elapsed += (s, e) =>
        {
            isManualOperation = false;
            pageSwitchTimer.Start();
            resumeTimer.Stop();
        };

        // コントロールの追加
        Controls.Add(tableLayoutPanel);
        Controls.Add(controlPanel);
    }

    /// <summary>
    /// ページを追加
    /// </summary>
    public void AddPage(PageModel page)
    {
        pages.Add(page);
        if (pages.Count == 1)
        {
            UpdateTable();
        }
    }

    /// <summary>
    /// 手動操作時のページ切り替え（自動切り替えを一時停止）
    /// </summary>
    private void ManualChangePage(int direction)
    {
        isManualOperation = true;
        pageSwitchTimer.Stop();
        ChangePage(direction);

        // 一定時間後に自動切り替えを再開
        resumeTimer.Start();
    }

    /// <summary>
    /// ページ切り替え
    /// </summary>
    private void ChangePage(int direction)
    {
        if (pages.Count == 0) return;

        currentPageIndex = (currentPageIndex + direction + pages.Count) % pages.Count;
        UpdateTable();
    }

    /// <summary>
    /// テーブルを更新
    /// </summary>
    private void UpdateTable()
    {
        if (pages.Count == 0) return;

        tableLayoutPanel.Controls.Clear();
        PageModel currentPage = pages[currentPageIndex];

        // ページの行・列を設定
        tableLayoutPanel.RowCount = currentPage.Rows;
        tableLayoutPanel.ColumnCount = currentPage.Columns;
        tableLayoutPanel.Controls.Clear();
        tableLayoutPanel.ColumnStyles.Clear();
        tableLayoutPanel.RowStyles.Clear();

        for (int i = 0; i < currentPage.Columns; i++)
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / currentPage.Columns));

        for (int i = 0; i < currentPage.Rows; i++)
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / currentPage.Rows));

        // コントロールを配置
        int index = 0;
        foreach (var control in currentPage.Controls)
        {
            int row = index / currentPage.Columns;
            int col = index % currentPage.Columns;
            if (row < currentPage.Rows)
                tableLayoutPanel.Controls.Add(control, col, row);
            index++;
        }

        lblPageInfo.Text = $"{currentPageIndex + 1} / {pages.Count}";
    }
}

/// <summary>
/// ページモデル：各ページのレイアウト情報を保持
/// </summary>
public class PageModel
{
    public int Columns { get; set; }
    public int Rows { get; set; }
    public List<Control> Controls { get; set; } = new List<Control>();

    public PageModel(int columns, int rows, IEnumerable<Control> controls)
    {
        Columns = columns;
        Rows = rows;
        Controls.AddRange(controls);
    }
}


---

改善点

✅ 手動操作後に自動ページ切り替えを一時停止

ManualChangePage(int direction) を使用して、手動でページを変更

pageSwitchTimer.Stop(); により自動切り替えを停止


✅ 一定時間（10秒後）に自動ページ切り替えを再開

resumeTimer により、10秒後に pageSwitchTimer.Start(); を実行


✅ 定期実行と手動操作の両立

手動操作が可能で、ページの切り替えを自由にできる

一定時間後に自動で切り替えが再開されるため、操作がない場合に自動ページ送りが続く



---

使用方法

var paginatedPanel = new PaginatedTableLayoutPanel
{
    Dock = DockStyle.Fill
};

// ページ1（3x2）
var page1 = new PageModel(3, 2, new List<Control>
{
    new Label { Text = "Item 1", BackColor = Color.LightBlue, AutoSize = true },
    new Label { Text = "Item 2", BackColor = Color.LightGreen, AutoSize = true },
    new Label { Text = "Item 3", BackColor = Color.LightCoral, AutoSize = true },
    new Label { Text = "Item 4", BackColor = Color.LightYellow, AutoSize = true },
    new Label { Text = "Item 5", BackColor = Color.LightPink, AutoSize = true }
});

// ページ2（2x2）
var page2 = new PageModel(2, 2, new List<Control>
{
    new Button { Text = "Button A", BackColor = Color.LightBlue },
    new Button { Text = "Button B", BackColor = Color.LightGreen },
    new Button { Text = "Button C", BackColor = Color.LightCoral }
});

// ページを追加
paginatedPanel.AddPage(page1);
paginatedPanel.AddPage(page2);

// フォームに追加
var form = new Form();
form.Controls.Add(paginatedPanel);
Application.Run(form);


---

拡張案

1. ページ変更のアニメーション

フェードイン・フェードアウトでスムーズな遷移



2. ページの切り替えをマウスホイールやキー入力で可能に

KeyDown や MouseWheel イベントを使う



3. ページごとの背景色やスタイルを個別に設定




---

この改良で、手動操作と自動切り替えが共存 する、より実用的な PaginatedTableLayoutPanel になりました！


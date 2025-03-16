StopAutoSwitching() を実行してもページの自動切り替えが止まらない問題を解決するために、lock を使用したスレッドセーフな実装を追加します。


---

改良点

✅ スレッドセーフな lock 機構を追加
✅ isAutoSwitching フラグを導入し、確実に StopAutoSwitching() で停止
✅ 手動操作時に pageSwitchTimer.Stop(); を確実に実行


---

修正コード

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Timers;

public class PaginatedTableLayoutPanel : UserControl
{
    private TableLayoutPanel tableLayoutPanel;
    private List<PageModel> pages = new List<PageModel>();
    private int currentPageIndex = 0;
    private System.Timers.Timer pageSwitchTimer;
    private System.Timers.Timer resumeTimer;
    private bool isManualOperation = false;
    private bool isAutoSwitching = true;
    private readonly object lockObj = new object(); // ロック用オブジェクト

    /// <summary>
    /// ページ変更時のイベント
    /// </summary>
    public event Action<int, int> PageChanged; // (現在のページ, 全ページ数)

    public PaginatedTableLayoutPanel()
    {
        // ダブルバッファリング有効化（ちらつき防止）
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        this.DoubleBuffered = true;

        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // メインのTableLayoutPanel
        tableLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            DoubleBuffered = true
        };

        // タイマー設定（ページ自動切り替え）
        pageSwitchTimer = new System.Timers.Timer(5000); // 5秒ごとに切り替え
        pageSwitchTimer.Elapsed += (s, e) =>
        {
            lock (lockObj) // スレッドセーフに処理
            {
                if (isAutoSwitching)
                {
                    this.Invoke(new Action(() => NextPage()));
                }
            }
        };
        pageSwitchTimer.Start();

        // 手動操作後の再開タイマー（10秒後に再開）
        resumeTimer = new System.Timers.Timer(10000);
        resumeTimer.Elapsed += (s, e) =>
        {
            lock (lockObj)
            {
                isManualOperation = false;
                isAutoSwitching = true;
                pageSwitchTimer.Start();
                resumeTimer.Stop();
            }
        };

        Controls.Add(tableLayoutPanel);
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
    /// 次のページへ移動
    /// </summary>
    public void NextPage()
    {
        if (pages.Count == 0) return;
        ChangePage(1);
    }

    /// <summary>
    /// 前のページへ移動
    /// </summary>
    public void PreviousPage()
    {
        if (pages.Count == 0) return;
        ChangePage(-1);
    }

    /// <summary>
    /// 手動操作時のページ切り替え（自動切り替えを一時停止）
    /// </summary>
    public void ManualChangePage(int direction)
    {
        lock (lockObj)
        {
            isManualOperation = true;
            isAutoSwitching = false;
            pageSwitchTimer.Stop();
            ChangePage(direction);
            resumeTimer.Start(); // 一定時間後に再開
        }
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

        // レイアウト更新を停止
        tableLayoutPanel.SuspendLayout();
        tableLayoutPanel.Controls.Clear();

        PageModel currentPage = pages[currentPageIndex];

        // ページの行・列を設定
        tableLayoutPanel.RowCount = currentPage.Rows;
        tableLayoutPanel.ColumnCount = currentPage.Columns;
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

        // ページ変更イベントを発火（UI更新用）
        PageChanged?.Invoke(currentPageIndex + 1, pages.Count);

        // レイアウト更新を再開
        tableLayoutPanel.ResumeLayout(true);
    }

    /// <summary>
    /// ページ自動切り替えを開始
    /// </summary>
    public void StartAutoSwitching()
    {
        lock (lockObj)
        {
            isAutoSwitching = true;
            pageSwitchTimer.Start();
        }
    }

    /// <summary>
    /// ページ自動切り替えを停止
    /// </summary>
    public void StopAutoSwitching()
    {
        lock (lockObj)
        {
            isAutoSwitching = false;
            pageSwitchTimer.Stop();
        }
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

改良点

✅ lock を使用して pageSwitchTimer.Elapsed 内の処理をスレッドセーフに実装
✅ isAutoSwitching フラグを導入し、確実に StopAutoSwitching() で自動切り替えを停止
✅ 手動操作時に pageSwitchTimer.Stop(); を確実に実行
✅ resumeTimer を使用して、手動操作後に一定時間経過後に自動切り替えを再開


---

使用例

ボタン付きでページ切り替え

var paginatedPanel = new PaginatedTableLayoutPanel
{
    Dock = DockStyle.Fill
};

// ページ追加
paginatedPanel.AddPage(new PageModel(3, 2, new List<Control>
{
    new Label { Text = "Item 1", BackColor = Color.LightBlue, AutoSize = true },
    new Label { Text = "Item 2", BackColor = Color.LightGreen, AutoSize = true }
}));

paginatedPanel.AddPage(new PageModel(2, 2, new List<Control>
{
    new Button { Text = "Button A", BackColor = Color.LightBlue },
    new Button { Text = "Button B", BackColor = Color.LightGreen }
}));

// UIボタン
var btnStop = new Button { Text = "Stop Auto", Dock = DockStyle.Bottom };
var btnStart = new Button { Text = "Start Auto", Dock = DockStyle.Bottom };

btnStop.Click += (s, e) => paginatedPanel.StopAutoSwitching();
btnStart.Click += (s, e) => paginatedPanel.StartAutoSwitching();

// フォームに追加
var form = new Form();
form.Controls.Add(paginatedPanel);
form.Controls.Add(btnStop);
form.Controls.Add(btnStart);

Application.Run(form);


---

これで、StopAutoSwitching() が確実に動作し、スレッドセーフなページネーション機能になりました！




PaginatedTableLayoutPanel を改良し、ページネーション機能のみを提供し、ボタンなどのUI要素を削除することで汎用性を向上させます。
これにより、フォーム側で好きなボタンや入力UIを自由に配置できます。


---

改良点

✅ UI要素（ボタン、ページラベル）を削除
✅ NextPage() / PreviousPage() メソッドを用意
✅ PageChanged イベントを追加（外部からUIを更新できる）
✅ 手動・自動ページ切り替えをサポート


---

コード

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Timers;

public class PaginatedTableLayoutPanel : UserControl
{
    private TableLayoutPanel tableLayoutPanel;
    private List<PageModel> pages = new List<PageModel>();
    private int currentPageIndex = 0;
    private System.Timers.Timer pageSwitchTimer;
    private System.Timers.Timer resumeTimer;
    private bool isManualOperation = false;

    /// <summary>
    /// ページ変更時のイベント
    /// </summary>
    public event Action<int, int> PageChanged; // (現在のページ, 全ページ数)

    public PaginatedTableLayoutPanel()
    {
        // ダブルバッファリング有効化（ちらつき防止）
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        this.DoubleBuffered = true;

        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // メインのTableLayoutPanel
        tableLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            DoubleBuffered = true
        };

        // タイマー設定（ページ自動切り替え）
        pageSwitchTimer = new System.Timers.Timer(5000); // 5秒ごとに切り替え
        pageSwitchTimer.Elapsed += (s, e) => this.Invoke(new Action(() => NextPage()));

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
    /// 次のページへ移動
    /// </summary>
    public void NextPage()
    {
        if (pages.Count == 0) return;
        ChangePage(1);
    }

    /// <summary>
    /// 前のページへ移動
    /// </summary>
    public void PreviousPage()
    {
        if (pages.Count == 0) return;
        ChangePage(-1);
    }

    /// <summary>
    /// 手動操作時のページ切り替え（自動切り替えを一時停止）
    /// </summary>
    public void ManualChangePage(int direction)
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

        // レイアウト更新を停止
        tableLayoutPanel.SuspendLayout();
        tableLayoutPanel.Controls.Clear();

        PageModel currentPage = pages[currentPageIndex];

        // ページの行・列を設定
        tableLayoutPanel.RowCount = currentPage.Rows;
        tableLayoutPanel.ColumnCount = currentPage.Columns;
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

        // ページ変更イベントを発火（UI更新用）
        PageChanged?.Invoke(currentPageIndex + 1, pages.Count);

        // レイアウト更新を再開
        tableLayoutPanel.ResumeLayout(true);
    }

    /// <summary>
    /// ページ自動切り替えを開始
    /// </summary>
    public void StartAutoSwitching()
    {
        pageSwitchTimer.Start();
    }

    /// <summary>
    /// ページ自動切り替えを停止
    /// </summary>
    public void StopAutoSwitching()
    {
        pageSwitchTimer.Stop();
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

変更点

✅ ボタンやUI要素を削除
✅ 外部からページ遷移できる NextPage() / PreviousPage() を提供
✅ ページ変更時に PageChanged イベントを発火
✅ 自動ページ送りの開始・停止を制御 (StartAutoSwitching() / StopAutoSwitching())


---

使用例

ボタン付きでページ切り替え

var paginatedPanel = new PaginatedTableLayoutPanel
{
    Dock = DockStyle.Fill
};

// ページ追加
paginatedPanel.AddPage(new PageModel(3, 2, new List<Control>
{
    new Label { Text = "Item 1", BackColor = Color.LightBlue, AutoSize = true },
    new Label { Text = "Item 2", BackColor = Color.LightGreen, AutoSize = true },
    new Label { Text = "Item 3", BackColor = Color.LightCoral, AutoSize = true }
}));

paginatedPanel.AddPage(new PageModel(2, 2, new List<Control>
{
    new Button { Text = "Button A", BackColor = Color.LightBlue },
    new Button { Text = "Button B", BackColor = Color.LightGreen }
}));

// UIボタンを作成
var btnNext = new Button { Text = "Next", Dock = DockStyle.Bottom };
var btnPrev = new Button { Text = "Previous", Dock = DockStyle.Bottom };

btnNext.Click += (s, e) => paginatedPanel.NextPage();
btnPrev.Click += (s, e) => paginatedPanel.PreviousPage();

// フォームに追加
var form = new Form();
form.Controls.Add(paginatedPanel);
form.Controls.Add(btnNext);
form.Controls.Add(btnPrev);

Application.Run(form);


---

拡張案

マウスホイールやキーボードでページ変更

ページ番号を直接入力してジャンプ

ページ遷移時のアニメーション追加



---

これで、自由にUIを構成できる汎用的なページネーション機能を持つ PaginatedTableLayoutPanel が完成しました！




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


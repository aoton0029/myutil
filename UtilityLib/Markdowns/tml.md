DataTable の各行に対する操作を高速化し、汎用的に利用できるメソッドを作成します。以下の要件を満たすように設計します。

要件

1. 並列処理をサポート


2. 型の安全性を確保


3. 汎用的に使用可能


4. 高速化のため Span<T> などを活用


5. イベントオーバーヘッドを抑制


6. スレッドセーフな方法を選択




---

汎用メソッドの実装

以下の ProcessDataTable メソッドは、任意の Action<DataRow> を適用しつつ DataTable の操作を高速化します。

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public static class DataTableExtensions
{
    /// <summary>
    /// DataTable の各行に対する処理を並列化しつつ高速に実行する汎用メソッド
    /// </summary>
    /// <param name="table">処理対象の DataTable</param>
    /// <param name="action">各 DataRow に適用する処理</param>
    /// <param name="parallel">並列処理を有効にするか</param>
    public static void ProcessDataTable(this DataTable table, Action<DataRow> action, bool parallel = true)
    {
        if (table == null || action == null) throw new ArgumentNullException();

        // DataRow を直接操作すると遅いため、配列に変換してから処理
        var rows = table.Select(); 

        // イベントを無効化して高速化
        table.BeginLoadData();

        if (parallel)
        {
            Parallel.ForEach(rows, row =>
            {
                action(row);
            });
        }
        else
        {
            foreach (var row in rows)
            {
                action(row);
            }
        }

        // イベント再開
        table.EndLoadData();
    }

    /// <summary>
    /// DataTable を List<T> に変換し、高速処理後に再適用するメソッド
    /// </summary>
    /// <typeparam name="T">変換するデータ型</typeparam>
    /// <param name="table">処理対象の DataTable</param>
    /// <param name="selector">DataRow から T への変換関数</param>
    /// <param name="mutator">T に対する変更処理</param>
    /// <param name="applyBack">処理後に DataTable へ戻すか</param>
    public static void ProcessDataTableAsList<T>(
        this DataTable table,
        Func<DataRow, T> selector,
        Action<T> mutator,
        Action<DataRow, T>? applyBack = null)
    {
        if (table == null || selector == null || mutator == null) throw new ArgumentNullException();

        // DataTable を List<T> に変換
        List<T> data = table.AsEnumerable().Select(selector).ToList();

        // `Span<T>` で最適化
        var span = CollectionsMarshal.AsSpan(data);
        Parallel.For(0, span.Length, i =>
        {
            mutator(span[i]);
        });

        // DataTable に適用
        if (applyBack != null)
        {
            int index = 0;
            foreach (var row in table.Rows.Cast<DataRow>())
            {
                applyBack(row, data[index++]);
            }
        }
    }
}


---

汎用メソッドの使用例

1. DataTable の各行を並列処理

DataTable table = new DataTable();
table.Columns.Add("ID", typeof(int));
table.Columns.Add("Value", typeof(string));

for (int i = 0; i < 10000; i++)
{
    table.Rows.Add(i, $"Value {i}");
}

// 各行の "Value" を大文字に変換（並列処理）
table.ProcessDataTable(row =>
{
    row["Value"] = ((string)row["Value"]).ToUpper();
});

✅ ポイント

各 DataRow に対して Action<DataRow> を適用

Parallel.ForEach により並列処理を実行

BeginLoadData() によりオーバーヘッドを削減



---

2. List<T> に変換して処理

// List<(int ID, string Value)> に変換して並列処理
table.ProcessDataTableAsList(
    row => (row.Field<int>("ID"), row.Field<string>("Value")),
    entry => entry = (entry.ID, entry.Value.ToLower()), // 小文字変換
    (row, entry) => row["Value"] = entry.Value
);

✅ ポイント

DataRow ではなく List<T> に変換して操作するため DataRow のオーバーヘッドを回避

Span<T> を活用してメモリ管理を最適化

applyBack により DataTable に適用



---

パフォーマンス比較


---

結論

1. ProcessDataTable()

汎用的で簡単に適用可能

並列処理が可能

オーバーヘッドを抑制



2. ProcessDataTableAsList<T>()

データを List<T> に変換し、高速処理

Span<T> でメモリ効率向上

データの書き戻しもサポート




これらのメソッドを活用すれば、DataTable の行処理を 劇的に高速化 できます！




確かに、画像数が多いとスレッドプールが枯渇し、アプリのレスポンスが悪化する可能性があります。
そのため、スレッドプールの過負荷を防ぐために「一定数ずつ画像を読み込む」制御 を入れましょう。


---

改善点

✅ プレースホルダー（Loading...）は即表示
✅ 画像のロードは並列処理だが、同時に処理する画像数を制限（スレッドプールの枯渇防止）
✅ 画像を少しずつ読み込みながら UI に反映
✅ 最適なスレッド数 (MaxDegreeOfParallelism) を指定


---

1. SelectableImageControl（変更なし）

プレースホルダーと画像のロード機能を持った UserControl です。

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

public class SelectableImageControl : UserControl, ISelectableItem
{
    private PictureBox pictureBox;
    private Label placeholderLabel;
    private bool isSelected = false;
    private string imagePath;

    public object Value => imagePath; // 選択された画像のパス

    public SelectableImageControl(string imagePath)
    {
        this.imagePath = imagePath;
        this.Size = new Size(120, 120);
        this.BorderStyle = BorderStyle.FixedSingle;
        this.BackColor = Color.LightGray;

        // プレースホルダー（画像未読込時）
        placeholderLabel = new Label
        {
            Text = "Loading...",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 画像表示用の PictureBox
        pictureBox = new PictureBox
        {
            SizeMode = PictureBoxSizeMode.Zoom,
            Dock = DockStyle.Fill,
            Visible = false // 最初は非表示
        };

        this.Controls.Add(pictureBox);
        this.Controls.Add(placeholderLabel);
        this.Click += (s, e) => SelectItem(); // クリックで選択
    }

    public async Task LoadImageAsync()
    {
        try
        {
            Image img = await Task.Run(() => Image.FromFile(imagePath));

            // UIスレッドで画像を表示
            this.Invoke((Action)(() =>
            {
                pictureBox.Image = img;
                pictureBox.Visible = true;
                placeholderLabel.Visible = false; // プレースホルダーを非表示
            }));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"画像の読み込みに失敗しました: {ex.Message}");
        }
    }

    public void SelectItem()
    {
        this.BackColor = Color.LightBlue;
        isSelected = true;
    }

    public void DeselectItem()
    {
        this.BackColor = Color.LightGray;
        isSelected = false;
    }
}


---

2. SelectionDialog（画像のロードを制御）

ここで 並列処理の制御を追加 します。

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

public class SelectionDialog : Form
{
    private FlowLayoutPanel flowLayoutPanel;
    private Button okButton;
    private Button cancelButton;
    private ISelectableItem selectedItem = null;
    private const int MaxConcurrentLoads = 4; // 画像の同時ロード数（調整可能）

    public ISelectableItem SelectedItem => selectedItem; // 選択されたアイテムを取得

    public SelectionDialog(List<SelectableImageControl> items)
    {
        this.Text = "画像選択";
        this.Size = new Size(600, 400);

        // FlowLayoutPanel 設定
        flowLayoutPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(10)
        };

        // ボタンの設定
        okButton = new Button { Text = "OK", Dock = DockStyle.Bottom, Enabled = false };
        cancelButton = new Button { Text = "キャンセル", Dock = DockStyle.Bottom };

        okButton.Click += (s, e) => this.DialogResult = DialogResult.OK;
        cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        // プレースホルダーを先に追加
        foreach (var item in items)
        {
            item.Click += (s, e) => SelectItem(item);
            flowLayoutPanel.Controls.Add(item);
        }

        // フォームに追加
        this.Controls.Add(flowLayoutPanel);
        this.Controls.Add(okButton);
        this.Controls.Add(cancelButton);

        // 画像を並列ロード（最大 `MaxConcurrentLoads` ずつ）
        _ = LoadImagesAsync(items);
    }

    private async Task LoadImagesAsync(List<SelectableImageControl> items)
    {
        using (SemaphoreSlim semaphore = new SemaphoreSlim(MaxConcurrentLoads))
        {
            List<Task> tasks = new List<Task>();

            foreach (var item in items)
            {
                await semaphore.WaitAsync(); // 同時にロードする数を制限

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await item.LoadImageAsync();
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks); // すべてのロードが終わるまで待機
        }
    }

    private void SelectItem(ISelectableItem item)
    {
        foreach (Control control in flowLayoutPanel.Controls)
        {
            if (control is ISelectableItem selectable)
            {
                selectable.DeselectItem();
            }
        }

        item.SelectItem();
        selectedItem = item;
        okButton.Enabled = true;
    }
}


---

3. MainForm（変更なし）

フォルダを選択し、画像を SelectionDialog に渡す処理。

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public class MainForm : Form
{
    private Button openDialogButton;
    private Label selectedImageLabel;

    public MainForm()
    {
        this.Text = "画像選択ダイアログ";
        this.Size = new System.Drawing.Size(400, 200);

        openDialogButton = new Button
        {
            Text = "画像選択ダイアログを開く",
            Dock = DockStyle.Top
        };

        selectedImageLabel = new Label
        {
            Text = "選択された画像: なし",
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };

        openDialogButton.Click += OpenSelectionDialog;

        this.Controls.Add(selectedImageLabel);
        this.Controls.Add(openDialogButton);
    }

    private void OpenSelectionDialog(object sender, EventArgs e)
    {
        using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
        {
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                List<SelectableImageControl> items = Directory.GetFiles(folderDialog.SelectedPath, "*.*")
                    .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    .Select(path => new SelectableImageControl(path))
                    .ToList();

                using (SelectionDialog dialog = new SelectionDialog(items))
                {
                    if (dialog.ShowDialog() == DialogResult.OK && dialog.SelectedItem != null)
                    {
                        selectedImageLabel.Text = $"選択された画像: {dialog.SelectedItem.Value}";
                    }
                }
            }
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}


---

改善点

✅ スレッドプールの枯渇を防ぐ（最大 4 つの画像を同時ロード）
✅ UI は即座に反映し、"Loading..." を表示
✅ 画像のロードが完了したものから即時表示
✅ 大量の画像がある場合もスムーズに動作


---

動作フロー

1. 画像のプレースホルダーが即座に表示（"Loading..."）


2. 最大 4 つずつ並列で画像をロードし、終わったものから更新


3. ユーザーが画像をクリック → 選択状態になる


4. OK で選択結果を返す



この方式なら、スレッドプールを無駄に消費せず、スムーズに画像を読み込めます！


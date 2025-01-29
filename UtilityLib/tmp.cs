using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public enum FilterOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,  // 文字列の部分一致検索
    StartsWith,
    EndsWith,
    In          // 複数値のリストとの一致
}

public class Filter<T>
{
    public string PropertyName { get; set; } // フィルター対象のプロパティ名
    public FilterOperator Operator { get; set; } // 比較演算子
    public T Value { get; set; } // フィルター値
    public List<T>? Values { get; set; } // 複数値（`In` 演算子用）

    public Expression<Func<TEntity, bool>> ToExpression<TEntity>()
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var member = Expression.Property(parameter, PropertyName);
        var constant = Expression.Constant(Value);

        Expression body = Operator switch
        {
            FilterOperator.Equals => Expression.Equal(member, constant),
            FilterOperator.NotEquals => Expression.NotEqual(member, constant),
            FilterOperator.GreaterThan => Expression.GreaterThan(member, constant),
            FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, constant),
            FilterOperator.LessThan => Expression.LessThan(member, constant),
            FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(member, constant),
            FilterOperator.Contains when typeof(T) == typeof(string) => Expression.Call(
                member, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constant),
            FilterOperator.StartsWith when typeof(T) == typeof(string) => Expression.Call(
                member, typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!, constant),
            FilterOperator.EndsWith when typeof(T) == typeof(string) => Expression.Call(
                member, typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!, constant),
            FilterOperator.In when Values != null && Values.Any() =>
                Expression.Call(typeof(Enumerable), "Contains", new[] { typeof(T) },
                    Expression.Constant(Values), member),
            _ => throw new NotSupportedException($"Unsupported operator: {Operator}")
        };

        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }
}


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




using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        var channel = new Channel<DataTable>();
        var cancellationTokenSource = new CancellationTokenSource();

        // データテーブルを取得するメソッドのリスト
        var dataTableTasks = new List<Func<Task<DataTable>>>
        {
            async () => await GetDataTableAsync("Table1"),
            async () => await GetDataTableAsync("Table2"),
            async () => await GetDataTableAsync("Table3")
        };

        // Producer: 各データ取得タスクを並列実行してチャネルに送信
        Task producer = Task.Run(async () =>
        {
            try
            {
                var tasks = new List<Task>();
                foreach (var getDataTable in dataTableTasks)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var dataTable = await getDataTable();
                        channel.Send(dataTable); // チャネルに送信
                    }, cancellationTokenSource.Token));
                }
                await Task.WhenAll(tasks); // 全タスクの完了を待機
                channel.Close(); // 全てのデータ送信が終わったらチャネルを閉じる
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Producer error: {ex.Message}");
                channel.Close();
            }
        });

        // Consumer: チャネルからデータを受信して処理
        Task consumer = Task.Run(async () =>
        {
            try
            {
                while (!channel.IsClosed || channel.HasData)
                {
                    var dataTable = await channel.ReceiveAsync();
                    Console.WriteLine($"Received DataTable: {dataTable.TableName} with {dataTable.Rows.Count} rows");
                }
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Channel closed.");
            }
        });

        // ProducerとConsumerのタスクを待機
        await Task.WhenAll(producer, consumer);
    }

    // サンプルのデータテーブルを取得する非同期メソッド
    private static async Task<DataTable> GetDataTableAsync(string tableName)
    {
        await Task.Delay(1000); // データ取得の遅延をシミュレート
        var dataTable = new DataTable(tableName);
        dataTable.Columns.Add("Id", typeof(int));
        dataTable.Columns.Add("Name", typeof(string));

        // サンプルデータの追加
        for (int i = 0; i < 5; i++)
        {
            dataTable.Rows.Add(i, $"{tableName}_Row{i}");
        }

        return dataTable;
    }
}

// Channelクラス (先ほどの拡張済みバージョンを使用)
public class Channel<T>
{
    private readonly BlockingCollection<T> _queue = new BlockingCollection<T>();

    public void Send(T item)
    {
        if (!_queue.IsAddingCompleted)
        {
            _queue.Add(item);
        }
        else
        {
            throw new InvalidOperationException("Channel is closed and cannot send data.");
        }
    }

    public T Receive()
    {
        return _queue.Take();
    }

    public async Task<T> ReceiveAsync()
    {
        return await Task.Run(() => _queue.Take());
    }

    public void Close()
    {
        _queue.CompleteAdding();
    }

    public bool IsClosed => _queue.IsAddingCompleted;
    public bool HasData => _queue.Count > 0;
}


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

public class AsyncProcessor
{
    // タスクの定義を保持
    private readonly List<Func<DataTable>> _methods;

    // 結果を格納する辞書
    private readonly ConcurrentDictionary<string, DataTable> _results;

    // 同時実行制御用のSemaphoreSlim
    private readonly SemaphoreSlim _semaphore;

    public AsyncProcessor(int maxDegreeOfParallelism)
    {
        _methods = new List<Func<DataTable>>();
        _results = new ConcurrentDictionary<string, DataTable>();
        _semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
    }

    // メソッドを登録
    public void AddMethod(string key, Func<DataTable> method)
    {
        _methods.Add(async () =>
        {
            await _semaphore.WaitAsync();
            try
            {
                var result = method();
                _results[key] = result;
                return result;
            }
            finally
            {
                _semaphore.Release();
            }
        });
    }

    // 非同期並列処理を実行
    public async Task ExecuteAsync()
    {
        var tasks = new List<Task>();

        foreach (var method in _methods)
        {
            tasks.Add(Task.Run(async () =>
            {
                await method();
            }));
        }

        await Task.WhenAll(tasks);
    }

    // 結果を取得
    public ConcurrentDictionary<string, DataTable> GetResults()
    {
        return _results;
    }
}

public class Program
{
    // シミュレーション用の同期メソッド
    public static DataTable GetDataByOrderNumber(string orderNumber)
    {
        var table = new DataTable();
        table.Columns.Add("OrderNumber");
        table.Rows.Add(orderNumber);
        return table;
    }

    public static DataTable GetDataByOrderKeyAndDetailNumber(int orderKey, int detailNumber)
    {
        var table = new DataTable();
        table.Columns.Add("OrderKey");
        table.Columns.Add("DetailNumber");
        table.Rows.Add(orderKey, detailNumber);
        return table;
    }

    public static DataTable GetDataBySerialNumber(string serialNumber)
    {
        var table = new DataTable();
        table.Columns.Add("SerialNumber");
        table.Rows.Add(serialNumber);
        return table;
    }

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Processing data...");

        var processor = new AsyncProcessor(2); // 同時実行数を2に制限

        // メソッドを登録
        processor.AddMethod("OrderNumber", () => GetDataByOrderNumber("12345"));
        processor.AddMethod("OrderKeyDetail", () => GetDataByOrderKeyAndDetailNumber(1, 2));
        processor.AddMethod("SerialNumber", () => GetDataBySerialNumber("SN-001"));

        // 非同期処理を実行
        await processor.ExecuteAsync();

        // 結果を取得して出力
        var results = processor.GetResults();
        foreach (var key in results.Keys)
        {
            Console.WriteLine($"Result for {key}:");
            foreach (DataRow row in results[key].Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    Console.Write(item + " ");
                }
                Console.WriteLine();
            }
        }

        Console.WriteLine("Processing completed.");
    }
}



using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

public class AsyncProcessor
{
    // タスクの定義を保持
    private readonly List<(string Key, Func<DataTable> Method)> _methods;

    // 結果を格納する辞書
    private readonly ConcurrentDictionary<string, DataTable> _results;

    // 同時実行制御用のSemaphoreSlim
    private readonly SemaphoreSlim _semaphore;

    public AsyncProcessor(int maxDegreeOfParallelism)
    {
        _methods = new List<(string, Func<DataTable>)>();
        _results = new ConcurrentDictionary<string, DataTable>();
        _semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
    }

    // メソッドを登録
    public void AddMethod(string key, Func<DataTable> method)
    {
        _methods.Add((key, method));
    }

    // 非同期並列処理を実行
    public async Task ExecuteAsync()
    {
        var tasks = new List<Task>();

        foreach (var (key, method) in _methods)
        {
            tasks.Add(Task.Run(async () =>
            {
                await _semaphore.WaitAsync();
                try
                {
                    var result = method();
                    _results[key] = result;
                }
                finally
                {
                    _semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    // 結果を取得
    public ConcurrentDictionary<string, DataTable> GetResults()
    {
        return _results;
    }
}

public class Program
{
    // シミュレーション用の同期メソッド
    public static DataTable GetDataByOrderNumber(string orderNumber)
    {
        var table = new DataTable();
        table.Columns.Add("OrderNumber");
        table.Rows.Add(orderNumber);
        return table;
    }

    public static DataTable GetDataByOrderKeyAndDetailNumber(int orderKey, int detailNumber)
    {
        var table = new DataTable();
        table.Columns.Add("OrderKey");
        table.Columns.Add("DetailNumber");
        table.Rows.Add(orderKey, detailNumber);
        return table;
    }

    public static DataTable GetDataBySerialNumber(string serialNumber)
    {
        var table = new DataTable();
        table.Columns.Add("SerialNumber");
        table.Rows.Add(serialNumber);
        return table;
    }

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Processing data...");

        var processor = new AsyncProcessor(2); // 同時実行数を2に制限

        // メソッドを登録
        processor.AddMethod("OrderNumber", () => GetDataByOrderNumber("12345"));
        processor.AddMethod("OrderKeyDetail", () => GetDataByOrderKeyAndDetailNumber(1, 2));
        processor.AddMethod("SerialNumber", () => GetDataBySerialNumber("SN-001"));

        // 非同期処理を実行
        await processor.ExecuteAsync();

        // 結果を取得して出力
        var results = processor.GetResults();
        foreach (var key in results.Keys)
        {
            Console.WriteLine($"Result for {key}:");
            foreach (DataRow row in results[key].Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    Console.Write(item + " ");
                }
                Console.WriteLine();
            }
        }

        Console.WriteLine("Processing completed.");
    }
}


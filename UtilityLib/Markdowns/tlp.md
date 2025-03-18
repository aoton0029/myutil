ファイルロックを導入して、複数プロセスやスレッドが同じファイルに同時に書き込むことを防ぐようにします。FileStream の FileShare.None を使用して排他ロックを実装します。


---

ファイルロック対応のポイント

1. ロックモード

FileShare.None を指定することで、他のプロセスやスレッドが同時にアクセスできないようにする。

読み取り専用のロックをかけたい場合は FileShare.Read を使用する。



2. ロック解除

書き込みが完了したら、FileStream.Dispose() によりロックを解除。



3. ロック競合時のリトライ

既にロックされている場合にリトライする機能を導入。





---

修正後の FileManager

using System;
using System.IO;
using System.Threading.Tasks;

public class FileManager
{
    private readonly BackupManager backupManager;
    private readonly ILogger logger;

    public FileManager(BackupManager backupManager, ILogger logger)
    {
        this.backupManager = backupManager;
        this.logger = logger;
    }

    public async Task<bool> SaveFileWithLockAsync(string filePath, string content, int retryCount = 3, int retryDelayMs = 1000)
    {
        string backupPath = await backupManager.CreateBackupAsync(filePath);
        if (backupPath != null)
        {
            logger.Log($"Backup created: {backupPath}");
        }

        for (int attempt = 0; attempt < retryCount; attempt++)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(fileStream))
                {
                    await writer.WriteAsync(content);
                }

                logger.Log($"File saved with lock: {filePath}");
                return true;
            }
            catch (IOException ex) when (attempt < retryCount - 1)
            {
                logger.Log($"File is locked, retrying... {attempt + 1}/{retryCount}");
                await Task.Delay(retryDelayMs);
            }
            catch (Exception ex)
            {
                logger.Log($"Error saving file: {ex.Message}");
                return false;
            }
        }

        logger.Log($"Failed to save file after {retryCount} attempts: {filePath}");
        return false;
    }
}


---

修正後の Program.cs

using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string filePath = "test.txt";
        string backupDir = "backups";

        ILogger logger = new ConsoleLogger();
        BackupManager backupManager = new BackupManager(backupDir);
        FileManager fileManager = new FileManager(backupManager, logger);

        string content = "File content with lock " + DateTime.Now;
        await fileManager.SaveFileWithLockAsync(filePath, content);
    }
}


---

改善点と追加機能

1. ファイルロック競合時のリトライ

すぐに書き込みできない場合、最大 retryCount 回リトライする。

リトライ間隔は retryDelayMs で調整（デフォルト1000ms）。



2. 書き込み処理を StreamWriter に委譲

FileStream を StreamWriter に渡し、安全にテキストを書き込む。



3. 例外処理の強化

IOException をキャッチしてリトライ。

その他の例外はログを記録して処理を終了。





---

拡張案

1. 非同期排他制御

Mutex または SemaphoreSlim を導入し、ファイル単位で非同期ロックを管理する。



2. ロック管理システム

ファイルのロック状態をデータベースやメモリ（Dictionary<string, bool>）で管理し、複数プロセスからのアクセスを調整。



3. ネットワークファイルシステム対応

リモートファイルサーバー上のファイルをロックする場合、分散ロック（例: Redis Lock）を導入。





---

これで、ファイルロックを適用しつつ、スムーズな書き込みが可能になります！




ファイル書き込み中のロック対策

ファイルが他のプロセスによってロックされている場合の対策として、以下の実装を行います。

対策方法

1. ファイルストリーム (FileStream) を使って書き込み

FileMode.Create を使用して、既存のファイルを置き換える。

FileAccess.Write と FileShare.None を指定し、他のプロセスが同時にアクセスできないようにする。



2. リトライ機能

IOException（ファイルロック）発生時に、一定回数リトライ。

一定時間 (Thread.Sleep) 待機しながら再試行。



3. エラーハンドリング

リトライ回数を超えた場合、適切なメッセージを表示。





---

修正後の Project クラス

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

public class Project
{
    public string Name { get; set; } = "Untitled";
    public string FilePath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ProjectSetting> StartupSettings { get; set; } = new List<ProjectSetting>();
    public List<ProjectSetting> ShutdownSettings { get; set; } = new List<ProjectSetting>();

    private string lastSavedData = string.Empty;

    public Project()
    {
        for (int i = 0; i < 10; i++)
        {
            StartupSettings.Add(new ProjectSetting { Key = $"Startup {i + 1}", Value = "Default" });
            ShutdownSettings.Add(new ProjectSetting { Key = $"Shutdown {i + 1}", Value = "Default" });
        }
    }

    public void Load(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                MessageBox.Show("指定されたファイルが見つかりません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string json = File.ReadAllText(path);
            var loadedProject = JsonSerializer.Deserialize<Project>(json);
            if (loadedProject != null)
            {
                Name = loadedProject.Name;
                FilePath = path;
                Description = loadedProject.Description;
                StartupSettings = loadedProject.StartupSettings;
                ShutdownSettings = loadedProject.ShutdownSettings;
                lastSavedData = json;
            }
        }
        catch (IOException)
        {
            MessageBox.Show("ファイルがロックされています。別のプログラムで開かれていないか確認してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ファイルの読み込みに失敗しました。\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            MessageBox.Show("ファイルパスが設定されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // バックアップを作成
        Backup();

        // ファイル書き込み処理 (ロック対策付き)
        bool success = TryWriteToFile(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));

        if (!success)
        {
            MessageBox.Show("ファイルの保存に失敗しました。\nファイルがロックされている可能性があります。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void SaveAs(string path)
    {
        // バックアップを作成
        Backup();

        // ファイル書き込み処理 (ロック対策付き)
        bool success = TryWriteToFile(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));

        if (success)
        {
            FilePath = path;
        }
        else
        {
            MessageBox.Show("ファイルの保存に失敗しました。\nファイルがロックされている可能性があります。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryWriteToFile(string path, string content, int retryCount = 5, int delayMilliseconds = 500)
    {
        for (int attempt = 0; attempt < retryCount; attempt++)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(content);
                }
                return true;
            }
            catch (IOException)
            {
                if (attempt < retryCount - 1)
                {
                    Thread.Sleep(delayMilliseconds); // ロックが解除されるのを待つ
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }

    public void Backup()
    {
        try
        {
            if (!string.IsNullOrEmpty(FilePath) && File.Exists(FilePath))
            {
                string backupPath = FilePath + ".bak";
                File.Copy(FilePath, backupPath, true);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"バックアップの作成に失敗しました。\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public bool HasUnsavedChanges()
    {
        return JsonSerializer.Serialize(this) != lastSavedData;
    }
}


---

改善点

1. TryWriteToFile() でロック対策

FileStream を使用し、FileMode.Create で新規作成 or 置き換え

FileAccess.Write & FileShare.None で他のプロセスのアクセスをブロック

IOException が発生した場合、最大 5 回リトライ (Thread.Sleep(500ms) 待機)



2. 保存前にバックアップを作成

.json.bak ファイルを生成し、保存失敗時に復旧可能



3. エラーハンドリング

IOException 発生時に適切なメッセージを表示





---

追加の拡張案

1. ロック解除の試行回数と待機時間を設定から変更可能に

retryCount と delayMilliseconds を設定可能にする

高速な処理を求める場合は待機時間を短縮、確実な保存を求める場合はリトライ回数を増やす



2. バックグラウンドで非同期保存

Task.Run() を使用して非同期にファイル書き込みを実行し、UI のフリーズを防ぐ



3. エラーログの保存

log.txt にエラー内容を記録し、後で確認できるようにする





---

この実装により、ファイルがロックされている場合でも、リトライ処理で適切に保存を試みることができ、万が一失敗してもバックアップから復元できるようになります！




DDD（ドメイン駆動設計）アーキテクチャにおいて、DI（依存性注入）を使わずに HttpClient を利用する REST API クライアントを実装する方法を考えます。

設計方針

依存性注入を使わない

HttpClient を直接インスタンス化する（適切に管理）


DDD を意識したレイヤー分割

Domain（ドメイン層）: ビジネスロジック

Application（アプリケーション層）: REST API クライアントのユースケース

Infrastructure（インフラ層）: REST API へのアクセス処理




---

実装

1. ドメイン層

ドメイン層には、API から取得するデータのモデルを定義します。

namespace Domain.Models
{
    public class WeatherInfo
    {
        public string City { get; }
        public float Temperature { get; }

        public WeatherInfo(string city, float temperature)
        {
            City = city;
            Temperature = temperature;
        }
    }
}


---

2. インフラ層

HttpClient を直接管理し、API にアクセスするクラスを作成します。

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Models;

namespace Infrastructure
{
    public class WeatherApiClient
    {
        private readonly HttpClient _httpClient;

        public WeatherApiClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.weather.com/v1/")
            };
        }

        public async Task<WeatherInfo> GetWeatherAsync(string city)
        {
            var response = await _httpClient.GetAsync($"weather?city={city}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var weatherData = JsonSerializer.Deserialize<WeatherDto>(json);

            return new WeatherInfo(weatherData.City, weatherData.Temperature);
        }

        private class WeatherDto
        {
            public string City { get; set; }
            public float Temperature { get; set; }
        }
    }
}


---

3. アプリケーション層

インフラ層の WeatherApiClient を使って、天気情報を取得するユースケースを定義します。

using System.Threading.Tasks;
using Domain.Models;
using Infrastructure;

namespace Application
{
    public class WeatherService
    {
        private readonly WeatherApiClient _apiClient;

        public WeatherService()
        {
            _apiClient = new WeatherApiClient();
        }

        public async Task<WeatherInfo> GetWeatherForCity(string city)
        {
            return await _apiClient.GetWeatherAsync(city);
        }
    }
}


---

4. UI 層

コンソールアプリケーションで WeatherService を利用して天気情報を取得します。

using System;
using System.Threading.Tasks;
using Application;

class Program
{
    static async Task Main(string[] args)
    {
        var weatherService = new WeatherService();

        Console.Write("都市名を入力してください: ");
        string city = Console.ReadLine();

        try
        {
            var weather = await weatherService.GetWeatherForCity(city);
            Console.WriteLine($"{weather.City} の現在の気温は {weather.Temperature}°C です。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"エラーが発生しました: {ex.Message}");
        }
    }
}


---

この設計のポイント

1. DI を使わずに HttpClient を直接インスタンス化

WeatherApiClient 内で HttpClient を保持し、API のリクエストを管理。

using を使わないのは、HttpClient を適切に再利用するため（ソケットの再生成を防ぐ）。



2. DDD の基本構成

ドメイン層 (Domain): WeatherInfo クラスでビジネスロジックを表現。

アプリケーション層 (Application): WeatherService がユースケースを管理。

インフラ層 (Infrastructure): WeatherApiClient が外部 API との通信を担当。



3. ユースケースの独立性

WeatherService は WeatherApiClient のインスタンスを直接持つが、DI を使わずに管理。





---

拡張案

HttpClient のライフサイクル管理

HttpClientFactory を使うことで、適切なライフサイクル管理を行う（ただし DI が必要）。

Singleton パターンを導入して HttpClient をアプリ全体で共通化。


エラーハンドリングの強化

API エラー時のリトライ機能（Polly ライブラリを使う）。

try-catch を使って、ネットワークエラーや API 仕様変更に対応。


非同期処理の並列化

Task.WhenAll() を使って複数都市の天気を同時取得。




---

この設計はシンプルながらも DDD の基本に沿った構成になっています。
DI なしで HttpClient を管理しつつ、レイヤーを分けて責務を明確にしました。






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


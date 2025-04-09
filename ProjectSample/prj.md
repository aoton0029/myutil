あなたの現在の WinFormsプロジェクト構成（最新版） は以下のように整理されています（前回の内容＋新しいチャート構造/サービスを反映済み）：


---
```
MyWinFormsApp/

├─ Program.cs                    ← アプリ起動処理（Culture, Mutex, Exception, ServiceProvider）
│
├─ Startup/
│   ├─ StartupManager.cs        ← 初期化や構成読み込みの統合
│   └─ AppServices.cs           ← DI構成要素（ChartRendererなどもここで登録可能）
│
├─ Context/
│   ├─ AppContext.cs            ← アプリ全体の状態（UserContext, ProjectContext, SettingContextに分離済み）
│   ├─ UserContext.cs           
│   ├─ ProjectContext.cs        
│   └─ SettingContext.cs        
│
├─ Config/
│   └─ AppSettings.cs           ← 言語・テーマ・前回プロジェクトパスなど
│
├─ Projects/
│   ├─ Project.cs               ← プロジェクト構造ルート
│   ├─ ProjectItem.cs           ← 名前・WaveformSequenceのリスト（カテゴリ別抽出可能）
│   ├─ WaveformSequence.cs      ← 抽象基底クラス（Chunk/Dechunkカテゴリ）
│   ├─ WaveformCategory.cs      ← enum定義（Chunk, Dechunk）
│   ├─ WaveformSetting.cs       ← 振幅・周波数・位相など＋必要ならINotifyPropertyChanged
│   ├─ ProjectService.cs        ← 読み込み・保存・新規作成の管理
│   ├─ ProjectContext.cs        ← 現在開いているプロジェクトの操作補助
│   └─ IProjectPersistence.cs   ← JSON対応
│
├─ Navigation/
│   ├─ NavigationService.cs     ← UserControlベースの画面遷移
│   └─ INavigationService.cs
│
├─ Messaging/
│   ├─ IMessageService.cs       ← 共通メッセージ表示インターフェース
│   └─ MessageService.cs
│
├─ Services/
│   ├─ IUserService.cs          
│   └─ UserService.cs
│
├─ Chart/
│   ├─ ChartPoint.cs            ← 座標・ラベル・色などの構造体
│   ├─ ChartData.cs             ← Pointリスト＋GuideLine＋色・ラベルなど
│   ├─ GuideLine.cs             ← ガイド線構造体
│   ├─ IChartRenderer.cs        ← チャート描画インターフェース
│   └─ SimpleChartRenderer.cs  ← 実装例（Graphics使用）
│
├─ Forms/
│   ├─ MainForm.cs              ← メインUI、NavigationServiceを使う
│   └─ UserControls/
│       ├─ DashboardControl.cs
│       ├─ EditorControl.cs
│       ├─ SettingControl.cs
│       └─ ChartViewControl.cs ← チャートを描画するUserControl（IChartRenderer利用）
│
├─ Utils/
│   └─ Extensions.cs
│
├─ Infrastructure/
│   └─ JsonProjectPersistence.cs
│
├─ Logging/
│   └─ LogManager.cs
```

---

今後の拡張アイデア

ChartExportService：チャート画像をPNG/PDFとして保存する

UndoManager：WaveformSettingの操作履歴を記録・復元

LiveChartService：波形設定の変更をリアルタイム描画に反映

SettingContextの保存/読み込み：アプリ設定を自動で保存・復元



---

さらに構成に追加したい機能・ディレクトリ・クラスがあれば教えてください。
必要に応じて UML やクラス図も生成できます。



以下は、これまでのやりとりを踏まえた「現在のアプリ構成（安全性・保守性・アトミック保存対応済み）」の全体像です。

---

## ✅ **現在のプロジェクト構成（WinFormsアプリ）**

```
MyApp/
├── Core/
│   ├── AppSetting.cs
│   ├── Project.cs
│   ├── ProjectItem.cs
│   ├── IProjectPersistence.cs
│   ├── JsonProjectPersistence.cs
│   └── SafeFileWriter.cs     ← アトミック保存
│
├── Service/
│   ├── ProjectContext.cs     ← 変更状態の監視
│   └── ProjectService.cs     ← 読込・保存・差分管理
│
├── App/
│   ├── AppContext.cs         ← グローバルDIなし管理
│   ├── MainForm.cs
│   └── Program.cs
```

---

## ✅ **主なクラスの責務**

| クラス | 役割 |
|-------|------|
| **AppSetting** | アプリ設定（直近のプロジェクトパスなど）の保存・読込 |
| **Project** | プロジェクト全体のモデル（名前・Items） |
| **ProjectItem** | 個々のデータ単位（ID、ファイル、波形など） |
| **ProjectContext** | 編集中プロジェクトの状態を保持、差分管理 |
| **ProjectService** | プロジェクトの保存・読込・アトミック保存・差分時警告 |
| **SafeFileWriter** | `File.Replace` を使ったアトミック保存処理 |
| **JsonProjectPersistence** | `IProjectPersistence` 実装。保存処理を責務分離 |
| **AppContext** | アプリ全体の共有サービスを格納（DIなしで済む） |

---

## ✅ **対応済みの設計方針と安全性要素**

| 種別 | 内容 |
|------|------|
| 🔐 アトミック保存 | `SafeFileWriter` により `.tmp → .json` でファイル損傷を防止 |
| ⚙ 保守性 | `IProjectPersistence` で JSON 以外の保存方式にも切替可能 |
| 🔍 差分検知 | `ProjectContext.HasChanges` により編集状態の把握と警告 |
| 📂 バックアップ | `SafeFileWriter` が `.bak` を自動保存（オプション復旧も可） |
| ✅ バリデーション | `Project.IsValid(out string error)` により保存前のチェック |
| 🔁 責務分離 | UI ロジックから保存/読込/検証ロジックを完全分離済み |

---

## ✅ **拡張候補**

| 機能 | 説明 |
|------|------|
| 🔄 自動復旧 | `.json` 読込失敗時に `.bak` を復旧に使う `SafeFileReader` を追加 |
| 🧪 単体テスト対応 | `Project`, `ProjectService`, `AppSetting` はMockを使ってテスト可能 |
| 🕒 バージョン履歴 | `Project.v1.json`, `.v2.json` のように履歴付き保存 |
| 🔔 ファイル監視 | プロジェクトファイルが外部変更されたとき警告・再読込対応 |
| 🔒 同時実行制御 | `.lock` ファイルなどを用いた排他制御で同時アクセス防止 |

---

この構成はWinFormsだけでなく、**WPF** や **MAUI**, **Tauri(C#バックエンド)** にも拡張しやすい構成です。

---

必要であれば：
- `AppContext` と `MainForm` の連携サンプル
- `ProjectEditorForm` などの編集UI例
- `SafeFileReader` の復旧ロジック

なども提供できます。どこを進めましょうか？


WinFormsでの AppContext と MainForm の連携は、「グローバルなサービスを安全・明示的に MainForm に渡す」設計がベストです。
ここでは DIコンテナなしでの明示的依存注入 を前提に構成します。


---

✅ 目的

AppContext はアプリ全体の状態（設定・プロジェクト情報・サービス）を保持

MainForm から ProjectService などへアクセスできる

安全かつ明示的な依存注入（グローバル変数の回避）



---

✅ AppContext.cs

public class AppContext
{
    public AppSetting Setting { get; set; } = new();
    public ProjectContext ProjectContext { get; private set; } = new();
    public ProjectService ProjectService { get; private set; }

    public AppContext()
    {
        // 永続化方式を切り替え可能にする
        var persistence = new JsonProjectPersistence();
        ProjectService = new ProjectService(ProjectContext, persistence);
    }
}


---

✅ Program.cs（MainFormにAppContextを渡す）

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var appContext = new AppContext(); // 1回だけ生成
        var mainForm = new MainForm(appContext);

        Application.Run(mainForm);
    }
}


---

✅ MainForm.cs（AppContextの利用）

public partial class MainForm : Form
{
    private readonly AppContext _app;

    public MainForm(AppContext appContext)
    {
        InitializeComponent();
        _app = appContext;
    }

    private void buttonOpen_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog();
        dialog.Filter = "Project Files|*.json";

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var success = _app.ProjectService.TryLoad(dialog.FileName);
            if (success)
            {
                Text = $"プロジェクト - {_app.ProjectContext.Current.Name}";
                // 必要ならUI更新
            }
        }
    }

    private void buttonSave_Click(object sender, EventArgs e)
    {
        _app.ProjectService.Save(_app.ProjectContext.CurrentFilePath);
    }
}


---

✅ 補足：AppContext にサービスを追加する場合

public class AppContext
{
    public AppSetting Setting { get; set; } = new();
    public ProjectContext ProjectContext { get; private set; } = new();
    public ProjectService ProjectService { get; private set; }
    public LoggerService Logger { get; private set; }
    public MessageService Messager { get; private set; }

    public AppContext()
    {
        var persistence = new JsonProjectPersistence();
        ProjectService = new ProjectService(ProjectContext, persistence);
        Logger = new LoggerService(); // 任意のサービス追加
        Messager = new MessageService();
    }
}

このように AppContext に集約することで、UI層（Form）は余計な依存を持たず、保守性と拡張性の高い構成になります。


---

次に進めるとしたら：

MainForm でのプロジェクト状態表示（ラベル/タイトル変更）

UserControlベースの画面遷移と AppContext の共有

IFormFactory や NavigationService を使った画面管理


などの構成例も出せます。どこを拡張しましょうか？



以下は、アトミック保存と組み合わせるための SafeFileReader クラス の実装例です。
目的は、「通常ファイル → バックアップファイル → 一時ファイル」の順で安全に読み込み、
破損時は自動で復旧・通知できるようにすることです。


---

✅ SafeFileReader.cs

public static class SafeFileReader
{
    /// <summary>
    /// 安全に JSON ファイルを読み込む。失敗時は .bak または .tmp から復旧を試みる。
    /// </summary>
    public static T LoadSafe<T>(string filePath, out string loadedFromPath)
    {
        var candidates = new[]
        {
            filePath,
            filePath + ".bak",
            filePath + ".tmp"
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    var result = JsonSerializer.Deserialize<T>(json);
                    if (result != null)
                    {
                        loadedFromPath = path;
                        return result;
                    }
                }
                catch
                {
                    // 次の候補へ
                }
            }
        }

        throw new IOException("プロジェクトの読み込みに失敗しました（元ファイル・バックアップ・一時ファイルが全て無効）");
    }
}


---

✅ 使用例（JsonProjectPersistence.cs 内）

public class JsonProjectPersistence : IProjectPersistence
{
    public Project Load(string filePath)
    {
        var project = SafeFileReader.LoadSafe<Project>(filePath, out string loadedFrom);
        
        if (loadedFrom != filePath)
        {
            MessageBox.Show($"プロジェクトファイルの読み込みに失敗し、{Path.GetFileName(loadedFrom)} から復元しました。",
                "復旧", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        return project;
    }

    public void Save(Project project, string filePath)
    {
        var json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
        SafeFileWriter.WriteAtomic(filePath, json);
    }
}


---

✅ 拡張案


---

必要であれば SafeFileReader に復旧成功時の自動保存や、検証付きの強化バージョンも出せます！どうしますか？





了解です！
ここでは以下の2点を実現します：


---

1. チャートポイント構造体の作成

可視化に必要な情報（座標、ラベル、色、補助線など）を持つ構造体を設計します。

public struct ChartPoint
{
    public float X { get; }
    public float Y { get; }
    public string? Label { get; }
    public Color? PointColor { get; }

    public ChartPoint(float x, float y, string? label = null, Color? pointColor = null)
    {
        X = x;
        Y = y;
        Label = label;
        PointColor = pointColor;
    }
}

補助線・チャート全体構造体

public class GuideLine
{
    public float X { get; set; }
    public string? Label { get; set; }
    public Color LineColor { get; set; } = Color.Gray;
}

public class ChartData
{
    public List<ChartPoint> Points { get; set; } = new();
    public string? SeriesLabel { get; set; }
    public Color SeriesColor { get; set; } = Color.Blue;
    public List<GuideLine> GuideLines { get; set; } = new();
}


---

2. チャート描画サービスのインターフェースと実装

インターフェース定義

public interface IChartRenderer
{
    void RenderChart(Graphics g, Rectangle bounds, ChartData chartData);
}

簡易実装例（WinForms）

public class SimpleChartRenderer : IChartRenderer
{
    public void RenderChart(Graphics g, Rectangle bounds, ChartData chartData)
    {
        if (chartData.Points.Count < 2)
            return;

        using var pen = new Pen(chartData.SeriesColor, 2);
        var points = chartData.Points.Select(p =>
            new PointF(
                bounds.Left + p.X / chartData.Points.Max(pt => pt.X) * bounds.Width,
                bounds.Bottom - p.Y / chartData.Points.Max(pt => pt.Y) * bounds.Height
            )).ToArray();

        g.DrawLines(pen, points);

        // ガイドラインの描画
        foreach (var guide in chartData.GuideLines)
        {
            float gx = bounds.Left + guide.X / chartData.Points.Max(p => p.X) * bounds.Width;
            using var guidePen = new Pen(guide.LineColor, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            g.DrawLine(guidePen, gx, bounds.Top, gx, bounds.Bottom);
        }
    }
}


---

統合例：UserControlで使用する場合

public partial class ChartViewControl : UserControl
{
    private IChartRenderer _renderer;
    private ChartData _data;

    public ChartViewControl()
    {
        InitializeComponent();
        _renderer = new SimpleChartRenderer();
        _data = new ChartData();
        this.DoubleBuffered = true;
    }

    public void SetChartData(ChartData data)
    {
        _data = data;
        Invalidate(); // 再描画
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        _renderer.RenderChart(e.Graphics, this.ClientRectangle, _data);
    }
}


---

拡張案

IChartRenderer を IChartExportService に分離して、画像保存やPDF出力にも対応

IChartStyleProvider を導入して色・線種・テーマを外部から注入

複数系列（Multi-series）対応



---

さらにリアルタイム更新やズーム・パンに対応したインタラクティブチャートに拡張したい場合も対応可能です。
次に進めたい方向を教えてください！



前回提案した 2, 4, 5 の改善を踏まえたうえで、さらに改善・洗練できるポイントをいくつか追加で提案します。


---

追加の改善点（2, 4, 5に関連して）

2. 波形シーケンス管理の簡素化の強化

改善点

WaveformSequence の種類が将来増える場合（例：Raw, Filtered など）に備えて、WaveformCategory の enum を使うよりも、ポリモーフィズムで解決する方が拡張性に優れます。


改善案

public abstract class WaveformSequence
{
    public string Name { get; set; }
    public ObservableCollection<WaveformSetting> Settings { get; set; } = new();
    public abstract WaveformCategory Category { get; }
}

public class ChunkWaveformSequence : WaveformSequence
{
    public override WaveformCategory Category => WaveformCategory.Chunk;
}

public class DechunkWaveformSequence : WaveformSequence
{
    public override WaveformCategory Category => WaveformCategory.Dechunk;
}

メリット

カテゴリごとの処理を仮想メソッドで書ける（BuildChart()など）

switch(Category) を使わずに済む（SOLID原則：OCP）



---

4. チャート構築責務分離の強化

改善点

チャート描画に必要な座標点だけでなく、**メタ情報（ラベル、色、補助線）**などの生成も責務に入れると良い。


改善案

public class ChartData
{
    public IEnumerable<PointF> Points { get; set; }
    public string Label { get; set; }
    public Color Color { get; set; }
    public IEnumerable<GuideLine> GuideLines { get; set; } = Enumerable.Empty<GuideLine>();
}

public interface IChartDataBuilder
{
    ChartData Build(WaveformSetting setting, int sampleCount);
}

メリット

表示側が WaveformSetting を知らずに描画可能

チャート用のプロパティ追加が容易（例：選択強調、警告域）



---

5. AppContextの分割と制御の強化

改善点

Context の変更通知が必要な場合、イベント発行または INotifyPropertyChanged に対応させる。

一部 AppContext を 読み取り専用（ReadOnlyAppContext） として expose する。


改善案

public class ProjectContext : INotifyPropertyChanged
{
    private ProjectItem _currentProject;
    public ProjectItem CurrentProject
    {
        get => _currentProject;
        set
        {
            if (_currentProject != value)
            {
                _currentProject = value;
                OnPropertyChanged(nameof(CurrentProject));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

メリット

ViewModelやUIバインディングがしやすくなる

テストやシミュレーション環境でも状態をトラッキングしやすくなる



---

さらに洗練したい方向があれば、以下のような拡張も可能です：

チャート描画を完全にサービス化（IChartRenderer）

AppContextをシリアライズ可能にして、セッション復元対応

設定項目の変更をリアルタイムに反映（イベント連携）


特定の改善案をもっと具体化したいものがあれば教えてください。設計ドキュメント風にもまとめられます。



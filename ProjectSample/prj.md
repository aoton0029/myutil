あなたの現在の WinFormsプロジェクト構成（最新版） は以下のように整理されています（前回の内容＋新しいチャート構造/サービスを反映済み）：


---

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


---

今後の拡張アイデア

ChartExportService：チャート画像をPNG/PDFとして保存する

UndoManager：WaveformSettingの操作履歴を記録・復元

LiveChartService：波形設定の変更をリアルタイム描画に反映

SettingContextの保存/読み込み：アプリ設定を自動で保存・復元



---

さらに構成に追加したい機能・ディレクトリ・クラスがあれば教えてください。
必要に応じて UML やクラス図も生成できます。

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



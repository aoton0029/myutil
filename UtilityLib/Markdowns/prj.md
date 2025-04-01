素晴らしい方向性です！C#で各クラスに「Create（新規作成）」と「Restore（復元）」の戦略を明確にすることで、初期化と永続化処理を明示的に分離でき、拡張・保守性が大幅に向上します。

以下はそれぞれのクラスに対する Create と Restore の設計戦略です。


---

1. ProjectContext

Create:

NewProject(name) メソッドで Project を新規作成。


Restore:

LoadProject(string path) で外部ファイル（JSONなど）から復元。


public class ProjectContext
{
    public Project? CurrentProject { get; private set; }

    public void CreateNewProject(string name)
    {
        CurrentProject = Project.Create(name);
    }

    public void RestoreFromFile(string path)
    {
        string json = File.ReadAllText(path);
        CurrentProject = Project.Restore(json);
    }
}


---

2. Project

Create:

名前付きコンストラクタで空のプロジェクト作成。


Restore:

JSON文字列などを元に Deserialize。


public class Project
{
    public string Name { get; set; }
    public List<ProjectItem> Items { get; set; } = new();

    public static Project Create(string name)
    {
        return new Project { Name = name };
    }

    public static Project Restore(string json)
    {
        return JsonSerializer.Deserialize<Project>(json) ?? throw new Exception("Invalid project data");
    }

    public string Save()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }
}


---

3. ProjectItem

Create:

ProjectCategory を指定して10個の波形と Config を生成。


Restore:

JSONデータやデシリアライズにより状態を復元。


public class ProjectItem
{
    public ProjectCategory Category { get; set; }
    public List<WaveformBase> Waveforms { get; set; } = new();
    public IProjectItemConfig Config { get; set; }

    public static ProjectItem Create(ProjectCategory category)
    {
        var item = new ProjectItem { Category = category };
        item.Config = category switch
        {
            ProjectCategory.Chuck => new ChuckConfig(),
            ProjectCategory.DeChuck => new DeChuckConfig(),
            _ => throw new NotSupportedException()
        };
        for (int i = 0; i < 10; i++)
        {
            item.Waveforms.Add(category == ProjectCategory.Chuck
                ? new ChuckWaveform { Name = $"ChuckWaveform{i + 1}" }
                : new DeChuckWaveform { Name = $"DeChuckWaveform{i + 1}" });
        }
        return item;
    }

    public static ProjectItem Restore(ProjectItem raw)
    {
        // Config の型を復元
        raw.Config = raw.Category switch
        {
            ProjectCategory.Chuck => JsonSerializer.Deserialize<ChuckConfig>(JsonSerializer.Serialize(raw.Config)),
            ProjectCategory.DeChuck => JsonSerializer.Deserialize<DeChuckConfig>(JsonSerializer.Serialize(raw.Config)),
            _ => throw new NotSupportedException()
        };
        return raw;
    }
}


---

4. Config（ChuckConfig / DeChuckConfig）

Create:

各カテゴリのデフォルト設定を返す。


Restore:

JSONや辞書、DBの値からプロパティを復元。


public static class ConfigFactory
{
    public static IProjectItemConfig Create(ProjectCategory category) => category switch
    {
        ProjectCategory.Chuck => new ChuckConfig(),
        ProjectCategory.DeChuck => new DeChuckConfig(),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static IProjectItemConfig Restore(ProjectCategory category, string json) => category switch
    {
        ProjectCategory.Chuck => JsonSerializer.Deserialize<ChuckConfig>(json)!,
        ProjectCategory.DeChuck => JsonSerializer.Deserialize<DeChuckConfig>(json)!,
        _ => throw new ArgumentOutOfRangeException()
    };
}


---

拡張案

IStorableインターフェース（Save / Restore メソッド）を導入して統一化。

StrategyパターンでCreate/Restore方法をカテゴリごとに切り替える。

バージョン管理対応：保存時にフォーマットバージョンを記録して復元時に分岐。



---

必要であれば IStorable や ICreatable インターフェースで統一設計もできます。次に進めるなら、「波形」や「Config」の保存/復元ロジックを掘り下げてもいいです。どうしましょう？




了解です！では、拡張案すべて（IStorable/IProjectCreatableインターフェース、Strategyパターン、バージョン管理対応）を導入した設計を提示します。


---

■ 1. インターフェース定義

IStorable（保存/復元対応）

public interface IStorable<T>
{
    string Save();
    static abstract T Restore(string json);
}

IProjectCreatable（カテゴリに応じたCreate戦略）

public interface IProjectCreatable<T>
{
    static abstract T Create(ProjectCategory category);
}


---

■ 2. ProjectItem の改修版

public class ProjectItem : IStorable<ProjectItem>, IProjectCreatable<ProjectItem>
{
    public const int FormatVersion = 1;

    public ProjectCategory Category { get; set; }
    public List<WaveformBase> Waveforms { get; set; } = new();
    public IProjectItemConfig Config { get; set; }
    public int Version { get; set; } = FormatVersion;

    public static ProjectItem Create(ProjectCategory category)
    {
        var item = new ProjectItem
        {
            Category = category,
            Config = ConfigFactory.Create(category),
            Waveforms = Enumerable.Range(1, 10).Select(i =>
                category == ProjectCategory.Chuck
                    ? new ChuckWaveform { Name = $"ChuckWaveform{i}" }
                    : new DeChuckWaveform { Name = $"DeChuckWaveform{i}" }
            ).Cast<WaveformBase>().ToList()
        };
        return item;
    }

    public string Save()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonConfigConverter() }
        });
    }

    public static ProjectItem Restore(string json)
    {
        var item = JsonSerializer.Deserialize<ProjectItem>(json, new JsonSerializerOptions
        {
            Converters = { new JsonConfigConverter() }
        }) ?? throw new InvalidOperationException("Invalid data");

        // バージョンが古ければマイグレーション処理など
        if (item.Version < FormatVersion)
        {
            // MigrationStrategy.Apply(item)
        }

        return item;
    }
}


---

■ 3. ConfigFactory（Strategy パターン）

public static class ConfigFactory
{
    public static IProjectItemConfig Create(ProjectCategory category) => category switch
    {
        ProjectCategory.Chuck => new ChuckConfig(),
        ProjectCategory.DeChuck => new DeChuckConfig(),
        _ => throw new ArgumentOutOfRangeException()
    };
}


---

■ 4. Json変換サポート：JsonConfigConverter

public class JsonConfigConverter : JsonConverter<IProjectItemConfig>
{
    public override IProjectItemConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDoc = JsonDocument.ParseValue(ref reader);
        if (!jsonDoc.RootElement.TryGetProperty("Category", out var categoryElement))
            throw new JsonException("Missing Category");

        var category = Enum.Parse<ProjectCategory>(categoryElement.GetString()!);
        var rawJson = jsonDoc.RootElement.GetRawText();

        return category switch
        {
            ProjectCategory.Chuck => JsonSerializer.Deserialize<ChuckConfig>(rawJson, options),
            ProjectCategory.DeChuck => JsonSerializer.Deserialize<DeChuckConfig>(rawJson, options),
            _ => throw new JsonException("Invalid Category")
        };
    }

    public override void Write(Utf8JsonWriter writer, IProjectItemConfig value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}


---

■ 5. Project も IStorable 対応

public class Project : IStorable<Project>
{
    public string Name { get; set; } = string.Empty;
    public List<ProjectItem> Items { get; set; } = new();

    public static Project Create(string name)
        => new() { Name = name };

    public string Save()
        => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

    public static Project Restore(string json)
        => JsonSerializer.Deserialize<Project>(json) ?? throw new Exception("Invalid project data");
}


---

■ バージョン管理戦略（将来的に）

ProjectItem.Version を元に、以下のような Strategy クラスでマイグレーションできます：

public static class MigrationStrategy
{
    public static void Apply(ProjectItem item)
    {
        if (item.Version == 0)
        {
            // 旧形式を最新版に変換
        }
    }
}


---

次の拡張候補：

波形データの保存/圧縮（バイナリ/CSV）

Undo/Redo履歴を各ProjectItem単位で管理

UIからカテゴリ別設定を編集できるようにデータバインド対応


ご希望があれば、上記に対応したViewModel構成やユニットテスト例もご提供できます！何を優先して進めたいですか？




了解です！では、拡張案すべて（IStorable/IProjectCreatableインターフェース、Strategyパターン、バージョン管理対応）を導入した設計を提示します。


---

■ 1. インターフェース定義

IStorable（保存/復元対応）

public interface IStorable<T>
{
    string Save();
    static abstract T Restore(string json);
}

IProjectCreatable（カテゴリに応じたCreate戦略）

public interface IProjectCreatable<T>
{
    static abstract T Create(ProjectCategory category);
}


---

■ 2. ProjectItem の改修版

public class ProjectItem : IStorable<ProjectItem>, IProjectCreatable<ProjectItem>
{
    public const int FormatVersion = 1;

    public ProjectCategory Category { get; set; }
    public List<WaveformBase> Waveforms { get; set; } = new();
    public IProjectItemConfig Config { get; set; }
    public int Version { get; set; } = FormatVersion;

    public static ProjectItem Create(ProjectCategory category)
    {
        var item = new ProjectItem
        {
            Category = category,
            Config = ConfigFactory.Create(category),
            Waveforms = Enumerable.Range(1, 10).Select(i =>
                category == ProjectCategory.Chuck
                    ? new ChuckWaveform { Name = $"ChuckWaveform{i}" }
                    : new DeChuckWaveform { Name = $"DeChuckWaveform{i}" }
            ).Cast<WaveformBase>().ToList()
        };
        return item;
    }

    public string Save()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonConfigConverter() }
        });
    }

    public static ProjectItem Restore(string json)
    {
        var item = JsonSerializer.Deserialize<ProjectItem>(json, new JsonSerializerOptions
        {
            Converters = { new JsonConfigConverter() }
        }) ?? throw new InvalidOperationException("Invalid data");

        // バージョンが古ければマイグレーション処理など
        if (item.Version < FormatVersion)
        {
            // MigrationStrategy.Apply(item)
        }

        return item;
    }
}


---

■ 3. ConfigFactory（Strategy パターン）

public static class ConfigFactory
{
    public static IProjectItemConfig Create(ProjectCategory category) => category switch
    {
        ProjectCategory.Chuck => new ChuckConfig(),
        ProjectCategory.DeChuck => new DeChuckConfig(),
        _ => throw new ArgumentOutOfRangeException()
    };
}


---

■ 4. Json変換サポート：JsonConfigConverter

public class JsonConfigConverter : JsonConverter<IProjectItemConfig>
{
    public override IProjectItemConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDoc = JsonDocument.ParseValue(ref reader);
        if (!jsonDoc.RootElement.TryGetProperty("Category", out var categoryElement))
            throw new JsonException("Missing Category");

        var category = Enum.Parse<ProjectCategory>(categoryElement.GetString()!);
        var rawJson = jsonDoc.RootElement.GetRawText();

        return category switch
        {
            ProjectCategory.Chuck => JsonSerializer.Deserialize<ChuckConfig>(rawJson, options),
            ProjectCategory.DeChuck => JsonSerializer.Deserialize<DeChuckConfig>(rawJson, options),
            _ => throw new JsonException("Invalid Category")
        };
    }

    public override void Write(Utf8JsonWriter writer, IProjectItemConfig value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}


---

■ 5. Project も IStorable 対応

public class Project : IStorable<Project>
{
    public string Name { get; set; } = string.Empty;
    public List<ProjectItem> Items { get; set; } = new();

    public static Project Create(string name)
        => new() { Name = name };

    public string Save()
        => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

    public static Project Restore(string json)
        => JsonSerializer.Deserialize<Project>(json) ?? throw new Exception("Invalid project data");
}


---

■ バージョン管理戦略（将来的に）

ProjectItem.Version を元に、以下のような Strategy クラスでマイグレーションできます：

public static class MigrationStrategy
{
    public static void Apply(ProjectItem item)
    {
        if (item.Version == 0)
        {
            // 旧形式を最新版に変換
        }
    }
}


---

次の拡張候補：

波形データの保存/圧縮（バイナリ/CSV）

Undo/Redo履歴を各ProjectItem単位で管理

UIからカテゴリ別設定を編集できるようにデータバインド対応


ご希望があれば、上記に対応したViewModel構成やユニットテスト例もご提供できます！何を優先して進めたいですか？




了解です！WinForms でカテゴリ別設定（IProjectItemConfig）を UI から編集できるようにするために、以下の構成で設計します：


---

■ 構成概要

IConfigEditor インターフェース：カテゴリごとの設定編集UI（UserControl）を統一的に扱う。

ChuckConfigEditor / DeChuckConfigEditor：カテゴリ別のUIコンポーネント（UserControl）実装。

ConfigEditorFactory：カテゴリに応じたEditorの生成。

ProjectItemEditorForm：設定を編集するためのホストフォーム。



---

■ 1. IConfigEditor インターフェース

public interface IConfigEditor
{
    IProjectItemConfig Config { get; set; }
    void LoadFromConfig();
    void SaveToConfig();
}


---

■ 2. ChuckConfigEditor の例

public partial class ChuckConfigEditor : UserControl, IConfigEditor
{
    public IProjectItemConfig Config { get; set; } = new ChuckConfig();

    public ChuckConfigEditor()
    {
        InitializeComponent();
    }

    public void LoadFromConfig()
    {
        if (Config is not ChuckConfig chuck) return;
        textBoxDescription.Text = chuck.Description;
        numericUpDownSamplingRate.Value = chuck.SamplingRate;
    }

    public void SaveToConfig()
    {
        if (Config is not ChuckConfig chuck) return;
        chuck.Description = textBoxDescription.Text;
        chuck.SamplingRate = (int)numericUpDownSamplingRate.Value;
    }
}

> ※ textBoxDescription や numericUpDownSamplingRate はデザイナ上で配置。




---

■ 3. DeChuckConfigEditor も同様に作成

public partial class DeChuckConfigEditor : UserControl, IConfigEditor
{
    public IProjectItemConfig Config { get; set; } = new DeChuckConfig();

    public DeChuckConfigEditor()
    {
        InitializeComponent();
    }

    public void LoadFromConfig()
    {
        if (Config is not DeChuckConfig deChuck) return;
        textBoxDescription.Text = deChuck.Description;
        numericUpDownSamplingRate.Value = deChuck.SamplingRate;
    }

    public void SaveToConfig()
    {
        if (Config is not DeChuckConfig deChuck) return;
        deChuck.Description = textBoxDescription.Text;
        deChuck.SamplingRate = (int)numericUpDownSamplingRate.Value;
    }
}


---

■ 4. ConfigEditorFactory

public static class ConfigEditorFactory
{
    public static IConfigEditor Create(ProjectCategory category)
    {
        return category switch
        {
            ProjectCategory.Chuck => new ChuckConfigEditor(),
            ProjectCategory.DeChuck => new DeChuckConfigEditor(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}


---

■ 5. ProjectItemEditorForm

public partial class ProjectItemEditorForm : Form
{
    private readonly ProjectItem _item;
    private IConfigEditor? _editor;

    public ProjectItemEditorForm(ProjectItem item)
    {
        InitializeComponent();
        _item = item;
    }

    private void ProjectItemEditorForm_Load(object sender, EventArgs e)
    {
        _editor = ConfigEditorFactory.Create(_item.Category);
        _editor.Config = _item.Config;
        _editor.LoadFromConfig();

        _editorControlHostPanel.Controls.Add((UserControl)_editor);
        ((UserControl)_editor).Dock = DockStyle.Fill;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        _editor?.SaveToConfig();
        DialogResult = DialogResult.OK;
    }
}

> _editorControlHostPanel は Panel。btnSave は保存ボタン。




---

拡張案

IConfigEditor に Validate() を追加し、入力検証対応。

IConfigEditorFactory をインターフェース化して DI 対応。

カテゴリ別アイコンや色、動的切り替えUIの追加（タブ or コンボボックス）。



---

WinForms Designer で UI を作成するコードも必要であれば補います。
「実装コードのテンプレート」や「デザイナ設定」まで必要ですか？



完璧です！次の3つの拡張を統合した WinForms UI 構成を提案します：


---

【拡張要件】

1. IValidater インターフェースで 設定の検証


2. カテゴリ別に UIの色（テーマカラー） を切り替え


3. 動的に カテゴリ別設定をタブ表示




---

■ 1. IValidater インターフェース

public interface IValidater
{
    bool ValidateConfig(out string errorMessage);
}


---

■ 2. IConfigEditor 拡張

public interface IConfigEditor : IValidater
{
    IProjectItemConfig Config { get; set; }
    void LoadFromConfig();
    void SaveToConfig();
}


---

■ 3. カテゴリ別色（テーマカラー）

public static class CategoryColorTheme
{
    public static Color GetColor(ProjectCategory category)
    {
        return category switch
        {
            ProjectCategory.Chuck => Color.LightBlue,
            ProjectCategory.DeChuck => Color.LightCoral,
            _ => SystemColors.Control
        };
    }
}


---

■ 4. ProjectItemEditorTabControlForm の例

public partial class ProjectItemEditorTabForm : Form
{
    private readonly List<ProjectItem> _items;

    public ProjectItemEditorTabForm(List<ProjectItem> items)
    {
        InitializeComponent();
        _items = items;
    }

    private void ProjectItemEditorTabForm_Load(object sender, EventArgs e)
    {
        foreach (var item in _items)
        {
            var editor = ConfigEditorFactory.Create(item.Category);
            editor.Config = item.Config;
            editor.LoadFromConfig();

            var tabPage = new TabPage($"{item.Category}")
            {
                BackColor = CategoryColorTheme.GetColor(item.Category)
            };

            var control = (UserControl)editor;
            control.Dock = DockStyle.Fill;

            tabPage.Controls.Add(control);
            tabControlEditors.TabPages.Add(tabPage);

            // 保持しておくなら Dictionary<ProjectItem, IConfigEditor> に格納
            tabPage.Tag = editor;
        }
    }

    private void btnSaveAll_Click(object sender, EventArgs e)
    {
        foreach (TabPage page in tabControlEditors.TabPages)
        {
            if (page.Tag is IConfigEditor editor)
            {
                if (!editor.ValidateConfig(out string error))
                {
                    MessageBox.Show($"Validation failed on {page.Text}: {error}", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                editor.SaveToConfig();
            }
        }

        DialogResult = DialogResult.OK;
    }
}

> tabControlEditors は TabControl、btnSaveAll は保存ボタン。




---

■ 5. ChuckConfigEditor に検証ロジック追加

public partial class ChuckConfigEditor : UserControl, IConfigEditor
{
    public IProjectItemConfig Config { get; set; } = new ChuckConfig();

    public void LoadFromConfig()
    {
        if (Config is not ChuckConfig chuck) return;
        textBoxDescription.Text = chuck.Description;
        numericUpDownSamplingRate.Value = chuck.SamplingRate;
    }

    public void SaveToConfig()
    {
        if (Config is not ChuckConfig chuck) return;
        chuck.Description = textBoxDescription.Text;
        chuck.SamplingRate = (int)numericUpDownSamplingRate.Value;
    }

    public bool ValidateConfig(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(textBoxDescription.Text))
        {
            errorMessage = "説明は必須です。";
            return false;
        }
        if (numericUpDownSamplingRate.Value <= 0)
        {
            errorMessage = "サンプリングレートは正の整数でなければなりません。";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}

> DeChuckConfigEditor も同様に対応可能。




---

今後の拡張アイデア

編集履歴を保持して Undo/Redo。

タブ内にグラフ表示コントロールを追加。

Waveform ごとの詳細設定 UI をタブ内に動的展開。



---

必要であれば Designer ファイル（UI のコード部分）やサンプルプロジェクト全体の構成も提示できます！どこまで進めましょうか？






プロジェクトの構成を以下のように設計します。

設計概要

1. プロジェクト (Project)

複数のプロパティを持つ

StartupSettings（起動時設定クラス10個）を持つ

ShutdownSettings（終了時設定クラス10個）を持つ



2. 設定クラス (ProjectSetting)

共通のプロパティを持つ（例：Key、Value など）



3. プロジェクト管理 (ProjectManager)

読み込み (Load)、保存 (Save)、別名保存 (SaveAs)、新規作成 (New) の機能を提供

差分管理 (HasUnsavedChanges)





---

C# 実装

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

public class ProjectSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class Project
{
    public string Name { get; set; } = "Untitled";
    public string FilePath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; // 追加のプロパティ
    public List<ProjectSetting> StartupSettings { get; set; } = new List<ProjectSetting>();
    public List<ProjectSetting> ShutdownSettings { get; set; } = new List<ProjectSetting>();

    private string lastSavedData = string.Empty;

    public Project()
    {
        // 設定リストを10個初期化
        for (int i = 0; i < 10; i++)
        {
            StartupSettings.Add(new ProjectSetting { Key = $"Startup {i + 1}", Value = "Default" });
            ShutdownSettings.Add(new ProjectSetting { Key = $"Shutdown {i + 1}", Value = "Default" });
        }
    }

    public void Load(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("File not found", path);
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

    public void Save()
    {
        if (string.IsNullOrEmpty(FilePath)) throw new InvalidOperationException("File path not set.");
        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
        lastSavedData = json;
    }

    public void SaveAs(string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        FilePath = path;
        lastSavedData = JsonSerializer.Serialize(this);
    }

    public void New()
    {
        Name = "Untitled";
        FilePath = string.Empty;
        Description = string.Empty;
        StartupSettings.Clear();
        ShutdownSettings.Clear();
        for (int i = 0; i < 10; i++)
        {
            StartupSettings.Add(new ProjectSetting { Key = $"Startup {i + 1}", Value = "Default" });
            ShutdownSettings.Add(new ProjectSetting { Key = $"Shutdown {i + 1}", Value = "Default" });
        }
        lastSavedData = JsonSerializer.Serialize(this);
    }

    public bool HasUnsavedChanges()
    {
        string currentData = JsonSerializer.Serialize(this);
        return currentData != lastSavedData;
    }
}

public class ProjectManager
{
    public Project CurrentProject { get; private set; } = new Project();

    public void NewProject()
    {
        if (CurrentProject.HasUnsavedChanges())
        {
            var result = MessageBox.Show("Save changes?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Yes) SaveProject();
            if (result == DialogResult.Cancel) return;
        }
        CurrentProject.New();
    }

    public void OpenProject()
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Project Files (*.json)|*.json" })
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (CurrentProject.HasUnsavedChanges())
                {
                    var result = MessageBox.Show("Save changes?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes) SaveProject();
                    if (result == DialogResult.Cancel) return;
                }
                CurrentProject.Load(openFileDialog.FileName);
            }
        }
    }

    public void SaveProject()
    {
        if (string.IsNullOrEmpty(CurrentProject.FilePath))
            SaveProjectAs();
        else
            CurrentProject.Save();
    }

    public void SaveProjectAs()
    {
        using (SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "Project Files (*.json)|*.json" })
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                CurrentProject.SaveAs(saveFileDialog.FileName);
            }
        }
    }
}

// UI 実装
public class MainForm : Form
{
    private MenuStrip menuStrip;
    private ProjectManager projectManager = new ProjectManager();

    public MainForm()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        menuStrip = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("File");

        fileMenu.DropDownItems.Add("New", null, (s, e) => projectManager.NewProject());
        fileMenu.DropDownItems.Add("Open", null, (s, e) => projectManager.OpenProject());
        fileMenu.DropDownItems.Add("Save", null, (s, e) => projectManager.SaveProject());
        fileMenu.DropDownItems.Add("Save As", null, (s, e) => projectManager.SaveProjectAs());

        menuStrip.Items.Add(fileMenu);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
    }
}

// アプリケーションのエントリーポイント
public static class Program
{
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}


---

拡張案

1. 設定のインポート/エクスポート

StartupSettings や ShutdownSettings を個別に保存・読み込み可能にする



2. GUI での設定編集

DataGridView を使って StartupSettings や ShutdownSettings を編集



3. プロジェクト履歴管理

最近開いたプロジェクトを Properties.Settings で管理し、メニューから簡単に開けるようにする



4. 自動保存

Timer を使い定期的に Save() を実行する



5. プロジェクトの複数タブ管理

TabControl を導入し、複数のプロジェクトを同時に開けるようにする




この設計で、プロジェクトのプロパティを持ちつつ、起動時/終了時の設定を10個ずつ持つ構造が完成します！


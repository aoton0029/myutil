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


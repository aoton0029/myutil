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


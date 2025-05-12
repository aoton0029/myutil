# C# プロジェクト管理システムの設計

お問い合わせいただいたアプリケーションの設計について、各クラスの役割とクラス設計を提案します。

## 各クラスの役割

1. **Project** - プロジェクトデータモデル
   - プロジェクトの実際のデータと状態を保持
   - ファイルパス、名前、内容などの基本情報を管理

2. **ProjectContext** - プロジェクト状態管理
   - 単一プロジェクトの編集状態を追跡
   - 変更通知メカニズムを提供
   - 未保存の変更があるかどうかを管理

3. **ProjectService** - プロジェクト操作サービス
   - ファイル操作（保存、読み込み）を担当
   - Save、Save As、Openの実装を提供
   - プロジェクトの永続化とシリアライズ処理を行う

4. **AppContext** - アプリケーション全体のコンテキスト
   - 現在アクティブなProjectContextを管理
   - アプリケーション全体の状態を保持
   - サービスの登録と取得の仕組みを提供

## クラス設計

以下にそれぞれのクラスの詳細設計を提案します：

```csharp name=Project.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectManagement
{
    /// <summary>
    /// プロジェクトのデータと状態を表すモデルクラス
    /// </summary>
    public class Project : INotifyPropertyChanged
    {
        private string _name;
        private string _filePath;
        private object _content; // 実際のアプリケーションに応じて型を変更
        private bool _isDirty;

        /// <summary>
        /// プロジェクト名
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// プロジェクトのファイルパス（未保存の場合はnull）
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        /// <summary>
        /// プロジェクトの内容
        /// </summary>
        public object Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        /// <summary>
        /// 未保存の変更があるかどうか
        /// </summary>
        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        /// <summary>
        /// 新規プロジェクトを作成
        /// </summary>
        /// <param name="name">プロジェクト名</param>
        public Project(string name = "New Project")
        {
            _name = name;
            _content = null;
            _isDirty = false;
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
```

```csharp name=ProjectContext.cs
using System;
using System.ComponentModel;

namespace ProjectManagement
{
    /// <summary>
    /// プロジェクトの状態を管理するコンテキストクラス
    /// </summary>
    public class ProjectContext : IDisposable
    {
        private Project _project;
        private bool _isModified = false;

        /// <summary>
        /// 現在のプロジェクト
        /// </summary>
        public Project Project
        {
            get => _project;
            private set
            {
                if (_project != null)
                {
                    // 古いプロジェクトのイベント購読を解除
                    _project.PropertyChanged -= Project_PropertyChanged;
                }

                _project = value;

                if (_project != null)
                {
                    // 新しいプロジェクトのイベント購読
                    _project.PropertyChanged += Project_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// プロジェクトが変更されたかどうか
        /// </summary>
        public bool IsModified
        {
            get => _isModified;
            private set
            {
                if (_isModified != value)
                {
                    _isModified = value;
                    ModifiedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// プロジェクトの変更状態が変わった時に発生するイベント
        /// </summary>
        public event EventHandler ModifiedChanged;

        /// <summary>
        /// 新しいプロジェクトでコンテキストを初期化
        /// </summary>
        /// <param name="project">プロジェクト</param>
        public ProjectContext(Project project)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
        }

        /// <summary>
        /// プロジェクトのプロパティが変更された時の処理
        /// </summary>
        private void Project_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Project.Content))
            {
                // コンテンツが変更されたらプロジェクトを変更状態にする
                MarkAsModified();
            }
        }

        /// <summary>
        /// プロジェクトを変更状態としてマーク
        /// </summary>
        public void MarkAsModified()
        {
            IsModified = true;
            Project.IsDirty = true;
        }

        /// <summary>
        /// プロジェクトを未変更状態としてマーク（保存後など）
        /// </summary>
        public void MarkAsUnmodified()
        {
            IsModified = false;
            Project.IsDirty = false;
        }

        /// <summary>
        /// リソースの解放
        /// </summary>
        public void Dispose()
        {
            if (_project != null)
            {
                _project.PropertyChanged -= Project_PropertyChanged;
            }
        }
    }
}
```

```csharp name=ProjectService.cs
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

namespace ProjectManagement
{
    /// <summary>
    /// プロジェクトのファイル操作を担当するサービスクラス
    /// </summary>
    public class ProjectService
    {
        private readonly AppContext _appContext;

        public ProjectService(AppContext appContext)
        {
            _appContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
        }

        /// <summary>
        /// 新規プロジェクトを作成
        /// </summary>
        /// <param name="name">プロジェクト名</param>
        /// <returns>作成されたプロジェクトコンテキスト</returns>
        public ProjectContext CreateNew(string name = "New Project")
        {
            var project = new Project(name);
            return new ProjectContext(project);
        }

        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>開いたプロジェクトコンテキスト</returns>
        public async Task<ProjectContext> OpenAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Project file not found", filePath);

            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                var project = JsonSerializer.Deserialize<Project>(json);
                
                if (project == null)
                    throw new InvalidOperationException("Failed to deserialize project");

                project.FilePath = filePath;
                project.IsDirty = false;

                var context = new ProjectContext(project);
                return context;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error opening project: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// プロジェクトを保存
        /// </summary>
        /// <param name="context">保存するプロジェクトコンテキスト</param>
        /// <returns>保存タスク</returns>
        public async Task SaveAsync(ProjectContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrEmpty(context.Project.FilePath))
            {
                throw new InvalidOperationException("Cannot save project without file path. Use SaveAs instead.");
            }

            await SaveToFileAsync(context, context.Project.FilePath);
            context.MarkAsUnmodified();
        }

        /// <summary>
        /// プロジェクトを別名で保存
        /// </summary>
        /// <param name="context">保存するプロジェクトコンテキスト</param>
        /// <param name="filePath">保存先ファイルパス</param>
        /// <returns>保存タスク</returns>
        public async Task SaveAsAsync(ProjectContext context, string filePath)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            await SaveToFileAsync(context, filePath);
            context.Project.FilePath = filePath;
            context.Project.Name = Path.GetFileNameWithoutExtension(filePath);
            context.MarkAsUnmodified();
        }

        /// <summary>
        /// プロジェクトを指定されたファイルに保存
        /// </summary>
        private async Task SaveToFileAsync(ProjectContext context, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(context.Project, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving project: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 現在のプロジェクトに未保存の変更がある場合、保存するかどうか確認
        /// </summary>
        /// <returns>続行可能か（true=続行可能、false=操作キャンセル）</returns>
        public async Task<bool> ConfirmSaveIfModifiedAsync()
        {
            var currentContext = _appContext.CurrentProjectContext;
            if (currentContext == null || !currentContext.IsModified)
                return true;

            // 実際のアプリケーションではUIから確認ダイアログを表示する
            // ここではコンソールアプリケーションを想定した簡易実装

            Console.WriteLine($"Project '{currentContext.Project.Name}' has unsaved changes. Save?");
            Console.WriteLine("1: Yes");
            Console.WriteLine("2: No");
            Console.WriteLine("3: Cancel");

            string input = Console.ReadLine();
            
            switch (input)
            {
                case "1": // Yes
                    try
                    {
                        if (string.IsNullOrEmpty(currentContext.Project.FilePath))
                        {
                            // 実際のアプリケーションではファイル選択ダイアログを表示
                            Console.WriteLine("Enter file path for saving:");
                            string path = Console.ReadLine();
                            if (string.IsNullOrEmpty(path))
                                return false;
                                
                            await SaveAsAsync(currentContext, path);
                        }
                        else
                        {
                            await SaveAsync(currentContext);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving: {ex.Message}");
                        return false;
                    }
                case "2": // No
                    return true;
                case "3": // Cancel
                default:
                    return false;
            }
        }
    }
}
```

```csharp name=AppContext.cs
using System;
using System.Threading.Tasks;

namespace ProjectManagement
{
    /// <summary>
    /// アプリケーション全体のコンテキストを管理するクラス
    /// </summary>
    public class AppContext
    {
        private ProjectContext _currentProjectContext;
        private readonly ProjectService _projectService;

        /// <summary>
        /// 現在アクティブなプロジェクトコンテキスト
        /// </summary>
        public ProjectContext CurrentProjectContext
        {
            get => _currentProjectContext;
            private set
            {
                if (_currentProjectContext != null)
                {
                    // 古いコンテキストのイベント購読を解除
                    _currentProjectContext.ModifiedChanged -= ProjectContext_ModifiedChanged;
                    _currentProjectContext.Dispose();
                }

                _currentProjectContext = value;

                if (_currentProjectContext != null)
                {
                    // 新しいコンテキストのイベント購読
                    _currentProjectContext.ModifiedChanged += ProjectContext_ModifiedChanged;
                }

                // タイトルバーなど、アプリケーションの状態更新
                UpdateApplicationState();
            }
        }

        /// <summary>
        /// アプリケーションタイトル
        /// </summary>
        public string ApplicationTitle
        {
            get
            {
                string baseTitle = "Project Management App";
                if (CurrentProjectContext == null)
                    return baseTitle;

                string modifiedIndicator = CurrentProjectContext.IsModified ? "*" : "";
                return $"{CurrentProjectContext.Project.Name}{modifiedIndicator} - {baseTitle}";
            }
        }

        /// <summary>
        /// アプリケーション状態が変更された時に発生するイベント
        /// </summary>
        public event EventHandler ApplicationStateChanged;

        /// <summary>
        /// AppContextのコンストラクタ
        /// </summary>
        public AppContext()
        {
            _projectService = new ProjectService(this);
        }

        /// <summary>
        /// アプリケーションの状態を更新
        /// </summary>
        private void UpdateApplicationState()
        {
            ApplicationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// プロジェクトの変更状態が変わった時の処理
        /// </summary>
        private void ProjectContext_ModifiedChanged(object sender, EventArgs e)
        {
            UpdateApplicationState();
        }

        #region Menu Commands

        /// <summary>
        /// 新規プロジェクトを作成
        /// </summary>
        /// <returns>操作成功かどうか</returns>
        public async Task<bool> NewProjectAsync()
        {
            if (!await _projectService.ConfirmSaveIfModifiedAsync())
                return false;

            CurrentProjectContext = _projectService.CreateNew();
            return true;
        }

        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>操作成功かどうか</returns>
        public async Task<bool> OpenProjectAsync(string filePath)
        {
            if (!await _projectService.ConfirmSaveIfModifiedAsync())
                return false;

            try
            {
                CurrentProjectContext = await _projectService.OpenAsync(filePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening project: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 現在のプロジェクトを保存
        /// </summary>
        /// <returns>操作成功かどうか</returns>
        public async Task<bool> SaveProjectAsync()
        {
            if (CurrentProjectContext == null)
                return false;

            try
            {
                if (string.IsNullOrEmpty(CurrentProjectContext.Project.FilePath))
                {
                    // パスがない場合はSaveAsに転送
                    return await SaveProjectAsAsync(null);
                }
                else
                {
                    await _projectService.SaveAsync(CurrentProjectContext);
                    UpdateApplicationState();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving project: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 現在のプロジェクトを別名で保存
        /// </summary>
        /// <param name="filePath">ファイルパス（nullの場合はダイアログを表示）</param>
        /// <returns>操作成功かどうか</returns>
        public async Task<bool> SaveProjectAsAsync(string filePath)
        {
            if (CurrentProjectContext == null)
                return false;

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    // 実際のアプリケーションではファイル選択ダイアログを表示
                    Console.WriteLine("Enter file path for saving:");
                    filePath = Console.ReadLine();
                    
                    if (string.IsNullOrEmpty(filePath))
                        return false;
                }

                await _projectService.SaveAsAsync(CurrentProjectContext, filePath);
                UpdateApplicationState();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving project: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
```

```csharp name=Program.cs
using System;
using System.Threading.Tasks;

namespace ProjectManagement
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Project Management Application Demo");
            Console.WriteLine("===================================");

            var appContext = new AppContext();
            appContext.ApplicationStateChanged += (s, e) => 
            {
                Console.Title = appContext.ApplicationTitle;
            };

            // 初期プロジェクト作成
            await appContext.NewProjectAsync();

            bool running = true;
            while (running)
            {
                Console.WriteLine();
                Console.WriteLine($"Current Project: {appContext.ApplicationTitle}");
                Console.WriteLine("1: New Project");
                Console.WriteLine("2: Open Project");
                Console.WriteLine("3: Save Project");
                Console.WriteLine("4: Save Project As");
                Console.WriteLine("5: Modify Project (Simulation)");
                Console.WriteLine("0: Exit");
                Console.Write("Select an option: ");

                string input = Console.ReadLine();
                Console.WriteLine();

                switch (input)
                {
                    case "1":
                        await appContext.NewProjectAsync();
                        break;
                    case "2":
                        Console.Write("Enter file path to open: ");
                        string openPath = Console.ReadLine();
                        await appContext.OpenProjectAsync(openPath);
                        break;
                    case "3":
                        await appContext.SaveProjectAsync();
                        break;
                    case "4":
                        Console.Write("Enter file path to save as: ");
                        string savePath = Console.ReadLine();
                        await appContext.SaveProjectAsAsync(savePath);
                        break;
                    case "5":
                        if (appContext.CurrentProjectContext != null)
                        {
                            Console.Write("Enter new content: ");
                            string newContent = Console.ReadLine();
                            appContext.CurrentProjectContext.Project.Content = newContent;
                            Console.WriteLine("Project modified.");
                        }
                        break;
                    case "0":
                        // 終了前に未保存の変更を確認
                        if (await new ProjectService(appContext).ConfirmSaveIfModifiedAsync())
                        {
                            running = false;
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid option");
                        break;
                }
            }

            Console.WriteLine("Application closed.");
        }
    }
}
```

## 仕組みの説明

1. **編集状態のモニタリング**:
   - `Project`クラスは`INotifyPropertyChanged`を実装し、プロパティ変更通知を提供
   - `ProjectContext`は`Project`の変更を監視し、`IsModified`フラグを管理
   - `AppContext`はアプリケーションタイトルに変更状態(*)を表示

2. **Save機能**:
   - 保存先パスが既にある場合は`ProjectService.SaveAsync`で同じ場所に保存
   - 保存状態をリセットして`IsModified = false`に設定

3. **Save As機能**:
   - 新しいパスを指定して`ProjectService.SaveAsAsync`で保存
   - 新しいファイルパスと名前を設定し、保存状態をリセット

4. **Open機能**:
   - 既存プロジェクトに未保存の変更がある場合、保存確認を行う
   - ファイルからプロジェクトをデシリアライズして新しい`ProjectContext`を作成

5. **クラス間の連携**:
   - `AppContext`がアプリケーション全体の状態を管理
   - `ProjectService`がファイル操作を担当
   - `ProjectContext`が単一プロジェクトの編集状態を管理
   - `Project`がデータモデルとして機能

この設計パターンは、クリーンアーキテクチャの原則に従っており、関心の分離が適切に行われています。また、MVVMパターンとの統合も容易です。


---

# 汎用的なアプリケーションベースへの拡張

`2025-05-12 12:49:09 UTC` にご依頼いただいた、提案したクラス構造を汎用的なアプリのベースとして拡張する方法をご提案します。

## 拡張の基本方針

既存の構造（AppContext、Project、ProjectContext、ProjectService）を基盤として、以下の観点から拡張します：

1. **依存性注入（DI）の導入**
2. **プラグインシステム**
3. **マルチドキュメント対応**
4. **コマンドパターンの実装**
5. **テーマと国際化**
6. **ロギングとエラー処理**

## 拡張したクラス設計

### 1. インターフェースとDIの導入

```csharp name=Interfaces/IProject.cs
using System.ComponentModel;

namespace AppFramework.Interfaces
{
    /// <summary>
    /// プロジェクトデータモデルのインターフェース
    /// </summary>
    public interface IProject : INotifyPropertyChanged
    {
        string Name { get; set; }
        string FilePath { get; set; }
        object Content { get; set; }
        bool IsDirty { get; set; }
        string ProjectType { get; }
    }
}
```

```csharp name=Interfaces/IProjectContext.cs
using System;
using System.Threading.Tasks;

namespace AppFramework.Interfaces
{
    /// <summary>
    /// プロジェクトコンテキストのインターフェース
    /// </summary>
    public interface IProjectContext : IDisposable
    {
        IProject Project { get; }
        bool IsModified { get; }
        event EventHandler ModifiedChanged;
        void MarkAsModified();
        void MarkAsUnmodified();
        Task<bool> CanCloseAsync();
    }
}
```

```csharp name=Interfaces/IProjectService.cs
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AppFramework.Interfaces
{
    /// <summary>
    /// プロジェクト操作サービスのインターフェース
    /// </summary>
    public interface IProjectService
    {
        IProjectContext CreateNew(string name = "New Project");
        Task<IProjectContext> OpenAsync(string filePath);
        Task SaveAsync(IProjectContext context);
        Task SaveAsAsync(IProjectContext context, string filePath);
        Task<bool> ConfirmSaveIfModifiedAsync(IProjectContext context);
        string[] SupportedFileExtensions { get; }
        string FileTypeDescription { get; }
    }
}
```

### 2. 依存性注入コンテナ

```csharp name=Services/DependencyContainer.cs
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AppFramework.Interfaces;

namespace AppFramework.Services
{
    /// <summary>
    /// 依存性注入コンテナを管理するクラス
    /// </summary>
    public class DependencyContainer
    {
        private readonly IServiceCollection _services;
        private IServiceProvider _serviceProvider;

        public DependencyContainer()
        {
            _services = new ServiceCollection();
            ConfigureServices(_services);
        }

        /// <summary>
        /// デフォルトのサービス設定
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
        {
            // コアサービスの登録
            services.AddSingleton<IAppContext, AppContext>();
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<ICommandManager, CommandManager>();
            services.AddSingleton<IPluginManager, PluginManager>();
            
            // プロジェクト関連のサービスはプラグイン化することも可能
            services.AddTransient<IProjectService, ProjectService>();
        }

        /// <summary>
        /// サービスの追加登録
        /// </summary>
        public void RegisterService<TService, TImplementation>() 
            where TImplementation : class, TService 
            where TService : class
        {
            _services.AddTransient<TService, TImplementation>();
        }

        /// <summary>
        /// サービスプロバイダーのビルド
        /// </summary>
        public void BuildServiceProvider()
        {
            _serviceProvider = _services.BuildServiceProvider();
        }

        /// <summary>
        /// サービスの取得
        /// </summary>
        public T GetService<T>() where T : class
        {
            if (_serviceProvider == null)
            {
                BuildServiceProvider();
            }
            return _serviceProvider.GetService<T>();
        }
    }
}
```

### 3. 拡張したAppContext

```csharp name=AppContext.cs
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AppFramework.Interfaces;
using AppFramework.Services;

namespace AppFramework
{
    /// <summary>
    /// アプリケーション全体のコンテキストを管理する拡張クラス
    /// </summary>
    public class AppContext : IAppContext
    {
        private readonly DependencyContainer _container;
        private readonly ILogService _logService;
        private readonly ISettingsService _settingsService;
        
        // マルチドキュメント対応のためのプロジェクトコレクション
        private ObservableCollection<IProjectContext> _projectContexts = new ObservableCollection<IProjectContext>();
        private IProjectContext _activeProjectContext;

        /// <summary>
        /// 開いているすべてのプロジェクトコンテキスト
        /// </summary>
        public ReadOnlyObservableCollection<IProjectContext> ProjectContexts { get; }

        /// <summary>
        /// 現在アクティブなプロジェクトコンテキスト
        /// </summary>
        public IProjectContext ActiveProjectContext
        {
            get => _activeProjectContext;
            set
            {
                if (_activeProjectContext != value)
                {
                    if (_activeProjectContext != null)
                    {
                        _activeProjectContext.ModifiedChanged -= ProjectContext_ModifiedChanged;
                    }

                    _activeProjectContext = value;

                    if (_activeProjectContext != null)
                    {
                        _activeProjectContext.ModifiedChanged += ProjectContext_ModifiedChanged;
                    }

                    ActiveProjectChanged?.Invoke(this, EventArgs.Empty);
                    UpdateApplicationState();
                }
            }
        }

        /// <summary>
        /// アプリケーションタイトル
        /// </summary>
        public string ApplicationTitle
        {
            get
            {
                string baseTitle = _settingsService.GetSetting<string>("ApplicationName", "Application Framework");
                if (ActiveProjectContext == null)
                    return baseTitle;

                string modifiedIndicator = ActiveProjectContext.IsModified ? "*" : "";
                return $"{ActiveProjectContext.Project.Name}{modifiedIndicator} - {baseTitle}";
            }
        }

        /// <summary>
        /// アプリケーション状態が変更された時に発生するイベント
        /// </summary>
        public event EventHandler ApplicationStateChanged;
        
        /// <summary>
        /// アクティブなプロジェクトが変更された時に発生するイベント
        /// </summary>
        public event EventHandler ActiveProjectChanged;

        /// <summary>
        /// AppContextのコンストラクタ
        /// </summary>
        public AppContext(DependencyContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logService = _container.GetService<ILogService>();
            _settingsService = _container.GetService<ISettingsService>();
            
            ProjectContexts = new ReadOnlyObservableCollection<IProjectContext>(_projectContexts);
            
            // プラグインの読み込み
            var pluginManager = _container.GetService<IPluginManager>();
            pluginManager.LoadPlugins();
            
            _logService.Log(LogLevel.Info, "Application initialized");
        }

        /// <summary>
        /// アプリケーションの状態を更新
        /// </summary>
        private void UpdateApplicationState()
        {
            ApplicationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// プロジェクトの変更状態が変わった時の処理
        /// </summary>
        private void ProjectContext_ModifiedChanged(object sender, EventArgs e)
        {
            UpdateApplicationState();
        }

        #region プロジェクト管理

        /// <summary>
        /// 新規プロジェクトを作成
        /// </summary>
        public async Task<IProjectContext> NewProjectAsync(string projectType = null)
        {
            try
            {
                // プロジェクトタイプに対応するサービスの取得
                IProjectService projectService = GetProjectService(projectType);
                
                // 新規プロジェクト作成
                var context = projectService.CreateNew();
                AddProjectContext(context);
                
                _logService.Log(LogLevel.Info, $"Created new project: {context.Project.Name}");
                return context;
            }
            catch (Exception ex)
            {
                _logService.Log(LogLevel.Error, $"Failed to create new project: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        public async Task<IProjectContext> OpenProjectAsync(string filePath)
        {
            try
            {
                // ファイル拡張子からプロジェクトタイプを判断
                string extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
                
                // 対応するサービスを検索
                IProjectService projectService = FindProjectServiceForExtension(extension);
                if (projectService == null)
                {
                    throw new NotSupportedException($"Unsupported file extension: {extension}");
                }
                
                // プロジェクトを開く
                var context = await projectService.OpenAsync(filePath);
                AddProjectContext(context);
                
                _logService.Log(LogLevel.Info, $"Opened project: {context.Project.Name} from {filePath}");
                return context;
            }
            catch (Exception ex)
            {
                _logService.Log(LogLevel.Error, $"Failed to open project from {filePath}: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// プロジェクトを閉じる
        /// </summary>
        public async Task<bool> CloseProjectAsync(IProjectContext context)
        {
            if (context == null)
                return false;

            try
            {
                // 未保存の変更がある場合、保存するか確認
                if (context.IsModified)
                {
                    // プロジェクトタイプに対応するサービスを取得
                    IProjectService projectService = GetProjectService(context.Project.ProjectType);
                    
                    bool canClose = await projectService.ConfirmSaveIfModifiedAsync(context);
                    if (!canClose)
                        return false;
                }
                
                // プロジェクトコンテキストを削除
                RemoveProjectContext(context);
                context.Dispose();
                
                _logService.Log(LogLevel.Info, $"Closed project: {context.Project.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logService.Log(LogLevel.Error, $"Failed to close project: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 現在のプロジェクトを保存
        /// </summary>
        public async Task<bool> SaveProjectAsync(IProjectContext context = null)
        {
            context = context ?? ActiveProjectContext;
            if (context == null)
                return false;

            try
            {
                // プロジェクトタイプに対応するサービスを取得
                IProjectService projectService = GetProjectService(context.Project.ProjectType);
                
                if (string.IsNullOrEmpty(context.Project.FilePath))
                {
                    // パスがない場合はSaveAsに転送
                    return await SaveProjectAsAsync(context, null);
                }
                else
                {
                    await projectService.SaveAsync(context);
                    UpdateApplicationState();
                    
                    _logService.Log(LogLevel.Info, $"Saved project: {context.Project.Name} to {context.Project.FilePath}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logService.Log(LogLevel.Error, $"Failed to save project: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 現在のプロジェクトを別名で保存
        /// </summary>
        public async Task<bool> SaveProjectAsAsync(IProjectContext context = null, string filePath = null)
        {
            context = context ?? ActiveProjectContext;
            if (context == null)
                return false;

            try
            {
                // プロジェクトタイプに対応するサービスを取得
                IProjectService projectService = GetProjectService(context.Project.ProjectType);
                
                if (string.IsNullOrEmpty(filePath))
                {
                    // 実際のアプリケーションではファイル選択ダイアログを表示
                    // この部分はプラットフォーム依存のため、インターフェースを介して呼び出す
                    var dialogService = _container.GetService<IDialogService>();
                    var fileInfo = await dialogService.ShowSaveFileDialogAsync(
                        "Save Project As",
                        projectService.SupportedFileExtensions,
                        projectService.FileTypeDescription);
                    
                    if (fileInfo == null || string.IsNullOrEmpty(fileInfo.FilePath))
                        return false;
                    
                    filePath = fileInfo.FilePath;
                }

                await projectService.SaveAsAsync(context, filePath);
                UpdateApplicationState();
                
                _logService.Log(LogLevel.Info, $"Saved project as: {context.Project.Name} to {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logService.Log(LogLevel.Error, $"Failed to save project as: {ex.Message}", ex);
                return false;
            }
        }
        
        /// <summary>
        /// プロジェクトコンテキストをコレクションに追加
        /// </summary>
        private void AddProjectContext(IProjectContext context)
        {
            _projectContexts.Add(context);
            ActiveProjectContext = context;
        }
        
        /// <summary>
        /// プロジェクトコンテキストをコレクションから削除
        /// </summary>
        private void RemoveProjectContext(IProjectContext context)
        {
            bool wasActive = ActiveProjectContext == context;
            _projectContexts.Remove(context);
            
            if (wasActive)
            {
                ActiveProjectContext = _projectContexts.FirstOrDefault();
            }
        }
        
        /// <summary>
        /// プロジェクトタイプに対応するサービスを取得
        /// </summary>
        private IProjectService GetProjectService(string projectType)
        {
            // デフォルトのプロジェクトサービスを取得
            var defaultService = _container.GetService<IProjectService>();
            
            if (string.IsNullOrEmpty(projectType))
                return defaultService;
            
            // プラグインからプロジェクトタイプに対応するサービスを検索
            var pluginManager = _container.GetService<IPluginManager>();
            var service = pluginManager.GetProjectService(projectType);
            
            return service ?? defaultService;
        }
        
        /// <summary>
        /// ファイル拡張子に対応するプロジェクトサービスを検索
        /// </summary>
        private IProjectService FindProjectServiceForExtension(string extension)
        {
            // デフォルトのサービスを確認
            var defaultService = _container.GetService<IProjectService>();
            if (defaultService.SupportedFileExtensions.Contains(extension))
                return defaultService;
            
            // プラグインから対応するサービスを検索
            var pluginManager = _container.GetService<IPluginManager>();
            return pluginManager.GetProjectServiceForExtension(extension);
        }
        
        #endregion
    }
}
```

### 4. コマンドパターンの導入

```csharp name=Commands/CommandBase.cs
using System;
using System.Windows.Input;

namespace AppFramework.Commands
{
    /// <summary>
    /// コマンドの基本クラス
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        private readonly Predicate<object> _canExecute;
        
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        
        protected CommandBase(Predicate<object> canExecute = null)
        {
            _canExecute = canExecute;
        }
        
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        
        public abstract void Execute(object parameter);
        
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
```

```csharp name=Commands/RelayCommand.cs
using System;

namespace AppFramework.Commands
{
    /// <summary>
    /// 汎用的なコマンド実装
    /// </summary>
    public class RelayCommand : CommandBase
    {
        private readonly Action<object> _execute;
        
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
            : base(canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }
        
        public override void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
```

```csharp name=Services/CommandManager.cs
using System;
using System.Collections.Generic;
using AppFramework.Commands;
using AppFramework.Interfaces;

namespace AppFramework.Services
{
    /// <summary>
    /// アプリケーションのコマンドを管理するクラス
    /// </summary>
    public class CommandManager : ICommandManager
    {
        private readonly Dictionary<string, CommandBase> _commands = new Dictionary<string, CommandBase>();
        private readonly Stack<IUndoableCommand> _undoStack = new Stack<IUndoableCommand>();
        private readonly Stack<IUndoableCommand> _redoStack = new Stack<IUndoableCommand>();
        
        public event EventHandler UndoRedoStateChanged;
        
        /// <summary>
        /// コマンドの登録
        /// </summary>
        public void RegisterCommand(string commandName, CommandBase command)
        {
            if (string.IsNullOrEmpty(commandName))
                throw new ArgumentException("Command name cannot be empty", nameof(commandName));
                
            if (command == null)
                throw new ArgumentNullException(nameof(command));
                
            _commands[commandName] = command;
        }
        
        /// <summary>
        /// コマンドの取得
        /// </summary>
        public CommandBase GetCommand(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
                throw new ArgumentException("Command name cannot be empty", nameof(commandName));
                
            return _commands.TryGetValue(commandName, out var command) ? command : null;
        }
        
        /// <summary>
        /// Undoが可能かどうか
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;
        
        /// <summary>
        /// Redoが可能かどうか
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;
        
        /// <summary>
        /// Undo操作の実行
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
                return;
                
            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            
            UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Redo操作の実行
        /// </summary>
        public void Redo()
        {
            if (!CanRedo)
                return;
                
            var command = _redoStack.Pop();
            command.Redo();
            _undoStack.Push(command);
            
            UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Undo可能なコマンドの実行と記録
        /// </summary>
        public void ExecuteUndoableCommand(IUndoableCommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
            
            UndoRedoStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
```

### 5. プラグインシステム

```csharp name=Services/PluginManager.cs
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using AppFramework.Interfaces;
using AppFramework.Plugins;

namespace AppFramework.Services
{
    /// <summary>
    /// プラグインを管理するクラス
    /// </summary>
    public class PluginManager : IPluginManager
    {
        private readonly ILogService _logService;
        private readonly ISettingsService _settingsService;
        private readonly DependencyContainer _container;
        private readonly List<IPlugin> _plugins = new List<IPlugin>();
        private readonly Dictionary<string, IProjectService> _projectServices = new Dictionary<string, IProjectService>();
        
        public IReadOnlyList<IPlugin> Plugins => _plugins.AsReadOnly();
        
        public PluginManager(ILogService logService, ISettingsService settingsService, DependencyContainer container)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }
        
        /// <summary>
        /// プラグインのロード
        /// </summary>
        public void LoadPlugins()
        {
            try
            {
                string pluginsDirectory = _settingsService.GetSetting<string>("PluginsDirectory", "Plugins");
                if (!Directory.Exists(pluginsDirectory))
                {
                    Directory.CreateDirectory(pluginsDirectory);
                    return;
                }
                
                foreach (string file in Directory.GetFiles(pluginsDirectory, "*.dll"))
                {
                    try
                    {
                        LoadPluginFromFile(file);
                    }
                    catch (Exception ex)
                    {
                        _logService.Log(LogLevel.Error, $"Failed to load plugin from {file}: {ex.Message}", ex);
                    }
                }
                
                _logService.Log(LogLevel.Info, $"Loaded {_plugins.Count} plugins");
            }
            catch (Exception ex)
            {
                _logService.Log(LogLevel.Error, $"Failed to load plugins: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// プラグインファイルからプラグインをロード
        /// </summary>
        private void LoadPluginFromFile(string filePath)
        {
            var assembly = Assembly.LoadFrom(filePath);
            
            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    var plugin = (IPlugin)Activator.CreateInstance(type);
                    plugin.Initialize(_container);
                    _plugins.Add(plugin);
                    
                    // プロジェクトサービスの登録
                    RegisterPluginProjectServices(plugin);
                    
                    _logService.Log(LogLevel.Info, $"Loaded plugin: {plugin.Name} v{plugin.Version}");
                }
            }
        }
        
        /// <summary>
        /// プラグインのプロジェクトサービスを登録
        /// </summary>
        private void RegisterPluginProjectServices(IPlugin plugin)
        {
            foreach (var service in plugin.GetProjectServices())
            {
                _projectServices[service.FileTypeDescription] = service;
                _logService.Log(LogLevel.Info, $"Registered project service: {service.FileTypeDescription}");
            }
        }
        
        /// <summary>
        /// プロジェクトタイプに対応するサービスを取得
        /// </summary>
        public IProjectService GetProjectService(string projectType)
        {
            return _projectServices.TryGetValue(projectType, out var service) ? service : null;
        }
        
        /// <summary>
        /// ファイル拡張子に対応するプロジェクトサービスを取得
        /// </summary>
        public IProjectService GetProjectServiceForExtension(string extension)
        {
            foreach (var service in _projectServices.Values)
            {
                if (service.SupportedFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    return service;
                }
            }
            
            return null;
        }
    }
}
```

```csharp name=Plugins/IPlugin.cs
using System.Collections.Generic;
using AppFramework.Interfaces;
using AppFramework.Services;

namespace AppFramework.Plugins
{
    /// <summary>
    /// プラグインのインターフェース
    /// </summary>
    public interface IPlugin
    {
        string Name { get; }
        string Version { get; }
        string Description { get; }
        string Author { get; }
        
        void Initialize(DependencyContainer container);
        IEnumerable<IProjectService> GetProjectServices();
    }
}
```

### 6. その他のサービス

```csharp name=Services/LogService.cs
using System;
using System.IO;
using AppFramework.Interfaces;

namespace AppFramework.Services
{
    /// <summary>
    /// ログレベルの列挙
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
    
    /// <summary>
    /// ログ出力を管理するサービス
    /// </summary>
    public class LogService : ILogService
    {
        private readonly object _lockObject = new object();
        private readonly string _logFilePath;
        private readonly ISettingsService _settingsService;
        
        public LogService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            
            string logDirectory = _settingsService.GetSetting<string>("LogDirectory", "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            string logFileName = $"Log_{DateTime.Now:yyyyMMdd}.log";
            _logFilePath = Path.Combine(logDirectory, logFileName);
        }
        
        /// <summary>
        /// ログの出力
        /// </summary>
        public void Log(LogLevel level, string message, Exception exception = null)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            
            if (exception != null)
            {
                logEntry += $"\r\nException: {exception.Message}\r\nStackTrace: {exception.StackTrace}";
            }
            
            lock (_lockObject)
            {
                try
                {
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                    
                    // 特定のレベル以上をコンソールに出力（デバッグ用）
                    if (level >= LogLevel.Warning)
                    {
                        Console.WriteLine(logEntry);
                    }
                }
                catch
                {
                    // ログ出力中のエラーは無視（無限ループ防止）
                }
            }
        }
    }
}
```

```csharp name=Services/SettingsService.cs
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using AppFramework.Interfaces;

namespace AppFramework.Services
{
    /// <summary>
    /// アプリケーション設定を管理するサービス
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private readonly object _lockObject = new object();
        private Dictionary<string, object> _settings;
        
        public SettingsService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appDirectory = Path.Combine(appDataPath, "AppFramework");
            
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }
            
            _settingsFilePath = Path.Combine(appDirectory, "settings.json");
            LoadSettings();
        }
        
        /// <summary>
        /// 設定の読み込み
        /// </summary>
        private void LoadSettings()
        {
            lock (_lockObject)
            {
                if (File.Exists(_settingsFilePath))
                {
                    try
                    {
                        string json = File.ReadAllText(_settingsFilePath);
                        _settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    }
                    catch
                    {
                        _settings = new Dictionary<string, object>();
                    }
                }
                else
                {
                    _settings = new Dictionary<string, object>();
                }
            }
        }
        
        /// <summary>
        /// 設定の保存
        /// </summary>
        private void SaveSettings()
        {
            lock (_lockObject)
            {
                try
                {
                    string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_settingsFilePath, json);
                }
                catch
                {
                    // 設定保存エラーは無視
                }
            }
        }
        
        /// <summary>
        /// 設定値の取得
        /// </summary>
        public T GetSetting<T>(string key, T defaultValue = default)
        {
            lock (_lockObject)
            {
                if (_settings.TryGetValue(key, out var value))
                {
                    try
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
                
                return defaultValue;
            }
        }
        
        /// <summary>
        /// 設定値の保存
        /// </summary>
        public void SaveSetting<T>(string key, T value)
        {
            lock (_lockObject)
            {
                _settings[key] = value;
                SaveSettings();
            }
        }
    }
}
```

```csharp name=Services/ThemeService.cs
using System;
using System.Collections.Generic;
using AppFramework.Interfaces;

namespace AppFramework.Services
{
    /// <summary>
    /// アプリケーションのテーマを管理するサービス
    /// </summary>
    public class ThemeService : IThemeService
    {
        private readonly ISettingsService _settingsService;
        private string _currentTheme;
        
        public event EventHandler ThemeChanged;
        
        public ThemeService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _currentTheme = _settingsService.GetSetting<string>("CurrentTheme", "Light");
        }
        
        /// <summary>
        /// 現在のテーマ
        /// </summary>
        public string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    _settingsService.SaveSetting("CurrentTheme", value);
                    ThemeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        
        /// <summary>
        /// 利用可能なテーマ一覧
        /// </summary>
        public IEnumerable<string> AvailableThemes => new[] { "Light", "Dark", "Blue", "High Contrast" };
        
        /// <summary>
        /// テーマに対応するリソースディクショナリを取得
        /// </summary>
        public Uri GetThemeResourceUri(string themeName)
        {
            string resourceName = string.IsNullOrEmpty(themeName) ? _currentTheme : themeName;
            return new Uri($"/Themes/{resourceName}Theme.xaml", UriKind.Relative);
        }
    }
}
```

### 7. 実装例：モデル拡張とプラグイン

```csharp name=Models/TextDocumentProject.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AppFramework.Interfaces;

namespace AppFramework.Models
{
    /// <summary>
    /// テキストドキュメントプロジェクトモデル
    /// </summary>
    public class TextDocumentProject : IProject
    {
        private string _name;
        private string _filePath;
        private string _content = string.Empty;
        private bool _isDirty;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public object Content
        {
            get => _content;
            set => SetProperty(ref _content, (string)value);
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        public string ProjectType => "Text Document";

        public TextDocumentProject(string name = "New Document")
        {
            _name = name;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
```

```csharp name=Plugins/TextDocumentPlugin.cs
using System;
using System.Collections.Generic;
using AppFramework.Interfaces;
using AppFramework.Models;
using AppFramework.Services;

namespace AppFramework.Plugins
{
    /// <summary>
    /// テキストドキュメント用プラグイン
    /// </summary>
    public class TextDocumentPlugin : IPlugin
    {
        private TextDocumentService _textDocumentService;
        
        public string Name => "Text Document Plugin";
        public string Version => "1.0.0";
        public string Description => "Provides text document editing capabilities";
        public string Author => "Your Name";
        
        public void Initialize(DependencyContainer container)
        {
            // プラグイン固有のサービスを登録
            _textDocumentService = new TextDocumentService(container);
            container.RegisterService<ITextDocumentService, TextDocumentService>();
            
            // コマンドを登録
            var commandManager = container.GetService<ICommandManager>();
            
            // UI拡張ポイント（実際のアプリケーションでは特定のUIフレームワークに依存）
            var uiExtensionService = container.GetService<IUIExtensionService>();
            if (uiExtensionService != null)
            {
                uiExtensionService.RegisterMenuItems(new[]
                {
                    new MenuItem { Path = "File/New/Text Document", Command = "NewTextDocument" },
                });
            }
        }
        
        public IEnumerable<IProjectService> GetProjectServices()
        {
            yield return _textDocumentService;
        }
    }
    
    /// <summary>
    /// テキストドキュメント用サービス
    /// </summary>
    public class TextDocumentService : IProjectService, ITextDocumentService
    {
        private readonly DependencyContainer _container;
        
        public string[] SupportedFileExtensions => new[] { ".txt", ".md", ".json", ".xml", ".html", ".css", ".js" };
        public string FileTypeDescription => "Text Document";
        
        public TextDocumentService(DependencyContainer container)
        {
            _container = container;
        }
        
        public IProjectContext CreateNew(string name = "New Document")
        {
            var project = new TextDocumentProject(name);
            return new ProjectContext(project);
        }
        
        public async Task<IProjectContext> OpenAsync(string filePath)
        {
            var project = new TextDocumentProject(System.IO.Path.GetFileNameWithoutExtension(filePath))
            {
                FilePath = filePath,
                Content = await System.IO.File.ReadAllTextAsync(filePath)
            };
            
            return new ProjectContext(project);
        }
        
        public async Task SaveAsync(IProjectContext context)
        {
            if (string.IsNullOrEmpty(context.Project.FilePath))
            {
                throw new InvalidOperationException("File path is required");
            }
            
            await System.IO.File.WriteAllTextAsync(context.Project.FilePath, context.Project.Content.ToString());
            context.MarkAsUnmodified();
        }
        
        public async Task SaveAsAsync(IProjectContext context, string filePath)
        {
            await System.IO.File.WriteAllTextAsync(filePath, context.Project.Content.ToString());
            context.Project.FilePath = filePath;
            context.Project.Name = System.IO.Path.GetFileNameWithoutExtension(filePath);
            context.MarkAsUnmodified();
        }
        
        public async Task<bool> ConfirmSaveIfModifiedAsync(IProjectContext context)
        {
            if (!context.IsModified)
                return true;
                
            var dialogService = _container.GetService<IDialogService>();
            var result = await dialogService.ShowMessageBoxAsync(
                $"Save changes to {context.Project.Name}?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
                
            switch (result)
            {
                case DialogResult.Yes:
                    if (string.IsNullOrEmpty(context.Project.FilePath))
                    {
                        var fileInfo = await dialogService.ShowSaveFileDialogAsync(
                            "Save Document",
                            SupportedFileExtensions,
                            FileTypeDescription);
                            
                        if (fileInfo == null || string.IsNullOrEmpty(fileInfo.FilePath))
                            return false;
                            
                        await SaveAsAsync(context, fileInfo.FilePath);
                    }
                    else
                    {
                        await SaveAsync(context);
                    }
                    return true;
                case DialogResult.No:
                    return true;
                default:
                    return false;
            }
        }
        
        // テキスト特有の機能
        public void FormatText(IProjectContext context, TextFormatOptions options)
        {
            if (context.Project is TextDocumentProject textProject)
            {
                string content = textProject.Content.ToString();
                
                // フォーマット処理
                if (options.TrimWhitespace)
                {
                    content = content.Trim();
                }
                
                if (options.NormalizeLineEndings)
                {
                    content = content.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
                }
                
                textProject.Content = content;
            }
        }
    }
    
    public interface ITextDocumentService
    {
        void FormatText(IProjectContext context, TextFormatOptions options);
    }
    
    public class TextFormatOptions
    {
        public bool TrimWhitespace { get; set; } = true;
        public bool NormalizeLineEndings { get; set; } = true;
    }
}
```

## 拡張の実装ポイント

1. **汎用性の向上**
   - インターフェースによる疎結合な設計
   - 依存性注入によるサービスの管理
   - プラグインによる機能拡張

2. **マルチドキュメント対応**
   - 複数プロジェクトの同時管理
   - アクティブなプロジェクトの切り替え
   - プロジェクトコレクションの提供

3. **コマンドパターン導入**
   - メニュー操作を抽象化
   - Undo/Redo機能の実装
   - コマンド履歴の管理

4. **プラグインシステム**
   - 動的なプラグインのロード
   - 拡張ポイントの提供
   - 新しいプロジェクトタイプの追加

5. **各種サービス**
   - ロギング機能
   - 設定管理
   - テーマとスタイル設定
   - ダイアログサービス

このアーキテクチャを使用すると、さまざまなタイプのアプリケーションに適用可能な汎用的なフレームワークとなり、機能の拡張も容易になります。基本構造を保ちながら、特定のアプリケーションドメインに合わせたカスタマイズが可能です。


---


# 汎用的なデスクトップアプリケーションフレームワークへの拡張

先ほど提案したプロジェクト管理システムを、より汎用的なデスクトップアプリケーションのベースとして拡張します。以下の機能を追加して、堅牢で拡張性の高いフレームワークを構築しましょう。

## 拡張アーキテクチャの概要

1. **MVVM パターンの完全実装**
2. **依存性注入（DI）によるサービスの管理**
3. **UI フレームワーク（WPF）との統合**
4. **コマンドパターンによるユーザーアクション処理**
5. **設定管理と永続化**
6. **テーマとスタイルのサポート**
7. **ロギングシステム**
8. **プラグインアーキテクチャ**

## 主要クラスの拡張

### AppContext の拡張（アプリケーションコア）

```csharp name=AppContext.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopAppFramework
{
    /// <summary>
    /// アプリケーション全体のコンテキストを管理する拡張クラス
    /// </summary>
    public class AppContext : IDisposable
    {
        private ProjectContext _currentProjectContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppContext> _logger;

        /// <summary>
        /// 依存性注入コンテナ
        /// </summary>
        public IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// 現在アクティブなプロジェクトコンテキスト
        /// </summary>
        public ProjectContext CurrentProjectContext
        {
            get => _currentProjectContext;
            private set
            {
                if (_currentProjectContext != null)
                {
                    _currentProjectContext.ModifiedChanged -= ProjectContext_ModifiedChanged;
                    _currentProjectContext.Dispose();
                }

                _currentProjectContext = value;

                if (_currentProjectContext != null)
                {
                    _currentProjectContext.ModifiedChanged += ProjectContext_ModifiedChanged;
                }

                UpdateApplicationState();
            }
        }

        /// <summary>
        /// アプリケーションタイトル
        /// </summary>
        public string ApplicationTitle
        {
            get
            {
                string baseTitle = AppSettings.ApplicationName;
                if (CurrentProjectContext == null)
                    return baseTitle;

                string modifiedIndicator = CurrentProjectContext.IsModified ? "*" : "";
                return $"{CurrentProjectContext.Project.Name}{modifiedIndicator} - {baseTitle}";
            }
        }

        /// <summary>
        /// 利用可能なテーマの一覧
        /// </summary>
        public IReadOnlyList<ThemeInfo> AvailableThemes { get; private set; }

        /// <summary>
        /// 現在のテーマ
        /// </summary>
        public ThemeInfo CurrentTheme { get; private set; }

        /// <summary>
        /// アプリケーション設定
        /// </summary>
        public AppSettings AppSettings { get; private set; }

        /// <summary>
        /// アプリケーション状態が変更された時に発生するイベント
        /// </summary>
        public event EventHandler ApplicationStateChanged;

        /// <summary>
        /// テーマが変更された時に発生するイベント
        /// </summary>
        public event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        /// <summary>
        /// アプリケーションが終了する時に発生するイベント
        /// </summary>
        public event EventHandler<AppClosingEventArgs> ApplicationClosing;

        /// <summary>
        /// プラグインマネージャー
        /// </summary>
        public PluginManager PluginManager { get; private set; }

        /// <summary>
        /// AppContextのコンストラクタ
        /// </summary>
        public AppContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = serviceProvider.GetRequiredService<ILogger<AppContext>>();
            
            // 設定のロード
            AppSettings = serviceProvider.GetRequiredService<AppSettings>();
            
            // 利用可能なテーマのロード
            LoadThemes();
            
            // プラグインマネージャーの初期化
            PluginManager = new PluginManager(this);
            PluginManager.LoadPlugins();
            
            _logger.LogInformation("Application initialized");
        }

        /// <summary>
        /// 利用可能なテーマをロード
        /// </summary>
        private void LoadThemes()
        {
            var themes = new List<ThemeInfo>
            {
                new ThemeInfo("Light", "pack://application:,,,/Themes/Light.xaml"),
                new ThemeInfo("Dark", "pack://application:,,,/Themes/Dark.xaml"),
                new ThemeInfo("Blue", "pack://application:,,,/Themes/Blue.xaml")
            };
            
            AvailableThemes = themes;
            
            // デフォルトテーマもしくは保存されたテーマの設定
            string savedTheme = AppSettings.CurrentTheme;
            CurrentTheme = themes.Find(t => t.Name == savedTheme) ?? themes[0];
        }

        /// <summary>
        /// テーマを変更
        /// </summary>
        /// <param name="themeName">テーマ名</param>
        public void ChangeTheme(string themeName)
        {
            var theme = AvailableThemes.FirstOrDefault(t => t.Name == themeName);
            if (theme != null && theme != CurrentTheme)
            {
                CurrentTheme = theme;
                AppSettings.CurrentTheme = theme.Name;
                AppSettings.Save();
                
                ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(theme));
                _logger.LogInformation($"Theme changed to {theme.Name}");
            }
        }

        /// <summary>
        /// アプリケーションの状態を更新
        /// </summary>
        private void UpdateApplicationState()
        {
            ApplicationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// プロジェクトの変更状態が変わった時の処理
        /// </summary>
        private void ProjectContext_ModifiedChanged(object sender, EventArgs e)
        {
            UpdateApplicationState();
        }

        /// <summary>
        /// アプリケーション終了前の処理
        /// </summary>
        /// <returns>終了処理が許可されたかどうか</returns>
        public bool OnApplicationClosing()
        {
            var args = new AppClosingEventArgs();
            ApplicationClosing?.Invoke(this, args);
            
            if (args.Cancel)
                return false;
                
            // 未保存の変更を確認
            if (CurrentProjectContext != null && CurrentProjectContext.IsModified)
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                if (!projectService.ConfirmSaveIfModified())
                    return false;
            }
            
            // 設定の保存
            AppSettings.Save();
            
            // プラグインのアンロード
            PluginManager.UnloadPlugins();
            
            _logger.LogInformation("Application closing");
            return true;
        }

        #region Project Commands

        /// <summary>
        /// 新規プロジェクトを作成
        /// </summary>
        /// <returns>操作成功かどうか</returns>
        public bool NewProject()
        {
            try
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                if (!projectService.ConfirmSaveIfModified())
                    return false;

                CurrentProjectContext = projectService.CreateNew();
                _logger.LogInformation("New project created");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new project");
                return false;
            }
        }

        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>操作成功かどうか</returns>
        public bool OpenProject(string filePath)
        {
            try
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                if (!projectService.ConfirmSaveIfModified())
                    return false;

                CurrentProjectContext = projectService.Open(filePath);
                
                // 最近使ったファイルリストに追加
                AppSettings.AddRecentFile(filePath);
                AppSettings.Save();
                
                _logger.LogInformation($"Project opened: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error opening project: {filePath}");
                return false;
            }
        }

        /// <summary>
        /// 現在のプロジェクトを保存
        /// </summary>
        /// <returns>操作成功かどうか</returns>
        public bool SaveProject()
        {
            if (CurrentProjectContext == null)
                return false;

            try
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                
                if (string.IsNullOrEmpty(CurrentProjectContext.Project.FilePath))
                {
                    // パスがない場合はSaveAsに転送
                    return SaveProjectAs(null);
                }
                
                projectService.Save(CurrentProjectContext);
                UpdateApplicationState();
                
                _logger.LogInformation($"Project saved: {CurrentProjectContext.Project.FilePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving project");
                return false;
            }
        }

        /// <summary>
        /// 現在のプロジェクトを別名で保存
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>操作成功かどうか</returns>
        public bool SaveProjectAs(string filePath)
        {
            if (CurrentProjectContext == null)
                return false;

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    // UIサービスを使用してファイル選択ダイアログを表示
                    var uiService = _serviceProvider.GetRequiredService<IUIService>();
                    filePath = uiService.ShowSaveFileDialog(
                        "Save Project",
                        AppSettings.ProjectFileFilter,
                        AppSettings.DefaultProjectExtension);
                    
                    if (string.IsNullOrEmpty(filePath))
                        return false;
                }

                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                projectService.SaveAs(CurrentProjectContext, filePath);
                
                // 最近使ったファイルリストに追加
                AppSettings.AddRecentFile(filePath);
                AppSettings.Save();
                
                UpdateApplicationState();
                
                _logger.LogInformation($"Project saved as: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving project as: {filePath}");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// リソースの解放
        /// </summary>
        public void Dispose()
        {
            if (_currentProjectContext != null)
            {
                _currentProjectContext.Dispose();
                _currentProjectContext = null;
            }
            
            PluginManager?.Dispose();
        }
    }
}
```

### 依存性注入によるサービスの登録

```csharp name=Program.cs
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DesktopAppFramework
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Serilogの設定
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                // サービスコンテナの構築
                var services = new ServiceCollection();
                ConfigureServices(services);
                
                var serviceProvider = services.BuildServiceProvider();

                // アプリケーションの実行
                var app = new App(serviceProvider);
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "アプリケーションで処理されない例外が発生しました");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // ロギング設定
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(dispose: true);
            });

            // アプリケーション設定
            services.AddSingleton<AppSettings>();
            
            // コアサービス
            services.AddSingleton<AppContext>();
            services.AddSingleton<IProjectService, ProjectService>();
            services.AddSingleton<IUIService, WpfUIService>();
            
            // 画面のビューモデル
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<ProjectPropertiesViewModel>();
            services.AddTransient<SettingsViewModel>();
            
            // コマンドハンドラ
            services.AddSingleton<ICommandHandler, FileCommandHandler>();
            services.AddSingleton<ICommandHandler, EditCommandHandler>();
            services.AddSingleton<ICommandHandler, ViewCommandHandler>();
            services.AddSingleton<ICommandHandler, HelpCommandHandler>();
            
            // ダイアログサービス
            services.AddSingleton<IDialogService, DialogService>();
        }
    }
}
```

### IProjectService インターフェースの導入

```csharp name=IProjectService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DesktopAppFramework
{
    /// <summary>
    /// プロジェクト操作サービスのインターフェース
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// プロジェクトタイプの一覧を取得
        /// </summary>
        IReadOnlyList<ProjectType> ProjectTypes { get; }
        
        /// <summary>
        /// 新規プロジェクトを作成
        /// </summary>
        /// <param name="projectType">プロジェクトタイプ（省略可）</param>
        /// <returns>作成されたプロジェクトコンテキスト</returns>
        ProjectContext CreateNew(ProjectType projectType = null);
        
        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>開いたプロジェクトコンテキスト</returns>
        ProjectContext Open(string filePath);
        
        /// <summary>
        /// プロジェクトを保存
        /// </summary>
        /// <param name="context">保存するプロジェクトコンテキスト</param>
        void Save(ProjectContext context);
        
        /// <summary>
        /// プロジェクトを別名で保存
        /// </summary>
        /// <param name="context">保存するプロジェクトコンテキスト</param>
        /// <param name="filePath">保存先ファイルパス</param>
        void SaveAs(ProjectContext context, string filePath);
        
        /// <summary>
        /// 現在のプロジェクトに未保存の変更がある場合、保存するかどうか確認
        /// </summary>
        /// <returns>続行可能か（true=続行可能、false=操作キャンセル）</returns>
        bool ConfirmSaveIfModified();
        
        /// <summary>
        /// プロジェクトファイルの有効性を検証
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>有効なプロジェクトファイルかどうか</returns>
        bool ValidateProjectFile(string filePath);
    }
}
```

### ProjectServiceの拡張実装

```csharp name=ProjectService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DesktopAppFramework
{
    /// <summary>
    /// プロジェクト操作サービスの実装クラス
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly AppContext _appContext;
        private readonly IUIService _uiService;
        private readonly ILogger<ProjectService> _logger;
        private readonly List<ProjectType> _projectTypes = new List<ProjectType>();

        /// <summary>
        /// プロジェクトタイプの一覧
        /// </summary>
        public IReadOnlyList<ProjectType> ProjectTypes => _projectTypes.AsReadOnly();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ProjectService(
            AppContext appContext, 
            IUIService uiService, 
            ILogger<ProjectService> logger)
        {
            _appContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
            _uiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // プロジェクトタイプの登録
            RegisterDefaultProjectTypes();
        }

        /// <summary>
        /// デフォルトのプロジェクトタイプを登録
        /// </summary>
        private void RegisterDefaultProjectTypes()
        {
            _projectTypes.Add(new ProjectType
            {
                Id = "default",
                Name = "Default Project",
                Description = "Standard project type",
                FileExtension = ".proj",
                Icon = "pack://application:,,,/Icons/default_project.png",
                CreateProject = () => new Project("New Project")
            });
            
            _logger.LogInformation("Default project types registered");
        }

        /// <summary>
        /// プロジェクトタイプを登録
        /// </summary>
        /// <param name="projectType">プロジェクトタイプ</param>
        public void RegisterProjectType(ProjectType projectType)
        {
            if (projectType == null)
                throw new ArgumentNullException(nameof(projectType));
                
            if (_projectTypes.Any(pt => pt.Id == projectType.Id))
            {
                throw new InvalidOperationException($"Project type with ID '{projectType.Id}' is already registered");
            }
            
            _projectTypes.Add(projectType);
            _logger.LogInformation($"Project type registered: {projectType.Id}");
        }

        /// <summary>
        /// 新規プロジェクトを作成
        /// </summary>
        /// <param name="projectType">プロジェクトタイプ（省略可）</param>
        /// <returns>作成されたプロジェクトコンテキスト</returns>
        public ProjectContext CreateNew(ProjectType projectType = null)
        {
            projectType ??= _projectTypes.First();
            
            var project = projectType.CreateProject();
            return new ProjectContext(project);
        }

        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>開いたプロジェクトコンテキスト</returns>
        public ProjectContext Open(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Project file not found", filePath);

            try
            {
                string json = File.ReadAllText(filePath);
                var project = JsonSerializer.Deserialize<Project>(json);
                
                if (project == null)
                    throw new InvalidOperationException("Failed to deserialize project");

                project.FilePath = filePath;
                project.IsDirty = false;

                var context = new ProjectContext(project);
                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error opening project: {filePath}");
                throw new InvalidOperationException($"Error opening project: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// プロジェクトを保存
        /// </summary>
        /// <param name="context">保存するプロジェクトコンテキスト</param>
        public void Save(ProjectContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrEmpty(context.Project.FilePath))
            {
                throw new InvalidOperationException("Cannot save project without file path. Use SaveAs instead.");
            }

            SaveToFile(context, context.Project.FilePath);
            context.MarkAsUnmodified();
        }

        /// <summary>
        /// プロジェクトを別名で保存
        /// </summary>
        /// <param name="context">保存するプロジェクトコンテキスト</param>
        /// <param name="filePath">保存先ファイルパス</param>
        public void SaveAs(ProjectContext context, string filePath)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            SaveToFile(context, filePath);
            context.Project.FilePath = filePath;
            context.Project.Name = Path.GetFileNameWithoutExtension(filePath);
            context.MarkAsUnmodified();
        }

        /// <summary>
        /// プロジェクトを指定されたファイルに保存
        /// </summary>
        private void SaveToFile(ProjectContext context, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(context.Project, options);
                File.WriteAllText(filePath, json);
                _logger.LogInformation($"Project saved to: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving project to file: {filePath}");
                throw new InvalidOperationException($"Error saving project: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 現在のプロジェクトに未保存の変更がある場合、保存するかどうか確認
        /// </summary>
        /// <returns>続行可能か（true=続行可能、false=操作キャンセル）</returns>
        public bool ConfirmSaveIfModified()
        {
            var currentContext = _appContext.CurrentProjectContext;
            if (currentContext == null || !currentContext.IsModified)
                return true;

            var result = _uiService.ShowMessageBox(
                $"プロジェクト '{currentContext.Project.Name}' に未保存の変更があります。保存しますか?",
                "保存確認",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            
            switch (result)
            {
                case MessageBoxResult.Yes:
                    try
                    {
                        if (string.IsNullOrEmpty(currentContext.Project.FilePath))
                        {
                            // ファイル選択ダイアログを表示
                            string filePath = _uiService.ShowSaveFileDialog(
                                "プロジェクトの保存",
                                _appContext.AppSettings.ProjectFileFilter,
                                _appContext.AppSettings.DefaultProjectExtension);
                                
                            if (string.IsNullOrEmpty(filePath))
                                return false;
                                
                            SaveAs(currentContext, filePath);
                        }
                        else
                        {
                            Save(currentContext);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during save confirmation");
                        _uiService.ShowMessageBox(
                            $"保存エラー: {ex.Message}", 
                            "エラー", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                        return false;
                    }
                case MessageBoxResult.No:
                    return true;
                case MessageBoxResult.Cancel:
                default:
                    return false;
            }
        }

        /// <summary>
        /// プロジェクトファイルの有効性を検証
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>有効なプロジェクトファイルかどうか</returns>
        public bool ValidateProjectFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;
                
            try
            {
                string json = File.ReadAllText(filePath);
                var project = JsonSerializer.Deserialize<Project>(json);
                return project != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
```

### UIサービスインターフェース

```csharp name=IUIService.cs
using System;
using System.Windows;

namespace DesktopAppFramework
{
    /// <summary>
    /// UI操作のサービスインターフェース
    /// </summary>
    public interface IUIService
    {
        /// <summary>
        /// メッセージボックスを表示
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="title">タイトル</param>
        /// <param name="buttons">ボタン</param>
        /// <param name="icon">アイコン</param>
        /// <returns>結果</returns>
        MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon);
        
        /// <summary>
        /// ファイルを開くダイアログを表示
        /// </summary>
        /// <param name="title">ダイアログのタイトル</param>
        /// <param name="filter">ファイルフィルタ</param>
        /// <param name="defaultExtension">デフォルト拡張子</param>
        /// <returns>選択されたファイルパス、キャンセル時は空文字</returns>
        string ShowOpenFileDialog(string title, string filter, string defaultExtension);
        
        /// <summary>
        /// ファイルを保存ダイアログを表示
        /// </summary>
        /// <param name="title">ダイアログのタイトル</param>
        /// <param name="filter">ファイルフィルタ</param>
        /// <param name="defaultExtension">デフォルト拡張子</param>
        /// <returns>選択されたファイルパス、キャンセル時は空文字</returns>
        string ShowSaveFileDialog(string title, string filter, string defaultExtension);
        
        /// <summary>
        /// フォルダ選択ダイアログを表示
        /// </summary>
        /// <param name="title">ダイアログのタイトル</param>
        /// <returns>選択されたフォルダパス、キャンセル時は空文字</returns>
        string ShowFolderBrowserDialog(string title);
        
        /// <summary>
        /// ダイアログを表示
        /// </summary>
        /// <typeparam name="TViewModel">ビューモデルの型</typeparam>
        /// <param name="viewModel">ダイアログのビューモデル</param>
        /// <param name="title">ダイアログのタイトル</param>
        /// <returns>ダイアログの結果</returns>
        bool? ShowDialog<TViewModel>(TViewModel viewModel, string title = null) where TViewModel : ViewModelBase;
        
        /// <summary>
        /// 指定されたビューをメインワークスペースに表示
        /// </summary>
        /// <typeparam name="TViewModel">ビューモデルの型</typeparam>
        void ShowView<TViewModel>(TViewModel viewModel = null) where TViewModel : ViewModelBase;
    }
}
```

### WPF向けUIサービス実装

```csharp name=WpfUIService.cs
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace DesktopAppFramework
{
    /// <summary>
    /// WPF向けUIサービスの実装
    /// </summary>
    public class WpfUIService : IUIService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WpfUIService> _logger;

        public WpfUIService(IServiceProvider serviceProvider, ILogger<WpfUIService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// メッセージボックスを表示
        /// </summary>
        public MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return MessageBox.Show(message, title, buttons, icon);
        }

        /// <summary>
        /// ファイルを開くダイアログを表示
        /// </summary>
        public string ShowOpenFileDialog(string title, string filter, string defaultExtension)
        {
            var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = defaultExtension,
                CheckFileExists = true
            };

            bool? result = dialog.ShowDialog();
            return result == true ? dialog.FileName : string.Empty;
        }

        /// <summary>
        /// ファイルを保存ダイアログを表示
        /// </summary>
        public string ShowSaveFileDialog(string title, string filter, string defaultExtension)
        {
            var dialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = defaultExtension,
                AddExtension = true
            };

            bool? result = dialog.ShowDialog();
            return result == true ? dialog.FileName : string.Empty;
        }

        /// <summary>
        /// フォルダ選択ダイアログを表示
        /// </summary>
        public string ShowFolderBrowserDialog(string title)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = title,
                ShowNewFolderButton = true
            };

            var result = dialog.ShowDialog();
            return result == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : string.Empty;
        }

        /// <summary>
        /// ダイアログを表示
        /// </summary>
        public bool? ShowDialog<TViewModel>(TViewModel viewModel, string title = null) where TViewModel : ViewModelBase
        {
            var viewType = ViewLocator.GetViewTypeForViewModel(typeof(TViewModel));
            if (viewType == null)
            {
                _logger.LogError($"View not found for view model type: {typeof(TViewModel).Name}");
                return null;
            }
            
            // ビューのインスタンス作成
            var view = Activator.CreateInstance(viewType) as Window;
            if (view == null)
            {
                _logger.LogError($"Failed to create view instance: {viewType.Name}");
                return null;
            }
            
            // タイトル設定
            if (!string.IsNullOrEmpty(title))
            {
                view.Title = title;
            }
            
            // データコンテキスト設定
            view.DataContext = viewModel;
            
            // モーダルダイアログとして表示
            return view.ShowDialog();
        }

        /// <summary>
        /// 指定されたビューをメインワークスペースに表示
        /// </summary>
        public void ShowView<TViewModel>(TViewModel viewModel = null) where TViewModel : ViewModelBase
        {
            viewModel ??= _serviceProvider.GetRequiredService<TViewModel>();
            
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
            {
                _logger.LogError("Main window not found or not a MainWindow instance");
                return;
            }
            
            // MainWindowのWorkspaceにビューを表示
            var mainViewModel = mainWindow.DataContext as MainWindowViewModel;
            mainViewModel?.ShowWorkspaceContent(viewModel);
        }
    }
}
```

### アプリケーション設定

```csharp name=AppSettings.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DesktopAppFramework
{
    /// <summary>
    /// アプリケーション設定
    /// </summary>
    public class AppSettings
    {
        private readonly ILogger<AppSettings> _logger;
        private const int MaxRecentFiles = 10;

        // 全般設定
        public string ApplicationName { get; set; } = "Desktop App Framework";
        public string CurrentTheme { get; set; } = "Light";
        public bool CheckForUpdatesOnStartup { get; set; } = true;
        public bool AutoSaveEnabled { get; set; } = true;
        public int AutoSaveIntervalMinutes { get; set; } = 5;
        
        // プロジェクト設定
        public string DefaultProjectExtension { get; set; } = ".proj";
        public string ProjectFileFilter { get; set; } = "Project Files (*.proj)|*.proj|All Files (*.*)|*.*";
        public string DefaultProjectsDirectory { get; set; } = "";
        
        // 最近使用したファイル
        public List<string> RecentFiles { get; set; } = new List<string>();
        
        // ウィンドウ設定
        public double WindowWidth { get; set; } = 1200;
        public double WindowHeight { get; set; } = 800;
        public bool StartMaximized { get; set; } = false;

        /// <summary>
        /// 設定ファイルのパス
        /// </summary>
        private string SettingsFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ApplicationName,
            "settings.json");

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AppSettings(ILogger<AppSettings> logger = null)
        {
            _logger = logger;
            
            // デフォルトプロジェクトディレクトリの設定
            if (string.IsNullOrEmpty(DefaultProjectsDirectory))
            {
                DefaultProjectsDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    ApplicationName,
                    "Projects");
            }
            
            // 設定のロード
            Load();
        }

        /// <summary>
        /// 設定をファイルからロード
        /// </summary>
        public void Load()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    // 設定ファイルが存在しない場合はデフォルト値を使用
                    Save();
                    return;
                }

                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                
                if (settings != null)
                {
                    // プロパティをコピー
                    ApplicationName = settings.ApplicationName;
                    CurrentTheme = settings.CurrentTheme;
                    CheckForUpdatesOnStartup = settings.CheckForUpdatesOnStartup;
                    AutoSaveEnabled = settings.AutoSaveEnabled;
                    AutoSaveIntervalMinutes = settings.AutoSaveIntervalMinutes;
                    DefaultProjectExtension = settings.DefaultProjectExtension;
                    ProjectFileFilter = settings.ProjectFileFilter;
                    DefaultProjectsDirectory = settings.DefaultProjectsDirectory;
                    RecentFiles = settings.RecentFiles;
                    WindowWidth = settings.WindowWidth;
                    WindowHeight = settings.WindowHeight;
                    StartMaximized = settings.StartMaximized;
                }
                
                _logger?.LogInformation("Application settings loaded");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading application settings");
            }
        }

        /// <summary>
        /// 設定をファイルに保存
        /// </summary>
        public void Save()
        {
            try
            {
                // ディレクトリが存在しない場合は作成
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(SettingsFilePath, json);
                
                _logger?.LogInformation("Application settings saved");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving application settings");
            }
        }

        /// <summary>
        /// 最近使用したファイルを追加
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public void AddRecentFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;
                
            // 既に存在する場合は削除して先頭に追加
            RecentFiles.Remove(filePath);
            RecentFiles.Insert(0, filePath);
            
            // 最大数を超える場合は古いものを削除
            while (RecentFiles.Count > MaxRecentFiles)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }
        }
    }
}
```

### プラグインシステム

```csharp name=PluginManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DesktopAppFramework
{
    /// <summary>
    /// プラグインを管理するクラス
    /// </summary>
    public class PluginManager : IDisposable
    {
        private readonly AppContext _appContext;
        private readonly List<IPlugin> _loadedPlugins = new List<IPlugin>();
        private readonly ILogger<PluginManager> _logger;

        /// <summary>
        /// 読み込まれたプラグインのリスト
        /// </summary>
        public IReadOnlyList<IPlugin> LoadedPlugins => _loadedPlugins.AsReadOnly();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="appContext">アプリケーションコンテキスト</param>
        public PluginManager(AppContext appContext)
        {
            _appContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
            _logger = appContext.ServiceProvider.GetService(typeof(ILogger<PluginManager>)) as ILogger<PluginManager>;
        }

        /// <summary>
        /// プラグインを読み込む
        /// </summary>
        public void LoadPlugins()
        {
            string pluginsDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Plugins");
                
            if (!Directory.Exists(pluginsDirectory))
            {
                Directory.CreateDirectory(pluginsDirectory);
                _logger?.LogInformation($"Created plugins directory: {pluginsDirectory}");
                return;
            }

            try
            {
                // プラグインディレクトリ内のDLLを検索
                string[] dllFiles = Directory.GetFiles(pluginsDirectory, "*.dll");
                
                foreach (string dllPath in dllFiles)
                {
                    try
                    {
                        LoadPluginFromAssembly(dllPath);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"Error loading plugin from {dllPath}");
                    }
                }
                
                _logger?.LogInformation($"Loaded {_loadedPlugins.Count} plugins");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading plugins");
            }
        }

        /// <summary>
        /// 指定されたアセンブリからプラグインを読み込む
        /// </summary>
        /// <param name="assemblyPath">アセンブリパス</param>
        private void LoadPluginFromAssembly(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            
            // IPluginを実装する型を検索
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();
                
            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    // プラグインのインスタンスを作成
                    var plugin = Activator.CreateInstance(pluginType) as IPlugin;
                    
                    if (plugin != null)
                    {
                        // プラグインを初期化
                        plugin.Initialize(_appContext);
                        
                        // リストに追加
                        _loadedPlugins.Add(plugin);
                        
                        _logger?.LogInformation($"Loaded plugin: {plugin.Name} ({plugin.Version})");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Error initializing plugin: {pluginType.FullName}");
                }
            }
        }

        /// <summary>
        /// プラグインをアンロード
        /// </summary>
        public void UnloadPlugins()
        {
            foreach (var plugin in _loadedPlugins)
            {
                try
                {
                    plugin.Shutdown();
                    _logger?.LogInformation($"Unloaded plugin: {plugin.Name}");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Error unloading plugin: {plugin.Name}");
                }
            }
            
            _loadedPlugins.Clear();
        }

        /// <summary>
        /// リソースの解放
        /// </summary>
        public void Dispose()
        {
            UnloadPlugins();
        }
    }
}
```

### プラグインインターフェース

```csharp name=IPlugin.cs
using System;

namespace DesktopAppFramework
{
    /// <summary>
    /// プラグインのインターフェース
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// プラグイン名
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// プラグインの説明
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// プラグインのバージョン
        /// </summary>
        Version Version { get; }
        
        /// <summary>
        /// プラグインの作者
        /// </summary>
        string Author { get; }
        
        /// <summary>
        /// プラグインを初期化
        /// </summary>
        /// <param name="appContext">アプリケーションコンテキスト</param>
        void Initialize(AppContext appContext);
        
        /// <summary>
        /// プラグインをシャットダウン
        /// </summary>
        void Shutdown();
    }
}
```

### MVVM基底クラス

```csharp name=ViewModelBase.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopAppFramework
{
    /// <summary>
    /// ビューモデルの基底クラス
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティ変更通知イベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ変更通知を発生させる
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// プロパティ値を設定し、変更時に通知を発生させる
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">バッキングフィールド</param>
        /// <param name="value">新しい値</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>値が変更されたかどうか</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
```

### コマンドインフラストラクチャ

```csharp name=RelayCommand.cs
using System;
using System.Windows.Input;

namespace DesktopAppFramework
{
    /// <summary>
    /// 再利用可能なコマンド実装
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// 実行可能状態変更時のイベント
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="execute">実行処理</param>
        /// <param name="canExecute">実行可能判定</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// コマンドが実行可能かどうか判定
        /// </summary>
        /// <param name="parameter">パラメータ</param>
        /// <returns>実行可能かどうか</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// コマンドを実行
        /// </summary>
        /// <param name="parameter">パラメータ</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// コマンド実行可能状態の再評価を要求
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
```

## 拡張機能の説明

この拡張フレームワークは、以下のような機能を提供します：

1. **MVVM パターンの実装**
   - `ViewModelBase` クラスによるプロパティ変更通知
   - `RelayCommand` によるコマンドバインディング
   - `ViewLocator` による View と ViewModel の自動結合

2. **サービスベースのアーキテクチャ**
   - インターフェースベースの疎結合設計
   - 依存性注入による柔軟な構成
   - テスト容易性の向上

3. **プラグインシステム**
   - アプリケーションの機能を動的に拡張
   - サードパーティによる開発が容易

4. **テーマとスタイル**
   - 動的なテーマ切り替え
   - カスタマイズ可能なUIスタイル

5. **設定管理**
   - 設定の永続化
   - ユーザー設定の保存と読み込み

6. **ロギング**
   - 包括的なエラーおよび操作ログ
   - Serilogによる柔軟なログ出力

この拡張フレームワークをベースに、様々な種類のデスクトップアプリケーションを開発することができます。例えば、テキストエディタ、画像編集ソフト、データ分析ツールなど、プロジェクトの概念を持つほとんどのアプリケーションに応用可能です。

## 実装のポイント

- **UIフレームワーク独立性**: コアロジックとUIの分離により、将来異なるUI技術（Avalonia, WinUIなど）への移行が容易
- **テスト容易性**: インターフェースとDIの活用により単体テストが容易
- **拡張性**: プラグインアーキテクチャにより機能拡張が可能
- **保守性**: 関心の分離と標準パターンの採用により保守が容易

これらの拡張により、汎用的なデスクトップアプリケーションのベースとして使用できる堅牢なフレームワークが構築できます。


---


# WinFormsへのMVVMフレームワーク適用

前回提案した汎用デスクトップアプリケーションのアーキテクチャをWinFormsプラットフォームに適用します。WinFormsはWPFと異なりMVVMパターンをネイティブでサポートしていないため、いくつかの追加実装が必要になります。

## WinForms向け主要コンポーネント

### 1. BindableBase - WinForms向けバインディングサポート

```csharp name=BindableBase.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinFormsAppFramework
{
    /// <summary>
    /// WinForms向けデータバインディングをサポートする基底クラス
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティが変更されたときに発生するイベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ変更通知を発生させる
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// プロパティの値を設定し、変更があれば通知を発生させる
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="storage">バッキングフィールド</param>
        /// <param name="value">新しい値</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>値が変更されたかどうか</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
```

### 2. WinFormsCommand - WinForms向けコマンド実装

```csharp name=WinFormsCommand.cs
using System;

namespace WinFormsAppFramework
{
    /// <summary>
    /// WinForms向けコマンドパターン実装
    /// </summary>
    public class WinFormsCommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        /// <summary>
        /// コマンドの実行可能状態が変更されたときに発生するイベント
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// 実行可能状態の変更を通知
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="execute">実行するアクション</param>
        /// <param name="canExecute">実行可能かどうかを判断する関数</param>
        public WinFormsCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (_ => true);
        }

        /// <summary>
        /// コマンドが実行可能かどうか判断
        /// </summary>
        /// <param name="parameter">パラメータ</param>
        /// <returns>実行可能ならtrue</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        /// <summary>
        /// コマンドを実行
        /// </summary>
        /// <param name="parameter">パラメータ</param>
        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                _execute(parameter);
        }
    }

    /// <summary>
    /// パラメータなしのWinFormsコマンド
    /// </summary>
    public class WinFormsCommand<T> : WinFormsCommand
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="execute">実行するアクション</param>
        /// <param name="canExecute">実行可能かどうかを判断する関数</param>
        public WinFormsCommand(Action<T> execute, Func<T, bool> canExecute = null)
            : base(
                  p => execute(p == null ? default : (T)p),
                  canExecute == null ? null : p => canExecute(p == null ? default : (T)p))
        {
        }
    }
}
```

### 3. WinFormsUIService - WinForms向けUIサービス

```csharp name=WinFormsUIService.cs
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace WinFormsAppFramework
{
    /// <summary>
    /// WinForms向けUIサービスの実装
    /// </summary>
    public class WinFormsUIService : IUIService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WinFormsUIService> _logger;
        private readonly Form _mainForm;
        private readonly Dictionary<Type, Type> _viewModelToFormMap = new Dictionary<Type, Type>();

        public WinFormsUIService(IServiceProvider serviceProvider, Form mainForm, ILogger<WinFormsUIService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // ビューモデルとフォームの関連付けを登録
            RegisterViewModelMappings();
        }

        /// <summary>
        /// ビューモデルとフォームのマッピングを登録
        /// </summary>
        private void RegisterViewModelMappings()
        {
            // 例: _viewModelToFormMap.Add(typeof(ProjectViewModel), typeof(ProjectForm));
        }

        /// <summary>
        /// ビューモデルと関連付けられたフォームを登録
        /// </summary>
        /// <typeparam name="TViewModel">ビューモデルの型</typeparam>
        /// <typeparam name="TForm">フォームの型</typeparam>
        public void RegisterView<TViewModel, TForm>()
            where TViewModel : ViewModelBase
            where TForm : Form
        {
            _viewModelToFormMap[typeof(TViewModel)] = typeof(TForm);
        }

        /// <summary>
        /// メッセージボックスを表示
        /// </summary>
        public DialogResult ShowMessageBox(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(_mainForm, message, title, buttons, icon);
        }

        /// <summary>
        /// ファイルを開くダイアログを表示
        /// </summary>
        public string ShowOpenFileDialog(string title, string filter, string defaultExtension)
        {
            using var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = defaultExtension,
                CheckFileExists = true
            };

            return dialog.ShowDialog(_mainForm) == DialogResult.OK ? dialog.FileName : string.Empty;
        }

        /// <summary>
        /// ファイルを保存ダイアログを表示
        /// </summary>
        public string ShowSaveFileDialog(string title, string filter, string defaultExtension)
        {
            using var dialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = defaultExtension,
                AddExtension = true
            };

            return dialog.ShowDialog(_mainForm) == DialogResult.OK ? dialog.FileName : string.Empty;
        }

        /// <summary>
        /// フォルダ選択ダイアログを表示
        /// </summary>
        public string ShowFolderBrowserDialog(string title)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = title,
                ShowNewFolderButton = true
            };

            return dialog.ShowDialog(_mainForm) == DialogResult.OK ? dialog.SelectedPath : string.Empty;
        }

        /// <summary>
        /// モーダルダイアログを表示
        /// </summary>
        public DialogResult ShowDialog<TViewModel>(TViewModel viewModel, string title = null) 
            where TViewModel : ViewModelBase
        {
            if (!_viewModelToFormMap.TryGetValue(typeof(TViewModel), out Type formType))
            {
                _logger.LogError($"フォームが登録されていません: {typeof(TViewModel).Name}");
                return DialogResult.Cancel;
            }

            try
            {
                // フォームのインスタンスを作成
                Form form = (Form)Activator.CreateInstance(formType);
                
                // タイトル設定
                if (!string.IsNullOrEmpty(title))
                {
                    form.Text = title;
                }
                
                // ビューモデルをフォームにバインド
                BindViewModel(form, viewModel);
                
                // モーダルダイアログとして表示
                return form.ShowDialog(_mainForm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ダイアログの表示に失敗しました: {typeof(TViewModel).Name}");
                return DialogResult.Cancel;
            }
        }

        /// <summary>
        /// 子フォームを表示
        /// </summary>
        public void ShowChildForm<TViewModel>(TViewModel viewModel = null) 
            where TViewModel : ViewModelBase
        {
            if (viewModel == null)
            {
                viewModel = _serviceProvider.GetService(typeof(TViewModel)) as TViewModel;
                if (viewModel == null)
                {
                    _logger.LogError($"ビューモデルの取得に失敗しました: {typeof(TViewModel).Name}");
                    return;
                }
            }

            if (!_viewModelToFormMap.TryGetValue(typeof(TViewModel), out Type formType))
            {
                _logger.LogError($"フォームが登録されていません: {typeof(TViewModel).Name}");
                return;
            }

            try
            {
                // フォームのインスタンスを作成
                Form form = (Form)Activator.CreateInstance(formType);
                
                // ビューモデルをフォームにバインド
                BindViewModel(form, viewModel);
                
                // 親フォームのコンテナにフォームを表示
                if (_mainForm is MainForm mainForm)
                {
                    mainForm.ShowChildForm(form);
                }
                else
                {
                    // MDIアプリケーションの場合
                    if (_mainForm.IsMdiContainer)
                    {
                        form.MdiParent = _mainForm;
                        form.Show();
                    }
                    else
                    {
                        // 通常のフォームとして表示
                        form.Show(_mainForm);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"子フォームの表示に失敗しました: {typeof(TViewModel).Name}");
            }
        }

        /// <summary>
        /// ビューモデルをフォームにバインド
        /// </summary>
        private void BindViewModel(Form form, ViewModelBase viewModel)
        {
            // WinFormsDataBinderを使用してビューモデルをフォームにバインド
            if (form is IBindableView bindableView)
            {
                bindableView.BindViewModel(viewModel);
            }
            else
            {
                // フォームにTag経由でビューモデルを設定
                form.Tag = viewModel;
                
                // 必要に応じて手動バインディングを実装
                ApplyManualBindings(form, viewModel);
            }
        }

        /// <summary>
        /// フォームコントロールへの手動バインディングを適用
        /// </summary>
        private void ApplyManualBindings(Form form, ViewModelBase viewModel)
        {
            // このメソッドは実際のアプリケーションで具体的に実装
        }
    }
}
```

### 4. WinFormsDataBinder - WinFormsのデータバインディング実装

```csharp name=WinFormsDataBinder.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;

namespace WinFormsAppFramework
{
    /// <summary>
    /// WinForms向けデータバインディングを提供するクラス
    /// </summary>
    public class WinFormsDataBinder
    {
        private readonly List<IDisposable> _bindings = new List<IDisposable>();

        /// <summary>
        /// TextBoxをプロパティにバインド
        /// </summary>
        public void BindTextBox<T>(TextBox textBox, T source, Expression<Func<T, string>> propertyExpression)
            where T : INotifyPropertyChanged
        {
            if (textBox == null || source == null || propertyExpression == null)
                throw new ArgumentNullException();

            var binding = new TextBoxBinding<T>(textBox, source, propertyExpression);
            _bindings.Add(binding);
        }

        /// <summary>
        /// CheckBoxをプロパティにバインド
        /// </summary>
        public void BindCheckBox<T>(CheckBox checkBox, T source, Expression<Func<T, bool>> propertyExpression)
            where T : INotifyPropertyChanged
        {
            if (checkBox == null || source == null || propertyExpression == null)
                throw new ArgumentNullException();

            var binding = new CheckBoxBinding<T>(checkBox, source, propertyExpression);
            _bindings.Add(binding);
        }

        /// <summary>
        /// ComboBoxをプロパティにバインド
        /// </summary>
        public void BindComboBox<T, TItem>(ComboBox comboBox, T source, 
            Expression<Func<T, TItem>> selectedItemExpression, 
            IList<TItem> items, 
            string displayMember = null)
            where T : INotifyPropertyChanged
        {
            if (comboBox == null || source == null || selectedItemExpression == null || items == null)
                throw new ArgumentNullException();

            var binding = new ComboBoxBinding<T, TItem>(comboBox, source, selectedItemExpression, items, displayMember);
            _bindings.Add(binding);
        }

        /// <summary>
        /// ButtonをWinFormsCommandにバインド
        /// </summary>
        public void BindButton(Button button, WinFormsCommand command)
        {
            if (button == null || command == null)
                throw new ArgumentNullException();

            var binding = new ButtonCommandBinding(button, command);
            _bindings.Add(binding);
        }

        /// <summary>
        /// DataGridViewをリストプロパティにバインド
        /// </summary>
        public void BindDataGridView<T, TItem>(DataGridView dataGridView, T source, 
            Expression<Func<T, IEnumerable<TItem>>> propertyExpression)
            where T : INotifyPropertyChanged
        {
            if (dataGridView == null || source == null || propertyExpression == null)
                throw new ArgumentNullException();

            var binding = new DataGridViewBinding<T, TItem>(dataGridView, source, propertyExpression);
            _bindings.Add(binding);
        }

        /// <summary>
        /// すべてのバインディングを解除
        /// </summary>
        public void Unbind()
        {
            foreach (var binding in _bindings)
            {
                binding.Dispose();
            }
            _bindings.Clear();
        }

        #region Binding Implementations

        private class TextBoxBinding<T> : IDisposable where T : INotifyPropertyChanged
        {
            private readonly TextBox _textBox;
            private readonly T _source;
            private readonly PropertyInfo _property;
            private bool _updating;

            public TextBoxBinding(TextBox textBox, T source, Expression<Func<T, string>> propertyExpression)
            {
                _textBox = textBox;
                _source = source;

                // プロパティ情報を取得
                var memberExpression = propertyExpression.Body as MemberExpression;
                _property = memberExpression?.Member as PropertyInfo;

                if (_property == null)
                    throw new ArgumentException("Invalid property expression", nameof(propertyExpression));

                // 初期値設定
                _textBox.Text = (string)_property.GetValue(_source) ?? string.Empty;

                // イベントハンドラ登録
                _textBox.TextChanged += TextBox_TextChanged;
                _source.PropertyChanged += Source_PropertyChanged;
            }

            private void TextBox_TextChanged(object sender, EventArgs e)
            {
                if (_updating) return;
                _updating = true;

                try
                {
                    _property.SetValue(_source, _textBox.Text);
                }
                catch (Exception)
                {
                    // エラーハンドリング
                }
                finally
                {
                    _updating = false;
                }
            }

            private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (_updating) return;
                if (e.PropertyName == _property.Name)
                {
                    _updating = true;
                    try
                    {
                        _textBox.Text = (string)_property.GetValue(_source) ?? string.Empty;
                    }
                    finally
                    {
                        _updating = false;
                    }
                }
            }

            public void Dispose()
            {
                _textBox.TextChanged -= TextBox_TextChanged;
                _source.PropertyChanged -= Source_PropertyChanged;
            }
        }

        private class CheckBoxBinding<T> : IDisposable where T : INotifyPropertyChanged
        {
            private readonly CheckBox _checkBox;
            private readonly T _source;
            private readonly PropertyInfo _property;
            private bool _updating;

            public CheckBoxBinding(CheckBox checkBox, T source, Expression<Func<T, bool>> propertyExpression)
            {
                _checkBox = checkBox;
                _source = source;

                // プロパティ情報を取得
                var memberExpression = propertyExpression.Body as MemberExpression;
                _property = memberExpression?.Member as PropertyInfo;

                if (_property == null)
                    throw new ArgumentException("Invalid property expression", nameof(propertyExpression));

                // 初期値設定
                _checkBox.Checked = (bool)_property.GetValue(_source);

                // イベントハンドラ登録
                _checkBox.CheckedChanged += CheckBox_CheckedChanged;
                _source.PropertyChanged += Source_PropertyChanged;
            }

            private void CheckBox_CheckedChanged(object sender, EventArgs e)
            {
                if (_updating) return;
                _updating = true;

                try
                {
                    _property.SetValue(_source, _checkBox.Checked);
                }
                finally
                {
                    _updating = false;
                }
            }

            private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (_updating) return;
                if (e.PropertyName == _property.Name)
                {
                    _updating = true;
                    try
                    {
                        _checkBox.Checked = (bool)_property.GetValue(_source);
                    }
                    finally
                    {
                        _updating = false;
                    }
                }
            }

            public void Dispose()
            {
                _checkBox.CheckedChanged -= CheckBox_CheckedChanged;
                _source.PropertyChanged -= Source_PropertyChanged;
            }
        }

        private class ComboBoxBinding<T, TItem> : IDisposable where T : INotifyPropertyChanged
        {
            private readonly ComboBox _comboBox;
            private readonly T _source;
            private readonly PropertyInfo _property;
            private readonly IList<TItem> _items;
            private bool _updating;

            public ComboBoxBinding(ComboBox comboBox, T source, Expression<Func<T, TItem>> propertyExpression, 
                IList<TItem> items, string displayMember)
            {
                _comboBox = comboBox;
                _source = source;
                _items = items;

                // プロパティ情報を取得
                var memberExpression = propertyExpression.Body as MemberExpression;
                _property = memberExpression?.Member as PropertyInfo;

                if (_property == null)
                    throw new ArgumentException("Invalid property expression", nameof(propertyExpression));

                // コンボボックスの設定
                _comboBox.DataSource = null;
                _comboBox.Items.Clear();
                _comboBox.DisplayMember = displayMember;
                _comboBox.DataSource = _items;

                // 初期値設定
                var selectedValue = _property.GetValue(_source);
                int index = -1;
                for (int i = 0; i < _items.Count; i++)
                {
                    if (Equals(_items[i], selectedValue))
                    {
                        index = i;
                        break;
                    }
                }
                if (index >= 0)
                    _comboBox.SelectedIndex = index;

                // イベントハンドラ登録
                _comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
                _source.PropertyChanged += Source_PropertyChanged;
            }

            private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (_updating) return;
                _updating = true;

                try
                {
                    if (_comboBox.SelectedIndex >= 0 && _comboBox.SelectedIndex < _items.Count)
                    {
                        _property.SetValue(_source, _items[_comboBox.SelectedIndex]);
                    }
                }
                finally
                {
                    _updating = false;
                }
            }

            private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (_updating) return;
                if (e.PropertyName == _property.Name)
                {
                    _updating = true;
                    try
                    {
                        var selectedValue = _property.GetValue(_source);
                        int index = -1;
                        for (int i = 0; i < _items.Count; i++)
                        {
                            if (Equals(_items[i], selectedValue))
                            {
                                index = i;
                                break;
                            }
                        }
                        if (index >= 0)
                            _comboBox.SelectedIndex = index;
                    }
                    finally
                    {
                        _updating = false;
                    }
                }
            }

            public void Dispose()
            {
                _comboBox.SelectedIndexChanged -= ComboBox_SelectedIndexChanged;
                _source.PropertyChanged -= Source_PropertyChanged;
            }
        }

        private class ButtonCommandBinding : IDisposable
        {
            private readonly Button _button;
            private readonly WinFormsCommand _command;

            public ButtonCommandBinding(Button button, WinFormsCommand command)
            {
                _button = button;
                _command = command;

                // ボタンの初期状態を設定
                UpdateButtonState();

                // イベントハンドラ登録
                _button.Click += Button_Click;
                _command.CanExecuteChanged += Command_CanExecuteChanged;
            }

            private void Button_Click(object sender, EventArgs e)
            {
                _command.Execute(null);
            }

            private void Command_CanExecuteChanged(object sender, EventArgs e)
            {
                UpdateButtonState();
            }

            private void UpdateButtonState()
            {
                _button.Enabled = _command.CanExecute(null);
            }

            public void Dispose()
            {
                _button.Click -= Button_Click;
                _command.CanExecuteChanged -= Command_CanExecuteChanged;
            }
        }

        private class DataGridViewBinding<T, TItem> : IDisposable where T : INotifyPropertyChanged
        {
            private readonly DataGridView _dataGridView;
            private readonly T _source;
            private readonly PropertyInfo _property;
            private bool _updating;

            public DataGridViewBinding(DataGridView dataGridView, T source, Expression<Func<T, IEnumerable<TItem>>> propertyExpression)
            {
                _dataGridView = dataGridView;
                _source = source;

                // プロパティ情報を取得
                var memberExpression = propertyExpression.Body as MemberExpression;
                _property = memberExpression?.Member as PropertyInfo;

                if (_property == null)
                    throw new ArgumentException("Invalid property expression", nameof(propertyExpression));

                // 初期値設定
                UpdateDataSource();

                // イベントハンドラ登録
                _source.PropertyChanged += Source_PropertyChanged;
            }

            private void UpdateDataSource()
            {
                var items = _property.GetValue(_source) as IEnumerable<TItem>;
                _dataGridView.DataSource = items;
            }

            private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (_updating) return;
                if (e.PropertyName == _property.Name)
                {
                    _updating = true;
                    try
                    {
                        UpdateDataSource();
                    }
                    finally
                    {
                        _updating = false;
                    }
                }
            }

            public void Dispose()
            {
                _source.PropertyChanged -= Source_PropertyChanged;
            }
        }

        #endregion
    }
}
```

### 5. バインド可能なビューのインターフェース

```csharp name=IBindableView.cs
namespace WinFormsAppFramework
{
    /// <summary>
    /// ビューモデルをバインド可能なビューのインターフェース
    /// </summary>
    public interface IBindableView
    {
        /// <summary>
        /// ビューモデルをバインド
        /// </summary>
        /// <param name="viewModel">バインドするビューモデル</param>
        void BindViewModel(ViewModelBase viewModel);
    }
}
```

### 6. AppContextのWinForms実装

```csharp name=AppContext.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WinFormsAppFramework
{
    /// <summary>
    /// アプリケーション全体のコンテキストを管理するクラス
    /// </summary>
    public class AppContext : IDisposable
    {
        private ProjectContext _currentProjectContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppContext> _logger;
        private readonly IUIService _uiService;
        
        /// <summary>
        /// 依存性注入コンテナ
        /// </summary>
        public IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// 現在アクティブなプロジェクトコンテキスト
        /// </summary>
        public ProjectContext CurrentProjectContext
        {
            get => _currentProjectContext;
            private set
            {
                if (_currentProjectContext != null)
                {
                    _currentProjectContext.ModifiedChanged -= ProjectContext_ModifiedChanged;
                    _currentProjectContext.Dispose();
                }

                _currentProjectContext = value;

                if (_currentProjectContext != null)
                {
                    _currentProjectContext.ModifiedChanged += ProjectContext_ModifiedChanged;
                }

                UpdateApplicationState();
            }
        }

        /// <summary>
        /// アプリケーションタイトル
        /// </summary>
        public string ApplicationTitle
        {
            get
            {
                string baseTitle = AppSettings.ApplicationName;
                if (CurrentProjectContext == null)
                    return baseTitle;

                string modifiedIndicator = CurrentProjectContext.IsModified ? "*" : "";
                return $"{CurrentProjectContext.Project.Name}{modifiedIndicator} - {baseTitle}";
            }
        }

        /// <summary>
        /// アプリケーション設定
        /// </summary>
        public AppSettings AppSettings { get; private set; }

        /// <summary>
        /// アプリケーション状態が変更された時に発生するイベント
        /// </summary>
        public event EventHandler ApplicationStateChanged;

        /// <summary>
        /// アプリケーションが終了する時に発生するイベント
        /// </summary>
        public event EventHandler<AppClosingEventArgs> ApplicationClosing;

        /// <summary>
        /// プラグインマネージャー
        /// </summary>
        public PluginManager PluginManager { get; private set; }

        /// <summary>
        /// AppContextのコンストラクタ
        /// </summary>
        public AppContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = serviceProvider.GetRequiredService<ILogger<AppContext>>();
            _uiService = serviceProvider.GetRequiredService<IUIService>();
            
            // 設定のロード
            AppSettings = serviceProvider.GetRequiredService<AppSettings>();
            
            // プラグインマネージャーの初期化
            PluginManager = new PluginManager(this);
            PluginManager.LoadPlugins();
            
            _logger.LogInformation("Application initialized");
        }

        /// <summary>
        /// メインフォームのタイトルを更新
        /// </summary>
        public void UpdateMainFormTitle()
        {
            if (System.Windows.Forms.Application.OpenForms.Count > 0 && 
                System.Windows.Forms.Application.OpenForms[0] is Form mainForm)
            {
                mainForm.Text = ApplicationTitle;
            }
        }

        /// <summary>
        /// アプリケーションの状態を更新
        /// </summary>
        private void UpdateApplicationState()
        {
            UpdateMainFormTitle();
            ApplicationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// プロジェクトの変更状態が変わった時の処理
        /// </summary>
        private void ProjectContext_ModifiedChanged(object sender, EventArgs e)
        {
            UpdateApplicationState();
        }

        /// <summary>
        /// アプリケーション終了前の処理
        /// </summary>
        /// <returns>終了処理が許可されたかどうか</returns>
        public bool OnApplicationClosing()
        {
            var args = new AppClosingEventArgs();
            ApplicationClosing?.Invoke(this, args);
            
            if (args.Cancel)
                return false;
                
            // 未保存の変更を確認
            if (CurrentProjectContext != null && CurrentProjectContext.IsModified)
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                if (!projectService.ConfirmSaveIfModified())
                    return false;
            }
            
            // 設定の保存
            AppSettings.Save();
            
            // プラグインのアンロード
            PluginManager.UnloadPlugins();
            
            _logger.LogInformation("Application closing");
            return true;
        }

        #region Project Commands

        /// <summary>
        /// 新規プロジェクトを作成
        /// </summary>
        /// <returns>操作成功かどうか</returns>
        public bool NewProject()
        {
            try
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                if (!projectService.ConfirmSaveIfModified())
                    return false;

                CurrentProjectContext = projectService.CreateNew();
                _logger.LogInformation("New project created");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new project");
                _uiService.ShowMessageBox(
                    $"新規プロジェクトの作成エラー: {ex.Message}", 
                    "エラー", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>操作成功かどうか</returns>
        public bool OpenProject(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = _uiService.ShowOpenFileDialog(
                        "プロジェクトを開く",
                        AppSettings.ProjectFileFilter,
                        AppSettings.DefaultProjectExtension);
                        
                    if (string.IsNullOrEmpty(filePath))
                        return false;
                }

                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                if (!projectService.ConfirmSaveIfModified())
                    return false;

                CurrentProjectContext = projectService.Open(filePath);
                
                // 最近使ったファイルリストに追加
                AppSettings.AddRecentFile(filePath);
                AppSettings.Save();
                
                _logger.LogInformation($"Project opened: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error opening project: {filePath}");
                _uiService.ShowMessageBox(
                    $"プロジェクトを開く際にエラーが発生しました: {ex.Message}", 
                    "エラー", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 現在のプロジェクトを保存
        /// </summary>
        /// <returns>操作成功かどうか</returns>
        public bool SaveProject()
        {
            if (CurrentProjectContext == null)
                return false;

            try
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                
                if (string.IsNullOrEmpty(CurrentProjectContext.Project.FilePath))
                {
                    // パスがない場合はSaveAsに転送
                    return SaveProjectAs(null);
                }
                
                projectService.Save(CurrentProjectContext);
                UpdateApplicationState();
                
                _logger.LogInformation($"Project saved: {CurrentProjectContext.Project.FilePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving project");
                _uiService.ShowMessageBox(
                    $"プロジェクトの保存エラー: {ex.Message}", 
                    "エラー", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 現在のプロジェクトを別名で保存
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>操作成功かどうか</returns>
        public bool SaveProjectAs(string filePath)
        {
            if (CurrentProjectContext == null)
                return false;

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    // UIサービスを使用してファイル選択ダイアログを表示
                    filePath = _uiService.ShowSaveFileDialog(
                        "プロジェクトを別名で保存",
                        AppSettings.ProjectFileFilter,
                        AppSettings.DefaultProjectExtension);
                    
                    if (string.IsNullOrEmpty(filePath))
                        return false;
                }

                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                projectService.SaveAs(CurrentProjectContext, filePath);
                
                // 最近使ったファイルリストに追加
                AppSettings.AddRecentFile(filePath);
                AppSettings.Save();
                
                UpdateApplicationState();
                
                _logger.LogInformation($"Project saved as: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving project as: {filePath}");
                _uiService.ShowMessageBox(
                    $"プロジェクトの保存エラー: {ex.Message}", 
                    "エラー", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        /// <summary>
        /// リソースの解放
        /// </summary>
        public void Dispose()
        {
            if (_currentProjectContext != null)
            {
                _currentProjectContext.Dispose();
                _currentProjectContext = null;
            }
            
            PluginManager?.Dispose();
        }
    }

    /// <summary>
    /// アプリケーション終了イベント引数
    /// </summary>
    public class AppClosingEventArgs : EventArgs
    {
        /// <summary>
        /// 終了をキャンセルするかどうか
        /// </summary>
        public bool Cancel { get; set; }
    }
}
```

### 7. WinForms向けメインフォーム

```csharp name=MainForm.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace WinFormsAppFramework
{
    public partial class MainForm : Form, IBindableView
    {
        private readonly AppContext _appContext;
        private readonly IServiceProvider _serviceProvider;
        private MainViewModel _viewModel;
        private readonly WinFormsDataBinder _dataBinder = new WinFormsDataBinder();
        private Form _activeChildForm;

        public MainForm(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _appContext = serviceProvider.GetRequiredService<AppContext>();
            
            // フォームが閉じられる前のイベントハンドラ
            FormClosing += MainForm_FormClosing;
            
            // アプリケーション状態変更イベントを購読
            _appContext.ApplicationStateChanged += AppContext_ApplicationStateChanged;
            
            // メニュー項目をセットアップ
            SetupMenuItems();
            
            // ビューモデルの取得とバインド
            _viewModel = serviceProvider.GetRequiredService<MainViewModel>();
            BindViewModel(_viewModel);
        }

        public void BindViewModel(ViewModelBase viewModel)
        {
            if (!(viewModel is MainViewModel mainViewModel))
                throw new ArgumentException("ViewModel must be of type MainViewModel");
                
            _viewModel = mainViewModel;
            
            // データバインディングをクリア
            _dataBinder.Unbind();
            
            // メニュー項目を有効/無効に設定する
            UpdateMenuItemsState();
            
            // ここでコントロールとビューモデルのバインディングを設定
            // 例: _dataBinder.BindTextBox(textBoxTitle, _viewModel, vm => vm.Title);
            
            // ステータスバーのバインディング
            _dataBinder.BindTextBox(textBoxStatus, _viewModel, vm => vm.StatusMessage);
        }

        /// <summary>
        /// 子フォームをコンテナに表示
        /// </summary>
        public void ShowChildForm(Form form)
        {
            // 現在のアクティブな子フォームを閉じる
            if (_activeChildForm != null && !_activeChildForm.IsDisposed)
            {
                _activeChildForm.Hide();
            }
            
            // 新しい子フォームを設定
            _activeChildForm = form;
            
            if (!form.TopLevel)
            {
                // コントロールとして追加する場合
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;
                
                panelContent.Controls.Clear();
                panelContent.Controls.Add(form);
                form.Show();
            }
            else
            {
                // 通常のフォームとして表示する場合
                form.Show(this);
            }
            
            // フォームタイトルを更新
            Text = _appContext.ApplicationTitle;
        }

        /// <summary>
        /// メニュー項目のセットアップ
        /// </summary>
        private void SetupMenuItems()
        {
            // Fileメニュー
            var fileMenu = new ToolStripMenuItem("ファイル(&F)");
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("新規作成(&N)", null, (s, e) => _appContext.NewProject()));
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("開く(&O)...", null, (s, e) => _appContext.OpenProject(null)));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("保存(&S)", null, (s, e) => _appContext.SaveProject()));
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("名前を付けて保存(&A)...", null, (s, e) => _appContext.SaveProjectAs(null)));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            
            // 最近使ったファイルのサブメニュー
            var recentFilesMenu = new ToolStripMenuItem("最近使ったファイル");
            fileMenu.DropDownItems.Add(recentFilesMenu);
            UpdateRecentFilesMenu(recentFilesMenu);
            
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("終了(&X)", null, (s, e) => Close()));
            
            // Editメニュー
            var editMenu = new ToolStripMenuItem("編集(&E)");
            // 編集メニューの項目を追加
            
            // Viewメニュー
            var viewMenu = new ToolStripMenuItem("表示(&V)");
            // 表示メニューの項目を追加
            
            // Helpメニュー
            var helpMenu = new ToolStripMenuItem("ヘルプ(&H)");
            helpMenu.DropDownItems.Add(new ToolStripMenuItem("バージョン情報(&A)...", null, (s, e) => ShowAboutDialog()));
            
            // メニューバーに追加
            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(editMenu);
            menuStrip.Items.Add(viewMenu);
            menuStrip.Items.Add(helpMenu);
        }

        /// <summary>
        /// 最近使ったファイルメニューの更新
        /// </summary>
        private void UpdateRecentFilesMenu(ToolStripMenuItem recentFilesMenu)
        {
            recentFilesMenu.DropDownItems.Clear();
            
            if (_appContext.AppSettings.RecentFiles.Count == 0)
            {
                var emptyItem = new ToolStripMenuItem("(最近使ったファイルはありません)");
                emptyItem.Enabled = false;
                recentFilesMenu.DropDownItems.Add(emptyItem);
                return;
            }
            
            foreach (var filePath in _appContext.AppSettings.RecentFiles)
            {
                var item = new ToolStripMenuItem(filePath);
                item.Click += (s, e) => _appContext.OpenProject(filePath);
                recentFilesMenu.DropDownItems.Add(item);
            }
            
            recentFilesMenu.DropDownItems.Add(new ToolStripSeparator());
            recentFilesMenu.DropDownItems.Add(new ToolStripMenuItem("リストをクリア", null, (s, e) => {
                _appContext.AppSettings.RecentFiles.Clear();
                _appContext.AppSettings.Save();
                UpdateRecentFilesMenu(recentFilesMenu);
            }));
        }

        /// <summary>
        /// メニュー項目の状態を更新
        /// </summary>
        private void UpdateMenuItemsState()
        {
            bool hasProject = _appContext.CurrentProjectContext != null;
            
            // ファイルメニューの項目
            foreach (ToolStripItem item in ((ToolStripMenuItem)menuStrip.Items[0]).DropDownItems)
            {
                if (item.Text.Contains("保存"))
                {
                    item.Enabled = hasProject;
                }
            }
            
            // 他のメニュー項目も同様に更新
        }

        /// <summary>
        /// バージョン情報ダイアログを表示
        /// </summary>
        private void ShowAboutDialog()
        {
            var about = new AboutBox
            {
                ApplicationName = _appContext.AppSettings.ApplicationName,
                Version = Application.ProductVersion,
                Copyright = "Copyright © " + DateTime.Now.Year
            };
            
            about.ShowDialog(this);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // アプリケーション終了前の確認
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (!_appContext.OnApplicationClosing())
                {
                    e.Cancel = true;
                }
            }
        }

        private void AppContext_ApplicationStateChanged(object sender, EventArgs e)
        {
            // UI更新はUIスレッドで行う
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUI()));
            }
            else
            {
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            // フォームタイトルを更新
            Text = _appContext.ApplicationTitle;
            
            // メニュー項目の状態を更新
            UpdateMenuItemsState();
            
            // 最近使ったファイルメニューを更新
            var fileMenu = menuStrip.Items[0] as ToolStripMenuItem;
            var recentFilesMenu = fileMenu?.DropDownItems
                .OfType<ToolStripMenuItem>()
                .FirstOrDefault(item => item.Text == "最近使ったファイル");
                
            if (recentFilesMenu != null)
            {
                UpdateRecentFilesMenu(recentFilesMenu);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dataBinder.Unbind();
                _appContext.ApplicationStateChanged -= AppContext_ApplicationStateChanged;
                
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private System.ComponentModel.IContainer components = null;
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;
        private Panel panelContent;
        private TextBox textBoxStatus;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip = new MenuStrip();
            this.toolStrip = new ToolStrip();
            this.statusStrip = new StatusStrip();
            this.panelContent = new Panel();
            this.textBoxStatus = new TextBox();
            
            // 
            // menuStrip
            // 
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(800, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            
            // 
            // toolStrip
            // 
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(800, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "toolStrip1";
            
            // 
            // statusStrip
            // 
            this.statusStrip.Location = new System.Drawing.Point(0, 428);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(800, 22);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip1";
            
            //
            // textBoxStatus
            //
            this.textBoxStatus.BorderStyle = BorderStyle.None;
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.Dock = DockStyle.Fill;
            this.statusStrip.Items.Add(new ToolStripControlHost(textBoxStatus));
            
            // 
            // panelContent
            // 
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(0, 49);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(800, 379);
            this.panelContent.TabIndex = 3;
            
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "WinForms Application";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
```

### 8. WinForms向けプログラムエントリポイント

```csharp name=Program.cs
using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace WinFormsAppFramework
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイント
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Serilogの設定
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.Console()
                .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                // デザインモードでない場合のみ
                if (!System.ComponentModel.LicenseManager.UsageMode.Equals(System.ComponentModel.LicenseUsageMode.Designtime))
                {
                    // サービスコンテナの構築
                    var services = new ServiceCollection();
                    ConfigureServices(services);
                    
                    using (var serviceProvider = services.BuildServiceProvider())
                    {
                        // アプリケーションの標準設定
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        
                        // メインフォームの作成とアプリケーション実行
                        var mainForm = new MainForm(serviceProvider);
                        
                        // UIサービスの登録と初期化
                        var uiService = serviceProvider.GetRequiredService<IUIService>() as WinFormsUIService;
                        InitializeUIService(uiService);
                        
                        // アプリケーションの実行
                        Application.Run(mainForm);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "アプリケーションで処理されない例外が発生しました");
                MessageBox.Show(
                    $"アプリケーションでエラーが発生しました。\n\n{ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // ロギング設定
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(dispose: true);
            });

            // アプリケーション設定
            services.AddSingleton<AppSettings>();
            
            // コアサービス
            services.AddSingleton<AppContext>();
            services.AddSingleton<IProjectService, ProjectService>();
            
            // UIサービス (MainFormはまだ作成されていないので後で初期化)
            services.AddSingleton<IUIService>(provider => {
                var mainForm = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
                var logger = provider.GetRequiredService<ILogger<WinFormsUIService>>();
                return new WinFormsUIService(provider, mainForm, logger);
            });
            
            // ビューモデル
            services.AddTransient<MainViewModel>();
            services.AddTransient<ProjectViewModel>();
            services.AddTransient<SettingsViewModel>();
        }

        private static void InitializeUIService(WinFormsUIService uiService)
        {
            // ビューモデルとフォームのマッピングを登録
            uiService.RegisterView<ProjectViewModel, ProjectForm>();
            uiService.RegisterView<SettingsViewModel, SettingsForm>();
        }
    }
}
```

## 実装の特徴と注意点

### 1. データバインディング

WinFormsには直接的なデータバインディングメカニズムがないため、カスタムデータバインディングクラスを実装しています。`WinFormsDataBinder`はWPFのデータバインディングに似た機能を提供します。

### 2. コマンドパターン

`WinFormsCommand`クラスはWPFの`ICommand`に相当する機能を提供します。ボタンクリックとコマンド実行を分離し、MVVMパターンを実現します。

### 3. ビューとビューモデルの接続

`IBindableView`インターフェースを実装することで、フォームはビューモデルとの接続方法を定義します。

### 4. UI更新の同期

WinFormsでは`InvokeRequired`を使用してUIスレッドでのUI更新を行う必要があります。

### 5. フォームのコンテナ化

`MainForm`のパネルにフォームを埋め込むことで、WPFのコンテンツコントロールに似た機能を実現しています。

## クライアントコード例

プロジェクト編集フォームの実装例：

```csharp name=ProjectForm.cs
using System;
using System.Windows.Forms;

namespace WinFormsAppFramework
{
    public partial class ProjectForm : Form, IBindableView
    {
        private ProjectViewModel _viewModel;
        private readonly WinFormsDataBinder _dataBinder = new WinFormsDataBinder();

        public ProjectForm()
        {
            InitializeComponent();
        }

        public void BindViewModel(ViewModelBase viewModel)
        {
            if (!(viewModel is ProjectViewModel projectViewModel))
                throw new ArgumentException("ViewModel must be of type ProjectViewModel");
                
            _viewModel = projectViewModel;
            
            // データバインディングをクリア
            _dataBinder.Unbind();
            
            // テキストボックスとプロパティのバインディング
            _dataBinder.BindTextBox(textBoxName, _viewModel, vm => vm.Name);
            _dataBinder.BindTextBox(textBoxDescription, _viewModel, vm => vm.Description);
            
            // チェックボックスのバインディング
            _dataBinder.BindCheckBox(checkBoxIsDirty, _viewModel, vm => vm.IsDirty);
            
            // ボタンとコマンドのバインディング
            _dataBinder.BindButton(buttonSave, _viewModel.SaveCommand);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dataBinder.Unbind();
                
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
```

## まとめ

WinFormsではMVVMパターンを完全にサポートする組み込み機能がないため、データバインディングやコマンドパターンを自前で実装する必要があります。しかし、上記のような拡張クラスを使用することで、WPFに似た構造を持つアプリケーションを開発することができます。

この実装により、アプリケーションロジック（ViewModelやService）を変更せずにUI技術（WPFからWinFormsなど）を切り替えることが可能になります。また、テスト容易性や保守性も向上します。

以上の実装でWinFormsアプリケーションでもMVVMパターンを活用できる堅牢なアーキテクチャを構築することができます。


---


# WinForms向けデスクトップアプリケーションフレームワーク

前回提案したアーキテクチャをWinFormsアプリケーション向けに適応させます。WinFormsではMVVMパターンがWPFほど自然に実装できないため、適応型MVVMパターンとMVPパターンを組み合わせたアプローチを採用します。

## 新しいアーキテクチャの概要

1. **データバインディングの実装**
2. **フォームとコントロールの管理**
3. **WinForms対応のUIサービス**
4. **Eventベースのコマンドパターン**
5. **コアロジックの維持**

## 主要コンポーネントの実装

### WinFormsアプリケーションのエントリポイント

```csharp name=Program.cs
using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace WinFormsAppFramework
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメインエントリポイント
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Serilogの設定
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // サービスコンテナの構築
                var services = new ServiceCollection();
                ConfigureServices(services);
                
                var serviceProvider = services.BuildServiceProvider();

                // システム情報を記録
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation($"Application started at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                logger.LogInformation($"Current user: {Environment.UserName}");

                // アプリケーションコンテキストの初期化
                var appContext = serviceProvider.GetRequiredService<AppContext>();
                
                // メインフォームの表示
                var mainForm = serviceProvider.GetRequiredService<MainForm>();
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "アプリケーションで処理されない例外が発生しました");
                MessageBox.Show(
                    $"予期しないエラーが発生しました: {ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // ロギング設定
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(dispose: true);
            });

            // アプリケーション設定
            services.AddSingleton<AppSettings>();
            
            // コアサービス
            services.AddSingleton<AppContext>();
            services.AddSingleton<IProjectService, ProjectService>();
            services.AddSingleton<IUIService, WinFormsUIService>();
            services.AddSingleton<IDialogService, WinFormsDialogService>();
            
            // コマンドマネージャー
            services.AddSingleton<CommandManager>();
            
            // コマンドハンドラ
            services.AddSingleton<ICommandHandler, FileCommandHandler>();
            services.AddSingleton<ICommandHandler, EditCommandHandler>();
            services.AddSingleton<ICommandHandler, ViewCommandHandler>();
            services.AddSingleton<ICommandHandler, HelpCommandHandler>();
            
            // フォーム
            services.AddTransient<MainForm>();
            services.AddTransient<ProjectPropertiesForm>();
            services.AddTransient<SettingsForm>();
            
            // ビューモデル（WinFormsでも共通コアロジック用に使用）
            services.AddTransient<MainViewModel>();
            services.AddTransient<ProjectPropertiesViewModel>();
            services.AddTransient<SettingsViewModel>();
        }
    }
}
```

### AppContext - ほぼコア実装と同じ

```csharp name=AppContext.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WinFormsAppFramework
{
    /// <summary>
    /// アプリケーション全体のコンテキストを管理するクラス
    /// </summary>
    public class AppContext : IDisposable
    {
        private ProjectContext _currentProjectContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppContext> _logger;

        /// <summary>
        /// 依存性注入コンテナ
        /// </summary>
        public IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// 現在アクティブなプロジェクトコンテキスト
        /// </summary>
        public ProjectContext CurrentProjectContext
        {
            get => _currentProjectContext;
            private set
            {
                if (_currentProjectContext != null)
                {
                    _currentProjectContext.ModifiedChanged -= ProjectContext_ModifiedChanged;
                    _currentProjectContext.Dispose();
                }

                _currentProjectContext = value;

                if (_currentProjectContext != null)
                {
                    _currentProjectContext.ModifiedChanged += ProjectContext_ModifiedChanged;
                }

                OnApplicationStateChanged();
            }
        }

        /// <summary>
        /// アプリケーションタイトル
        /// </summary>
        public string ApplicationTitle
        {
            get
            {
                string baseTitle = AppSettings.ApplicationName;
                if (CurrentProjectContext == null)
                    return baseTitle;

                string modifiedIndicator = CurrentProjectContext.IsModified ? "*" : "";
                return $"{CurrentProjectContext.Project.Name}{modifiedIndicator} - {baseTitle}";
            }
        }

        /// <summary>
        /// 利用可能なテーマの一覧
        /// </summary>
        public IReadOnlyList<ThemeInfo> AvailableThemes { get; private set; }

        /// <summary>
        /// 現在のテーマ
        /// </summary>
        public ThemeInfo CurrentTheme { get; private set; }

        /// <summary>
        /// アプリケーション設定
        /// </summary>
        public AppSettings AppSettings { get; private set; }

        /// <summary>
        /// アプリケーション状態が変更された時に発生するイベント
        /// </summary>
        public event EventHandler ApplicationStateChanged;

        /// <summary>
        /// テーマが変更された時に発生するイベント
        /// </summary>
        public event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        /// <summary>
        /// アプリケーションが終了する時に発生するイベント
        /// </summary>
        public event EventHandler<AppClosingEventArgs> ApplicationClosing;

        /// <summary>
        /// プラグインマネージャー
        /// </summary>
        public PluginManager PluginManager { get; private set; }

        /// <summary>
        /// AppContextのコンストラクタ
        /// </summary>
        public AppContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = serviceProvider.GetRequiredService<ILogger<AppContext>>();
            
            // 設定のロード
            AppSettings = serviceProvider.GetRequiredService<AppSettings>();
            
            // 利用可能なテーマのロード
            LoadThemes();
            
            // プラグインマネージャーの初期化
            PluginManager = new PluginManager(this);
            PluginManager.LoadPlugins();
            
            _logger.LogInformation("Application initialized");
        }

        /// <summary>
        /// 利用可能なテーマをロード
        /// </summary>
        private void LoadThemes()
        {
            var themes = new List<ThemeInfo>
            {
                new ThemeInfo("Default", "Default"),
                new ThemeInfo("Blue", "Blue"),
                new ThemeInfo("Dark", "Dark")
            };
            
            AvailableThemes = themes;
            
            // デフォルトテーマもしくは保存されたテーマの設定
            string savedTheme = AppSettings.CurrentTheme;
            CurrentTheme = themes.Find(t => t.Name == savedTheme) ?? themes[0];
        }

        /// <summary>
        /// テーマを変更
        /// </summary>
        /// <param name="themeName">テーマ名</param>
        public void ChangeTheme(string themeName)
        {
            var theme = AvailableThemes.FirstOrDefault(t => t.Name == themeName);
            if (theme != null && theme != CurrentTheme)
            {
                CurrentTheme = theme;
                AppSettings.CurrentTheme = theme.Name;
                AppSettings.Save();
                
                OnThemeChanged(new ThemeChangedEventArgs(theme));
                _logger.LogInformation($"Theme changed to {theme.Name}");
            }
        }

        /// <summary>
        /// アプリケーションの状態を更新
        /// </summary>
        protected virtual void OnApplicationStateChanged()
        {
            ApplicationStateChanged?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// テーマ変更イベントを発生
        /// </summary>
        protected virtual void OnThemeChanged(ThemeChangedEventArgs args)
        {
            ThemeChanged?.Invoke(this, args);
        }

        /// <summary>
        /// プロジェクトの変更状態が変わった時の処理
        /// </summary>
        private void ProjectContext_ModifiedChanged(object sender, EventArgs e)
        {
            OnApplicationStateChanged();
        }

        /// <summary>
        /// アプリケーション終了前の処理
        /// </summary>
        /// <returns>終了処理が許可されたかどうか</returns>
        public bool OnApplicationClosing()
        {
            var args = new AppClosingEventArgs();
            ApplicationClosing?.Invoke(this, args);
            
            if (args.Cancel)
                return false;
                
            // 未保存の変更を確認
            if (CurrentProjectContext != null && CurrentProjectContext.IsModified)
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                if (!projectService.ConfirmSaveIfModified())
                    return false;
            }
            
            // 設定の保存
            AppSettings.Save();
            
            // プラグインのアンロード
            PluginManager.UnloadPlugins();
            
            _logger.LogInformation("Application closing");
            return true;
        }

        #region Project Commands

        /// <summary>
        /// 新規プロジェクトを作成
        /// </summary>
        /// <returns>操作成功かどうか</returns>
        public bool NewProject()
        {
            try
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                if (!projectService.ConfirmSaveIfModified())
                    return false;

                CurrentProjectContext = projectService.CreateNew();
                _logger.LogInformation("New project created");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new project");
                return false;
            }
        }

        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>操作成功かどうか</returns>
        public bool OpenProject(string filePath)
        {
            try
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                if (!projectService.ConfirmSaveIfModified())
                    return false;

                CurrentProjectContext = projectService.Open(filePath);
                
                // 最近使ったファイルリストに追加
                AppSettings.AddRecentFile(filePath);
                AppSettings.Save();
                
                _logger.LogInformation($"Project opened: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error opening project: {filePath}");
                return false;
            }
        }

        /// <summary>
        /// 現在のプロジェクトを保存
        /// </summary>
        /// <returns>操作成功かどうか</returns>
        public bool SaveProject()
        {
            if (CurrentProjectContext == null)
                return false;

            try
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                
                if (string.IsNullOrEmpty(CurrentProjectContext.Project.FilePath))
                {
                    // パスがない場合はSaveAsに転送
                    return SaveProjectAs(null);
                }
                
                projectService.Save(CurrentProjectContext);
                OnApplicationStateChanged();
                
                _logger.LogInformation($"Project saved: {CurrentProjectContext.Project.FilePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving project");
                return false;
            }
        }

        /// <summary>
        /// 現在のプロジェクトを別名で保存
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>操作成功かどうか</returns>
        public bool SaveProjectAs(string filePath)
        {
            if (CurrentProjectContext == null)
                return false;

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    // UIサービスを使用してファイル選択ダイアログを表示
                    var uiService = _serviceProvider.GetRequiredService<IUIService>();
                    filePath = uiService.ShowSaveFileDialog(
                        "Save Project",
                        AppSettings.ProjectFileFilter,
                        AppSettings.DefaultProjectExtension);
                    
                    if (string.IsNullOrEmpty(filePath))
                        return false;
                }

                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                projectService.SaveAs(CurrentProjectContext, filePath);
                
                // 最近使ったファイルリストに追加
                AppSettings.AddRecentFile(filePath);
                AppSettings.Save();
                
                OnApplicationStateChanged();
                
                _logger.LogInformation($"Project saved as: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving project as: {filePath}");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// リソースの解放
        /// </summary>
        public void Dispose()
        {
            if (_currentProjectContext != null)
            {
                _currentProjectContext.Dispose();
                _currentProjectContext = null;
            }
            
            PluginManager?.Dispose();
        }
    }

    /// <summary>
    /// テーマ情報クラス
    /// </summary>
    public class ThemeInfo
    {
        public string Name { get; }
        public string ResourcePath { get; }

        public ThemeInfo(string name, string resourcePath)
        {
            Name = name;
            ResourcePath = resourcePath;
        }
    }

    /// <summary>
    /// テーマ変更イベント引数
    /// </summary>
    public class ThemeChangedEventArgs : EventArgs
    {
        public ThemeInfo Theme { get; }

        public ThemeChangedEventArgs(ThemeInfo theme)
        {
            Theme = theme;
        }
    }

    /// <summary>
    /// アプリケーション終了イベント引数
    /// </summary>
    public class AppClosingEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }
}
```

### WinForms向けUIサービス

```csharp name=WinFormsUIService.cs
using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WinFormsAppFramework
{
    /// <summary>
    /// WinForms向けUIサービスの実装
    /// </summary>
    public class WinFormsUIService : IUIService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WinFormsUIService> _logger;
        private Form _activeForm;

        public WinFormsUIService(IServiceProvider serviceProvider, ILogger<WinFormsUIService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// メッセージボックスを表示
        /// </summary>
        public DialogResult ShowMessageBox(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(_activeForm, message, title, buttons, icon);
        }

        /// <summary>
        /// ファイルを開くダイアログを表示
        /// </summary>
        public string ShowOpenFileDialog(string title, string filter, string defaultExtension)
        {
            using var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = defaultExtension,
                CheckFileExists = true
            };

            var result = dialog.ShowDialog(_activeForm);
            return result == DialogResult.OK ? dialog.FileName : string.Empty;
        }

        /// <summary>
        /// ファイルを保存ダイアログを表示
        /// </summary>
        public string ShowSaveFileDialog(string title, string filter, string defaultExtension)
        {
            using var dialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = defaultExtension,
                AddExtension = true
            };

            var result = dialog.ShowDialog(_activeForm);
            return result == DialogResult.OK ? dialog.FileName : string.Empty;
        }

        /// <summary>
        /// フォルダ選択ダイアログを表示
        /// </summary>
        public string ShowFolderBrowserDialog(string title)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = title,
                ShowNewFolderButton = true
            };

            var result = dialog.ShowDialog(_activeForm);
            return result == DialogResult.OK ? dialog.SelectedPath : string.Empty;
        }

        /// <summary>
        /// 指定されたフォームをダイアログとして表示
        /// </summary>
        public DialogResult ShowDialog<TForm>(object viewModel = null) where TForm : Form
        {
            try
            {
                var form = _serviceProvider.GetRequiredService<TForm>();
                
                // ビューモデルを設定（フォームがIViewModelHostを実装している場合）
                if (viewModel != null && form is IViewModelHost host)
                {
                    host.SetViewModel(viewModel);
                }
                
                return form.ShowDialog(_activeForm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error showing dialog: {typeof(TForm).Name}");
                return DialogResult.None;
            }
        }

        /// <summary>
        /// アクティブなフォームを設定
        /// </summary>
        /// <param name="activeForm">アクティブなフォーム</param>
        public void SetActiveForm(Form activeForm)
        {
            _activeForm = activeForm;
        }

        /// <summary>
        /// プログレスダイアログを表示
        /// </summary>
        public void ShowProgressDialog(string title, string message, Action<IProgressReporter> action)
        {
            using var progressForm = new ProgressForm(title, message);
            progressForm.StartPosition = FormStartPosition.CenterParent;
            
            // バックグラウンド処理の設定
            progressForm.SetOperation(action);
            
            // ダイアログとして表示
            progressForm.ShowDialog(_activeForm);
        }

        /// <summary>
        /// フォームのテーマを適用
        /// </summary>
        public void ApplyTheme(Form form, ThemeInfo theme)
        {
            try
            {
                switch (theme.Name)
                {
                    case "Default":
                        // デフォルトテーマ
                        ApplyDefaultTheme(form);
                        break;
                    case "Blue":
                        // ブルーテーマ
                        ApplyBlueTheme(form);
                        break;
                    case "Dark":
                        // ダークテーマ
                        ApplyDarkTheme(form);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error applying theme: {theme.Name}");
            }
        }

        private void ApplyDefaultTheme(Form form)
        {
            form.BackColor = System.Drawing.SystemColors.Control;
            form.ForeColor = System.Drawing.SystemColors.ControlText;
            
            // フォーム内のコントロールにテーマを適用
            ApplyThemeToControls(form.Controls, ThemeColors.Default);
        }

        private void ApplyBlueTheme(Form form)
        {
            form.BackColor = ThemeColors.Blue.BackColor;
            form.ForeColor = ThemeColors.Blue.ForeColor;
            
            // フォーム内のコントロールにテーマを適用
            ApplyThemeToControls(form.Controls, ThemeColors.Blue);
        }

        private void ApplyDarkTheme(Form form)
        {
            form.BackColor = ThemeColors.Dark.BackColor;
            form.ForeColor = ThemeColors.Dark.ForeColor;
            
            // フォーム内のコントロールにテーマを適用
            ApplyThemeToControls(form.Controls, ThemeColors.Dark);
        }

        private void ApplyThemeToControls(Control.ControlCollection controls, ThemeColorScheme colors)
        {
            foreach (Control control in controls)
            {
                // 再帰的にすべてのコントロールにテーマを適用
                if (control.Controls.Count > 0)
                {
                    ApplyThemeToControls(control.Controls, colors);
                }
                
                // コントロールタイプに基づいてテーマを適用
                if (control is Button btn)
                {
                    btn.BackColor = colors.ButtonBackColor;
                    btn.ForeColor = colors.ButtonForeColor;
                    btn.FlatStyle = colors.ButtonFlatStyle;
                }
                else if (control is TextBox txt)
                {
                    txt.BackColor = colors.TextBoxBackColor;
                    txt.ForeColor = colors.TextBoxForeColor;
                }
                else if (control is Panel panel)
                {
                    panel.BackColor = colors.PanelBackColor;
                    panel.ForeColor = colors.PanelForeColor;
                }
                // その他のコントロールタイプに対しても適宜設定
            }
        }
    }

    /// <summary>
    /// プログレス報告インターフェース
    /// </summary>
    public interface IProgressReporter
    {
        void ReportProgress(int percentComplete, string message = null);
        bool IsCancellationRequested { get; }
    }

    /// <summary>
    /// テーマカラースキーム
    /// </summary>
    public class ThemeColorScheme
    {
        public System.Drawing.Color BackColor { get; set; }
        public System.Drawing.Color ForeColor { get; set; }
        public System.Drawing.Color ButtonBackColor { get; set; }
        public System.Drawing.Color ButtonForeColor { get; set; }
        public System.Drawing.Color TextBoxBackColor { get; set; }
        public System.Drawing.Color TextBoxForeColor { get; set; }
        public System.Drawing.Color PanelBackColor { get; set; }
        public System.Drawing.Color PanelForeColor { get; set; }
        public FlatStyle ButtonFlatStyle { get; set; } = FlatStyle.Standard;
    }

    /// <summary>
    /// テーマカラー定義
    /// </summary>
    public static class ThemeColors
    {
        public static ThemeColorScheme Default { get; } = new ThemeColorScheme
        {
            BackColor = System.Drawing.SystemColors.Control,
            ForeColor = System.Drawing.SystemColors.ControlText,
            ButtonBackColor = System.Drawing.SystemColors.Control,
            ButtonForeColor = System.Drawing.SystemColors.ControlText,
            TextBoxBackColor = System.Drawing.SystemColors.Window,
            TextBoxForeColor = System.Drawing.SystemColors.WindowText,
            PanelBackColor = System.Drawing.SystemColors.Control,
            PanelForeColor = System.Drawing.SystemColors.ControlText,
            ButtonFlatStyle = FlatStyle.Standard
        };

        public static ThemeColorScheme Blue { get; } = new ThemeColorScheme
        {
            BackColor = System.Drawing.Color.FromArgb(240, 240, 255),
            ForeColor = System.Drawing.Color.FromArgb(0, 0, 64),
            ButtonBackColor = System.Drawing.Color.FromArgb(210, 230, 255),
            ButtonForeColor = System.Drawing.Color.FromArgb(0, 0, 100),
            TextBoxBackColor = System.Drawing.Color.White,
            TextBoxForeColor = System.Drawing.Color.FromArgb(0, 0, 100),
            PanelBackColor = System.Drawing.Color.FromArgb(230, 240, 255),
            PanelForeColor = System.Drawing.Color.FromArgb(0, 0, 64),
            ButtonFlatStyle = FlatStyle.Flat
        };

        public static ThemeColorScheme Dark { get; } = new ThemeColorScheme
        {
            BackColor = System.Drawing.Color.FromArgb(50, 50, 50),
            ForeColor = System.Drawing.Color.FromArgb(230, 230, 230),
            ButtonBackColor = System.Drawing.Color.FromArgb(70, 70, 70),
            ButtonForeColor = System.Drawing.Color.FromArgb(240, 240, 240),
            TextBoxBackColor = System.Drawing.Color.FromArgb(30, 30, 30),
            TextBoxForeColor = System.Drawing.Color.FromArgb(220, 220, 220),
            PanelBackColor = System.Drawing.Color.FromArgb(60, 60, 60),
            PanelForeColor = System.Drawing.Color.FromArgb(230, 230, 230),
            ButtonFlatStyle = FlatStyle.Flat
        };
    }
}
```

### IUIサービスインターフェース

```csharp name=IUIService.cs
using System;
using System.Windows.Forms;

namespace WinFormsAppFramework
{
    /// <summary>
    /// UI操作のサービスインターフェース
    /// </summary>
    public interface IUIService
    {
        /// <summary>
        /// メッセージボックスを表示
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="title">タイトル</param>
        /// <param name="buttons">ボタン</param>
        /// <param name="icon">アイコン</param>
        /// <returns>結果</returns>
        DialogResult ShowMessageBox(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon);
        
        /// <summary>
        /// ファイルを開くダイアログを表示
        /// </summary>
        /// <param name="title">ダイアログのタイトル</param>
        /// <param name="filter">ファイルフィルタ</param>
        /// <param name="defaultExtension">デフォルト拡張子</param>
        /// <returns>選択されたファイルパス、キャンセル時は空文字</returns>
        string ShowOpenFileDialog(string title, string filter, string defaultExtension);
        
        /// <summary>
        /// ファイルを保存ダイアログを表示
        /// </summary>
        /// <param name="title">ダイアログのタイトル</param>
        /// <param name="filter">ファイルフィルタ</param>
        /// <param name="defaultExtension">デフォルト拡張子</param>
        /// <returns>選択されたファイルパス、キャンセル時は空文字</returns>
        string ShowSaveFileDialog(string title, string filter, string defaultExtension);
        
        /// <summary>
        /// フォルダ選択ダイアログを表示
        /// </summary>
        /// <param name="title">ダイアログのタイトル</param>
        /// <returns>選択されたフォルダパス、キャンセル時は空文字</returns>
        string ShowFolderBrowserDialog(string title);
        
        /// <summary>
        /// 指定されたフォームをダイアログとして表示
        /// </summary>
        /// <typeparam name="TForm">フォームの型</typeparam>
        /// <param name="viewModel">ビューモデル（オプション）</param>
        /// <returns>ダイアログの結果</returns>
        DialogResult ShowDialog<TForm>(object viewModel = null) where TForm : Form;
        
        /// <summary>
        /// アクティブなフォームを設定
        /// </summary>
        /// <param name="activeForm">アクティブなフォーム</param>
        void SetActiveForm(Form activeForm);
        
        /// <summary>
        /// プログレスダイアログを表示
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="action">バックグラウンド処理</param>
        void ShowProgressDialog(string title, string message, Action<IProgressReporter> action);
        
        /// <summary>
        /// フォームのテーマを適用
        /// </summary>
        /// <param name="form">テーマを適用するフォーム</param>
        /// <param name="theme">テーマ情報</param>
        void ApplyTheme(Form form, ThemeInfo theme);
    }
}
```

### WinForms向けプログレスフォーム

```csharp name=ProgressForm.cs
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace WinFormsAppFramework
{
    public partial class ProgressForm : Form, IProgressReporter
    {
        private readonly BackgroundWorker _worker;
        private CancellationTokenSource _cancellationSource;
        private Action<IProgressReporter> _operation;

        public bool IsCancellationRequested => _cancellationSource.IsCancellationRequested;

        public ProgressForm(string title, string message)
        {
            InitializeComponent();
            
            Text = title;
            labelMessage.Text = message;
            
            // BackgroundWorkerの設定
            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            
            _worker.DoWork += Worker_DoWork;
            _worker.ProgressChanged += Worker_ProgressChanged;
            _worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            
            _cancellationSource = new CancellationTokenSource();
            
            // キャンセルボタン設定
            buttonCancel.Click += (s, e) => 
            {
                buttonCancel.Enabled = false;
                buttonCancel.Text = "キャンセル中...";
                _cancellationSource.Cancel();
                _worker.CancelAsync();
            };
        }

        /// <summary>
        /// 実行する操作を設定
        /// </summary>
        public void SetOperation(Action<IProgressReporter> operation)
        {
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        /// <summary>
        /// フォームがロードされた時に処理を開始
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            if (_operation != null)
            {
                progressBar.Value = 0;
                _worker.RunWorkerAsync();
            }
            else
            {
                Close();
            }
        }

        /// <summary>
        /// プログレスを報告
        /// </summary>
        public void ReportProgress(int percentComplete, string message = null)
        {
            if (_worker.IsBusy && !_worker.CancellationPending)
            {
                _worker.ReportProgress(percentComplete, message);
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _operation(this);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            
            if (e.UserState is string message && !string.IsNullOrEmpty(message))
            {
                labelMessage.Text = message;
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(
                    $"処理中にエラーが発生しました: {e.Error.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else if (e.Result is Exception resultEx)
            {
                MessageBox.Show(
                    $"処理中にエラーが発生しました: {resultEx.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _worker.DoWork -= Worker_DoWork;
                _worker.ProgressChanged -= Worker_ProgressChanged;
                _worker.RunWorkerCompleted -= Worker_RunWorkerCompleted;
                _cancellationSource?.Dispose();
            }
            
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelMessage = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 39);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(360, 23);
            this.progressBar.TabIndex = 0;
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.Location = new System.Drawing.Point(12, 13);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(87, 15);
            this.labelMessage.TabIndex = 1;
            this.labelMessage.Text = "処理中です...";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(297, 77);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "キャンセル";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // ProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 112);
            this.ControlBox = false;
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressForm";
            this.Text = "処理中";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Button buttonCancel;
    }
}
```

### WinForms用コマンドインフラストラクチャ

```csharp name=CommandManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WinFormsAppFramework
{
    /// <summary>
    /// コマンドの処理を統括するクラス
    /// </summary>
    public class CommandManager
    {
        private readonly Dictionary<string, CommandInfo> _commands = new Dictionary<string, CommandInfo>();
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CommandManager> _logger;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CommandManager(IServiceProvider serviceProvider, ILogger<CommandManager> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // コマンドハンドラの登録
            RegisterCommandHandlers();
        }

        /// <summary>
        /// サービスプロバイダーから全てのコマンドハンドラを登録
        /// </summary>
        private void RegisterCommandHandlers()
        {
            var handlers = _serviceProvider.GetServices<ICommandHandler>();
            
            foreach (var handler in handlers)
            {
                foreach (var command in handler.GetCommands())
                {
                    RegisterCommand(command.CommandId, command.CommandName, command.Execute, command.CanExecute);
                }
            }
            
            _logger.LogInformation($"Registered {_commands.Count} commands");
        }

        /// <summary>
        /// コマンドを登録
        /// </summary>
        public void RegisterCommand(string commandId, string name, Action<object> execute, Func<object, bool> canExecute = null)
        {
            if (string.IsNullOrEmpty(commandId))
                throw new ArgumentException("Command ID cannot be empty", nameof(commandId));
                
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));
                
            _commands[commandId] = new CommandInfo
            {
                CommandId = commandId,
                Name = name ?? commandId,
                Execute = execute,
                CanExecute = canExecute ?? (_ => true)
            };
            
            // コマンド状態変更通知
            CommandStateChanged?.Invoke(this, new CommandEventArgs(commandId));
        }

        /// <summary>
        /// コマンドを実行
        /// </summary>
        public bool ExecuteCommand(string commandId, object parameter = null)
        {
            if (!_commands.TryGetValue(commandId, out var command))
            {
                _logger.LogWarning($"Command not found: {commandId}");
                return false;
            }
            
            if (!command.CanExecute(parameter))
            {
                _logger.LogInformation($"Command cannot be executed: {commandId}");
                return false;
            }
            
            try
            {
                command.Execute(parameter);
                _logger.LogInformation($"Command executed: {commandId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing command: {commandId}");
                return false;
            }
        }

        /// <summary>
        /// コマンドが実行可能かどうかを判定
        /// </summary>
        public bool CanExecuteCommand(string commandId, object parameter = null)
        {
            if (!_commands.TryGetValue(commandId, out var command))
            {
                return false;
            }
            
            return command.CanExecute(parameter);
        }

        /// <summary>
        /// コマンドの状態を更新（UI要素の有効/無効を更新するために呼び出す）
        /// </summary>
        public void UpdateCommandStates()
        {
            foreach (var commandId in _commands.Keys.ToList())
            {
                CommandStateChanged?.Invoke(this, new CommandEventArgs(commandId));
            }
        }

        /// <summary>
        /// コマンドの状態が変更された時に発生するイベント
        /// </summary>
        public event EventHandler<CommandEventArgs> CommandStateChanged;
    }

    /// <summary>
    /// コマンド情報
    /// </summary>
    public class CommandInfo
    {
        public string CommandId { get; set; }
        public string Name { get; set; }
        public Action<object> Execute { get; set; }
        public Func<object, bool> CanExecute { get; set; }
    }

    /// <summary>
    /// コマンドイベント引数
    /// </summary>
    public class CommandEventArgs : EventArgs
    {
        public string CommandId { get; }

        public CommandEventArgs(string commandId)
        {
            CommandId = commandId;
        }
    }
}
```

### ビューモデルとフォームの連携インターフェース

```csharp name=IViewModelHost.cs
namespace WinFormsAppFramework
{
    /// <summary>
    /// ビューモデルをホストするフォームが実装するインターフェース
    /// </summary>
    public interface IViewModelHost
    {
        /// <summary>
        /// ビューモデルを設定
        /// </summary>
        void SetViewModel(object viewModel);
        
        /// <summary>
        /// 現在のビューモデルを取得
        /// </summary>
        object GetViewModel();
    }
}
```

### メインフォーム実装

```csharp name=MainForm.cs
using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace WinFormsAppFramework
{
    public partial class MainForm : Form, IViewModelHost
    {
        private readonly AppContext _appContext;
        private readonly IUIService _uiService;
        private readonly CommandManager _commandManager;
        private readonly ILogger<MainForm> _logger;
        private MainViewModel _viewModel;

        // トラッキングのための変数
        private bool _updatingUI = false;

        public MainForm(
            AppContext appContext,
            IUIService uiService,
            CommandManager commandManager,
            ILogger<MainForm> logger)
        {
            _appContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
            _uiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            _commandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            InitializeComponent();
            
            // UIサービスにアクティブフォームとして登録
            _uiService.SetActiveForm(this);
            
            // イベントハンドラの設定
            _appContext.ApplicationStateChanged += AppContext_ApplicationStateChanged;
            _appContext.ThemeChanged += AppContext_ThemeChanged;
            _commandManager.CommandStateChanged += CommandManager_CommandStateChanged;
            
            // フォームクローズ時の処理
            FormClosing += MainForm_FormClosing;
            
            // ビューモデルの作成と設定
            _viewModel = new MainViewModel(_appContext);
        }

        /// <summary>
        /// フォーム読み込み時の初期化
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            try
            {
                // テーマの適用
                _uiService.ApplyTheme(this, _appContext.CurrentTheme);
                
                // メニューとツールバーの初期設定
                SetupMenuCommands();
                
                // ウィンドウサイズと状態の復元
                RestoreWindowState();
                
                // 初期状態の更新
                UpdateUI();
                
                // 起動時に新規プロジェクト作成
                _appContext.NewProject();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing main form");
                MessageBox.Show($"初期化中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// ウィンドウの状態を復元
        /// </summary>
        private void RestoreWindowState()
        {
            var settings = _appContext.AppSettings;
            
            Width = (int)settings.WindowWidth;
            Height = (int)settings.WindowHeight;
            
            if (settings.StartMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        /// <summary>
        /// ウィンドウの状態を保存
        /// </summary>
        private void SaveWindowState()
        {
            var settings = _appContext.AppSettings;
            
            if (WindowState == FormWindowState.Normal)
            {
                settings.WindowWidth = Width;
                settings.WindowHeight = Height;
            }
            
            settings.StartMaximized = (WindowState == FormWindowState.Maximized);
        }

        /// <summary>
        /// フォームが閉じる前の処理
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // アプリケーション終了前の処理
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = !_appContext.OnApplicationClosing();
                
                if (!e.Cancel)
                {
                    // ウィンドウの状態を保存
                    SaveWindowState();
                }
            }
        }

        /// <summary>
        /// アプリケーション状態変更時の処理
        /// </summary>
        private void AppContext_ApplicationStateChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateUI));
            }
            else
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// テーマ変更時の処理
        /// </summary>
        private void AppContext_ThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<ThemeInfo>(_uiService.ApplyTheme), this, e.Theme);
            }
            else
            {
                _uiService.ApplyTheme(this, e.Theme);
            }
        }

        /// <summary>
        /// コマンド状態変更時の処理
        /// </summary>
        private void CommandManager_CommandStateChanged(object sender, CommandEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateCommandUI));
            }
            else
            {
                UpdateCommandUI();
            }
        }

        /// <summary>
        /// UIの状態を更新
        /// </summary>
        private void UpdateUI()
        {
            if (_updatingUI) return;
            
            _updatingUI = true;
            try
            {
                // タイトルの更新
                Text = _appContext.ApplicationTitle;
                
                // ステータスバーの更新
                UpdateStatusBar();
                
                // コマンド状態の更新
                UpdateCommandUI();
                
                // プロジェクト情報の表示
                UpdateProjectDisplay();
            }
            finally
            {
                _updatingUI = false;
            }
        }

        /// <summary>
        /// コマンドUIの更新
        /// </summary>
        private void UpdateCommandUI()
        {
            // メニュー項目の有効/無効を更新
            foreach (ToolStripItem item in menuStrip.Items)
            {
                UpdateMenuItemState(item);
            }
            
            // ツールバーボタンの有効/無効を更新
            foreach (ToolStripItem item in toolStrip.Items)
            {
                UpdateToolbarItemState(item);
            }
        }

        /// <summary>
        /// メニュー項目の状態を更新
        /// </summary>
        private void UpdateMenuItemState(ToolStripItem item)
        {
            if (item is ToolStripMenuItem menuItem)
            {
                // コマンドIDがタグとして設定されている場合
                if (menuItem.Tag is string commandId)
                {
                    menuItem.Enabled = _commandManager.CanExecuteCommand(commandId);
                }
                
                // サブメニューの処理
                foreach (ToolStripItem subItem in menuItem.DropDownItems)
                {
                    UpdateMenuItemState(subItem);
                }
            }
        }

        /// <summary>
        /// ツールバー項目の状態を更新
        /// </summary>
        private void UpdateToolbarItemState(ToolStripItem item)
        {
            if (item.Tag is string commandId)
            {
                item.Enabled = _commandManager.CanExecuteCommand(commandId);
            }
        }

        /// <summary>
        /// ステータスバーの更新
        /// </summary>
        private void UpdateStatusBar()
        {
            var project = _appContext.CurrentProjectContext?.Project;
            if (project != null)
            {
                toolStripStatusProject.Text = $"プロジェクト: {project.Name}";
                toolStripStatusModified.Text = project.IsDirty ? "変更あり" : "保存済み";
            }
            else
            {
                toolStripStatusProject.Text = "プロジェクト: なし";
                toolStripStatusModified.Text = "";
            }
            
            // 現在の日時を表示
            toolStripStatusDateTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            
            // 現在のユーザー名を表示
            toolStripStatusUser.Text = $"ユーザー: {Environment.UserName}";
        }

        /// <summary>
        /// プロジェクト情報の表示を更新
        /// </summary>
        private void UpdateProjectDisplay()
        {
            // プロジェクト内容を表示するコントロールを更新
            var project = _appContext.CurrentProjectContext?.Project;
            if (project != null)
            {
                // プロジェクトの内容に応じた表示更新
                textBoxContent.Text = project.Content?.ToString() ?? "";
            }
            else
            {
                textBoxContent.Text = "";
            }
        }

        /// <summary>
        /// メニューのセットアップ
        /// </summary>
        private void SetupMenuCommands()
        {
            // ファイルメニュー
            SetupMenuItem(newToolStripMenuItem, "FileNew", NewToolStripMenuItem_Click);
            SetupMenuItem(openToolStripMenuItem, "FileOpen", OpenToolStripMenuItem_Click);
            SetupMenuItem(saveToolStripMenuItem, "FileSave", SaveToolStripMenuItem_Click);
            SetupMenuItem(saveAsToolStripMenuItem, "FileSaveAs", SaveAsToolStripMenuItem_Click);
            SetupMenuItem(exitToolStripMenuItem, "FileExit", ExitToolStripMenuItem_Click);
            
            // 編集メニュー
            SetupMenuItem(undoToolStripMenuItem, "EditUndo", UndoToolStripMenuItem_Click);
            SetupMenuItem(redoToolStripMenuItem, "EditRedo", RedoToolStripMenuItem_Click);
            SetupMenuItem(cutToolStripMenuItem, "EditCut", CutToolStripMenuItem_Click);
            SetupMenuItem(copyToolStripMenuItem, "EditCopy", CopyToolStripMenuItem_Click);
            SetupMenuItem(pasteToolStripMenuItem, "EditPaste", PasteToolStripMenuItem_Click);
            
            // 表示メニュー
            SetupMenuItem(themesToolStripMenuItem, "ViewThemes", null);
            foreach (var theme in _appContext.AvailableThemes)
            {
                var themeMenuItem = new ToolStripMenuItem(theme.Name);
                themeMenuItem.Click += (s, e) => _appContext.ChangeTheme(theme.Name);
                themesToolStripMenuItem.DropDownItems.Add(themeMenuItem);
            }
            
            // ツールメニュー
            SetupMenuItem(optionsToolStripMenuItem, "ToolsOptions", OptionsToolStripMenuItem_Click);
            
            // ヘルプメニュー
            SetupMenuItem(aboutToolStripMenuItem, "HelpAbout", AboutToolStripMenuItem_Click);
            
            // ツールバー
            SetupToolbarButton(newToolStripButton, "FileNew");
            SetupToolbarButton(openToolStripButton, "FileOpen");
            SetupToolbarButton(saveToolStripButton, "FileSave");
            
            // タイマーでステータスバーの更新
            var timer = new Timer
            {
                Interval = 1000
            };
            timer.Tick += (s, e) => 
            {
                toolStripStatusDateTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            };
            timer.Start();
        }

        /// <summary>
        /// メニュー項目のセットアップ
        /// </summary>
        private void SetupMenuItem(ToolStripMenuItem menuItem, string commandId, EventHandler clickHandler)
        {
            menuItem.Tag = commandId;
            
            if (clickHandler != null)
            {
                menuItem.Click += clickHandler;
            }
            else
            {
                menuItem.Click += (s, e) => 
                {
                    if (s is ToolStripMenuItem item && item.Tag is string cmd)
                    {
                        _commandManager.ExecuteCommand(cmd);
                    }
                };
            }
        }

        /// <summary>
        /// ツールバーボタンのセットアップ
        /// </summary>
        private void SetupToolbarButton(ToolStripButton button, string commandId)
        {
            button.Tag = commandId;
            button.Click += (s, e) => 
            {
                if (s is ToolStripButton btn && btn.Tag is string cmd)
                {
                    _commandManager.ExecuteCommand(cmd);
                }
            };
        }

        #region メニューイベントハンドラ

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _appContext.NewProject();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filePath = _uiService.ShowOpenFileDialog(
                "プロジェクトを開く",
                _appContext.AppSettings.ProjectFileFilter,
                _appContext.AppSettings.DefaultProjectExtension);
                
            if (!string.IsNullOrEmpty(filePath))
            {
                _appContext.OpenProject(filePath);
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _appContext.SaveProject();
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _appContext.SaveProjectAs(null);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveControl is TextBox textBox)
            {
                textBox.Undo();
            }
        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // WinFormsの標準TextBoxにはRedoがないため、カスタム実装が必要
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveControl is TextBox textBox)
            {
                textBox.Cut();
            }
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveControl is TextBox textBox)
            {
                textBox.Copy();
            }
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveControl is TextBox textBox)
            {
                textBox.Paste();
            }
        }

        private void OptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _uiService.ShowDialog<SettingsForm>();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                $"{_appContext.AppSettings.ApplicationName}\nVersion 1.0\n\n© 2025 Your Company",
                "このアプリケーションについて",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        #endregion

        /// <summary>
        /// テキスト変更時の処理
        /// </summary>
        private void textBoxContent_TextChanged(object sender, EventArgs e)
        {
            if (_updatingUI) return;
            
            if (_appContext.CurrentProjectContext != null)
            {
                _appContext.CurrentProjectContext.Project.Content = textBoxContent.Text;
            }
        }

        // IViewModelHost インターフェース実装
        public void SetViewModel(object viewModel)
        {
            if (viewModel is MainViewModel mainViewModel)
            {
                _viewModel = mainViewModel;
            }
        }

        public object GetViewModel()
        {
            return _viewModel;
        }

        // デザイナーコード (簡略化)
        private void InitializeComponent()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolSt
```

---



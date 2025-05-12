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
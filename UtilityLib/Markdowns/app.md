# C# �v���W�F�N�g�Ǘ��V�X�e���̐݌v

���₢���킹�����������A�v���P�[�V�����̐݌v�ɂ��āA�e�N���X�̖����ƃN���X�݌v���Ă��܂��B

## �e�N���X�̖���

1. **Project** - �v���W�F�N�g�f�[�^���f��
   - �v���W�F�N�g�̎��ۂ̃f�[�^�Ə�Ԃ�ێ�
   - �t�@�C���p�X�A���O�A���e�Ȃǂ̊�{�����Ǘ�

2. **ProjectContext** - �v���W�F�N�g��ԊǗ�
   - �P��v���W�F�N�g�̕ҏW��Ԃ�ǐ�
   - �ύX�ʒm���J�j�Y�����
   - ���ۑ��̕ύX�����邩�ǂ������Ǘ�

3. **ProjectService** - �v���W�F�N�g����T�[�r�X
   - �t�@�C������i�ۑ��A�ǂݍ��݁j��S��
   - Save�ASave As�AOpen�̎������
   - �v���W�F�N�g�̉i�����ƃV���A���C�Y�������s��

4. **AppContext** - �A�v���P�[�V�����S�̂̃R���e�L�X�g
   - ���݃A�N�e�B�u��ProjectContext���Ǘ�
   - �A�v���P�[�V�����S�̂̏�Ԃ�ێ�
   - �T�[�r�X�̓o�^�Ǝ擾�̎d�g�݂��

## �N���X�݌v

�ȉ��ɂ��ꂼ��̃N���X�̏ڍא݌v���Ă��܂��F

```csharp name=Project.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjectManagement
{
    /// <summary>
    /// �v���W�F�N�g�̃f�[�^�Ə�Ԃ�\�����f���N���X
    /// </summary>
    public class Project : INotifyPropertyChanged
    {
        private string _name;
        private string _filePath;
        private object _content; // ���ۂ̃A�v���P�[�V�����ɉ����Č^��ύX
        private bool _isDirty;

        /// <summary>
        /// �v���W�F�N�g��
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// �v���W�F�N�g�̃t�@�C���p�X�i���ۑ��̏ꍇ��null�j
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        /// <summary>
        /// �v���W�F�N�g�̓��e
        /// </summary>
        public object Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        /// <summary>
        /// ���ۑ��̕ύX�����邩�ǂ���
        /// </summary>
        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        /// <summary>
        /// �V�K�v���W�F�N�g���쐬
        /// </summary>
        /// <param name="name">�v���W�F�N�g��</param>
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
    /// �v���W�F�N�g�̏�Ԃ��Ǘ�����R���e�L�X�g�N���X
    /// </summary>
    public class ProjectContext : IDisposable
    {
        private Project _project;
        private bool _isModified = false;

        /// <summary>
        /// ���݂̃v���W�F�N�g
        /// </summary>
        public Project Project
        {
            get => _project;
            private set
            {
                if (_project != null)
                {
                    // �Â��v���W�F�N�g�̃C�x���g�w�ǂ�����
                    _project.PropertyChanged -= Project_PropertyChanged;
                }

                _project = value;

                if (_project != null)
                {
                    // �V�����v���W�F�N�g�̃C�x���g�w��
                    _project.PropertyChanged += Project_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// �v���W�F�N�g���ύX���ꂽ���ǂ���
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
        /// �v���W�F�N�g�̕ύX��Ԃ��ς�������ɔ�������C�x���g
        /// </summary>
        public event EventHandler ModifiedChanged;

        /// <summary>
        /// �V�����v���W�F�N�g�ŃR���e�L�X�g��������
        /// </summary>
        /// <param name="project">�v���W�F�N�g</param>
        public ProjectContext(Project project)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
        }

        /// <summary>
        /// �v���W�F�N�g�̃v���p�e�B���ύX���ꂽ���̏���
        /// </summary>
        private void Project_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Project.Content))
            {
                // �R���e���c���ύX���ꂽ��v���W�F�N�g��ύX��Ԃɂ���
                MarkAsModified();
            }
        }

        /// <summary>
        /// �v���W�F�N�g��ύX��ԂƂ��ă}�[�N
        /// </summary>
        public void MarkAsModified()
        {
            IsModified = true;
            Project.IsDirty = true;
        }

        /// <summary>
        /// �v���W�F�N�g�𖢕ύX��ԂƂ��ă}�[�N�i�ۑ���Ȃǁj
        /// </summary>
        public void MarkAsUnmodified()
        {
            IsModified = false;
            Project.IsDirty = false;
        }

        /// <summary>
        /// ���\�[�X�̉��
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
    /// �v���W�F�N�g�̃t�@�C�������S������T�[�r�X�N���X
    /// </summary>
    public class ProjectService
    {
        private readonly AppContext _appContext;

        public ProjectService(AppContext appContext)
        {
            _appContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
        }

        /// <summary>
        /// �V�K�v���W�F�N�g���쐬
        /// </summary>
        /// <param name="name">�v���W�F�N�g��</param>
        /// <returns>�쐬���ꂽ�v���W�F�N�g�R���e�L�X�g</returns>
        public ProjectContext CreateNew(string name = "New Project")
        {
            var project = new Project(name);
            return new ProjectContext(project);
        }

        /// <summary>
        /// �v���W�F�N�g���J��
        /// </summary>
        /// <param name="filePath">�t�@�C���p�X</param>
        /// <returns>�J�����v���W�F�N�g�R���e�L�X�g</returns>
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
        /// �v���W�F�N�g��ۑ�
        /// </summary>
        /// <param name="context">�ۑ�����v���W�F�N�g�R���e�L�X�g</param>
        /// <returns>�ۑ��^�X�N</returns>
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
        /// �v���W�F�N�g��ʖ��ŕۑ�
        /// </summary>
        /// <param name="context">�ۑ�����v���W�F�N�g�R���e�L�X�g</param>
        /// <param name="filePath">�ۑ���t�@�C���p�X</param>
        /// <returns>�ۑ��^�X�N</returns>
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
        /// �v���W�F�N�g���w�肳�ꂽ�t�@�C���ɕۑ�
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
        /// ���݂̃v���W�F�N�g�ɖ��ۑ��̕ύX������ꍇ�A�ۑ����邩�ǂ����m�F
        /// </summary>
        /// <returns>���s�\���itrue=���s�\�Afalse=����L�����Z���j</returns>
        public async Task<bool> ConfirmSaveIfModifiedAsync()
        {
            var currentContext = _appContext.CurrentProjectContext;
            if (currentContext == null || !currentContext.IsModified)
                return true;

            // ���ۂ̃A�v���P�[�V�����ł�UI����m�F�_�C�A���O��\������
            // �����ł̓R���\�[���A�v���P�[�V������z�肵���ȈՎ���

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
                            // ���ۂ̃A�v���P�[�V�����ł̓t�@�C���I���_�C�A���O��\��
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
    /// �A�v���P�[�V�����S�̂̃R���e�L�X�g���Ǘ�����N���X
    /// </summary>
    public class AppContext
    {
        private ProjectContext _currentProjectContext;
        private readonly ProjectService _projectService;

        /// <summary>
        /// ���݃A�N�e�B�u�ȃv���W�F�N�g�R���e�L�X�g
        /// </summary>
        public ProjectContext CurrentProjectContext
        {
            get => _currentProjectContext;
            private set
            {
                if (_currentProjectContext != null)
                {
                    // �Â��R���e�L�X�g�̃C�x���g�w�ǂ�����
                    _currentProjectContext.ModifiedChanged -= ProjectContext_ModifiedChanged;
                    _currentProjectContext.Dispose();
                }

                _currentProjectContext = value;

                if (_currentProjectContext != null)
                {
                    // �V�����R���e�L�X�g�̃C�x���g�w��
                    _currentProjectContext.ModifiedChanged += ProjectContext_ModifiedChanged;
                }

                // �^�C�g���o�[�ȂǁA�A�v���P�[�V�����̏�ԍX�V
                UpdateApplicationState();
            }
        }

        /// <summary>
        /// �A�v���P�[�V�����^�C�g��
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
        /// �A�v���P�[�V������Ԃ��ύX���ꂽ���ɔ�������C�x���g
        /// </summary>
        public event EventHandler ApplicationStateChanged;

        /// <summary>
        /// AppContext�̃R���X�g���N�^
        /// </summary>
        public AppContext()
        {
            _projectService = new ProjectService(this);
        }

        /// <summary>
        /// �A�v���P�[�V�����̏�Ԃ��X�V
        /// </summary>
        private void UpdateApplicationState()
        {
            ApplicationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// �v���W�F�N�g�̕ύX��Ԃ��ς�������̏���
        /// </summary>
        private void ProjectContext_ModifiedChanged(object sender, EventArgs e)
        {
            UpdateApplicationState();
        }

        #region Menu Commands

        /// <summary>
        /// �V�K�v���W�F�N�g���쐬
        /// </summary>
        /// <returns>���쐬�����ǂ���</returns>
        public async Task<bool> NewProjectAsync()
        {
            if (!await _projectService.ConfirmSaveIfModifiedAsync())
                return false;

            CurrentProjectContext = _projectService.CreateNew();
            return true;
        }

        /// <summary>
        /// �v���W�F�N�g���J��
        /// </summary>
        /// <param name="filePath">�t�@�C���p�X</param>
        /// <returns>���쐬�����ǂ���</returns>
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
        /// ���݂̃v���W�F�N�g��ۑ�
        /// </summary>
        /// <returns>���쐬�����ǂ���</returns>
        public async Task<bool> SaveProjectAsync()
        {
            if (CurrentProjectContext == null)
                return false;

            try
            {
                if (string.IsNullOrEmpty(CurrentProjectContext.Project.FilePath))
                {
                    // �p�X���Ȃ��ꍇ��SaveAs�ɓ]��
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
        /// ���݂̃v���W�F�N�g��ʖ��ŕۑ�
        /// </summary>
        /// <param name="filePath">�t�@�C���p�X�inull�̏ꍇ�̓_�C�A���O��\���j</param>
        /// <returns>���쐬�����ǂ���</returns>
        public async Task<bool> SaveProjectAsAsync(string filePath)
        {
            if (CurrentProjectContext == null)
                return false;

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    // ���ۂ̃A�v���P�[�V�����ł̓t�@�C���I���_�C�A���O��\��
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

            // �����v���W�F�N�g�쐬
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
                        // �I���O�ɖ��ۑ��̕ύX���m�F
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

## �d�g�݂̐���

1. **�ҏW��Ԃ̃��j�^�����O**:
   - `Project`�N���X��`INotifyPropertyChanged`���������A�v���p�e�B�ύX�ʒm���
   - `ProjectContext`��`Project`�̕ύX���Ď����A`IsModified`�t���O���Ǘ�
   - `AppContext`�̓A�v���P�[�V�����^�C�g���ɕύX���(*)��\��

2. **Save�@�\**:
   - �ۑ���p�X�����ɂ���ꍇ��`ProjectService.SaveAsync`�œ����ꏊ�ɕۑ�
   - �ۑ���Ԃ����Z�b�g����`IsModified = false`�ɐݒ�

3. **Save As�@�\**:
   - �V�����p�X���w�肵��`ProjectService.SaveAsAsync`�ŕۑ�
   - �V�����t�@�C���p�X�Ɩ��O��ݒ肵�A�ۑ���Ԃ����Z�b�g

4. **Open�@�\**:
   - �����v���W�F�N�g�ɖ��ۑ��̕ύX������ꍇ�A�ۑ��m�F���s��
   - �t�@�C������v���W�F�N�g���f�V���A���C�Y���ĐV����`ProjectContext`���쐬

5. **�N���X�Ԃ̘A�g**:
   - `AppContext`���A�v���P�[�V�����S�̂̏�Ԃ��Ǘ�
   - `ProjectService`���t�@�C�������S��
   - `ProjectContext`���P��v���W�F�N�g�̕ҏW��Ԃ��Ǘ�
   - `Project`���f�[�^���f���Ƃ��ċ@�\

���̐݌v�p�^�[���́A�N���[���A�[�L�e�N�`���̌����ɏ]���Ă���A�֐S�̕������K�؂ɍs���Ă��܂��B�܂��AMVVM�p�^�[���Ƃ̓������e�Ղł��B


---

# �ėp�I�ȃA�v���P�[�V�����x�[�X�ւ̊g��

`2025-05-12 12:49:09 UTC` �ɂ��˗������������A��Ă����N���X�\����ėp�I�ȃA�v���̃x�[�X�Ƃ��Ċg��������@������Ă��܂��B

## �g���̊�{���j

�����̍\���iAppContext�AProject�AProjectContext�AProjectService�j����ՂƂ��āA�ȉ��̊ϓ_����g�����܂��F

1. **�ˑ��������iDI�j�̓���**
2. **�v���O�C���V�X�e��**
3. **�}���`�h�L�������g�Ή�**
4. **�R�}���h�p�^�[���̎���**
5. **�e�[�}�ƍ��ۉ�**
6. **���M���O�ƃG���[����**

## �g�������N���X�݌v

### 1. �C���^�[�t�F�[�X��DI�̓���

```csharp name=Interfaces/IProject.cs
using System.ComponentModel;

namespace AppFramework.Interfaces
{
    /// <summary>
    /// �v���W�F�N�g�f�[�^���f���̃C���^�[�t�F�[�X
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
    /// �v���W�F�N�g�R���e�L�X�g�̃C���^�[�t�F�[�X
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
    /// �v���W�F�N�g����T�[�r�X�̃C���^�[�t�F�[�X
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

### 2. �ˑ��������R���e�i

```csharp name=Services/DependencyContainer.cs
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AppFramework.Interfaces;

namespace AppFramework.Services
{
    /// <summary>
    /// �ˑ��������R���e�i���Ǘ�����N���X
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
        /// �f�t�H���g�̃T�[�r�X�ݒ�
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
        {
            // �R�A�T�[�r�X�̓o�^
            services.AddSingleton<IAppContext, AppContext>();
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<ICommandManager, CommandManager>();
            services.AddSingleton<IPluginManager, PluginManager>();
            
            // �v���W�F�N�g�֘A�̃T�[�r�X�̓v���O�C�������邱�Ƃ��\
            services.AddTransient<IProjectService, ProjectService>();
        }

        /// <summary>
        /// �T�[�r�X�̒ǉ��o�^
        /// </summary>
        public void RegisterService<TService, TImplementation>() 
            where TImplementation : class, TService 
            where TService : class
        {
            _services.AddTransient<TService, TImplementation>();
        }

        /// <summary>
        /// �T�[�r�X�v���o�C�_�[�̃r���h
        /// </summary>
        public void BuildServiceProvider()
        {
            _serviceProvider = _services.BuildServiceProvider();
        }

        /// <summary>
        /// �T�[�r�X�̎擾
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

### 3. �g������AppContext

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
    /// �A�v���P�[�V�����S�̂̃R���e�L�X�g���Ǘ�����g���N���X
    /// </summary>
    public class AppContext : IAppContext
    {
        private readonly DependencyContainer _container;
        private readonly ILogService _logService;
        private readonly ISettingsService _settingsService;
        
        // �}���`�h�L�������g�Ή��̂��߂̃v���W�F�N�g�R���N�V����
        private ObservableCollection<IProjectContext> _projectContexts = new ObservableCollection<IProjectContext>();
        private IProjectContext _activeProjectContext;

        /// <summary>
        /// �J���Ă��邷�ׂẴv���W�F�N�g�R���e�L�X�g
        /// </summary>
        public ReadOnlyObservableCollection<IProjectContext> ProjectContexts { get; }

        /// <summary>
        /// ���݃A�N�e�B�u�ȃv���W�F�N�g�R���e�L�X�g
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
        /// �A�v���P�[�V�����^�C�g��
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
        /// �A�v���P�[�V������Ԃ��ύX���ꂽ���ɔ�������C�x���g
        /// </summary>
        public event EventHandler ApplicationStateChanged;
        
        /// <summary>
        /// �A�N�e�B�u�ȃv���W�F�N�g���ύX���ꂽ���ɔ�������C�x���g
        /// </summary>
        public event EventHandler ActiveProjectChanged;

        /// <summary>
        /// AppContext�̃R���X�g���N�^
        /// </summary>
        public AppContext(DependencyContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logService = _container.GetService<ILogService>();
            _settingsService = _container.GetService<ISettingsService>();
            
            ProjectContexts = new ReadOnlyObservableCollection<IProjectContext>(_projectContexts);
            
            // �v���O�C���̓ǂݍ���
            var pluginManager = _container.GetService<IPluginManager>();
            pluginManager.LoadPlugins();
            
            _logService.Log(LogLevel.Info, "Application initialized");
        }

        /// <summary>
        /// �A�v���P�[�V�����̏�Ԃ��X�V
        /// </summary>
        private void UpdateApplicationState()
        {
            ApplicationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// �v���W�F�N�g�̕ύX��Ԃ��ς�������̏���
        /// </summary>
        private void ProjectContext_ModifiedChanged(object sender, EventArgs e)
        {
            UpdateApplicationState();
        }

        #region �v���W�F�N�g�Ǘ�

        /// <summary>
        /// �V�K�v���W�F�N�g���쐬
        /// </summary>
        public async Task<IProjectContext> NewProjectAsync(string projectType = null)
        {
            try
            {
                // �v���W�F�N�g�^�C�v�ɑΉ�����T�[�r�X�̎擾
                IProjectService projectService = GetProjectService(projectType);
                
                // �V�K�v���W�F�N�g�쐬
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
        /// �v���W�F�N�g���J��
        /// </summary>
        public async Task<IProjectContext> OpenProjectAsync(string filePath)
        {
            try
            {
                // �t�@�C���g���q����v���W�F�N�g�^�C�v�𔻒f
                string extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
                
                // �Ή�����T�[�r�X������
                IProjectService projectService = FindProjectServiceForExtension(extension);
                if (projectService == null)
                {
                    throw new NotSupportedException($"Unsupported file extension: {extension}");
                }
                
                // �v���W�F�N�g���J��
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
        /// �v���W�F�N�g�����
        /// </summary>
        public async Task<bool> CloseProjectAsync(IProjectContext context)
        {
            if (context == null)
                return false;

            try
            {
                // ���ۑ��̕ύX������ꍇ�A�ۑ����邩�m�F
                if (context.IsModified)
                {
                    // �v���W�F�N�g�^�C�v�ɑΉ�����T�[�r�X���擾
                    IProjectService projectService = GetProjectService(context.Project.ProjectType);
                    
                    bool canClose = await projectService.ConfirmSaveIfModifiedAsync(context);
                    if (!canClose)
                        return false;
                }
                
                // �v���W�F�N�g�R���e�L�X�g���폜
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
        /// ���݂̃v���W�F�N�g��ۑ�
        /// </summary>
        public async Task<bool> SaveProjectAsync(IProjectContext context = null)
        {
            context = context ?? ActiveProjectContext;
            if (context == null)
                return false;

            try
            {
                // �v���W�F�N�g�^�C�v�ɑΉ�����T�[�r�X���擾
                IProjectService projectService = GetProjectService(context.Project.ProjectType);
                
                if (string.IsNullOrEmpty(context.Project.FilePath))
                {
                    // �p�X���Ȃ��ꍇ��SaveAs�ɓ]��
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
        /// ���݂̃v���W�F�N�g��ʖ��ŕۑ�
        /// </summary>
        public async Task<bool> SaveProjectAsAsync(IProjectContext context = null, string filePath = null)
        {
            context = context ?? ActiveProjectContext;
            if (context == null)
                return false;

            try
            {
                // �v���W�F�N�g�^�C�v�ɑΉ�����T�[�r�X���擾
                IProjectService projectService = GetProjectService(context.Project.ProjectType);
                
                if (string.IsNullOrEmpty(filePath))
                {
                    // ���ۂ̃A�v���P�[�V�����ł̓t�@�C���I���_�C�A���O��\��
                    // ���̕����̓v���b�g�t�H�[���ˑ��̂��߁A�C���^�[�t�F�[�X����ČĂяo��
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
        /// �v���W�F�N�g�R���e�L�X�g���R���N�V�����ɒǉ�
        /// </summary>
        private void AddProjectContext(IProjectContext context)
        {
            _projectContexts.Add(context);
            ActiveProjectContext = context;
        }
        
        /// <summary>
        /// �v���W�F�N�g�R���e�L�X�g���R���N�V��������폜
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
        /// �v���W�F�N�g�^�C�v�ɑΉ�����T�[�r�X���擾
        /// </summary>
        private IProjectService GetProjectService(string projectType)
        {
            // �f�t�H���g�̃v���W�F�N�g�T�[�r�X���擾
            var defaultService = _container.GetService<IProjectService>();
            
            if (string.IsNullOrEmpty(projectType))
                return defaultService;
            
            // �v���O�C������v���W�F�N�g�^�C�v�ɑΉ�����T�[�r�X������
            var pluginManager = _container.GetService<IPluginManager>();
            var service = pluginManager.GetProjectService(projectType);
            
            return service ?? defaultService;
        }
        
        /// <summary>
        /// �t�@�C���g���q�ɑΉ�����v���W�F�N�g�T�[�r�X������
        /// </summary>
        private IProjectService FindProjectServiceForExtension(string extension)
        {
            // �f�t�H���g�̃T�[�r�X���m�F
            var defaultService = _container.GetService<IProjectService>();
            if (defaultService.SupportedFileExtensions.Contains(extension))
                return defaultService;
            
            // �v���O�C������Ή�����T�[�r�X������
            var pluginManager = _container.GetService<IPluginManager>();
            return pluginManager.GetProjectServiceForExtension(extension);
        }
        
        #endregion
    }
}
```

### 4. �R�}���h�p�^�[���̓���

```csharp name=Commands/CommandBase.cs
using System;
using System.Windows.Input;

namespace AppFramework.Commands
{
    /// <summary>
    /// �R�}���h�̊�{�N���X
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
    /// �ėp�I�ȃR�}���h����
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
    /// �A�v���P�[�V�����̃R�}���h���Ǘ�����N���X
    /// </summary>
    public class CommandManager : ICommandManager
    {
        private readonly Dictionary<string, CommandBase> _commands = new Dictionary<string, CommandBase>();
        private readonly Stack<IUndoableCommand> _undoStack = new Stack<IUndoableCommand>();
        private readonly Stack<IUndoableCommand> _redoStack = new Stack<IUndoableCommand>();
        
        public event EventHandler UndoRedoStateChanged;
        
        /// <summary>
        /// �R�}���h�̓o�^
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
        /// �R�}���h�̎擾
        /// </summary>
        public CommandBase GetCommand(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
                throw new ArgumentException("Command name cannot be empty", nameof(commandName));
                
            return _commands.TryGetValue(commandName, out var command) ? command : null;
        }
        
        /// <summary>
        /// Undo���\���ǂ���
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;
        
        /// <summary>
        /// Redo���\���ǂ���
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;
        
        /// <summary>
        /// Undo����̎��s
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
        /// Redo����̎��s
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
        /// Undo�\�ȃR�}���h�̎��s�ƋL�^
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

### 5. �v���O�C���V�X�e��

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
    /// �v���O�C�����Ǘ�����N���X
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
        /// �v���O�C���̃��[�h
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
        /// �v���O�C���t�@�C������v���O�C�������[�h
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
                    
                    // �v���W�F�N�g�T�[�r�X�̓o�^
                    RegisterPluginProjectServices(plugin);
                    
                    _logService.Log(LogLevel.Info, $"Loaded plugin: {plugin.Name} v{plugin.Version}");
                }
            }
        }
        
        /// <summary>
        /// �v���O�C���̃v���W�F�N�g�T�[�r�X��o�^
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
        /// �v���W�F�N�g�^�C�v�ɑΉ�����T�[�r�X���擾
        /// </summary>
        public IProjectService GetProjectService(string projectType)
        {
            return _projectServices.TryGetValue(projectType, out var service) ? service : null;
        }
        
        /// <summary>
        /// �t�@�C���g���q�ɑΉ�����v���W�F�N�g�T�[�r�X���擾
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
    /// �v���O�C���̃C���^�[�t�F�[�X
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

### 6. ���̑��̃T�[�r�X

```csharp name=Services/LogService.cs
using System;
using System.IO;
using AppFramework.Interfaces;

namespace AppFramework.Services
{
    /// <summary>
    /// ���O���x���̗�
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
    /// ���O�o�͂��Ǘ�����T�[�r�X
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
        /// ���O�̏o��
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
                    
                    // ����̃��x���ȏ���R���\�[���ɏo�́i�f�o�b�O�p�j
                    if (level >= LogLevel.Warning)
                    {
                        Console.WriteLine(logEntry);
                    }
                }
                catch
                {
                    // ���O�o�͒��̃G���[�͖����i�������[�v�h�~�j
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
    /// �A�v���P�[�V�����ݒ���Ǘ�����T�[�r�X
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
        /// �ݒ�̓ǂݍ���
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
        /// �ݒ�̕ۑ�
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
                    // �ݒ�ۑ��G���[�͖���
                }
            }
        }
        
        /// <summary>
        /// �ݒ�l�̎擾
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
        /// �ݒ�l�̕ۑ�
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
    /// �A�v���P�[�V�����̃e�[�}���Ǘ�����T�[�r�X
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
        /// ���݂̃e�[�}
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
        /// ���p�\�ȃe�[�}�ꗗ
        /// </summary>
        public IEnumerable<string> AvailableThemes => new[] { "Light", "Dark", "Blue", "High Contrast" };
        
        /// <summary>
        /// �e�[�}�ɑΉ����郊�\�[�X�f�B�N�V���i�����擾
        /// </summary>
        public Uri GetThemeResourceUri(string themeName)
        {
            string resourceName = string.IsNullOrEmpty(themeName) ? _currentTheme : themeName;
            return new Uri($"/Themes/{resourceName}Theme.xaml", UriKind.Relative);
        }
    }
}
```

### 7. ������F���f���g���ƃv���O�C��

```csharp name=Models/TextDocumentProject.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AppFramework.Interfaces;

namespace AppFramework.Models
{
    /// <summary>
    /// �e�L�X�g�h�L�������g�v���W�F�N�g���f��
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
    /// �e�L�X�g�h�L�������g�p�v���O�C��
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
            // �v���O�C���ŗL�̃T�[�r�X��o�^
            _textDocumentService = new TextDocumentService(container);
            container.RegisterService<ITextDocumentService, TextDocumentService>();
            
            // �R�}���h��o�^
            var commandManager = container.GetService<ICommandManager>();
            
            // UI�g���|�C���g�i���ۂ̃A�v���P�[�V�����ł͓����UI�t���[�����[�N�Ɉˑ��j
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
    /// �e�L�X�g�h�L�������g�p�T�[�r�X
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
        
        // �e�L�X�g���L�̋@�\
        public void FormatText(IProjectContext context, TextFormatOptions options)
        {
            if (context.Project is TextDocumentProject textProject)
            {
                string content = textProject.Content.ToString();
                
                // �t�H�[�}�b�g����
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

## �g���̎����|�C���g

1. **�ėp���̌���**
   - �C���^�[�t�F�[�X�ɂ��a�����Ȑ݌v
   - �ˑ��������ɂ��T�[�r�X�̊Ǘ�
   - �v���O�C���ɂ��@�\�g��

2. **�}���`�h�L�������g�Ή�**
   - �����v���W�F�N�g�̓����Ǘ�
   - �A�N�e�B�u�ȃv���W�F�N�g�̐؂�ւ�
   - �v���W�F�N�g�R���N�V�����̒�

3. **�R�}���h�p�^�[������**
   - ���j���[����𒊏ۉ�
   - Undo/Redo�@�\�̎���
   - �R�}���h�����̊Ǘ�

4. **�v���O�C���V�X�e��**
   - ���I�ȃv���O�C���̃��[�h
   - �g���|�C���g�̒�
   - �V�����v���W�F�N�g�^�C�v�̒ǉ�

5. **�e��T�[�r�X**
   - ���M���O�@�\
   - �ݒ�Ǘ�
   - �e�[�}�ƃX�^�C���ݒ�
   - �_�C�A���O�T�[�r�X

���̃A�[�L�e�N�`�����g�p����ƁA���܂��܂ȃ^�C�v�̃A�v���P�[�V�����ɓK�p�\�Ȕėp�I�ȃt���[�����[�N�ƂȂ�A�@�\�̊g�����e�ՂɂȂ�܂��B��{�\����ۂ��Ȃ���A����̃A�v���P�[�V�����h���C���ɍ��킹���J�X�^�}�C�Y���\�ł��B


---


# �ėp�I�ȃf�X�N�g�b�v�A�v���P�[�V�����t���[�����[�N�ւ̊g��

��قǒ�Ă����v���W�F�N�g�Ǘ��V�X�e�����A���ėp�I�ȃf�X�N�g�b�v�A�v���P�[�V�����̃x�[�X�Ƃ��Ċg�����܂��B�ȉ��̋@�\��ǉ����āA���S�Ŋg�����̍����t���[�����[�N���\�z���܂��傤�B

## �g���A�[�L�e�N�`���̊T�v

1. **MVVM �p�^�[���̊��S����**
2. **�ˑ��������iDI�j�ɂ��T�[�r�X�̊Ǘ�**
3. **UI �t���[�����[�N�iWPF�j�Ƃ̓���**
4. **�R�}���h�p�^�[���ɂ�郆�[�U�[�A�N�V��������**
5. **�ݒ�Ǘ��Ɖi����**
6. **�e�[�}�ƃX�^�C���̃T�|�[�g**
7. **���M���O�V�X�e��**
8. **�v���O�C���A�[�L�e�N�`��**

## ��v�N���X�̊g��

### AppContext �̊g���i�A�v���P�[�V�����R�A�j

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
    /// �A�v���P�[�V�����S�̂̃R���e�L�X�g���Ǘ�����g���N���X
    /// </summary>
    public class AppContext : IDisposable
    {
        private ProjectContext _currentProjectContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppContext> _logger;

        /// <summary>
        /// �ˑ��������R���e�i
        /// </summary>
        public IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// ���݃A�N�e�B�u�ȃv���W�F�N�g�R���e�L�X�g
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
        /// �A�v���P�[�V�����^�C�g��
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
        /// ���p�\�ȃe�[�}�̈ꗗ
        /// </summary>
        public IReadOnlyList<ThemeInfo> AvailableThemes { get; private set; }

        /// <summary>
        /// ���݂̃e�[�}
        /// </summary>
        public ThemeInfo CurrentTheme { get; private set; }

        /// <summary>
        /// �A�v���P�[�V�����ݒ�
        /// </summary>
        public AppSettings AppSettings { get; private set; }

        /// <summary>
        /// �A�v���P�[�V������Ԃ��ύX���ꂽ���ɔ�������C�x���g
        /// </summary>
        public event EventHandler ApplicationStateChanged;

        /// <summary>
        /// �e�[�}���ύX���ꂽ���ɔ�������C�x���g
        /// </summary>
        public event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        /// <summary>
        /// �A�v���P�[�V�������I�����鎞�ɔ�������C�x���g
        /// </summary>
        public event EventHandler<AppClosingEventArgs> ApplicationClosing;

        /// <summary>
        /// �v���O�C���}�l�[�W���[
        /// </summary>
        public PluginManager PluginManager { get; private set; }

        /// <summary>
        /// AppContext�̃R���X�g���N�^
        /// </summary>
        public AppContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = serviceProvider.GetRequiredService<ILogger<AppContext>>();
            
            // �ݒ�̃��[�h
            AppSettings = serviceProvider.GetRequiredService<AppSettings>();
            
            // ���p�\�ȃe�[�}�̃��[�h
            LoadThemes();
            
            // �v���O�C���}�l�[�W���[�̏�����
            PluginManager = new PluginManager(this);
            PluginManager.LoadPlugins();
            
            _logger.LogInformation("Application initialized");
        }

        /// <summary>
        /// ���p�\�ȃe�[�}�����[�h
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
            
            // �f�t�H���g�e�[�}�������͕ۑ����ꂽ�e�[�}�̐ݒ�
            string savedTheme = AppSettings.CurrentTheme;
            CurrentTheme = themes.Find(t => t.Name == savedTheme) ?? themes[0];
        }

        /// <summary>
        /// �e�[�}��ύX
        /// </summary>
        /// <param name="themeName">�e�[�}��</param>
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
        /// �A�v���P�[�V�����̏�Ԃ��X�V
        /// </summary>
        private void UpdateApplicationState()
        {
            ApplicationStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// �v���W�F�N�g�̕ύX��Ԃ��ς�������̏���
        /// </summary>
        private void ProjectContext_ModifiedChanged(object sender, EventArgs e)
        {
            UpdateApplicationState();
        }

        /// <summary>
        /// �A�v���P�[�V�����I���O�̏���
        /// </summary>
        /// <returns>�I�������������ꂽ���ǂ���</returns>
        public bool OnApplicationClosing()
        {
            var args = new AppClosingEventArgs();
            ApplicationClosing?.Invoke(this, args);
            
            if (args.Cancel)
                return false;
                
            // ���ۑ��̕ύX���m�F
            if (CurrentProjectContext != null && CurrentProjectContext.IsModified)
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                if (!projectService.ConfirmSaveIfModified())
                    return false;
            }
            
            // �ݒ�̕ۑ�
            AppSettings.Save();
            
            // �v���O�C���̃A�����[�h
            PluginManager.UnloadPlugins();
            
            _logger.LogInformation("Application closing");
            return true;
        }

        #region Project Commands

        /// <summary>
        /// �V�K�v���W�F�N�g���쐬
        /// </summary>
        /// <returns>���쐬�����ǂ���</returns>
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
        /// �v���W�F�N�g���J��
        /// </summary>
        /// <param name="filePath">�t�@�C���p�X</param>
        /// <returns>���쐬�����ǂ���</returns>
        public bool OpenProject(string filePath)
        {
            try
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                if (!projectService.ConfirmSaveIfModified())
                    return false;

                CurrentProjectContext = projectService.Open(filePath);
                
                // �ŋߎg�����t�@�C�����X�g�ɒǉ�
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
        /// ���݂̃v���W�F�N�g��ۑ�
        /// </summary>
        /// <returns>���쐬�����ǂ���</returns>
        public bool SaveProject()
        {
            if (CurrentProjectContext == null)
                return false;

            try
            {
                var projectService = _serviceProvider.GetRequiredService<IProjectService>();
                
                if (string.IsNullOrEmpty(CurrentProjectContext.Project.FilePath))
                {
                    // �p�X���Ȃ��ꍇ��SaveAs�ɓ]��
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
        /// ���݂̃v���W�F�N�g��ʖ��ŕۑ�
        /// </summary>
        /// <param name="filePath">�t�@�C���p�X</param>
        /// <returns>���쐬�����ǂ���</returns>
        public bool SaveProjectAs(string filePath)
        {
            if (CurrentProjectContext == null)
                return false;

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    // UI�T�[�r�X���g�p���ăt�@�C���I���_�C�A���O��\��
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
                
                // �ŋߎg�����t�@�C�����X�g�ɒǉ�
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
        /// ���\�[�X�̉��
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

### �ˑ��������ɂ��T�[�r�X�̓o�^

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
            // Serilog�̐ݒ�
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                // �T�[�r�X�R���e�i�̍\�z
                var services = new ServiceCollection();
                ConfigureServices(services);
                
                var serviceProvider = services.BuildServiceProvider();

                // �A�v���P�[�V�����̎��s
                var app = new App(serviceProvider);
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "�A�v���P�[�V�����ŏ�������Ȃ���O���������܂���");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // ���M���O�ݒ�
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(dispose: true);
            });

            // �A�v���P�[�V�����ݒ�
            services.AddSingleton<AppSettings>();
            
            // �R�A�T�[�r�X
            services.AddSingleton<AppContext>();
            services.AddSingleton<IProjectService, ProjectService>();
            services.AddSingleton<IUIService, WpfUIService>();
            
            // ��ʂ̃r���[���f��
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<ProjectPropertiesViewModel>();
            services.AddTransient<SettingsViewModel>();
            
            // �R�}���h�n���h��
            services.AddSingleton<ICommandHandler, FileCommandHandler>();
            services.AddSingleton<ICommandHandler, EditCommandHandler>();
            services.AddSingleton<ICommandHandler, ViewCommandHandler>();
            services.AddSingleton<ICommandHandler, HelpCommandHandler>();
            
            // �_�C�A���O�T�[�r�X
            services.AddSingleton<IDialogService, DialogService>();
        }
    }
}
```

### IProjectService �C���^�[�t�F�[�X�̓���

```csharp name=IProjectService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DesktopAppFramework
{
    /// <summary>
    /// �v���W�F�N�g����T�[�r�X�̃C���^�[�t�F�[�X
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// �v���W�F�N�g�^�C�v�̈ꗗ���擾
        /// </summary>
        IReadOnlyList<ProjectType> ProjectTypes { get; }
        
        /// <summary>
        /// �V�K�v���W�F�N�g���쐬
        /// </summary>
        /// <param name="projectType">�v���W�F�N�g�^�C�v�i�ȗ��j</param>
        /// <returns>�쐬���ꂽ�v���W�F�N�g�R���e�L�X�g</returns>
        ProjectContext CreateNew(ProjectType projectType = null);
        
        /// <summary>
        /// �v���W�F�N�g���J��
        /// </summary>
        /// <param name="filePath">�t�@�C���p�X</param>
        /// <returns>�J�����v���W�F�N�g�R���e�L�X�g</returns>
        ProjectContext Open(string filePath);
        
        /// <summary>
        /// �v���W�F�N�g��ۑ�
        /// </summary>
        /// <param name="context">�ۑ�����v���W�F�N�g�R���e�L�X�g</param>
        void Save(ProjectContext context);
        
        /// <summary>
        /// �v���W�F�N�g��ʖ��ŕۑ�
        /// </summary>
        /// <param name="context">�ۑ�����v���W�F�N�g�R���e�L�X�g</param>
        /// <param name="filePath">�ۑ���t�@�C���p�X</param>
        void SaveAs(ProjectContext context, string filePath);
        
        /// <summary>
        /// ���݂̃v���W�F�N�g�ɖ��ۑ��̕ύX������ꍇ�A�ۑ����邩�ǂ����m�F
        /// </summary>
        /// <returns>���s�\���itrue=���s�\�Afalse=����L�����Z���j</returns>
        bool ConfirmSaveIfModified();
        
        /// <summary>
        /// �v���W�F�N�g�t�@�C���̗L����������
        /// </summary>
        /// <param name="filePath">�t�@�C���p�X</param>
        /// <returns>�L���ȃv���W�F�N�g�t�@�C�����ǂ���</returns>
        bool ValidateProjectFile(string filePath);
    }
}
```

### ProjectService�̊g������

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
    /// �v���W�F�N�g����T�[�r�X�̎����N���X
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly AppContext _appContext;
        private readonly IUIService _uiService;
        private readonly ILogger<ProjectService> _logger;
        private readonly List<ProjectType> _projectTypes = new List<ProjectType>();

        /// <summary>
        /// �v���W�F�N�g�^�C�v�̈ꗗ
        /// </summary>
        public IReadOnlyList<ProjectType> ProjectTypes => _projectTypes.AsReadOnly();

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public ProjectService(
            AppContext appContext, 
            IUIService uiService, 
            ILogger<ProjectService> logger)
        {
            _appContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
            _uiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // �v���W�F�N�g�^�C�v�̓o�^
            RegisterDefaultProjectTypes();
        }

        /// <summary>
        /// �f�t�H���g�̃v���W�F�N�g�^�C�v��o�^
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
        /// �v���W�F�N�g�^�C�v��o�^
        /// </summary>
        /// <param name="projectType">�v���W�F�N�g�^�C�v</param>
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
        /// �V�K�v���W�F�N�g���쐬
        /// </summary>
        /// <param name="projectType">�v���W�F�N�g�^�C�v�i�ȗ��j</param>
        /// <returns>�쐬���ꂽ�v���W�F�N�g�R���e�L�X�g</returns>
        public ProjectContext CreateNew(ProjectType projectType = null)
        {
            projectType ??= _projectTypes.First();
            
            var project = projectType.CreateProject();
            return new ProjectContext(project);
        }

        /// <summary>
        /// �v���W�F�N�g���J��
        /// </summary>
        /// <param name="filePath">�t�@�C���p�X</param>
        /// <returns>�J�����v���W�F�N�g�R���e�L�X�g</returns>
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
        /// �v���W�F�N�g��ۑ�
        /// </summary>
        /// <param name="context">�ۑ�����v���W�F�N�g�R���e�L�X�g</param>
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
        /// �v���W�F�N�g��ʖ��ŕۑ�
        /// </summary>
        /// <param name="context">�ۑ�����v���W�F�N�g�R���e�L�X�g</param>
        /// <param name="filePath">�ۑ���t�@�C���p�X</param>
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
        /// �v���W�F�N�g���w�肳�ꂽ�t�@�C���ɕۑ�
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
        /// ���݂̃v���W�F�N�g�ɖ��ۑ��̕ύX������ꍇ�A�ۑ����邩�ǂ����m�F
        /// </summary>
        /// <returns>���s�\���itrue=���s�\�Afalse=����L�����Z���j</returns>
        public bool ConfirmSaveIfModified()
        {
            var currentContext = _appContext.CurrentProjectContext;
            if (currentContext == null || !currentContext.IsModified)
                return true;

            var result = _uiService.ShowMessageBox(
                $"�v���W�F�N�g '{currentContext.Project.Name}' �ɖ��ۑ��̕ύX������܂��B�ۑ����܂���?",
                "�ۑ��m�F",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            
            switch (result)
            {
                case MessageBoxResult.Yes:
                    try
                    {
                        if (string.IsNullOrEmpty(currentContext.Project.FilePath))
                        {
                            // �t�@�C���I���_�C�A���O��\��
                            string filePath = _uiService.ShowSaveFileDialog(
                                "�v���W�F�N�g�̕ۑ�",
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
                            $"�ۑ��G���[: {ex.Message}", 
                            "�G���[", 
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
        /// �v���W�F�N�g�t�@�C���̗L����������
        /// </summary>
        /// <param name="filePath">�t�@�C���p�X</param>
        /// <returns>�L���ȃv���W�F�N�g�t�@�C�����ǂ���</returns>
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

### UI�T�[�r�X�C���^�[�t�F�[�X

```csharp name=IUIService.cs
using System;
using System.Windows;

namespace DesktopAppFramework
{
    /// <summary>
    /// UI����̃T�[�r�X�C���^�[�t�F�[�X
    /// </summary>
    public interface IUIService
    {
        /// <summary>
        /// ���b�Z�[�W�{�b�N�X��\��
        /// </summary>
        /// <param name="message">���b�Z�[�W</param>
        /// <param name="title">�^�C�g��</param>
        /// <param name="buttons">�{�^��</param>
        /// <param name="icon">�A�C�R��</param>
        /// <returns>����</returns>
        MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon);
        
        /// <summary>
        /// �t�@�C�����J���_�C�A���O��\��
        /// </summary>
        /// <param name="title">�_�C�A���O�̃^�C�g��</param>
        /// <param name="filter">�t�@�C���t�B���^</param>
        /// <param name="defaultExtension">�f�t�H���g�g���q</param>
        /// <returns>�I�����ꂽ�t�@�C���p�X�A�L�����Z�����͋󕶎�</returns>
        string ShowOpenFileDialog(string title, string filter, string defaultExtension);
        
        /// <summary>
        /// �t�@�C����ۑ��_�C�A���O��\��
        /// </summary>
        /// <param name="title">�_�C�A���O�̃^�C�g��</param>
        /// <param name="filter">�t�@�C���t�B���^</param>
        /// <param name="defaultExtension">�f�t�H���g�g���q</param>
        /// <returns>�I�����ꂽ�t�@�C���p�X�A�L�����Z�����͋󕶎�</returns>
        string ShowSaveFileDialog(string title, string filter, string defaultExtension);
        
        /// <summary>
        /// �t�H���_�I���_�C�A���O��\��
        /// </summary>
        /// <param name="title">�_�C�A���O�̃^�C�g��</param>
        /// <returns>�I�����ꂽ�t�H���_�p�X�A�L�����Z�����͋󕶎�</returns>
        string ShowFolderBrowserDialog(string title);
        
        /// <summary>
        /// �_�C�A���O��\��
        /// </summary>
        /// <typeparam name="TViewModel">�r���[���f���̌^</typeparam>
        /// <param name="viewModel">�_�C�A���O�̃r���[���f��</param>
        /// <param name="title">�_�C�A���O�̃^�C�g��</param>
        /// <returns>�_�C�A���O�̌���</returns>
        bool? ShowDialog<TViewModel>(TViewModel viewModel, string title = null) where TViewModel : ViewModelBase;
        
        /// <summary>
        /// �w�肳�ꂽ�r���[�����C�����[�N�X�y�[�X�ɕ\��
        /// </summary>
        /// <typeparam name="TViewModel">�r���[���f���̌^</typeparam>
        void ShowView<TViewModel>(TViewModel viewModel = null) where TViewModel : ViewModelBase;
    }
}
```

### WPF����UI�T�[�r�X����

```csharp name=WpfUIService.cs
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace DesktopAppFramework
{
    /// <summary>
    /// WPF����UI�T�[�r�X�̎���
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
        /// ���b�Z�[�W�{�b�N�X��\��
        /// </summary>
        public MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return MessageBox.Show(message, title, buttons, icon);
        }

        /// <summary>
        /// �t�@�C�����J���_�C�A���O��\��
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
        /// �t�@�C����ۑ��_�C�A���O��\��
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
        /// �t�H���_�I���_�C�A���O��\��
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
        /// �_�C�A���O��\��
        /// </summary>
        public bool? ShowDialog<TViewModel>(TViewModel viewModel, string title = null) where TViewModel : ViewModelBase
        {
            var viewType = ViewLocator.GetViewTypeForViewModel(typeof(TViewModel));
            if (viewType == null)
            {
                _logger.LogError($"View not found for view model type: {typeof(TViewModel).Name}");
                return null;
            }
            
            // �r���[�̃C���X�^���X�쐬
            var view = Activator.CreateInstance(viewType) as Window;
            if (view == null)
            {
                _logger.LogError($"Failed to create view instance: {viewType.Name}");
                return null;
            }
            
            // �^�C�g���ݒ�
            if (!string.IsNullOrEmpty(title))
            {
                view.Title = title;
            }
            
            // �f�[�^�R���e�L�X�g�ݒ�
            view.DataContext = viewModel;
            
            // ���[�_���_�C�A���O�Ƃ��ĕ\��
            return view.ShowDialog();
        }

        /// <summary>
        /// �w�肳�ꂽ�r���[�����C�����[�N�X�y�[�X�ɕ\��
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
            
            // MainWindow��Workspace�Ƀr���[��\��
            var mainViewModel = mainWindow.DataContext as MainWindowViewModel;
            mainViewModel?.ShowWorkspaceContent(viewModel);
        }
    }
}
```

### �A�v���P�[�V�����ݒ�

```csharp name=AppSettings.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DesktopAppFramework
{
    /// <summary>
    /// �A�v���P�[�V�����ݒ�
    /// </summary>
    public class AppSettings
    {
        private readonly ILogger<AppSettings> _logger;
        private const int MaxRecentFiles = 10;

        // �S�ʐݒ�
        public string ApplicationName { get; set; } = "Desktop App Framework";
        public string CurrentTheme { get; set; } = "Light";
        public bool CheckForUpdatesOnStartup { get; set; } = true;
        public bool AutoSaveEnabled { get; set; } = true;
        public int AutoSaveIntervalMinutes { get; set; } = 5;
        
        // �v���W�F�N�g�ݒ�
        public string DefaultProjectExtension { get; set; } = ".proj";
        public string ProjectFileFilter { get; set; } = "Project Files (*.proj)|*.proj|All Files (*.*)|*.*";
        public string DefaultProjectsDirectory { get; set; } = "";
        
        // �ŋߎg�p�����t�@�C��
        public List<string> RecentFiles { get; set; } = new List<string>();
        
        // �E�B���h�E�ݒ�
        public double WindowWidth { get; set; } = 1200;
        public double WindowHeight { get; set; } = 800;
        public bool StartMaximized { get; set; } = false;

        /// <summary>
        /// �ݒ�t�@�C���̃p�X
        /// </summary>
        private string SettingsFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ApplicationName,
            "settings.json");

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public AppSettings(ILogger<AppSettings> logger = null)
        {
            _logger = logger;
            
            // �f�t�H���g�v���W�F�N�g�f�B���N�g���̐ݒ�
            if (string.IsNullOrEmpty(DefaultProjectsDirectory))
            {
                DefaultProjectsDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    ApplicationName,
                    "Projects");
            }
            
            // �ݒ�̃��[�h
            Load();
        }

        /// <summary>
        /// �ݒ���t�@�C�����烍�[�h
        /// </summary>
        public void Load()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    // �ݒ�t�@�C�������݂��Ȃ��ꍇ�̓f�t�H���g�l���g�p
                    Save();
                    return;
                }

                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                
                if (settings != null)
                {
                    // �v���p�e�B���R�s�[
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
        /// �ݒ���t�@�C���ɕۑ�
        /// </summary>
        public void Save()
        {
            try
            {
                // �f�B���N�g�������݂��Ȃ��ꍇ�͍쐬
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
        /// �ŋߎg�p�����t�@�C����ǉ�
        /// </summary>
        /// <param name="filePath">�t�@�C���p�X</param>
        public void AddRecentFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;
                
            // ���ɑ��݂���ꍇ�͍폜���Đ擪�ɒǉ�
            RecentFiles.Remove(filePath);
            RecentFiles.Insert(0, filePath);
            
            // �ő吔�𒴂���ꍇ�͌Â����̂��폜
            while (RecentFiles.Count > MaxRecentFiles)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }
        }
    }
}
```

### �v���O�C���V�X�e��

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
    /// �v���O�C�����Ǘ�����N���X
    /// </summary>
    public class PluginManager : IDisposable
    {
        private readonly AppContext _appContext;
        private readonly List<IPlugin> _loadedPlugins = new List<IPlugin>();
        private readonly ILogger<PluginManager> _logger;

        /// <summary>
        /// �ǂݍ��܂ꂽ�v���O�C���̃��X�g
        /// </summary>
        public IReadOnlyList<IPlugin> LoadedPlugins => _loadedPlugins.AsReadOnly();

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="appContext">�A�v���P�[�V�����R���e�L�X�g</param>
        public PluginManager(AppContext appContext)
        {
            _appContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
            _logger = appContext.ServiceProvider.GetService(typeof(ILogger<PluginManager>)) as ILogger<PluginManager>;
        }

        /// <summary>
        /// �v���O�C����ǂݍ���
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
                // �v���O�C���f�B���N�g������DLL������
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
        /// �w�肳�ꂽ�A�Z���u������v���O�C����ǂݍ���
        /// </summary>
        /// <param name="assemblyPath">�A�Z���u���p�X</param>
        private void LoadPluginFromAssembly(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            
            // IPlugin����������^������
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();
                
            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    // �v���O�C���̃C���X�^���X���쐬
                    var plugin = Activator.CreateInstance(pluginType) as IPlugin;
                    
                    if (plugin != null)
                    {
                        // �v���O�C����������
                        plugin.Initialize(_appContext);
                        
                        // ���X�g�ɒǉ�
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
        /// �v���O�C�����A�����[�h
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
        /// ���\�[�X�̉��
        /// </summary>
        public void Dispose()
        {
            UnloadPlugins();
        }
    }
}
```

### �v���O�C���C���^�[�t�F�[�X

```csharp name=IPlugin.cs
using System;

namespace DesktopAppFramework
{
    /// <summary>
    /// �v���O�C���̃C���^�[�t�F�[�X
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// �v���O�C����
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// �v���O�C���̐���
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// �v���O�C���̃o�[�W����
        /// </summary>
        Version Version { get; }
        
        /// <summary>
        /// �v���O�C���̍��
        /// </summary>
        string Author { get; }
        
        /// <summary>
        /// �v���O�C����������
        /// </summary>
        /// <param name="appContext">�A�v���P�[�V�����R���e�L�X�g</param>
        void Initialize(AppContext appContext);
        
        /// <summary>
        /// �v���O�C�����V���b�g�_�E��
        /// </summary>
        void Shutdown();
    }
}
```

### MVVM���N���X

```csharp name=ViewModelBase.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopAppFramework
{
    /// <summary>
    /// �r���[���f���̊��N���X
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// �v���p�e�B�ύX�ʒm�C�x���g
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// �v���p�e�B�ύX�ʒm�𔭐�������
        /// </summary>
        /// <param name="propertyName">�v���p�e�B��</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// �v���p�e�B�l��ݒ肵�A�ύX���ɒʒm�𔭐�������
        /// </summary>
        /// <typeparam name="T">�v���p�e�B�̌^</typeparam>
        /// <param name="field">�o�b�L���O�t�B�[���h</param>
        /// <param name="value">�V�����l</param>
        /// <param name="propertyName">�v���p�e�B��</param>
        /// <returns>�l���ύX���ꂽ���ǂ���</returns>
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

### �R�}���h�C���t���X�g���N�`��

```csharp name=RelayCommand.cs
using System;
using System.Windows.Input;

namespace DesktopAppFramework
{
    /// <summary>
    /// �ė��p�\�ȃR�}���h����
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// ���s�\��ԕύX���̃C�x���g
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="execute">���s����</param>
        /// <param name="canExecute">���s�\����</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// �R�}���h�����s�\���ǂ�������
        /// </summary>
        /// <param name="parameter">�p�����[�^</param>
        /// <returns>���s�\���ǂ���</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// �R�}���h�����s
        /// </summary>
        /// <param name="parameter">�p�����[�^</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// �R�}���h���s�\��Ԃ̍ĕ]����v��
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
```

## �g���@�\�̐���

���̊g���t���[�����[�N�́A�ȉ��̂悤�ȋ@�\��񋟂��܂��F

1. **MVVM �p�^�[���̎���**
   - `ViewModelBase` �N���X�ɂ��v���p�e�B�ύX�ʒm
   - `RelayCommand` �ɂ��R�}���h�o�C���f�B���O
   - `ViewLocator` �ɂ�� View �� ViewModel �̎�������

2. **�T�[�r�X�x�[�X�̃A�[�L�e�N�`��**
   - �C���^�[�t�F�[�X�x�[�X�̑a�����݌v
   - �ˑ��������ɂ��_��ȍ\��
   - �e�X�g�e�Ր��̌���

3. **�v���O�C���V�X�e��**
   - �A�v���P�[�V�����̋@�\�𓮓I�Ɋg��
   - �T�[�h�p�[�e�B�ɂ��J�����e��

4. **�e�[�}�ƃX�^�C��**
   - ���I�ȃe�[�}�؂�ւ�
   - �J�X�^�}�C�Y�\��UI�X�^�C��

5. **�ݒ�Ǘ�**
   - �ݒ�̉i����
   - ���[�U�[�ݒ�̕ۑ��Ɠǂݍ���

6. **���M���O**
   - ��I�ȃG���[����ё��샍�O
   - Serilog�ɂ��_��ȃ��O�o��

���̊g���t���[�����[�N���x�[�X�ɁA�l�X�Ȏ�ނ̃f�X�N�g�b�v�A�v���P�[�V�������J�����邱�Ƃ��ł��܂��B�Ⴆ�΁A�e�L�X�g�G�f�B�^�A�摜�ҏW�\�t�g�A�f�[�^���̓c�[���ȂǁA�v���W�F�N�g�̊T�O�����قƂ�ǂ̃A�v���P�[�V�����ɉ��p�\�ł��B

## �����̃|�C���g

- **UI�t���[�����[�N�Ɨ���**: �R�A���W�b�N��UI�̕����ɂ��A�����قȂ�UI�Z�p�iAvalonia, WinUI�Ȃǁj�ւ̈ڍs���e��
- **�e�X�g�e�Ր�**: �C���^�[�t�F�[�X��DI�̊��p�ɂ��P�̃e�X�g���e��
- **�g����**: �v���O�C���A�[�L�e�N�`���ɂ��@�\�g�����\
- **�ێ琫**: �֐S�̕����ƕW���p�^�[���̗̍p�ɂ��ێ炪�e��

�����̊g���ɂ��A�ėp�I�ȃf�X�N�g�b�v�A�v���P�[�V�����̃x�[�X�Ƃ��Ďg�p�ł��錘�S�ȃt���[�����[�N���\�z�ł��܂��B
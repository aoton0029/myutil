C#��WinForms��WPF�Ń��j���[�o�[�́u�t�@�C���v�^�u�̋@�\���܂Ƃ߂邽�߂ɁA��p�̃N���X���쐬����ƊǗ����₷���Ȃ�܂��B

---

### **�\��**
1. **`FileMenuHandler` �N���X**  
   - �u�t�@�C���v���j���[�̃C�x���g���Ǘ�����N���X�B
   - �V�K�쐬�A�J���A�ۑ��A�I�� �Ȃǂ̋@�\��񋟁B

2. **`IMainForm` �C���^�[�t�F�[�X**  
   - ���j���[����ŉe�����󂯂郁�C���t�H�[���̑���𓝈ꂷ�邽�߂̃C���^�[�t�F�[�X�B

3. **WinForms�ł̗��p��**  
   - `FileMenuHandler` ���t�H�[���ɓ����B

---

### **����**

#### **1. `IMainForm` �C���^�[�t�F�[�X**
���C���t�H�[���ŕK�v�ȑ���𓝈ꂷ��B

```csharp
public interface IMainForm
{
    void CreateNewFile();
    void OpenFile();
    void SaveFile();
    void ExitApplication();
}
```

---

#### **2. `FileMenuHandler` �N���X**
���j���[�̓�����J�v�Z�����B

```csharp
using System;
using System.Windows.Forms;

public class FileMenuHandler
{
    private readonly IMainForm _mainForm;

    public FileMenuHandler(IMainForm mainForm)
    {
        _mainForm = mainForm;
    }

    public void HandleMenuClick(object sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem menuItem)
        {
            switch (menuItem.Name)
            {
                case "newFileMenuItem":
                    _mainForm.CreateNewFile();
                    break;
                case "openFileMenuItem":
                    _mainForm.OpenFile();
                    break;
                case "saveFileMenuItem":
                    _mainForm.SaveFile();
                    break;
                case "exitMenuItem":
                    _mainForm.ExitApplication();
                    break;
            }
        }
    }
}
```

---

#### **3. ���C���t�H�[���ł̗��p**
`FileMenuHandler` �𓝍����A�t�H�[���̋@�\�������B

```csharp
public partial class MainForm : Form, IMainForm
{
    private FileMenuHandler _fileMenuHandler;

    public MainForm()
    {
        InitializeComponent();
        _fileMenuHandler = new FileMenuHandler(this);

        // �C�x���g�n���h�����Z�b�g
        newFileMenuItem.Click += _fileMenuHandler.HandleMenuClick;
        openFileMenuItem.Click += _fileMenuHandler.HandleMenuClick;
        saveFileMenuItem.Click += _fileMenuHandler.HandleMenuClick;
        exitMenuItem.Click += _fileMenuHandler.HandleMenuClick;
    }

    public void CreateNewFile()
    {
        MessageBox.Show("�V�K�t�@�C���쐬");
    }

    public void OpenFile()
    {
        using (OpenFileDialog openFileDialog = new OpenFileDialog())
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show($"�J�����t�@�C��: {openFileDialog.FileName}");
            }
        }
    }

    public void SaveFile()
    {
        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show($"�ۑ������t�@�C��: {saveFileDialog.FileName}");
            }
        }
    }

    public void ExitApplication()
    {
        Application.Exit();
    }
}
```

---

### **�g����**
1. **�ŋߊJ�����t�@�C���@�\**
   - `Properties.Settings` �����p���A�ŋߊJ�����t�@�C����ۑ��E�\���B

2. **�V���[�g�J�b�g�L�[�̃T�|�[�g**
   - `KeyPreview = true` ��ݒ肵�A`KeyDown` �C�x���g�� `Ctrl + S` �Ȃǂ̃V���[�g�J�b�g�������B

3. **�v���O�C���Ή�**
   - ���j���[�𓮓I�ɕύX�ł���悤 `Action` �f���Q�[�g���g���B

---

���̂悤�� `FileMenuHandler` ���쐬���邱�ƂŁA�R�[�h�̌��ʂ����ǂ��Ȃ�A�g�������₷���Ȃ�܂��I



WinForms �̃��j���[�o�[��ėp�I�ɐ݌v���A�قȂ�t�H�[����A�v���P�[�V�����ōė��p�ł���A�[�L�e�N�`�����\�z������@���l���܂��B

## �݌v���j
1. **���j���[�o�[�� `UserControl` �Ƃ��č쐬**
   - ���ʂ̃��j���[�\���� `UserControl` �ɃJ�v�Z��������B
   - �e���j���[���ڂ𓮓I�ɕύX�\�ɂ���B

2. **���j���[���ڂ̐ݒ���O���t�@�C���iJSON�Ȃǁj�ŊǗ�**
   - ���j���[���ڂ̖��O��C�x���g�n���h����ݒ�t�@�C������ǂݍ��ށB
   - �_��Ƀ��j���[�\����ύX�\�ɂ���B

3. **�R�}���h�p�^�[�����g�p���ē����؂�ւ�**
   - `ICommand` �C���^�[�t�F�[�X���`���A�e���j���[�̓������������B
   - �ݒ�t�@�C���łǂ̃R�}���h�����s���邩�w��ł���悤�ɂ���B

---

## ����

### 1. ���j���[�̒�` (`MenuItemConfig.cs`)
�܂��A���j���[�̍\����\���N���X���쐬���܂��B

```csharp
public class MenuItemConfig
{
    public string Name { get; set; }
    public string Command { get; set; } // ���s����R�}���h�̃L�[
    public List<MenuItemConfig> SubItems { get; set; } = new List<MenuItemConfig>();
}
```

---

### 2. �R�}���h�p�^�[�� (`ICommand.cs`)
�R�}���h�̎�����ėp�����A������_��ɕύX�\�ɂ��܂��B

```csharp
public interface ICommand
{
    void Execute();
}
```

�e���j���[�̃A�N�V�������R�}���h�Ƃ��Ē�`���܂��B

```csharp
public class OpenFileCommand : ICommand
{
    public void Execute()
    {
        MessageBox.Show("�t�@�C�����J������");
    }
}

public class ExitCommand : ICommand
{
    public void Execute()
    {
        Application.Exit();
    }
}
```

�R�}���h���Ǘ�����N���X��p�ӂ��܂��B

```csharp
public class CommandRegistry
{
    private Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

    public void Register(string key, ICommand command)
    {
        commands[key] = command;
    }

    public ICommand GetCommand(string key)
    {
        return commands.ContainsKey(key) ? commands[key] : null;
    }
}
```

---

### 3. ���j���[�̐ݒ��JSON�ŊǗ� (`menu.json`)

```json
[
    {
        "Name": "�t�@�C��",
        "SubItems": [
            {
                "Name": "�J��",
                "Command": "OpenFile"
            },
            {
                "Name": "�I��",
                "Command": "Exit"
            }
        ]
    }
]
```

---

### 4. �ėp���j���[�o�[�N���X (`CustomMenuStrip.cs`)
���̃N���X�Ń��j���[�o�[���\�z���A�ݒ��ǂݍ���ŃR�}���h�Ɗ֘A�t���܂��B

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

public class CustomMenuStrip : MenuStrip
{
    private CommandRegistry commandRegistry;

    public CustomMenuStrip(CommandRegistry registry, string configPath)
    {
        this.commandRegistry = registry;
        LoadMenu(configPath);
    }

    private void LoadMenu(string configPath)
    {
        if (!File.Exists(configPath))
        {
            MessageBox.Show("���j���[�ݒ�t�@�C����������܂���");
            return;
        }

        string json = File.ReadAllText(configPath);
        var menuItems = JsonSerializer.Deserialize<List<MenuItemConfig>>(json);

        foreach (var item in menuItems)
        {
            this.Items.Add(CreateMenuItem(item));
        }
    }

    private ToolStripMenuItem CreateMenuItem(MenuItemConfig config)
    {
        ToolStripMenuItem menuItem = new ToolStripMenuItem(config.Name);

        if (!string.IsNullOrEmpty(config.Command))
        {
            ICommand command = commandRegistry.GetCommand(config.Command);
            if (command != null)
            {
                menuItem.Click += (sender, e) => command.Execute();
            }
        }

        foreach (var subItem in config.SubItems)
        {
            menuItem.DropDownItems.Add(CreateMenuItem(subItem));
        }

        return menuItem;
    }
}
```

---

### 5. �t�H�[���ł̗��p (`MainForm.cs`)

```csharp
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        InitializeMenu();
    }

    private void InitializeMenu()
    {
        CommandRegistry registry = new CommandRegistry();
        registry.Register("OpenFile", new OpenFileCommand());
        registry.Register("Exit", new ExitCommand());

        CustomMenuStrip menuStrip = new CustomMenuStrip(registry, "menu.json");
        this.MainMenuStrip = menuStrip;
        this.Controls.Add(menuStrip);
    }
}
```

---

## �g����
1. **���I���[�h�@�\**
   - JSON��ύX����Α����Ƀ��j���[��ύX�ł���@�\��ǉ��B

2. **�v���O�C���V�X�e��**
   - ���j���[�̃R�}���h���O����DLL���瓮�I�Ƀ��[�h�\�ɂ���B

3. **���[���Ǘ�**
   - ���[�U�[�̌����ɉ����āA�\�����郁�j���[�𐧌�B

---

���̃A�[�L�e�N�`���ɂ��A���j���[�o�[�̍\�����O������ݒ�ł��A�R�[�h�̕ύX�Ȃ��ŃJ�X�^�}�C�Y���\�ɂȂ�܂��I
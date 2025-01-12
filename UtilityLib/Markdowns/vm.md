WinForms��**�R�}���h�p�^�[��**�����p����ƁAUI�C�x���g�i�{�^���̃N���b�N�Ȃǁj��ViewModel���̃r�W�l�X���W�b�N�ɈϏ����邱�Ƃ��ł��A�R�[�h�̍\������胂�W���[�����ł��܂��B����ɂ��AUI���W�b�N�ƃr�W�l�X���W�b�N�𖾊m�ɕ������A�ێ琫�����サ�܂��B

�ȉ���WinForms�ŃR�}���h�p�^�[�������p������@��������܂��B

---

### **�R�}���h�p�^�[���̎d�g��**
�R�}���h�p�^�[���ł́A�A�N�V�����i�Ⴆ�΃{�^���̃N���b�N�C�x���g�j���u�R�}���h�v�Ƃ��Ē��ۉ����܂��B  
����ɂ��A�A�N�V�������I�u�W�F�N�g�Ƃ��Ĉ����A�ȒP�ɍė��p��e�X�g���\�ɂȂ�܂��B

---

### **������**

#### **1. ICommand�C���^�[�t�F�[�X�̒�`**
�R�}���h�̊�{�\�����`���܂��B

```csharp
using System;

public interface ICommand
{
    bool CanExecute(object parameter);
    void Execute(object parameter);
    event EventHandler CanExecuteChanged;
}
```

---

#### **2. RelayCommand�N���X�̎���**
�ėp�I�ȃR�}���h������񋟂��܂��B

```csharp
using System;

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object parameter)
    {
        _execute(parameter);
    }

    public event EventHandler CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
```

---

#### **3. ViewModel�̍쐬**
ViewModel�ŃR�}���h���g�p���܂��B

```csharp
using System.ComponentModel;

public class CounterViewModel : INotifyPropertyChanged
{
    private int _count;
    public int Count
    {
        get => _count;
        set
        {
            if (_count != value)
            {
                _count = value;
                OnPropertyChanged(nameof(Count));
            }
        }
    }

    public ICommand IncrementCommand { get; }
    public ICommand DecrementCommand { get; }

    public CounterViewModel()
    {
        IncrementCommand = new RelayCommand(
            _ => Count++, // ���s����A�N�V����
            _ => Count < 10 // ���s�\����
        );

        DecrementCommand = new RelayCommand(
            _ => Count--,
            _ => Count > 0
        );
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

---

#### **4. �t�H�[���ł̎g�p**
ViewModel���t�H�[���Ƀo�C���h���A�R�}���h���{�^���ɐڑ����܂��B

```csharp
using System;
using System.Windows.Forms;

public partial class MainForm : Form
{
    private CounterViewModel _viewModel;

    public MainForm()
    {
        InitializeComponent();
        _viewModel = new CounterViewModel();

        // �f�[�^�o�C���h
        textBoxCount.DataBindings.Add("Text", _viewModel, nameof(CounterViewModel.Count));

        // �{�^���ɃR�}���h��ݒ�
        buttonIncrement.Click += (s, e) =>
        {
            if (_viewModel.IncrementCommand.CanExecute(null))
                _viewModel.IncrementCommand.Execute(null);
        };

        buttonDecrement.Click += (s, e) =>
        {
            if (_viewModel.DecrementCommand.CanExecute(null))
                _viewModel.DecrementCommand.Execute(null);
        };
    }
}
```

---

### **�������**
1. `IncrementCommand`�́A`Count`��10�����̏ꍇ�ɂ̂ݎ��s�\�ł��B
2. `DecrementCommand`�́A`Count`��0���傫���ꍇ�ɂ̂ݎ��s�\�ł��B
3. �{�^�����N���b�N����ƁA`RelayCommand`��`Execute`���\�b�h���Ăяo����A�J�E���g�l���ύX����܂��B

---

### **���p��**

#### **1. �{�^���̖�����**
�{�^���̗L��/������`CanExecute`�Ɋ�Â��Đ��䂵�܂��B

```csharp
buttonIncrement.Enabled = _viewModel.IncrementCommand.CanExecute(null);
buttonDecrement.Enabled = _viewModel.DecrementCommand.CanExecute(null);

// ViewModel�̏�Ԃ��ς�����ꍇ�ɍĕ]��
_viewModel.PropertyChanged += (s, e) =>
{
    if (e.PropertyName == nameof(CounterViewModel.Count))
    {
        buttonIncrement.Enabled = _viewModel.IncrementCommand.CanExecute(null);
        buttonDecrement.Enabled = _viewModel.DecrementCommand.CanExecute(null);
    }
};
```

---

#### **2. �p�����[�^�t���R�}���h**
�p�����[�^���󂯎��R�}���h�������ł��܂��B

```csharp
IncrementCommand = new RelayCommand(
    param => Count += Convert.ToInt32(param),
    param => true
);
```

�g�p��F

```csharp
buttonIncrement.Click += (s, e) =>
{
    _viewModel.IncrementCommand.Execute(5); // 5���p�����[�^�Ƃ��ēn��
};
```

---

### **�����b�g**
1. **���W���[����**  
   - �r�W�l�X���W�b�N��ViewModel�ɃJ�v�Z�����B

2. **�e�X�g�\��**  
   - �R�}���h��P�̃e�X�g���₷���B

3. **UI���W�b�N�̊ȑf��**  
   - �R���g���[���̃N���b�N�C�x���g�ɕ��G�ȃ��W�b�N���L�q�����A�R�}���h�ɈϏ��B

---

�K�v�ɉ����Ă���ɏڍׂȎ����≞�p������`���ł��܂��I
`async void` ���\�b�h��C#�Ŕ񓯊��Ɏ��s�����ꍇ�A����UI�X���b�h��œ��삵�Ă���󋵂ɂ��Ă̒��ӓ_������܂��B�ȉ��A`async void`���\�b�h��await�����Ɏ��s�����Ƃ��̓���ɂ��Đ������܂��B

### `async void` �̓���
- `async void`���\�b�h�͔񓯊��Ŏ��s����܂����A�߂�l��Ԃ��Ȃ����߁Aawait���邱�Ƃ��ł��܂���B
- �ʏ�A�񓯊����\�b�h��`Task`��`Task<T>`��Ԃ��A�Ăяo���������̃^�X�N��await���邱�ƂŁA�񓯊��̊�����҂��Ƃ��ł��܂��B
- `async void`�̓C�x���g�n���h���̂悤�ȓ���ȏ󋵂ŗ��p����邱�Ƃ�z�肳��Ă���A�G���[�n���h�����O�⊮���̊m�F������Ƃ����_�ň�ʓI�Ȕ񓯊������ɂ͓K���Ă��܂���B

### UI �X���b�h�ł̓���
- UI�X���b�h��`async void`���\�b�h���Ăяo�����ꍇ�A���̃��\�b�h���̔񓯊��������n�܂�ƁA�Ăяo�����̃R�[�h�̐���͂����ɕԂ���܂����A�񓯊��������̂�UI�X���b�h��Ŕ񓯊��I�ɐi�s���܂��B
- ���̂Ƃ��AUI�X���b�h�͔񓯊������̊�����҂��Ȃ����߁A�񓯊��������ł����Ă�UI�̑��̑���͑��s�\�ł��B
- ����ɂ��AUI�X���b�h��`async void`��await�����Ɏ��s����ƁAUI�̓u���b�N���ꂸ�A���[�U�[������𑱂��邱�Ƃ��ł��܂��B�������A�񓯊��������I���O�ɕʂ̑��삪��������AUI���X�V�����\�������邽�߁A�\�����Ȃ��������N���郊�X�N������܂��B

### ���s�̗���
�Ⴆ�΁A�ȉ��̂悤�ȃR�[�h���l���܂��B

```csharp
private async void Button_Click(object sender, EventArgs e)
{
    await Task.Delay(3000); // �񓯊���3�b�҂���
    MessageBox.Show("�������������܂���");
}
```

���̃R�[�h�̓{�^�����N���b�N���ꂽ�Ƃ��ɌĂяo����܂����A�ȉ��̂悤�ȋ����������܂��B
1. �{�^�����N���b�N������`Button_Click`���\�b�h���Ăяo����܂��B
2. ���\�b�h����`await Task.Delay(3000)`�����s����A������3�b�ҋ@����񓯊��������n�܂�܂��B
3. `await`�ɂ���ČĂяo�����ɐ��䂪�߂�AUI�X���b�h�͑��̃��[�U�[�C���^�[�t�F�C�X������p�����܂��B
4. 3�b���`MessageBox.Show("�������������܂���")`�����s����A���b�Z�[�W�{�b�N�X���\������܂��B

`async void`���g�p����ƁA�񓯊������̓r���Ń��\�b�h���甲���Ă��܂����߁A�G���[�n���h�����O����s�̊����𑼂̃R�[�h�ŒǐՂ���̂�����Ȃ�܂��B�܂��A`void`��Ԃ����߁A�Ăяo�����Ń^�X�N��await���đ҂��Ƃ��ł��܂���B

### ���������΍�
- **`async Task`�̎g�p**�F�ł������`async void`�ł͂Ȃ�`async Task`���g�p���A�Ăяo������await���Ĕ񓯊�������҂悤�ɂ��邱�Ƃ���������܂��B����ɂ��A�G���[�n���h�����O����s�����̒ǐՂ��e�ՂɂȂ�܂��B
  
  ```csharp
  private async Task DoSomethingAsync()
  {
      await Task.Delay(3000);
      MessageBox.Show("�������������܂���");
  }

  private async void Button_Click(object sender, EventArgs e)
  {
      await DoSomethingAsync();
  }
  ```

- **��O�̎�舵��**�F`async void`���\�b�h�ŗ�O�����������ꍇ�A�ʏ��`Task`��Ԃ����\�b�h�Ƃ͈قȂ�A��O���L���b�`���邱�Ƃ�������߁A�A�v���P�[�V�����̗\�����Ȃ��N���b�V�����������ꂪ����܂��B���̂��߁A`async void`�̎g�p�̓C�x���g�n���h���Ɍ��肷�邱�Ƃ���ʓI�ł��B

### �܂Ƃ�
- `async void`��await������UI�X���b�h�Ŏ��s����ƁAUI�X���b�h���u���b�N����Ȃ��܂ܔ񓯊��������i�s���܂��B
- UI�X���b�h��`async void`���g���Ƃ��ɂ́A�񓯊������̊�����҂ĂȂ����߁A�G���[�����⏇�����䂪����Ȃ�_�ɒ��ӂ��K�v�ł��B
- `async Task`���g�p���邱�ƂŁA�񓯊������̊�����await���đ҂��Ƃ��ł��A���m���ɔ񓯊������𐧌䂷�邱�Ƃ��\�ł��B











�ȉ��ɁA`ContinueWith`���g�p�����ۂ̃X���b�h���A�����`Task`���ŗ�O�����������ꍇ�̃X���b�h����ǉ������T���v���R�[�h�Ɛ����������܂��B

---

### **�T���v���R�[�h**

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AwaitVsWaitExample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnContinueWith_Click(object sender, EventArgs e)
        {
            Log("btnContinueWith_Click (Start)");

            // �^�X�N���J�n���A�������ContinueWith�ŏ����𑱂���
            Task.Run(() =>
            {
                Log("Task.Run (Inside Background Task)");
                Thread.Sleep(1000); // 1�b�ҋ@
                return "Task completed";
            })
            .ContinueWith(task =>
            {
                Log("ContinueWith (On Completion)");
                Log($"Task Result: {task.Result}");
            }, TaskScheduler.FromCurrentSynchronizationContext()); // UI�X���b�h�ő��s

            Log("btnContinueWith_Click (End)");
        }

        private void btnException_Click(object sender, EventArgs e)
        {
            Log("btnException_Click (Start)");

            // �^�X�N���ŗ�O�𔭐�������
            Task.Run(() =>
            {
                Log("Task.Run (Inside Background Task - Before Exception)");
                throw new InvalidOperationException("Something went wrong in the task");
            })
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Log("ContinueWith (On Exception)");
                    Log($"Exception: {task.Exception?.Flatten().InnerException?.Message}");
                }
            }, TaskScheduler.FromCurrentSynchronizationContext()); // UI�X���b�h�ő��s

            Log("btnException_Click (End)");
        }

        private void Log(string message)
        {
            // ���݂̃X���b�hID�����O�ɏo��
            string log = $"{DateTime.Now:HH:mm:ss.fff} [Thread: {Thread.CurrentThread.ManagedThreadId}] {message}";
            Console.WriteLine(log);
            listBox1.Items.Add(log); // ���O��UI�ɕ\��
        }
    }
}
```

---

### **�����Ɠ��쌋��**

#### **1. `btnContinueWith_Click`�i`ContinueWith`�̗�j**

���̃{�^�����N���b�N����ƁA�ȉ��̂悤�ȏ��������s����܂��F

1. `Task.Run`���Ŕ񓯊��������o�b�N�O���E���h�X���b�h�Ŏ��s����܂��B
2. ����������A`ContinueWith`�ŏ��������s����܂��B
   - `TaskScheduler.FromCurrentSynchronizationContext()`���w�肵�Ă��邽�߁A`ContinueWith`�̏�����UI�X���b�h�Ŏ��s����܂��B

**���s���ʗ�**:
```plaintext
09:00:00.000 [Thread: 1] btnContinueWith_Click (Start)
09:00:00.005 [Thread: 4] Task.Run (Inside Background Task)
09:00:00.010 [Thread: 1] btnContinueWith_Click (End)
09:00:01.015 [Thread: 1] ContinueWith (On Completion)
09:00:01.020 [Thread: 1] Task Result: Task completed
```

- **`Task.Run`**: �o�b�N�O���E���h�X���b�h�i`Thread: 4`�j�Ŏ��s�B
- **`ContinueWith`**: UI�X���b�h�i`Thread: 1`�j�Ŏ��s�B

#### **2. `btnException_Click`�i��O�̗�j**

���̃{�^�����N���b�N����ƁA�ȉ��̂悤�ȏ��������s����܂��F

1. `Task.Run`���ŗ�O���X���[����܂��B
2. `ContinueWith`�ŗ�O�����擾���AUI�X���b�h�ŗ�O���������܂��B

**���s���ʗ�**:
```plaintext
09:00:00.000 [Thread: 1] btnException_Click (Start)
09:00:00.005 [Thread: 4] Task.Run (Inside Background Task - Before Exception)
09:00:00.010 [Thread: 1] btnException_Click (End)
09:00:00.015 [Thread: 1] ContinueWith (On Exception)
09:00:00.020 [Thread: 1] Exception: Something went wrong in the task
```

- **��O����**:
  - �o�b�N�O���E���h�X���b�h�i`Thread: 4`�j�ŗ�O���X���[����܂��B
  - `ContinueWith`���ŗ�O���L���b�`����AUI�X���b�h�i`Thread: 1`�j�ŏ�������܂��B

---

### **�|�C���g**

1. **`ContinueWith`�̃X���b�h�w��**
   - `ContinueWith`�́A�f�t�H���g�ł̓o�b�N�O���E���h�X���b�h�ő��s����܂��B
   - `TaskScheduler.FromCurrentSynchronizationContext()`���w�肷�邱�ƂŁAUI�X���b�h�Ŏ��s�����邱�Ƃ��ł��܂��B

2. **��O����**
   - �^�X�N���ŗ�O�����������`Task.IsFaulted`��`true`�ɂȂ�܂��B
   - `ContinueWith`�ŗ�O���������ł��܂��B
   - `task.Exception`�����O�̏ڍ׏����擾�ł��܂��B

3. **�f�t�H���g�̃X���b�h����**
   - `Task.Run`: �o�b�N�O���E���h�X���b�h�œ���B
   - `ContinueWith`: �X���b�h�X�P�W���[���[���w�肵�Ȃ�����A�o�b�N�O���E���h�X���b�h�Ŏ��s�B

---

### **�܂Ƃ�**

- **`ContinueWith`�̃X���b�h����**:
  - �f�t�H���g�ł́A�񓯊�����������Ƀo�b�N�O���E���h�X���b�h�ő��s�B
  - �����I�ɃX�P�W���[���[�i��: `TaskScheduler.FromCurrentSynchronizationContext`�j���w�肷�邱�ƂŁA����̃X���b�h�iUI�X���b�h�Ȃǁj�ő��s�\�B

- **`Task`���̗�O**:
  - ����������O��`task.Exception`�Ɋi�[�����B
  - `ContinueWith`�𗘗p����UI�X���b�h�ŗ�O�������s�����Ƃ��\�B

���̃T���v���ɂ��A`ContinueWith`�̃X���b�h�������O�����𒼊��I�ɗ����ł���ł��傤�I
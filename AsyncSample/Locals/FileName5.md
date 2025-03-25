`TaskFactory.StartNew`���g�p�����ꍇ�̃X���b�h�����́A`Task.Run`�Ǝ��Ă��܂����A����̃J�X�^�}�C�Y���s�������ꍇ�Ɏg�p����܂��B���̋��������n��Ŏ����A`await`��`Wait`�Ƃ̈Ⴂ��������܂��B

---

### **1. `TaskFactory.StartNew`�̓���**

- **�o�b�N�O���E���h�X���b�h**�Ń^�X�N�����s���܂��B
- �J�X�^�}�C�Y�\�ȃI�v�V�����i��: �X�P�W���[�����O�A�e�q�^�X�N�̊֌W�j���w��ł��܂��B
- **�񐄏��̏ꍇ������**:
  - �ʏ�̔񓯊������ɂ�`Task.Run`����������܂��B
  - ���x�Ȑ��䂪�K�v�ȏꍇ�̂�`TaskFactory.StartNew`�𗘗p���܂��B

---

### **2. �X���b�h�̋���**

#### **���n��}**

```plaintext
UI�X���b�h              |        �o�b�N�O���E���h�X���b�h
-----------------------|---------------------------------
�����J�n               | 
   ��                   | 
   ��TaskFactory.StartNew |
UI�X���b�h���          | ----> �^�X�N�J�n
                       |       �i��: �v�Z������I/O����j
                       | ----> �^�X�N���s��
                       | ----> �^�X�N����
�����ĊJ�i�߂�Ȃ��ꍇ�j| 
```

#### **`await`���g�����ꍇ**
- `TaskFactory.StartNew`�̃^�X�N��������A`await`�ŃX���b�h�؂�ւ����������܂��B
- �f�t�H���g�ł͌Ăяo�����X���b�h�iUI�X���b�h�j�ɖ߂�܂��B

---

### **3. `TaskFactory.StartNew`�̃R�[�h��Ɠ���**

#### **(1) ��{�I�Ȏg�p��**

```csharp
private async void btnStartNew_Click(object sender, EventArgs e)
{
    Log("btnStartNew_Click (Start)");

    // �o�b�N�O���E���h�Ń^�X�N���J�n
    await Task.Factory.StartNew(() =>
    {
        Log("TaskFactory.StartNew (Background Thread)");
        Thread.Sleep(1000); // 1�b�ԑҋ@
    });

    Log("btnStartNew_Click (End)");
}

private void Log(string message)
{
    string log = $"{DateTime.Now:HH:mm:ss.fff} [Thread: {Thread.CurrentThread.ManagedThreadId}] {message}";
    Console.WriteLine(log);
    listBox1.Items.Add(log);
}
```

**���s���ʁiUI�X���b�h�ɖ߂�ꍇ�j**:
```plaintext
09:00:00.000 [Thread: 1] btnStartNew_Click (Start)
09:00:00.005 [Thread: 4] TaskFactory.StartNew (Background Thread)
09:00:01.010 [Thread: 1] btnStartNew_Click (End)
```

---

#### **(2) `ConfigureAwait(false)`���g�p**

```csharp
private async void btnStartNewWithConfigureAwait_Click(object sender, EventArgs e)
{
    Log("btnStartNewWithConfigureAwait_Click (Start)");

    await Task.Factory.StartNew(() =>
    {
        Log("TaskFactory.StartNew (Background Thread)");
        Thread.Sleep(1000);
    }).ConfigureAwait(false);

    Log("btnStartNewWithConfigureAwait_Click (End - Not UI Thread)");
}
```

**���s���ʁiUI�X���b�h�ɖ߂�Ȃ��ꍇ�j**:
```plaintext
09:00:00.000 [Thread: 1] btnStartNewWithConfigureAwait_Click (Start)
09:00:00.005 [Thread: 4] TaskFactory.StartNew (Background Thread)
09:00:01.010 [Thread: 4] btnStartNewWithConfigureAwait_Click (End - Not UI Thread)
```

---

#### **(3) `Wait`���g�p**

```csharp
private void btnStartNewWait_Click(object sender, EventArgs e)
{
    Log("btnStartNewWait_Click (Start)");

    Task.Factory.StartNew(() =>
    {
        Log("TaskFactory.StartNew (Background Thread)");
        Thread.Sleep(1000);
    }).Wait(); // �����I�ɑҋ@

    Log("btnStartNewWait_Click (End)");
}
```

**���s���ʁiUI�X���b�h���u���b�N�����j**:
```plaintext
09:00:00.000 [Thread: 1] btnStartNewWait_Click (Start)
09:00:00.005 [Thread: 4] TaskFactory.StartNew (Background Thread)
09:00:01.010 [Thread: 1] btnStartNewWait_Click (End)
```

---

### **4. `TaskFactory.StartNew`�̈Ⴂ�ƒ��ӓ_**

#### **(1) `Task.Run`�Ƃ̈Ⴂ**
- `Task.Run`��`TaskFactory.StartNew`�̊ȗ������ꂽ���@�ŁA���������W���I�Ȕ񓯊������̕��@�ł��B
- `TaskFactory.StartNew`�́A�X�P�W���[�����O�I�v�V������e�q�^�X�N�𖾎��I�ɐݒ肵�����ꍇ�Ɏg�p���܂��B

#### **(2) UI�X���b�h�ɖ߂�ꍇ�̉e��**
- �f�t�H���g�ł́A`await`���g�p�����UI�X���b�h�ɖ߂�܂��B
- UI�X���b�h�ɖ߂邱�ƂŁAUI�v�f�����S�ɑ���ł��܂��B

#### **(3) UI�X���b�h�ɖ߂�Ȃ��ꍇ�̗��_**
- `ConfigureAwait(false)`���g�p����ƃX���b�h�؂�ւ����������A�p�t�H�[�}���X�����サ�܂��B
- �������AUI���삪�K�v�ȏꍇ�͗�O���������܂��B

#### **(4) �����I�ɑҋ@����ꍇ�̃��X�N**
- `Wait`���g�p����ƁAUI�X���b�h���u���b�N����A�A�v���P�[�V�����̉��������ቺ���܂��B
- ����Windows Forms��WPF�ł͔�����ׂ��ł��B

---

### **5. �X���b�h�����̔�r�\**

| �@�\                          | `Task.Run`                     | `TaskFactory.StartNew`         | `TaskFactory.StartNew` + `Wait` |
|-------------------------------|--------------------------------|--------------------------------|---------------------------------|
| **�o�b�N�O���E���h�X���b�h**   | �g�p                          | �g�p                          | �g�p                           |
| **�Ăяo�����X���b�h�ɖ߂�**   | `await`�Ŗ߂�                | `await`�Ŗ߂�                | �����I�ɏ���                   |
| **�J�X�^�}�C�Y�\��**         | �Ⴂ                          | �����i�X�P�W���[�����O�Ȃǁj  | ����                           |
| **UI�X���b�h�̉��**           | �񓯊����������              | �񓯊����������              | �u���b�N�����                 |

---

### **6. �܂Ƃ�**

- **`Task.Run`��D��**:
  - �ʏ�̔񓯊������ɂ�`Task.Run`���g�p�B
  - �V���v���Ő����������@�B

- **`TaskFactory.StartNew`�̗p�r**:
  - ���x�ȃX�P�W���[�����O��^�X�N�K�w�\����K�v�Ƃ���ꍇ�Ɍ��肵�Ďg�p�B

- **UI�X���b�h�̈��S��**:
  - `await`���g�p���AUI�X���b�h�ł̏��������S�ɍs���B
  - UI���삪�s�v�ȏꍇ��`ConfigureAwait(false)`�����p���Č������B

- **�����I�ȑҋ@�͔�����**:
  - `Wait`�͉�������ቺ�����邽�߁A����UI�A�v���P�[�V�����ł͎g�p�������B
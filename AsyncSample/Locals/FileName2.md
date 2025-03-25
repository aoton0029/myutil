`await`����**�Ăяo�����X���b�h**��**���s�X���b�h**�̋����́AC#�̔񓯊��v���O���~���O�̏d�v�ȈႢ�𗝉����錮�ƂȂ�܂��B����ŁA`Wait`���g�p�����ꍇ�A�����I�ȓ���ɂȂ邽�ߋ������قȂ�܂��B�ȉ��ɗ��҂̈Ⴂ���ڂ���������܂��B

---

## **1. `await`�̋���**

### **�Ăяo�����X���b�h�̋���**
- **�񓯊��������J�n�����Ƃ�**�A`await`�ɂ��Ăяo�����̃X���b�h�iUI�X���b�h�Ȃǁj�͉������܂��B
- ������ꂽ�X���b�h�͑��̃^�X�N�iUI�̍X�V��ʂ̏����j�����s�\�ɂȂ�܂��B

### **���s�X���b�h�̋���**
- �񓯊��������̂͒ʏ�A�X���b�h�v�[���i�o�b�N�O���E���h�X���b�h�j�Ŏ��s����܂��B
- �񓯊���������������ƁA`SynchronizationContext`�Ɋ�Â��������ĊJ����܂��B
  - **UI�A�v���P�[�V�����̏ꍇ**: UI�X���b�h�i���̌Ăяo�����X���b�h�j�ōĊJ�B
  - **�R���\�[���A�v����`ConfigureAwait(false)`�̏ꍇ**: �C�ӂ̃X���b�h�ōĊJ�����\��������܂��B

### **��̓I�ȗ�**
```csharp
private async Task DoAsyncWork()
{
    Console.WriteLine($"Before await: {Thread.CurrentThread.ManagedThreadId}");
    
    // �񓯊�����
    await Task.Delay(1000);
    
    Console.WriteLine($"After await: {Thread.CurrentThread.ManagedThreadId}");
}
```

**�����̃|�C���g**
1. **`Before await`**
   - �Ăяo�����̃X���b�h�i�Ⴆ��UI�X���b�h�j��ID���o�͂���܂��B
2. **`await`��**
   - �Ăяo�����X���b�h�͉������A�񓯊������͕ʃX���b�h�Ői�s���܂��B
3. **`After await`**
   - **UI�A�v���P�[�V�����̏ꍇ**: �����͍Ă�UI�X���b�h�ōĊJ����܂��B
   - **��UI�A�v���P�[�V�����܂���`ConfigureAwait(false)`���g�p�����ꍇ**: �C�ӂ̃X���b�h�ōĊJ����邱�Ƃ�����܂��B

---

## **2. `Wait`�̋���**

### **�Ăяo�����X���b�h�̋���**
- `Task.Wait()`��`Task.Result`�͓����I�ɔ񓯊������̊�����ҋ@���܂��B
- �Ăяo�����X���b�h�i�Ⴆ��UI�X���b�h�j�̓u���b�N����A���̏����i����UI�̍X�V�Ȃǁj�����s�ł��Ȃ��Ȃ�܂��B

### **���s�X���b�h�̋���**
- �񓯊��������̂̓o�b�N�O���E���h�X���b�h�Ŏ��s����܂��B
- **�������AUI�X���b�h���u���b�N����Ă���ԂɁA�񓯊�������UI�X���b�h�ɖ߂�K�v������ꍇ�i��: `await`���g�p���������j**�A�f�b�h���b�N����������\��������܂��B

### **��̓I�ȗ�**
```csharp
private void DoSyncWork()
{
    Console.WriteLine($"Before Wait: {Thread.CurrentThread.ManagedThreadId}");
    
    // �񓯊������𓯊��I�ɑҋ@
    Task.Delay(1000).Wait();
    
    Console.WriteLine($"After Wait: {Thread.CurrentThread.ManagedThreadId}");
}
```

**�����̃|�C���g**
1. **`Before Wait`**
   - �Ăяo�����X���b�h�i�Ⴆ��UI�X���b�h�j��ID���o�͂���܂��B
2. **`Wait`��**
   - �Ăяo�����X���b�h�̓u���b�N����A�񓯊���������������܂ŉ����ł��܂���B
3. **`After Wait`**
   - �����������I�ɍĊJ����܂��B�����X���b�h�Ŏ��s����܂����AUI�X���b�h���������Ă��Ȃ����߁A����UI���삪�u���b�N����܂��B

---

## **3. `await`��`Wait`�̋����̈Ⴂ**

| ����                              | `await`                                    | `Wait`                                     |
|-----------------------------------|-------------------------------------------|-------------------------------------------|
| **�Ăяo�����X���b�h�̉��**       | ��������i�񓯊��������͑��̍�Ƃ��\�j  | �u���b�N�����i���̍�Ƃ����s�s�\�j       |
| **���s�X���b�h**                  | �񓯊�����������ɃX���b�h�v�[���܂��͌��̃X���b�h�ōĊJ | �����X���b�h�ŏ������ĊJ�����             |
| **�f�b�h���b�N�̃��X�N**           | �Ⴂ�i`ConfigureAwait(false)`�ł���ɒጸ�j| �����i����UI�X���b�h�Ŏg�p����Ɩ�肪�����j|
| **�p�t�H�[�}���X**                | �����i�X���b�h�̌����I�ȗ��p�j             | �Ⴂ�i�X���b�h���u���b�N�����j           |
| **�p�r**                          | �񓯊��v���O���~���O�Ő���                 | �����I�ɑҋ@���K�v�ȓ���P�[�X�Ŏg�p       |

---

## **4. �f�b�h���b�N�̗�**

**`Wait`�ɂ��f�b�h���b�N�̗�**:
```csharp
private void DeadlockExample()
{
    // UI�X���b�h�ł��̃R�[�h�����s����ƃf�b�h���b�N������
    Task.Run(async () =>
    {
        await Task.Delay(1000); // �񓯊�����
    }).Wait(); // �����I�ɑҋ@
}
```

- **���̌���**:
  - `Task.Run`����`await`���񓯊�������ҋ@���A�������UI�X���b�h�ɖ߂낤�Ƃ��܂��B
  - �������A`Wait`��UI�X���b�h���u���b�N���Ă��邽�߁A�������i�܂��f�b�h���b�N���������܂��B

**������**:
- `await`���g���񓯊��v���O���~���O��D�悷��B
- �K�v�Ȃ�`ConfigureAwait(false)`��UI�X���b�h�ւ̖߂������B

---

## **�܂Ƃ�**

1. **`await`**
   - �Ăяo�����X���b�h�͉�������B
   - �񓯊�����������̓f�t�H���g�Ō��̃X���b�h�i�Ⴆ��UI�X���b�h�j�ōĊJ�B
   - �X���b�h�������I�ɗ��p���AUI�X���b�h�̃u���b�N��h���B

2. **`Wait`**
   - �Ăяo�����X���b�h���u���b�N����B
   - �����I�ȏ������K�v�ȏꍇ�Ɍ��肵�Ďg�p�B
   - UI�X���b�h�ł̎g�p�̓f�b�h���b�N�̌����ɂȂ邽�ߔ�����B

��ʓI�ɂ́A`await`���g�����񓯊��v���O���~���O���̗p���邱�Ƃ���������܂��B
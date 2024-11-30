`ValueTask`��C#�Ŕ񓯊��v���O���~���O�ɗp������\���̂ŁA���Ɍy�ʂȔ񓯊�����������悭�������߂Ɏg���܂��B`Task`�ɔ�ׂă������̊��蓖�Ă����点��\��������A�p�t�H�[�}���X���d�v�ȏ󋵂ŗL���ł��B�ȉ���`ValueTask`�̊�{�I�Ȏg������������܂��B

### `ValueTask`�̊T�v
- `ValueTask`�́A�y�ʂȔ񓯊��������s�����߂̃f�[�^�^�ŁA`Task`�Ǝ��Ă��܂����A��ɏ��K�͂Ȕ񓯊������Ɏg�p����܂��B
- `Task`�͈�x����������Ԃɂł��܂��񂪁A`ValueTask`�͎g���񂵉\�ŁA�����I�Ɋ����������ʂ�A�񓯊��Ŋ������鑀��̗����ɑΉ��ł��܂��B
- `ValueTask`�̗��_�́A�g�����ƂŃ������̊��蓖�Ă�}���邱�Ƃ��ł���_�ł��B`Task`���쐬����ƃ������̃q�[�v�Ɋ��蓖�Ă��������܂����A`ValueTask`�ł͂��̊��蓖�Ă�����邱�Ƃ��ł���ꍇ������܂��B

### �g�����̗�

#### ��{�I�Ȏg�p��
���ɁA`ValueTask`��Ԃ��ȒP�ȃ��\�b�h�̗�������܂��B

```csharp
using System;
using System.Threading.Tasks;

public class Example
{
    public async ValueTask<int> GetNumberAsync()
    {
        await Task.Delay(1000); // �񓯊������̃V�~�����[�V����
        return 42; // �������ʂ�Ԃ�
    }

    public async Task RunExample()
    {
        int result = await GetNumberAsync();
        Console.WriteLine(result);
    }
}
```

��L�̗��`GetNumberAsync`���\�b�h�́A�񓯊��Ő�����Ԃ��܂��B���̏ꍇ�A`ValueTask<int>`��`Task<int>`�Ǝ������������܂����A�����������̌��オ���҂ł��܂��B

#### �����I�Ɋ�������ꍇ
`ValueTask`�̃����b�g�́A�����������I�Ɋ�������ꍇ�ɓ��Ɍ����ł��B�Ⴆ�΁A����̏����ŏ����������Ɋ�������悤�ȏꍇ�A`ValueTask`�𗘗p���邱�ƂŃ��������蓖�Ă�����邱�Ƃ��ł��܂��B

```csharp
public ValueTask<int> GetNumberConditionallyAsync(bool immediate)
{
    if (immediate)
    {
        // �����������Ɋ�������ꍇ
        return new ValueTask<int>(42);
    }
    else
    {
        // �񓯊��Ɋ�������ꍇ
        return new ValueTask<int>(Task.Delay(1000).ContinueWith(_ => 42));
    }
}
```

��L�̃R�[�h�ł́A`immediate`��`true`�̏ꍇ�ɂ�`ValueTask`�������Ɍ��ʂ�Ԃ��悤�ɂȂ��Ă���A�񓯊��Ɋ�������K�v������܂���B

### ���ӓ_
`ValueTask`�̎g�p�ɂ͂��������ӂ��K�v�ł��B

1. **��x����await���邱��**�F
   `ValueTask`�͍�await���Ȃ��ł��������B`ValueTask`��await����͈̂�x�����ɂ��ׂ��ł��B����`ValueTask`�C���X�^���X����await����ƁA���삪�ۏ؂���Ȃ����߁A���ݓI�Ȗ�肪��������\��������܂��B

2. **IValueTaskSource�̓K�؂Ȏg����**�F
   `ValueTask`�͎g���񂳂��ꍇ�����邽�߁A���\�b�h�̌Ăяo����ɈقȂ�^�C�~���O�ōė��p����邱�Ƃ�z�肵���݌v���K�v�ł��B����ɂ��A�\�����Ȃ��������������邱�Ƃ�h���܂��B

3. **�p�t�H�[�}���X�ւ̈ӎ�**�F
   `ValueTask`�́A�y�ʂȑ���i��F�L���b�V�����ꂽ�f�[�^�̎擾�Ȃǁj�ɓK���Ă��܂��B�d��������A���т���await���K�v�ɂȂ�ꍇ�ɂ́A�ʏ��`Task`�̂ق��������₷���ł��B

### `Task` vs `ValueTask`
- **`Task`**�F�ʏ�͔񓯊�������`Task`���g���܂��B�V���v���ŁA�����̔񓯊�API�ŕW���I�Ɏg���Ă���A�G���[�n���h�����O���e�Ղł��B
- **`ValueTask`**�F`Task`���g�����Ƃɂ�郁�������蓖�Ă��{�g���l�b�N�ƂȂ�悤�ȁA�y�ʂ��p�ɂɌĂяo����鑀���`ValueTask`���g�p���܂��B

�Ⴆ�΁A�ȉ��̂悤�ȃV�i���I��`ValueTask`���g�p���邱�Ƃ��L���ł��B
- ���������蓖�Ă��R�X�g�ƂȂ�悤�ȕp�ɂȌĂяo���̃��\�b�h�B
- �f�[�^���L���b�V������Ă���A�قƂ�ǂ̏ꍇ�����I�ɒl��Ԃ��K�v������ꍇ�B

### �܂Ƃ�
- `ValueTask`��`Task`�ɔ�ׂČy�ʂł���A���ɓ����I�Ɋ�������\���̂���񓯊������ɗL���ł��B
- `ValueTask`�́A1�񂾂�await����邱�Ƃ�z�肵�Ă���A�K�؂Ɏg�p���Ȃ��Ɛ��ݓI�ȃo�O�̌����ɂȂ�܂��B
- ��ʓI�ɂ�`Task`���񓯊������̕W���ł����A����̏�������`ValueTask`���g�����ƂŃp�t�H�[�}���X�����シ��ꍇ������܂��B

���̂悤�ɁA�K�؂ȃV�`���G�[�V�����Ŏg�������邱�Ƃ��|�C���g�ł��B
- �Ȍ��Ōy�ʂȑ���F`ValueTask`
- �d�������╡�G�Ȕ񓯊��t���[�F`Task`
https://goatreview.com/masstransit-inmemory-mediator-basics/

MassTransit�́A.NET�����̃I�[�v���\�[�X�̕��U�A�v���P�[�V�����t���[�����[�N�ł���A�T�[�r�X�Ԃ̒ʐM��e�Ղɂ��A�X�P�[���u���Ō��S�ȃA�v���P�[�V�����̍\�z���x�����܂��B���̒��ł��A���f�B�G�[�^�[�iMediator�j�p�^�[���̎����́A�R���|�[�l���g�Ԃ̒ʐM���ȑf�����A�a�������ێ����邽�߂̏d�v�ȋ@�\�ł��B

**���f�B�G�[�^�[�iMediator�j�p�^�[���Ƃ́H**

�I�u�W�F�N�g�w���v���O���~���O�ɂ����郁�f�B�G�[�^�[�iMediator�j�p�^�[���́A�I�u�W�F�N�g�Ԃ̑��ݍ�p���J�v�Z�������A���ړI�Ȉˑ��֌W�����炷�݌v�p�^�[���ł��BMassTransit�͂��̃p�^�[�������p���A�v���Z�X�����b�Z�[�W���O��e�Ղɂ��A�A�v���P�[�V�����݌v�̉��P��}���Ă��܂��B

**IMediator���g�p�������b�Z�[�W�̑��M**

MassTransit��`IMediator`���g�p����ƁA����v���Z�X���Ń��b�Z�[�W�𑗐M���A���������邱�Ƃ��ł��܂��B����́A`Send`��`Publish`���\�b�h���v���Z�X�ԒʐM�Ɏg�p�����̂Ƃ͈قȂ�A�v���Z�X���ʐM�ɓ������Ă��܂��B�ȉ���`IMediator`���g�p�������b�Z�[�W���M�̗�������܂��B

```csharp
public static async Task<IResult> SubmitGoat(Guid id, IMediator mediator, CancellationToken cancellationToken)
{
    var response = await mediator.SendRequest<GoatSubmitted, GoatAccepted>(new GoatSubmitted(id), cancellationToken);
    // ���X�|���X�̏����A��: return Results.Ok(response);
}
```

`SendRequest`���\�b�h�́A���N�G�X�g�𑗐M���A���X�|���X��ҋ@���܂��B���̍ہA���M���郁�b�Z�[�W�̌^�Ǝ�M���郁�b�Z�[�W�̌^���w�肵�܂��B�d�v�Ȃ̂́A�R���V���[�}�[���Ԃ��^�����f�B�G�[�^�[�Ŋ��҂����^�ƈ�v���Ă���K�v������_�ł��B��v���Ȃ��ꍇ�AMassTransit�̓����N���m���ł����A�G���[�������N�����܂��B

**IConsumer���g�p�������b�Z�[�W�̏���**

`IConsumer`�C���^�[�t�F�[�X�́A����̌^�̃��b�Z�[�W�������N���X��\���܂��B�{���I�ɁA`IConsumer`�̓��b�Z�[�W�n���h���[�ł���A���b�Z�[�W�o�X���烁�b�Z�[�W����M���A������������܂��B�ȉ���`IConsumer`�̎�����������܂��B

```csharp
public class GoatSubmittedConsumer : IConsumer<GoatSubmitted>
{
    public async Task Consume(ConsumeContext<GoatSubmitted> context)
    {
        var goat = context.Message;
        // ���b�Z�[�W�̏������W�b�N�������ɒǉ�
        await context.RespondAsync<GoatAccepted>(new GoatAccepted());
    }
}
```

`ConsumeContext<T>`�́A�R���V���[�}�[��`Consume`���\�b�h���Ō��݂̃��b�Z�[�W���������邽�߂̑����̗L�p�ȏ��ƃ��[�e�B���e�B��񋟂��܂��B��Ȃ��̂Ƃ��āF

1. **Message**: ���M���ꂽ���b�Z�[�W�̃v���p�e�B�ɃA�N�Z�X���܂��B�^��`T`�ł��B�ʏ�A����̓��N�G�X�g���������邽�߂ɕK�v�ȏ�������`record`�ł��B

2. **CancellationToken**: �񓯊��֐��S�̂ŒP��̃g�[�N����]������ۂɔ��ɗL�p�ł��B����̓��\�b�h�����łȂ��A���������̃��b�Z�[�W�S�̂̊Ǘ��ɑ����܂��B����Entity Framework Core���g�p���Ă���ꍇ�A�񓯊��Ăяo���ł��̃g�[�N����]������̂��f�t�H���g�ł��B

3. **RespondAsync**: ���f�B�G�[�^�[ �p�^�[���ł́A���M�҂Ƀ��b�Z�[�W��Ԃ��܂��B����́A���̃p�^�[���ŕK�v�Ƃ���鐳�m�ȓ���ł��B���N�G�X�g�̌��ʂ́A���̃��\�b�h����ĕʂ�`record`�Ƃ��đ��M����܂��B

�S�̓I�Ɍ��āA`ConsumeContext<T>`�́A���b�Z�[�W�̑���M���@�̐���A�G���[�����A���g���C�|���V�[�̐���A�����Ԏ��s������b�̊Ǘ��ȂǁA�R���V���[�}�[�ł̃��b�Z�[�W�����Ɋւ��镝�L���@�\��񋟂��܂��B

**ServiceProvider�ւ̃R���V���[�}�[�̓o�^**

�R���V���[�}�[���g�p����ɂ́AMassTransit���񋟂���`AddMediator`�g�����\�b�h����āA�ˑ��������ɓo�^���܂��B�ȉ��ɂ��̗�������܂��B

```csharp
services.AddMediator(x =>
{
    // �ʂɃR���V���[�}�[��o�^
    x.AddConsumer<GoatSubmittedConsumer>();

    // ����̖��O��ԓ��̂��ׂẴR���V���[�}�[��o�^
    x.AddConsumersFromNamespaceContaining(typeof(GoatSubmittedConsumer));

    // ����̃A�Z���u�����̂��ׂẴR���V���[�}�[��o�^
    x.AddConsumers(typeof(GoatSubmittedConsumer).Assembly);
});
```

���̂悤�ɂ��āA`IMediator`�̎������ˑ��������R���e�i�ɓo�^����A�A�v���P�[�V�������ŗ��p�\�ɂȂ�܂��B

**�܂Ƃ�**

MassTransit�̃��f�B�G�[�^�[ �p�^�[���́A.NET�A�v���P�[�V�������ł̃v���Z�X�����b�Z�[�W���O���ȑf�����A�R���|�[�l���g�Ԃ̑a�����𑣐i���܂��B`IMediator`��`IConsumer`�̑g�ݍ��킹�ɂ��A�J�� 
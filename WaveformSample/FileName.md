�ݒ�l���Ǘ����邽�߂̃N���X���쐬���܂��B  
���̃N���X�́A�g�`�̎�ށA�U���A���g���A�T���v�����[�g�A�V�[�P���X�̒����Ȃǂ̐ݒ���ꌳ�Ǘ����܂��B  
�܂��AJSON �Őݒ��ۑ�/�ǂݍ��݂ł���@�\���ǉ����܂��B

---

## **1. �݌v���j**
- **`WaveformSettings`** �N���X���쐬���A�g�`�̊e��p�����[�^���Ǘ��B
- **JSON�Őݒ��ۑ��E�ǂݍ���** �ł���悤�ɂ���B
- **�g�����₷���݌v**�i�����I��GUI�A�g��f�o�C�X�ݒ�ɂ��Ή��j�B

---

## **2. ����**

### **(1) �ݒ�l�N���X**
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class WaveformSettings
{
    public int SampleRate { get; set; } = 10000;  // �T���v�����[�g (Hz)
    public List<WaveformConfig> Waveforms { get; set; } = new List<WaveformConfig>();

    // �ݒ��JSON�t�@�C���ɕۑ�
    public void SaveToFile(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(filePath, json);
    }

    // �ݒ��JSON�t�@�C������ǂݍ���
    public static WaveformSettings LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("�ݒ�t�@�C����������܂���B");
        }

        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<WaveformSettings>(json);
    }
}

// �g�`�̐ݒ���
public class WaveformConfig
{
    public string Type { get; set; } = "Sine"; // "Sine", "Square", "Triangle", "Noise"
    public double Frequency { get; set; } = 50.0;  // ���g�� (Hz)
    public double Amplitude { get; set; } = 1.0;   // �U��
    public double Duration { get; set; } = 1.0;    // �p������ (�b)
}
```

---

### **(2) �ݒ�̕ۑ��Ɠǂݍ���**
```csharp
public class Program
{
    static void Main()
    {
        // �ݒ�l���쐬
        WaveformSettings settings = new WaveformSettings
        {
            SampleRate = 20000, // 20 kHz
            Waveforms = new List<WaveformConfig>
            {
                new WaveformConfig { Type = "Sine", Frequency = 100, Amplitude = 1.0, Duration = 0.5 },
                new WaveformConfig { Type = "Square", Frequency = 50, Amplitude = 0.8, Duration = 1.0 }
            }
        };

        // �ݒ��ۑ�
        string filePath = "waveform_settings.json";
        settings.SaveToFile(filePath);

        // �ݒ��ǂݍ���
        WaveformSettings loadedSettings = WaveformSettings.LoadFromFile(filePath);

        // �ǂݍ��񂾐ݒ��\��
        Console.WriteLine($"SampleRate: {loadedSettings.SampleRate} Hz");
        foreach (var wave in loadedSettings.Waveforms)
        {
            Console.WriteLine($"Type: {wave.Type}, Frequency: {wave.Frequency} Hz, Amplitude: {wave.Amplitude}, Duration: {wave.Duration} s");
        }
    }
}
```

---

## **3. JSON �ݒ�t�@�C���̗�**
��L�̃v���O���������s����ƁA�ȉ��̂悤��JSON�t�@�C�����쐬����܂��B

```json
{
  "SampleRate": 20000,
  "Waveforms": [
    {
      "Type": "Sine",
      "Frequency": 100.0,
      "Amplitude": 1.0,
      "Duration": 0.5
    },
    {
      "Type": "Square",
      "Frequency": 50.0,
      "Amplitude": 0.8,
      "Duration": 1.0
    }
  ]
}
```

---

## **4. �܂Ƃ�**
- **`WaveformSettings` �N���X** �̓V�[�P���T�̐ݒ���ꌳ�Ǘ�����B
- **JSON�̕ۑ��E�ǂݍ��݋@�\** ��񋟂��A�_��ɐݒ��ύX�ł���B
- �����I�� **GUI�ƘA�g** ���ă��[�U�[���ݒ��ύX�ł���悤�ɂ���̂��ȒP�B

���̐ݒ�N���X�� `WaveformSequence` �ɓ�������΁AJSON �����[�h���邾���Ŕg�`�V�[�P���X������悤�ɂȂ�܂��I


C# �œd���g�`�𐶐����A������q�����킹��V�[�P���T��݌v����ꍇ�A�ȉ��̃A�[�L�e�N�`�����l�����܂��B

---

## **1. �V�X�e���̊T�v**
���̃V�[�P���T�́A�����̓d���g�`�i��: �����g�A��`�g�A�O�p�g�A�����_���g�`�Ȃǁj�𐶐����A�������V�[�P���X�ɏ]���Čq�����킹�A�ŏI�I�Ȕg�`���o�͂���V�X�e���ł��B

- **�g�`�������W���[��**: �قȂ��ނ̔g�`�𐶐�
- **�V�[�P���T���W���[��**: �g�`���w�肳�ꂽ�����Ōq�����킹��
- **�o�̓��W���[��**: �V�[�P���X���ꂽ�g�`���t�@�C���ۑ��܂��̓n�[�h�E�F�A�o��
- **GUI/�ݒ�Ǘ�**: ���[�U�[���g�`�V�[�P���X��ݒ�

---

## **2. �A�[�L�e�N�`���݌v**
### **(1) �S�̍\��**
��v�ȃR���|�[�l���g�͈ȉ��̒ʂ�:

1. **`IWaveform`�i�C���^�[�t�F�[�X�j**  
   - �g�`�̊�{�I�ȐU�镑�����`�i�U���A���g���A�T���v�����Ȃǁj
  
2. **`WaveformGenerator`�i�g�`�������W���[���j**  
   - **�T�C���g (`SineWave`)�A��`�g (`SquareWave`)�A�O�p�g (`TriangleWave`)�A�m�C�Y (`NoiseWave`)** �Ȃǂ̎����N���X

3. **`WaveformSequence`�i�V�[�P���T���W���[���j**  
   - �����̔g�`�����Ԃɑg�ݍ��킹�A�V�[�P���X�S�̂��Ǘ�

4. **`WaveformExporter`�i�o�̓��W���[���j**  
   - ���������g�`���t�@�C���o�́iCSV�AWAV�A�o�C�i���Ȃǁj�܂��̓��A���^�C���o��

5. **`WaveformController`�i�V�X�e�����䃂�W���[���j**  
   - GUI�܂��͐ݒ�t�@�C������Ĕg�`�ƃV�[�P���X���Ǘ�

---

## **3. C# �ł̎�����**

### **(1) `IWaveform` �C���^�[�t�F�[�X**
�e�g�`�̋��ʎd�l���`�B
```csharp
public interface IWaveform
{
    double[] Generate(int sampleRate, double duration);  // �g�`�𐶐�
}
```

### **(2) �g�`�����N���X**
#### **�T�C���g�̎���**
```csharp
public class SineWave : IWaveform
{
    public double Frequency { get; }
    public double Amplitude { get; }

    public SineWave(double frequency, double amplitude)
    {
        Frequency = frequency;
        Amplitude = amplitude;
    }

    public double[] Generate(int sampleRate, double duration)
    {
        int sampleCount = (int)(sampleRate * duration);
        double[] wave = new double[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            wave[i] = Amplitude * Math.Sin(2 * Math.PI * Frequency * i / sampleRate);
        }
        return wave;
    }
}
```

#### **��`�g�̎���**
```csharp
public class SquareWave : IWaveform
{
    public double Frequency { get; }
    public double Amplitude { get; }

    public SquareWave(double frequency, double amplitude)
    {
        Frequency = frequency;
        Amplitude = amplitude;
    }

    public double[] Generate(int sampleRate, double duration)
    {
        int sampleCount = (int)(sampleRate * duration);
        double[] wave = new double[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            wave[i] = (Math.Sin(2 * Math.PI * Frequency * i / sampleRate) >= 0) ? Amplitude : -Amplitude;
        }
        return wave;
    }
}
```

---

### **(3) `WaveformSequence`�i�V�[�P���T�j**
```csharp
public class WaveformSequence
{
    private List<IWaveform> _waveforms = new List<IWaveform>();
    private List<double> _durations = new List<double>();

    public void AddWaveform(IWaveform waveform, double duration)
    {
        _waveforms.Add(waveform);
        _durations.Add(duration);
    }

    public double[] Generate(int sampleRate)
    {
        List<double> fullSequence = new List<double>();

        for (int i = 0; i < _waveforms.Count; i++)
        {
            fullSequence.AddRange(_waveforms[i].Generate(sampleRate, _durations[i]));
        }

        return fullSequence.ToArray();
    }
}
```

---

### **(4) `WaveformExporter`�i�o�̓��W���[���j**
```csharp
public class WaveformExporter
{
    public static void ExportToCsv(string filePath, double[] data)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var value in data)
            {
                writer.WriteLine(value);
            }
        }
    }
}
```

---

### **(5) `WaveformController`�i�����Ǘ��j**
```csharp
public class WaveformController
{
    public void Run()
    {
        int sampleRate = 10000;  // �T���v�����[�g 10kHz

        // �g�`���쐬
        SineWave sine = new SineWave(50, 1.0);
        SquareWave square = new SquareWave(50, 1.0);

        // �V�[�P���X���쐬
        WaveformSequence sequence = new WaveformSequence();
        sequence.AddWaveform(sine, 0.1);  // 0.1�b�̃T�C���g
        sequence.AddWaveform(square, 0.1); // 0.1�b�̋�`�g

        // �g�`�𐶐�
        double[] waveform = sequence.Generate(sampleRate);

        // CSV �ɏo��
        WaveformExporter.ExportToCsv("output.csv", waveform);
    }
}
```

---

## **4. �g���|�C���g**
���̃A�[�L�e�N�`���𔭓W������ƁA�ȉ��̂悤�ȋ@�\��ǉ��ł��܂��B

- **GUI �g�ݍ���**: WinForms �� WPF �𗘗p���Ĕg�`�V�[�P���X���r�W���A���Ǘ�
- **���A���^�C���o��**: �T�E���h�J�[�h��O���f�o�C�X�iUSB-DAC�Ȃǁj�փ��A���^�C���ɏo��
- **�t�@�C�����͑Ή�**: �ݒ�t�@�C���iJSON, XML�j����V�[�P���X�����[�h
- **�t�B���^�K�p**: ����g/�����g�t�B���^�A�m�C�Y����
- **AI/�@�B�w�K�ƘA�g**: �V�[�P���X�̍œK����ُ팟�o

---

## **5. �܂Ƃ�**
���̐݌v�ł́A**�g�`�̐����A�V�[�P���X�̊Ǘ��A�t�@�C���o��** �Ƃ������������W���[�������A�g�����̍����A�[�L�e�N�`�����\�z���܂����B  
����ɂ��A�����I��**�g�`�̎�ނ𑝂₷�A�V�[�P���X��GUI�ŊǗ�����A���A���^�C���o�͂���** �Ȃǂ̔��W���e�ՂɂȂ�܂��B

���̃A�[�L�e�N�`�������ɁA����̓I�ȗv���ɉ������J�X�^�}�C�Y���s���܂��B
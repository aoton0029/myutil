設定値を管理するためのクラスを作成します。  
このクラスは、波形の種類、振幅、周波数、サンプルレート、シーケンスの長さなどの設定を一元管理します。  
また、JSON で設定を保存/読み込みできる機能も追加します。

---

## **1. 設計方針**
- **`WaveformSettings`** クラスを作成し、波形の各種パラメータを管理。
- **JSONで設定を保存・読み込み** できるようにする。
- **拡張しやすい設計**（将来的にGUI連携やデバイス設定にも対応）。

---

## **2. 実装**

### **(1) 設定値クラス**
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class WaveformSettings
{
    public int SampleRate { get; set; } = 10000;  // サンプルレート (Hz)
    public List<WaveformConfig> Waveforms { get; set; } = new List<WaveformConfig>();

    // 設定をJSONファイルに保存
    public void SaveToFile(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(filePath, json);
    }

    // 設定をJSONファイルから読み込み
    public static WaveformSettings LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("設定ファイルが見つかりません。");
        }

        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<WaveformSettings>(json);
    }
}

// 波形の設定情報
public class WaveformConfig
{
    public string Type { get; set; } = "Sine"; // "Sine", "Square", "Triangle", "Noise"
    public double Frequency { get; set; } = 50.0;  // 周波数 (Hz)
    public double Amplitude { get; set; } = 1.0;   // 振幅
    public double Duration { get; set; } = 1.0;    // 継続時間 (秒)
}
```

---

### **(2) 設定の保存と読み込み**
```csharp
public class Program
{
    static void Main()
    {
        // 設定値を作成
        WaveformSettings settings = new WaveformSettings
        {
            SampleRate = 20000, // 20 kHz
            Waveforms = new List<WaveformConfig>
            {
                new WaveformConfig { Type = "Sine", Frequency = 100, Amplitude = 1.0, Duration = 0.5 },
                new WaveformConfig { Type = "Square", Frequency = 50, Amplitude = 0.8, Duration = 1.0 }
            }
        };

        // 設定を保存
        string filePath = "waveform_settings.json";
        settings.SaveToFile(filePath);

        // 設定を読み込み
        WaveformSettings loadedSettings = WaveformSettings.LoadFromFile(filePath);

        // 読み込んだ設定を表示
        Console.WriteLine($"SampleRate: {loadedSettings.SampleRate} Hz");
        foreach (var wave in loadedSettings.Waveforms)
        {
            Console.WriteLine($"Type: {wave.Type}, Frequency: {wave.Frequency} Hz, Amplitude: {wave.Amplitude}, Duration: {wave.Duration} s");
        }
    }
}
```

---

## **3. JSON 設定ファイルの例**
上記のプログラムを実行すると、以下のようなJSONファイルが作成されます。

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

## **4. まとめ**
- **`WaveformSettings` クラス** はシーケンサの設定を一元管理する。
- **JSONの保存・読み込み機能** を提供し、柔軟に設定を変更できる。
- 将来的に **GUIと連携** してユーザーが設定を変更できるようにするのも簡単。

この設定クラスを `WaveformSequence` に統合すれば、JSON をロードするだけで波形シーケンスを作れるようになります！


C# で電圧波形を生成し、それを繋ぎ合わせるシーケンサを設計する場合、以下のアーキテクチャが考えられます。

---

## **1. システムの概要**
このシーケンサは、複数の電圧波形（例: 正弦波、矩形波、三角波、ランダム波形など）を生成し、それらをシーケンスに従って繋ぎ合わせ、最終的な波形を出力するシステムです。

- **波形生成モジュール**: 異なる種類の波形を生成
- **シーケンサモジュール**: 波形を指定された順序で繋ぎ合わせる
- **出力モジュール**: シーケンスされた波形をファイル保存またはハードウェア出力
- **GUI/設定管理**: ユーザーが波形シーケンスを設定

---

## **2. アーキテクチャ設計**
### **(1) 全体構成**
主要なコンポーネントは以下の通り:

1. **`IWaveform`（インターフェース）**  
   - 波形の基本的な振る舞いを定義（振幅、周波数、サンプル数など）
  
2. **`WaveformGenerator`（波形生成モジュール）**  
   - **サイン波 (`SineWave`)、矩形波 (`SquareWave`)、三角波 (`TriangleWave`)、ノイズ (`NoiseWave`)** などの実装クラス

3. **`WaveformSequence`（シーケンサモジュール）**  
   - 複数の波形を順番に組み合わせ、シーケンス全体を管理

4. **`WaveformExporter`（出力モジュール）**  
   - 生成した波形をファイル出力（CSV、WAV、バイナリなど）またはリアルタイム出力

5. **`WaveformController`（システム制御モジュール）**  
   - GUIまたは設定ファイルを介して波形とシーケンスを管理

---

## **3. C# での実装例**

### **(1) `IWaveform` インターフェース**
各波形の共通仕様を定義。
```csharp
public interface IWaveform
{
    double[] Generate(int sampleRate, double duration);  // 波形を生成
}
```

### **(2) 波形生成クラス**
#### **サイン波の実装**
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

#### **矩形波の実装**
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

### **(3) `WaveformSequence`（シーケンサ）**
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

### **(4) `WaveformExporter`（出力モジュール）**
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

### **(5) `WaveformController`（統括管理）**
```csharp
public class WaveformController
{
    public void Run()
    {
        int sampleRate = 10000;  // サンプルレート 10kHz

        // 波形を作成
        SineWave sine = new SineWave(50, 1.0);
        SquareWave square = new SquareWave(50, 1.0);

        // シーケンスを作成
        WaveformSequence sequence = new WaveformSequence();
        sequence.AddWaveform(sine, 0.1);  // 0.1秒のサイン波
        sequence.AddWaveform(square, 0.1); // 0.1秒の矩形波

        // 波形を生成
        double[] waveform = sequence.Generate(sampleRate);

        // CSV に出力
        WaveformExporter.ExportToCsv("output.csv", waveform);
    }
}
```

---

## **4. 拡張ポイント**
このアーキテクチャを発展させると、以下のような機能を追加できます。

- **GUI 組み込み**: WinForms や WPF を利用して波形シーケンスをビジュアル管理
- **リアルタイム出力**: サウンドカードや外部デバイス（USB-DACなど）へリアルタイムに出力
- **ファイル入力対応**: 設定ファイル（JSON, XML）からシーケンスをロード
- **フィルタ適用**: 低周波/高周波フィルタ、ノイズ除去
- **AI/機械学習と連携**: シーケンスの最適化や異常検出

---

## **5. まとめ**
この設計では、**波形の生成、シーケンスの管理、ファイル出力** という役割をモジュール化し、拡張性の高いアーキテクチャを構築しました。  
これにより、将来的に**波形の種類を増やす、シーケンスをGUIで管理する、リアルタイム出力する** などの発展が容易になります。

このアーキテクチャを元に、より具体的な要件に応じたカスタマイズを行えます。
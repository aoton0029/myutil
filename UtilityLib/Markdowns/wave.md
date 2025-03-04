HD画面 (1920x1080) レイアウト設計

画面構成

タイトルバー (上部)

画面タイトル: 「波形設定画面」

設定ボタン

保存ボタン

読み込みボタン


メインエリア (中央)

波形リスト (左側 / 300px幅)

起動時波形 10個

終了時波形 10個

選択すると詳細が表示される

並べ替え・削除機能あり


波形詳細エリア (右側 / 1620px幅)

波形名入力

設定情報

データ点数: 1〜1000

時間間隔: 1ms〜10ms


チャート (波形グラフ)

サイズ: 1200px × 400px

横軸: 時間(ms)

縦軸: 電圧割合 (%)


データグリッド

サイズ: 1200px × 200px

列: (Index, 時間(ms), 電圧割合(%))

手動入力・CSV読み込み可能


波形操作ボタン

データ追加

データ削除

データクリア

CSVエクスポート




ステータスバー (下部)

処理状況表示

エラーメッセージ表示

現在選択中の波形情報表示




---

画面レイアウト詳細

+-----------------------------------------------------+
|  波形設定画面                        [設定] [保存] [読込]  |
+-----------------------------------------------------+
| 波形リスト                  |   波形詳細エリア                         |
|  [波形1]                    |   名称: [__________]                     |
|  [波形2]                    |   データ点数: [ 100 ]                    |
|  ...                        |   時間間隔: [ 5ms ]                      |
|  [波形10]                   |                                         |
|  ----------------          |   [ 波形グラフ: 1200px x 400px ]         |
|  [終了波形1]               |   ┌─────────────────────────┐   |
|  [終了波形2]               |   │                             │   |
|  ...                        |   │  (電圧割合波形)             │   |
|  [終了波形10]              |   │                             │   |
|  ----------------          |   └─────────────────────────┘   |
|  [追加] [削除] [並べ替え]   |                                         |
|                             |   [データグリッド: 1200px x 200px]       |
|                             |   ┌──────────────┐                       |
|                             |   │ Index | 時間(ms) | 電圧(%) │         |
|                             |   │   1   |   0     |   50    │         |
|                             |   │   2   |   1     |   55    │         |
|                             |   │   ... |   ...   |   ...   │         |
|                             |   └──────────────┘                       |
|                             |   [ 追加 ] [ 削除 ] [ クリア ] [CSV出力] |
+-----------------------------------------------------+
| 状態: OK  | 選択中: 波形1 (100点)  | エラーなし |
+-----------------------------------------------------+


---

設計ポイント

1. 波形管理のしやすさ

起動波形と終了波形を明確に区別

一覧で選択して詳細を編集可能



2. データの視認性

20波形分の設定をスムーズに切り替え

チャートで波形の形状を一目で確認

グリッドで個別の値を直接編集可能



3. 編集の柔軟性

CSVインポート/エクスポートで外部データ管理可能

並べ替え、追加、削除機能で編集しやすい



4. レスポンシブ対応

横幅を最大限活用 (HD画面: 1920px)

フルHDモニターで快適に操作可能





拡張案

1. マルチウィンドウ対応

個別波形エディタ

波形ごとに新しいウィンドウを開いて詳細編集

波形エディタはドラッグ＆ドロップでリサイズ可能

メリット: 他の波形を参照しながら編集可能


比較ビュー

2つ以上の波形を並べて比較する専用ウィンドウ

重ね合わせモード（透過表示）や差分モードを追加




---

2. 高度な波形編集

補間機能

指定した範囲のデータをリニア補間、スプライン補間


波形計算

既存の波形をコピーして、新規波形に対して演算

例: 波形A + 波形B, 波形Aの50%減衰


FFT解析

周波数成分を解析し、フィルタリング適用




---

3. チャート強化

ズーム & スクロール

チャートの横軸・縦軸を自由にズーム

ピンチズーム or マウスホイール対応


カーソル追従

マウスホバーで時間と電圧の値をリアルタイム表示


リアルタイム更新

値をグリッドに入力すると即座に波形へ反映




---

4. プリセット管理

波形テンプレート

よく使う波形をプリセットとして保存・呼び出し

例: 正弦波、矩形波、三角波


バージョン管理

変更履歴を記録し、前のバージョンに戻せる




---

5. 外部連携

データ入出力

CSVだけでなく、Excel、JSON、SQLiteデータベースに対応


API連携

TCP通信でリアルデバイスとデータをやり取り

REST API経由で他のアプリとデータ交換




---

6. UIカスタマイズ

ダークモード対応

黒背景で目に優しいUI


カラーテーマ変更

ユーザーが好きな配色を選択可能


フォントサイズ変更

小さい文字、大きい文字を選択可能




---

7. シナリオ機能

波形シーケンス作成

複数の波形を時間順に組み合わせ、シナリオを作成

例: 「波形A → 5秒後に波形B → 10秒後に波形C」


ループ & 条件分岐

繰り返し再生や、条件を満たしたら別の波形に遷移


リアルタイム実行モード

シミュレーションしないが、一定時間で画面上の波形が進行するプレビュー機能




---

8. マルチデバイス対応

クラウド同期

設定データをAzureやCloudflareに保存して、複数PCで共有


モバイル版ビューア

Tauriアプリでモバイル端末でも波形を閲覧できる


タブレットUI最適化

タッチ操作用のボタン配置モード




---

拡張後の画面レイアウト (例)

+----------------------------------------------------------+
|  波形設定画面  [設定] [保存] [読込] [プリセット] [解析]  |
+----------------------------------------------------------+
| 波形リスト                   | 波形詳細エリア                          |
|  [波形1]                      |  名称: [__________]                     |
|  [波形2]                      |  データ点数: [ 100 ]                    |
|  ...                          |  時間間隔: [ 5ms ]                      |
|  [終了波形10]                 |                                          |
|  [追加] [削除] [並べ替え]      |  [ FFT解析 ] [ 補間 ] [ 波形計算 ]      |
|                                |  [ 波形グラフ ]                         |
|                                |  ┌──────────────────────────┐ |
|                                |  │                             │ |
|                                |  │ (ズーム対応電圧割合波形)    │ |
|                                |  │                             │ |
|                                |  └──────────────────────────┘ |
|                                |  [データグリッド]                        |
|                                |  ┌──────────────┐                      |
|                                |  │ Index | 時間(ms) | 電圧(%) │        |
|                                |  │   1   |   0     |   50    │        |
|                                |  │   2   |   1     |   55    │        |
|                                |  │   ... |   ...   |   ...   │        |
|                                |  └──────────────┘                      |
|                                |  [ 追加 ] [ 削除 ] [ クリア ] [CSV出力]|
+----------------------------------------------------------+
| 状態: OK  | 選択中: 波形1 (100点)  | FFT適用済み | ループON  |
+----------------------------------------------------------+


---

どの拡張を優先する？

UI強化 (ズーム、カーソル追従)

データ管理強化 (プリセット、バージョン管理)

計算機能 (FFT、補間、演算)

外部連携 (API、クラウド同期)












設定値を管理するためのクラスを作成します。
このクラスは、波形の種類、振幅、周波数、サンプルレート、シーケンスの長さなどの設定を一元管理します。
また、JSON で設定を保存/読み込みできる機能も追加します。


---

1. 設計方針

WaveformSettings クラスを作成し、波形の各種パラメータを管理。

JSONで設定を保存・読み込み できるようにする。

拡張しやすい設計（将来的にGUI連携やデバイス設定にも対応）。



---

2. 実装

(1) 設定値クラス

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


---

(2) 設定の保存と読み込み

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


---

3. JSON 設定ファイルの例

上記のプログラムを実行すると、以下のようなJSONファイルが作成されます。

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


---

4. まとめ

WaveformSettings クラス はシーケンサの設定を一元管理する。

JSONの保存・読み込み機能 を提供し、柔軟に設定を変更できる。

将来的に GUIと連携 してユーザーが設定を変更できるようにするのも簡単。


この設定クラスを WaveformSequence に統合すれば、JSON をロードするだけで波形シーケンスを作れるようになります！








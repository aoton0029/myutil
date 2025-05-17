using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WaveformSample.Waveforms;

namespace WaveformSample.Serialization
{
    /// <summary>
    /// シリアライズ機能を提供するインターフェース
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// オブジェクトをシリアライズします
        /// </summary>
        /// <typeparam name="T">シリアライズ対象の型</typeparam>
        /// <param name="obj">シリアライズするオブジェクト</param>
        /// <returns>シリアライズされた文字列</returns>
        string Serialize<T>(T obj);
        
        /// <summary>
        /// オブジェクトをシリアライズしてファイルに保存します
        /// </summary>
        /// <typeparam name="T">シリアライズ対象の型</typeparam>
        /// <param name="obj">シリアライズするオブジェクト</param>
        /// <param name="filePath">出力先ファイルパス</param>
        /// <returns>保存が成功したかどうか</returns>
        bool SerializeToFile<T>(T obj, string filePath);
        
        /// <summary>
        /// 複数のオブジェクトをシリアライズしてファイルに保存します
        /// </summary>
        /// <typeparam name="T">シリアライズ対象の型</typeparam>
        /// <param name="objects">シリアライズするオブジェクトのコレクション</param>
        /// <param name="directory">出力先ディレクトリ</param>
        /// <returns>保存結果のマッピング</returns>
        Dictionary<string, bool> SerializeMultipleToFiles<T>(IEnumerable<T> objects, string directory) where T : IWaveformSequence;
    }
    
    /// <summary>
    /// 波形シーケンスをCSV形式にシリアライズするクラス
    /// </summary>
    public class WaveformSequenceCsvSerializer : ISerializer
    {
        /// <summary>
        /// オブジェクトをシリアライズします
        /// </summary>
        /// <typeparam name="T">シリアライズ対象の型</typeparam>
        /// <param name="obj">シリアライズするオブジェクト</param>
        /// <returns>シリアライズされた文字列</returns>
        public string Serialize<T>(T obj)
        {
            // IWaveformSequenceへの変換を試みる
            if (obj is IWaveformSequence sequence)
            {
                return SerializeWaveformSequence(sequence);
            }
            
            throw new ArgumentException($"Object of type {typeof(T).Name} is not supported by this serializer");
        }
        
        /// <summary>
        /// オブジェクトをシリアライズしてファイルに保存します
        /// </summary>
        /// <typeparam name="T">シリアライズ対象の型</typeparam>
        /// <param name="obj">シリアライズするオブジェクト</param>
        /// <param name="filePath">出力先ファイルパス</param>
        /// <returns>保存が成功したかどうか</returns>
        public bool SerializeToFile<T>(T obj, string filePath)
        {
            try
            {
                string content = Serialize(obj);
                File.WriteAllText(filePath, content);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save to file: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 複数のオブジェクトをシリアライズしてファイルに保存します
        /// </summary>
        /// <typeparam name="T">シリアライズ対象の型</typeparam>
        /// <param name="objects">シリアライズするオブジェクトのコレクション</param>
        /// <param name="directory">出力先ディレクトリ</param>
        /// <returns>保存結果のマッピング</returns>
        public Dictionary<string, bool> SerializeMultipleToFiles<T>(IEnumerable<T> objects, string directory) where T : IWaveformSequence
        {
            Dictionary<string, bool> results = new Dictionary<string, bool>();
            
            // ディレクトリが存在することを確認
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            foreach (var obj in objects)
            {
                string fileName = GenerateFileName(obj);
                string filePath = Path.Combine(directory, fileName);
                
                results[obj.Name] = SerializeToFile(obj, filePath);
                
                // ステップ情報も保存
                string stepsFileName = Path.GetFileNameWithoutExtension(fileName) + "_steps.csv";
                string stepsFilePath = Path.Combine(directory, stepsFileName);
                SerializeStepsToFile(obj, stepsFilePath);
            }
            
            return results;
        }
        
        /// <summary>
        /// 波形シーケンスをCSV形式にシリアライズします
        /// </summary>
        /// <param name="sequence">シリアライズする波形シーケンス</param>
        /// <returns>CSV形式の文字列</returns>
        private string SerializeWaveformSequence(IWaveformSequence sequence)
        {
            var baseSequence = sequence as WaveformSequenceBase;
            if (baseSequence == null)
                throw new ArgumentException("The sequence must be derived from WaveformSequenceBase");
            
            var dataValues = baseSequence.CreateExportDataValues();
            
            StringBuilder csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Time,Value");
            
            foreach (var (time, value) in dataValues)
            {
                csvBuilder.AppendLine($"{time},{value}");
            }
            
            return csvBuilder.ToString();
        }
        
        /// <summary>
        /// 波形シーケンスのステップ情報をCSV形式にシリアライズします
        /// </summary>
        /// <param name="sequence">シリアライズする波形シーケンス</param>
        /// <returns>CSV形式の文字列</returns>
        public string SerializeSteps(IWaveformSequence sequence)
        {
            StringBuilder csvBuilder = new StringBuilder();
            
            // ヘッダー行の追加
            csvBuilder.AppendLine("StepIndex,WaveType,Duration,StartFrequency,EndFrequency,IsFrequencySweep," +
                                 "StartAmplitude,EndAmplitude,IsAmplitudeSweep,StartDCOffset,EndDCOffset," +
                                 "IsDCOffsetSweep,Phase");
            
            // ステップデータの追加
            for (int i = 0; i < sequence.WaveformSteps.Count; i++)
            {
                var step = sequence.WaveformSteps[i];
                csvBuilder.AppendLine(
                    $"{i+1},{step.WaveType},{step.Duration},{step.StartFrequency},{step.EndFrequency}," +
                    $"{step.IsFrequencySweep},{step.StartAmplitude},{step.EndAmplitude},{step.IsAmplitudeSweep}," +
                    $"{step.StartDCOffset},{step.EndDCOffset},{step.IsDCOffsetSweep},{step.Phase}");
            }
            
            return csvBuilder.ToString();
        }
        
        /// <summary>
        /// 波形シーケンスのステップ情報をシリアライズしてファイルに保存します
        /// </summary>
        /// <param name="sequence">シリアライズする波形シーケンス</param>
        /// <param name="filePath">出力先ファイルパス</param>
        /// <returns>保存が成功したかどうか</returns>
        public bool SerializeStepsToFile(IWaveformSequence sequence, string filePath)
        {
            try
            {
                string csvContent = SerializeSteps(sequence);
                File.WriteAllText(filePath, csvContent);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save steps CSV file: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// ファイル名を生成します
        /// </summary>
        /// <param name="sequence">波形シーケンス</param>
        /// <returns>ファイル名</returns>
        private string GenerateFileName(IWaveformSequence sequence)
        {
            string sequenceType = sequence.SequenceType.ToString();
            
            // 具体的なタイプに基づいてファイル名を調整
            if (sequence is ChuckWaveformSequence)
            {
                sequenceType = "Chuck";
            }
            else if (sequence is DeChuckWaveformSequence)
            {
                sequenceType = "DeChuck";
            }
            
            return $"{sequenceType}_{sequence.Number + 1:D2}_{sequence.Name}.csv";
        }
    }
    
    /// <summary>
    /// 波形シーケンスデータを出力用に整形するクラス
    /// </summary>
    public class WaveformSequenceOutputFormatter
    {
        private readonly ISerializer _serializer;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="serializer">使用するシリアライザー</param>
        public WaveformSequenceOutputFormatter(ISerializer serializer)
        {
            _serializer = serializer;
        }
        
        /// <summary>
        /// ChuckWaveformSequenceをCSVに出力します
        /// </summary>
        /// <param name="sequence">出力するChuck波形シーケンス</param>
        /// <param name="outputDirectory">出力先ディレクトリ</param>
        /// <returns>出力が成功したかどうか</returns>
        public bool ExportChuckSequence(ChuckWaveformSequence sequence, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            string fileName = $"Chuck_{sequence.Number + 1:D2}_{sequence.Name}.csv";
            string filePath = Path.Combine(outputDirectory, fileName);
            
            bool result = _serializer.SerializeToFile(sequence, filePath);
            
            // ステップ情報も出力
            if (result && _serializer is WaveformSequenceCsvSerializer csvSerializer)
            {
                string stepsFileName = $"Chuck_{sequence.Number + 1:D2}_{sequence.Name}_steps.csv";
                string stepsFilePath = Path.Combine(outputDirectory, stepsFileName);
                csvSerializer.SerializeStepsToFile(sequence, stepsFilePath);
            }
            
            return result;
        }
        
        /// <summary>
        /// DeChuckWaveformSequenceをCSVに出力します
        /// </summary>
        /// <param name="sequence">出力するDeChuck波形シーケンス</param>
        /// <param name="outputDirectory">出力先ディレクトリ</param>
        /// <returns>出力が成功したかどうか</returns>
        public bool ExportDeChuckSequence(DeChuckWaveformSequence sequence, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            string fileName = $"DeChuck_{sequence.Number + 1:D2}_{sequence.Name}.csv";
            string filePath = Path.Combine(outputDirectory, fileName);
            
            bool result = _serializer.SerializeToFile(sequence, filePath);
            
            // ステップ情報も出力
            if (result && _serializer is WaveformSequenceCsvSerializer csvSerializer)
            {
                string stepsFileName = $"DeChuck_{sequence.Number + 1:D2}_{sequence.Name}_steps.csv";
                string stepsFilePath = Path.Combine(outputDirectory, stepsFileName);
                csvSerializer.SerializeStepsToFile(sequence, stepsFilePath);
            }
            
            return result;
        }
        
        /// <summary>
        /// 複数の波形シーケンスをCSVに出力します
        /// </summary>
        /// <param name="sequences">出力する波形シーケンスのコレクション</param>
        /// <param name="outputDirectory">出力先ディレクトリ</param>
        /// <returns>出力結果のマッピング</returns>
        public Dictionary<string, bool> ExportMultipleSequences(IEnumerable<IWaveformSequence> sequences, string outputDirectory)
        {
            return _serializer.SerializeMultipleToFiles(sequences, outputDirectory);
        }
    }
    
    /// <summary>
    /// 波形シーケンスのデータ構造
    /// </summary>
    public class WaveformSequenceData
    {
        // 基本情報
        public string SequenceType { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public int Pitch { get; set; }
        
        // データポイント
        public List<(int Time, int Value)> DataPoints { get; set; } = new List<(int Time, int Value)>();
        
        // ステップ情報
        public List<WaveformStepData> Steps { get; set; } = new List<WaveformStepData>();
        
        /// <summary>
        /// WaveformSequenceからデータを作成します
        /// </summary>
        /// <param name="sequence">波形シーケンス</param>
        /// <returns>データ構造</returns>
        public static WaveformSequenceData FromSequence(IWaveformSequence sequence)
        {
            var baseSequence = sequence as WaveformSequenceBase;
            if (baseSequence == null)
                throw new ArgumentException("The sequence must be derived from WaveformSequenceBase");
                
            var data = new WaveformSequenceData
            {
                SequenceType = DetermineSequenceType(sequence),
                Number = sequence.Number,
                Name = sequence.Name,
                Pitch = sequence.Pitch
            };
            
            // 波形データポイントを追加
            data.DataPoints.AddRange(baseSequence.CreateExportDataValues());
            
            // 波形ステップを追加
            foreach (var step in sequence.WaveformSteps)
            {
                data.Steps.Add(WaveformStepData.FromStep(step));
            }
            
            return data;
        }
        
        /// <summary>
        /// シーケンスタイプを判定します
        /// </summary>
        /// <param name="sequence">波形シーケンス</param>
        /// <returns>シーケンスタイプの文字列</returns>
        private static string DetermineSequenceType(IWaveformSequence sequence)
        {
            if (sequence is ChuckWaveformSequence)
            {
                return "Chuck";
            }
            else if (sequence is DeChuckWaveformSequence)
            {
                return "DeChuck";
            }
            else
            {
                return sequence.SequenceType.ToString();
            }
        }
    }
    
    /// <summary>
    /// 波形ステップのデータ構造
    /// </summary>
    public class WaveformStepData
    {
        public WaveformType WaveType { get; set; }
        public int Duration { get; set; }
        public double StartFrequency { get; set; }
        public double EndFrequency { get; set; }
        public bool IsFrequencySweep { get; set; }
        public double StartAmplitude { get; set; }
        public double EndAmplitude { get; set; }
        public bool IsAmplitudeSweep { get; set; }
        public double StartDCOffset { get; set; }
        public double EndDCOffset { get; set; }
        public bool IsDCOffsetSweep { get; set; }
        public double Phase { get; set; }
        
        /// <summary>
        /// WaveformStepからデータを作成します
        /// </summary>
        /// <param name="step">波形ステップ</param>
        /// <returns>ステップデータ</returns>
        public static WaveformStepData FromStep(WaveformStep step)
        {
            return new WaveformStepData
            {
                WaveType = step.WaveType,
                Duration = step.Duration,
                StartFrequency = step.StartFrequency,
                EndFrequency = step.EndFrequency,
                IsFrequencySweep = step.IsFrequencySweep,
                StartAmplitude = step.StartAmplitude,
                EndAmplitude = step.EndAmplitude,
                IsAmplitudeSweep = step.IsAmplitudeSweep,
                StartDCOffset = step.StartDCOffset,
                EndDCOffset = step.EndDCOffset,
                IsDCOffsetSweep = step.IsDCOffsetSweep,
                Phase = step.Phase
            };
        }
    }
    
    /// <summary>
    /// シリアライザーのファクトリクラス
    /// </summary>
    public static class SerializerFactory
    {
        /// <summary>
        /// CSV形式のシリアライザーを作成します
        /// </summary>
        /// <returns>シリアライザー</returns>
        public static ISerializer CreateCsvSerializer()
        {
            return new WaveformSequenceCsvSerializer();
        }
    }
}
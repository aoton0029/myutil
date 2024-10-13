using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample
{
    internal class _WaveFormMng
    {
        //// Method to generate a sine wave (for example, can be extended for other wave types)
        //private List<decimal> GenerateBaseWaveform(int samples)
        //{
        //    List<decimal> waveform = new List<decimal>();
        //    for (int i = 0; i < samples; i++)
        //    {
        //        decimal time = i * _steptime;
        //        decimal angle = 2 * (decimal)Math.PI * time; // Angle for sine wave
        //        waveform.Add((decimal)Math.Sin((double)angle)); // Add sine wave value
        //    }
        //    return waveform;
        //}

        //// Method to apply frequency sweep
        //private List<decimal> ApplyFrequencySweep(List<decimal> waveform, int samples)
        //{
        //    if (_isSweepFreq)
        //    {
        //        for (int i = 0; i < samples; i++)
        //        {
        //            decimal t = (decimal)i / samples; // Normalized time (0 to 1)
        //            decimal freq = _startFreq + (_stopFreq - _startFreq) * t; // Linear frequency sweep
        //            decimal angle = 2 * (decimal)Math.PI * freq * i * _steptime;
        //            waveform[i] = (decimal)Math.Sin((double)angle);
        //        }
        //    }
        //    return waveform;
        //}

        //// Method to apply amplitude sweep
        //private List<decimal> ApplyAmplitudeSweep(List<decimal> waveform, int samples)
        //{
        //    if (_isSweepAmpl)
        //    {
        //        for (int i = 0; i < samples; i++)
        //        {
        //            decimal t = (decimal)i / samples; // Normalized time (0 to 1)
        //            decimal amplitude = _startAmpl + (_stopAmpl - _startAmpl) * t; // Linear amplitude sweep
        //            waveform[i] *= amplitude; // Scale waveform by amplitude
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 0; i < samples; i++)
        //        {
        //            waveform[i] *= _startAmpl; // Apply constant amplitude
        //        }
        //    }
        //    return waveform;
        //}

        //// Method to apply DC offset sweep
        //private List<decimal> ApplyDcOffsetSweep(List<decimal> waveform, int samples)
        //{
        //    if (_isSweepDcOffset)
        //    {
        //        for (int i = 0; i < samples; i++)
        //        {
        //            decimal t = (decimal)i / samples; // Normalized time (0 to 1)
        //            decimal offset = _startDcOffset + (_stopDcOffset - _startDcOffset) * t; // Linear DC offset sweep
        //            waveform[i] += offset; // Add DC offset to waveform
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 0; i < samples; i++)
        //        {
        //            waveform[i] += _startDcOffset; // Apply constant DC offset
        //        }
        //    }
        //    return waveform;
        //}

        //// Method to generate the waveform with sweeps applied
        //public List<decimal> GenerateWaveform(int samples)
        //{
        //    // Step 1: Generate base waveform (e.g., sine wave)
        //    List<decimal> waveform = GenerateBaseWaveform(samples);

        //    // Step 2: Apply frequency sweep
        //    waveform = ApplyFrequencySweep(waveform, samples);

        //    // Step 3: Apply amplitude sweep
        //    waveform = ApplyAmplitudeSweep(waveform, samples);

        //    // Step 4: Apply DC offset sweep
        //    waveform = ApplyDcOffsetSweep(waveform, samples);

        //    return waveform;
        //}

        //public List<decimal> GenerateWaveform()
        //{
        //    int _duration = 0;
        //    int _sampleRate = 0;
        //    int totalSamples = (int)(_duration * _sampleRate);  // 総サンプル数を計算
        //    List<decimal> waveform = new List<decimal>(totalSamples);

        //    for (int i = 0; i < totalSamples; i++)
        //    {
        //        decimal time = (decimal)i / _sampleRate;  // 時間を計算

        //        // 周波数の計算
        //        decimal freq = _isSweepFreq ? _startFreq + (_stopFreq - _startFreq) * time / _duration : _startFreq;
        //        decimal angle = 2 * (decimal)Math.PI * freq * time;

        //        // 波形の基礎値（正弦波を例に）
        //        decimal baseValue = 0;
        //        if (_wavetype == "Sine")
        //        {
        //            baseValue = (decimal)Math.Sin((double)angle);
        //        }
        //        // 他の波形（例えばSquare, Triangleなど）もここに追加可能

        //        // 振幅の適用
        //        decimal amplitude = _isSweepAmpl ? _startAmpl + (_stopAmpl - _startAmpl) * time / _duration : _startAmpl;
        //        baseValue *= amplitude;

        //        // DCオフセットの適用
        //        decimal dcOffset = _isSweepDcOffset ? _startDcOffset + (_stopDcOffset - _startDcOffset) * time / _duration : _startDcOffset;
        //        baseValue += dcOffset;

        //        // 最終的な波形値をリストに追加
        //        waveform.Add(baseValue);
        //    }

        //    return waveform;
        //}

    }
}

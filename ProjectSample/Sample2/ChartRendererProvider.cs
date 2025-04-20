using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSample.Sample2
{
    class ChartRendererProvider : IChartRendererProvider
    {
        private readonly Dictionary<SequenceType, IChartRenderer> _renderers;

        public ChartRendererProvider()
        {
            _renderers = new Dictionary<SequenceType, IChartRenderer>
        {
            { SequenceType.Chuck, new ChunkChartRenderer() },
            { SequenceType.Dechuck, new DechunkChartRenderer() }
        };
        }

        public IChartRenderer GetRenderer(SequenceType type)
        {
            return _renderers.TryGetValue(type, out var renderer)
                ? renderer
                : throw new InvalidOperationException($"No renderer found for type: {type}");
        }
    }

    class ChunkChartRenderer : IChartRenderer
    {
        public void Render(UcChart chart, int pitch, List<WaveformStep> steps)
        {
            // チャンク用の描画処理
        }
    }

    class DechunkChartRenderer : IChartRenderer
    {
        public void Render(UcChart chart, int pitch, List<WaveformStep> steps)
        {
            // デチャンク用の描画処理
        }
    }

    interface IChartRendererProvider
    {
        IChartRenderer GetRenderer(SequenceType type);
    }
}

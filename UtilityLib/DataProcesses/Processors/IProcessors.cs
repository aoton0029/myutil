using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DataProcesses.Processors
{
    public interface IProcessor<T>
    {
        T Process(T input);
    }

    public class UpperCaseProcessor : IProcessor<string>
    {
        public string Process(string input) => input.ToUpper();
    }

    public class TrimProcessor : IProcessor<string>
    {
        public string Process(string input) => input.Trim();
    }

    public class ProcessorPipeline<T>
    {
        private readonly List<IProcessor<T>> _processors = new();

        public void AddProcessor(IProcessor<T> processor) => _processors.Add(processor);

        public T Execute(T input)
        {
            T result = input;
            foreach (var processor in _processors)
            {
                result = processor.Process(result);
            }
            return result;
        }
    }
}

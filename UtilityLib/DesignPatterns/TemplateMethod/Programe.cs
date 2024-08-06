using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.TemplateMethod
{
    internal class Programe
    {
        public Programe()
        {
            Console.WriteLine("CSV Processor:");
            DataProcessor csvProcessor = new CsvDataProcessor();
            csvProcessor.ProcessData();

            Console.WriteLine("\nJSON Processor:");
            DataProcessor jsonProcessor = new JsonDataProcessor();
            jsonProcessor.ProcessData();
        }
    }
}

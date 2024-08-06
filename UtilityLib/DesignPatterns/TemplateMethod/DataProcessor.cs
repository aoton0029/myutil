using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.TemplateMethod
{
    public abstract class DataProcessor
    {
        // The template method
        public void ProcessData()
        {
            var data = ReadData();
            var processedData = ProcessDataCore(data);
            SaveData(processedData);
        }

        protected virtual string ReadData()
        {
            // Generic implementation, can be overridden
            Console.WriteLine("Reading generic data...");
            return "Sample data";
        }

        protected abstract string ProcessDataCore(string data); // Requires implementation

        protected virtual void SaveData(string data)
        {
            // Default implementation for saving data
            Console.WriteLine($"Saving data: {data}");
        }
    }

    public class CsvDataProcessor : DataProcessor
    {
        protected override string ReadData()
        {
            Console.WriteLine("Reading CSV data...");
            return "Name, Age, City\nJohn Doe, 29, New York";
        }

        protected override string ProcessDataCore(string data)
        {
            Console.WriteLine("Processing CSV data...");
            // Simulate processing CSV data
            return data.ToUpper(); // Simple transformation for demonstration
        }
    }

    public class JsonDataProcessor : DataProcessor
    {
        protected override string ReadData()
        {
            Console.WriteLine("Reading JSON data...");
            return "{\"name\": \"John Doe\", \"age\": 29, \"city\": \"New York\"}";
        }

        protected override string ProcessDataCore(string data)
        {
            Console.WriteLine("Processing JSON data...");
            // Simulate processing JSON data
            return data.Replace(" ", "").Replace("\"", ""); // Simple transformation for demonstration
        }
    }
}

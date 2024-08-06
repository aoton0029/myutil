using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Decorator
{
    public interface IFileOperation
    {
        byte[] Read(string filePath);
        void Write(string filePath, byte[] data);
    }


    public class FileOperation : IFileOperation
    {
        public byte[] Read(string filePath)
        {
            Console.WriteLine($"Reading file {filePath}.");
            return File.ReadAllBytes(filePath);
        }

        public void Write(string filePath, byte[] data)
        {
            Console.WriteLine($"Writing to file {filePath}.");
            File.WriteAllBytes(filePath, data);
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class Config
    {
        public string OutputPath { get; set; } = "./output";
        public Dictionary<string, string> Settings { get; set; } = new();
    }

}

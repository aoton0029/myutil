using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class Project
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public List<ProjectItem> Items { get; set; } = new();
        public Config Config { get; set; } = new();

        public int Version { get; set; } = 1; // 初期バージョン
    }

}

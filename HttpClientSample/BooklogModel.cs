using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientSample
{
    public class BooklogModel
    {
        public Tana tana { get; set; }
        public object[] category { get; set; }
        public Book[] books { get; set; }
    }

    public class Tana
    {
        public string account { get; set; }
        public string name { get; set; }
        public string image_url { get; set; }
    }

    public class Book
    {
        public string url { get; set; }
        public string title { get; set; }
        public string image { get; set; }
        public string catalog { get; set; }
    }
}

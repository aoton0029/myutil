using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFLib
{

    /// <summary>
    /// PDFの構造情報を格納するクラス
    /// </summary>
    public class PdfStructure
    {
        public int PageCount { get; set; }
        public List<PageInfo> Pages { get; set; }
    }

    /// <summary>
    /// PDFの各ページ情報を格納するクラス
    /// </summary>
    public class PageInfo
    {
        public int Index { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Orientation { get; set; }
        public bool HasContent { get; set; }
    }
}

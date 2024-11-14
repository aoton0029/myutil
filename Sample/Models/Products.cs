using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Models
{
    public class Products
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal BasePrice { get; set; }
    }

    public class Parts
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal IsStandard { get; set; }
    }

    public class Options
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class OptionParts
    {
        public int Id { get; set; }
        public int PartsId { get; set; }
        public int QtyChange { get; set; }
    }

    public class ProductParts
    {
        public int Id { get; set; }
        public int PartsId { get; set; }
        public int Qty { get; set; }
        public bool IsOptional{ get; set; }
    }

    public class ProductOptions
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string OptionName { get; set; }
        public decimal OptionPrice { get; set; }
    }
}

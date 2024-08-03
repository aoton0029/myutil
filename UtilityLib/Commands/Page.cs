using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Commands
{
    public class PageModel
    {
        public int CustomerCount { get; set; }
        public string ProductStatus { get; set; }
        public DateOnly OrderDate { get; set; }
        public decimal TransactionValue { get; set; }
        public TimeOnly ShipmentDelivery { get; set; }
        public bool LocationStatus { get; set; }
    }

    public class PageViewModel : BaseViewModel
    {
        private readonly PageModel _model;

        public int CustermerCount
        {
            get => _model.CustomerCount;
            set
            {
                _model.CustomerCount = value;
                NotifyPropertyChanged(nameof(CustermerCount));
            }
        }

        public PageViewModel()
        {
            _model = new PageModel();
            CustermerCount = 0;
        }
    }
}

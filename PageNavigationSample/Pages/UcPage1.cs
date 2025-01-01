using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PageNavigationSample
{
    public partial class UcPage1 : BasePage
    {
        public UcPage1() : base(null, null)
        {
            InitializeComponent();
        }

        public UcPage1(SharedStateManager<SharedData> mediator, PageManager screenManager) : base(mediator, screenManager)
        {
            InitializeComponent();
        }

        public override void Display()
        {
            Console.WriteLine("Screen3: Review and Finish");
            var data = Mediator.CurrentState;
            Console.WriteLine($"User: {data.CurrentUser}, Step: {data.StepNumber}");

            GoToPreviousScreen();
        }
    }
}

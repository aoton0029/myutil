using PageNavigationSample.Test1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PageNavigationSample.Test2
{
    public partial class Form1 : Form, INavigationResultHandler
    {
        public Form1()
        {
            InitializeComponent();
            var provider = new ServiceProvider();
            provider.RegisterSingleton(provider);
            provider.RegisterSingleton(new SharedData());
            provider.RegisterSingleton(new NavigationFlowService(this, provider));

            provider.RegisterTransient<UcPage1>();
            provider.RegisterTransient<UcPage2>();
            provider.RegisterTransient<UcPage3>();
            provider.RegisterTransient<UcStart>();
            provider.RegisterTransient<UcEnd>();

            var flow = provider.Resolve<NavigationFlowService>();
            flow.Start<UcStart>(this);
        }

        public void OnNavigationResult(NavigationResult result)
        {
            if (result.IsCompleted)
            {
                MessageBox.Show("完了: " + result.Data);
            }
            else
            {
                MessageBox.Show("キャンセル: " + result.Data);
            }
        }
    }
}

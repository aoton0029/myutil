using ChartSample.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample
{
    public class WaveFormMng
    {
       public  BindingList<DataGridItem> datas;

        public WaveFormMng() 
        {
            datas = new BindingList<DataGridItem>();
        }

        public List<ChartItem> Generate()
        {
            List<ChartItem> ret = new List<ChartItem>();
            foreach(var item in datas)
            {
                //if(item.WaveType == Common.WaveformType.Sine.ToString())
                //{
                Debug.WriteLine(item.ToString());
                int samplerate = 1000;
                foreach (int i in Enumerable.Range(0, samplerate))
                {
                    double t = i * ((double)item.StepTime) / (double)samplerate;
                    double val = WaveFormGenerator.GenerateSineWave(t, item.StartAmplitude, item.StopAmplitude, item.StartFrequency, item.StopFrequency, item.StartDcOffset, item.StopDcOffset, item.StepTime, 0);
                    ret.Add(new ChartItem(t, val));
                }
                //}

            }

            return ret;
        }
    }
}

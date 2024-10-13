using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartSample
{
    internal class WaveFormGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="A1"></param>
        /// <param name="A2"></param>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="D1"></param>
        /// <param name="D2"></param>
        /// <param name="T"></param>
        /// <param name="phi"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static double GenerateSineWave(double t, double A1, double A2, double f1, double f2, double D1, double D2, double T, double phi)
        {
            double amplitude = A1 + ((A2 - A1) * (t / T));
            double offset = D1 + ((D2 - D1) * (t / T));
            double frequency = f1 + ((f2 - f1) * (t / T));
            return amplitude * Math.Sin(2 * Math.PI * (f1 * t + (f2 - f1) * (t * t) / (2 * T)) + phi) + offset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="A1"></param>
        /// <param name="A2"></param>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="D1"></param>
        /// <param name="D2"></param>
        /// <param name="T"></param>
        /// <param name="phi"></param>
        /// <returns></returns>
        public static double GenerateSquareWave(double t, double A1, double A2, double f1, double f2, double D1, double D2, double T, double phi)
        {
            double amplitude = A1 + (A2 - A1) * (t / T);
            double offset = D1 + (D2 - D1) * (t / T);
            double frequency = f1 + (f2 - f1) * (t / T);
            double sineValue = Math.Sin(2 * Math.PI * (f1 * t + (f2 - f1) * (t * t) / (2 * T)) + phi);
            return amplitude * Math.Sign(sineValue) + offset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="A1"></param>
        /// <param name="A2"></param>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="D1"></param>
        /// <param name="D2"></param>
        /// <param name="T"></param>
        /// <param name="phi"></param>
        /// <returns></returns>
        public static double GenerateTriangleWave(double t, double A1, double A2, double f1, double f2, double D1, double D2, double T, double phi)
        {
            double amplitude = A1 + (A2 - A1) * (t / T);
            double offset = D1 + (D2 - D1) * (t / T);
            double frequency = f1 + (f2 - f1) * (t / T);
            double sineValue = Math.Sin(2 * Math.PI * (f1 * t + (f2 - f1) * (t * t) / (2 * T)) + phi);
            return amplitude * (2 / Math.PI) * Math.Asin(sineValue) + offset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="A1"></param>
        /// <param name="A2"></param>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="D1"></param>
        /// <param name="D2"></param>
        /// <param name="T"></param>
        /// <param name="phi"></param>
        /// <returns></returns>
        public static double GenerateSawtoothWave(double t, double A1, double A2, double f1, double f2, double D1, double D2, double T, double phi)
        {
            double amplitude = A1 + (A2 - A1) * (t / T);
            double offset = D1 + (D2 - D1) * (t / T);
            double frequency = f1 + (f2 - f1) * (t / T);
            double modValue = (f1 * t + (f2 - f1) * (t * t) / (2 * T)) % 1.0;
            return amplitude * (2 * (modValue - Math.Floor(modValue + 0.5))) + offset;
        }
    }
}

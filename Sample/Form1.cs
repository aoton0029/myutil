using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sample
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

            main();
        }

        // 数値を日本語の単位に変換するメソッド
        static string ConvertToJapaneseUnits(decimal number)
        {
            if (number >= 1_0000_0000_0000) // 兆
                return (number / 1_0000_0000_0000).ToString("F2") + "兆";
            if (number >= 1_0000_0000) // 億
                return (number / 1_0000_0000).ToString("F2") + "億";
            if (number >= 1_0000) // 万
                return (number / 1_0000).ToString("F2") + "万";
            return number.ToString();
        }

        // 科学的記数法の表記を10^の形式に変換するメソッド
        static string ConvertToScientificNotation(double number)
        {
            return number.ToString("E2").Replace("E", "×10^");
        }

        public void main()
        {
            Debug.WriteLine("Size of byte: " + sizeof(byte) + " byte (" + sizeof(byte) * 8 + " bits)");
            Debug.WriteLine("Range of byte: " + ConvertToJapaneseUnits(byte.MinValue) + " 〜 " + ConvertToJapaneseUnits(byte.MaxValue));

            Debug.WriteLine("Size of sbyte: " + sizeof(sbyte) + " byte (" + sizeof(sbyte) * 8 + " bits)");
            Debug.WriteLine("Range of sbyte: " + ConvertToJapaneseUnits(sbyte.MinValue) + " 〜 " + ConvertToJapaneseUnits(sbyte.MaxValue));

            Debug.WriteLine("Size of short: " + sizeof(short) + " bytes (" + sizeof(short) * 8 + " bits)");
            Debug.WriteLine("Range of short: " + ConvertToJapaneseUnits(short.MinValue) + " 〜 " + ConvertToJapaneseUnits(short.MaxValue));

            Debug.WriteLine("Size of ushort: " + sizeof(ushort) + " bytes (" + sizeof(ushort) * 8 + " bits)");
            Debug.WriteLine("Range of ushort: " + ConvertToJapaneseUnits(ushort.MinValue) + " 〜 " + ConvertToJapaneseUnits(ushort.MaxValue));

            Debug.WriteLine("Size of int: " + sizeof(int) + " bytes (" + sizeof(int) * 8 + " bits)");
            Debug.WriteLine("Range of int: " + ConvertToJapaneseUnits(int.MinValue) + " 〜 " + ConvertToJapaneseUnits(int.MaxValue));

            Debug.WriteLine("Size of uint: " + sizeof(uint) + " bytes (" + sizeof(uint) * 8 + " bits)");
            Debug.WriteLine("Range of uint: " + ConvertToJapaneseUnits(uint.MinValue) + " 〜 " + ConvertToJapaneseUnits(uint.MaxValue));

            Debug.WriteLine("Size of long: " + sizeof(long) + " bytes (" + sizeof(long) * 8 + " bits)");
            Debug.WriteLine("Range of long: " + ConvertToJapaneseUnits(long.MinValue) + " 〜 " + ConvertToJapaneseUnits(long.MaxValue));

            Debug.WriteLine("Size of ulong: " + sizeof(ulong) + " bytes (" + sizeof(ulong) * 8 + " bits)");
            Debug.WriteLine("Range of ulong: " + ConvertToJapaneseUnits(ulong.MinValue) + " 〜 " + ConvertToJapaneseUnits(ulong.MaxValue));

            Debug.WriteLine("Size of float: " + sizeof(float) + " bytes (" + sizeof(float) * 8 + " bits)");
            Debug.WriteLine("Range of float: " + ConvertToScientificNotation(float.MinValue) + " 〜 " + ConvertToScientificNotation(float.MaxValue));

            Debug.WriteLine("Size of double: " + sizeof(double) + " bytes (" + sizeof(double) * 8 + " bits)");
            Debug.WriteLine("Range of double: " + ConvertToScientificNotation(double.MinValue) + " 〜 " + ConvertToScientificNotation(double.MaxValue));

            Debug.WriteLine("Size of decimal: " + Marshal.SizeOf(typeof(decimal)) + " bytes (" + Marshal.SizeOf(typeof(decimal)) * 8 + " bits)");
            Debug.WriteLine("Range of decimal: " + ConvertToJapaneseUnits(decimal.MinValue) + " 〜 " + ConvertToJapaneseUnits(decimal.MaxValue));
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

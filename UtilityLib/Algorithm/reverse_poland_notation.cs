using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Algorithm
{
    internal class reverse_poland_notation
    {
        // reverse poland notation
        public static int rpn(string[] tokens)
        {
            Stack<int> stack = new Stack<int>();
            foreach (string token in tokens)
            {
                if (token == "+" || token == "-" || token == "*" || token == "/")
                {
                    int a = stack.Pop();
                    int b = stack.Pop();
                    if (token == "+")
                    {
                        stack.Push(a + b);
                    }
                    else if (token == "-")
                    {
                        stack.Push(b - a);
                    }
                    else if (token == "*")
                    {
                        stack.Push(a * b);
                    }
                    else if (token == "/")
                    {
                        stack.Push(b / a);
                    }
                }
                else
                {
                    stack.Push(int.Parse(token));
                }
            }
            return stack.Pop();
        }


    }
}

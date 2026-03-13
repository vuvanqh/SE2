using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public static class StringCalculator
    {
        public static int Calculate(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return 0;
            }

           
            List<string> delimiters = new List<string>() { "\n", "," };
            if (arg[0] =='/' && arg[1]=='/')
            {
                int nlIndex = arg.IndexOf('\n');
                string delimsPart = arg.Substring(2, nlIndex - 2); 
                arg = arg.Substring(nlIndex + 1);

                if(delimsPart.Contains('['))
                {
                    string[] customDelims = delimsPart.Split(new[] { '[', ']' });
                    foreach(string d in customDelims)
                    {
                        if (!string.IsNullOrEmpty(d))
                        {
                            delimiters.Add(d);
                        }
                            
                    }
                }
                else
                {
                    delimiters.Add(delimsPart);
                }

            }
            List<string> list = arg.Split(delimiters.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            int sum = 0;
            for(int i = 0; i <list.Count; i++)
            {
                int number = int.Parse(list[i]);
                if(number <0)
                {
                    throw new ArgumentException("Negative");
                }
                if(number > 1000)
                {
                    continue;
                }
                sum += number;
            }
            return sum;
        }
    }
}

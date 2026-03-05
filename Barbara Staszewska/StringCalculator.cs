using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    public class StringCalculator
    {
        public static int Calculate(string arg)
        {
            if(arg == "") return 0;

            if (int.TryParse(arg, out int result))
                if (result < 0)
                    throw new ArgumentException("Numbers cannot be negative");
                else
                    return result;

            List<string> delimiters = new List<string> { ",", "\n" };

            if (arg.StartsWith("//"))
            {
                if (arg[2] != '[')
                {
                    delimiters.Add(arg.Substring(2, 1));
                    arg = arg.Substring(4);
                }
                else
                {
                    int i = 2;
                    while (arg[i] == '[')
                    {
                        int start = i + 1;
                        int end = arg.IndexOf(']', start);
                        delimiters.Add(arg.Substring(start, end - start));
                        i = end + 1;
                    }
                    arg = arg.Substring(arg.IndexOf('\n') + 1);
                }
            }

            string[] numbers = arg.Split(
                delimiters.OrderByDescending(d => d.Length).ToArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in numbers)
            {
                int number = int.Parse(item);
                if (number < 0)
                { 
                    throw new ArgumentException("Numbers cannot be negative"); 
                }
            }

            int sum = numbers
                .Select(int.Parse)
                .Where(n => n <= 1000)
                .Sum();

            return sum;
        }
    }
}

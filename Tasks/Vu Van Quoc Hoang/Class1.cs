using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lab2;

public class StringCalculator
{
    public static int Calculate(string arg)
    {
        if(arg.Length== 0)  return 0;

    

        List<string> delims = new List<string>() { ",", "\n" };
        if (arg.StartsWith("//"))
        {
            string[] parts = arg.Split('\n', 2);
            string header = parts[0].Substring(2);

            if (header.StartsWith("["))
            {
                string[] dels = header.Split('[', ']', StringSplitOptions.RemoveEmptyEntries);

                foreach (string str in dels)
                {
                    if (string.IsNullOrEmpty(str)) continue;
                    delims.Add(str);
                }
            }
            else
            {
                delims.Add(header);
            }

            arg = parts[1];
        }
        string[] strings = arg.Split(delims.ToArray(), StringSplitOptions.RemoveEmptyEntries);

        int res = 0;
        foreach(var str in strings)
        {
            if(int.TryParse(str,out int r))
            {
                if (r < 0)
                    throw new ArgumentException("Numbers cannot be negative");
                if (r > 1000) continue;
                res += r;
            }
        }

        //List<string> delimiters = arg.Split("[,]");
        return res;
    }


}

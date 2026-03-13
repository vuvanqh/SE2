namespace Calculator
{
public class Calc
{
    public int StringCalculator(string str)
    {
        if(str == string.Empty) return 0;
        if (Int32.TryParse(str, out int j))
        {
            return j;
        }
        var delimiters = new List<string> { ",", "\n" };
            if (str.StartsWith("//"))
            {
                int nl = str.IndexOf('\n');
                if (nl < 0) return -1;

                string header = str.Substring(2, nl - 2);
                str = str.Substring(nl + 1);
                if (header.StartsWith("["))
                {
                    int i = 0;
                    while (i < header.Length)
                    {
                        if (header[i] != '[') return -1;
                        int end = header.IndexOf(']', i + 1);
                        if (end < 0) return -1;

                        string d = header.Substring(i + 1, end - i - 1);
                        if (d.Length == 0) return -1;

                        delimiters.Add(d);
                        i = end + 1;
                    }
                }
                else
                {
                    if (header.Length == 0) return -1;
                    delimiters.Add(header);
                }
            }
        var result  = str.Split(delimiters.ToArray(), StringSplitOptions.None);
        if(result.Length > 3) return -1;
        if (Int32.TryParse(result[0], out int num_1) && Int32.TryParse(result[1], out int num_2) )
        {
            if(num_1 >= 1000) num_1 =0;
            if(num_1 < 0) throw new InvalidOperationException();
            if(num_2 < 0) throw new InvalidOperationException();
            if(num_2 >= 1000) num_2 =0;

            if(result.Length == 3)
                {
                    if(Int32.TryParse(result[2], out int num_3))
                    {
                        if(num_3 >= 1000) num_3 =0;
                        if(num_3 < 0) throw new InvalidOperationException();
                        return num_1+num_2+num_3;
                    }
                    else
                    {
                        return -1;
                    }
                }
            return num_1 + num_2;
        }


        return -1;
    }
}    
}
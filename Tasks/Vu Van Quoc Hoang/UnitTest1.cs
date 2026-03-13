using Lab2;
using System.Security;

namespace StringCalculatorTest;

public class UnitTest1
{
    [Fact]
    public void EmptyStringShouldReturnZeri()
    {
        int res = StringCalculator.Calculate(string.Empty);

        Assert.Equal(0, res);
    }

    [Fact]
    public void SingleNumberReturnsValue()
    {
        string arg = "9";

        if (int.TryParse(arg, out int r))
        {
            int res = StringCalculator.Calculate("9");
            Assert.Equal(9, res);
        }
    }
    [Fact]
    public void CommaDelimitedReturnsSum()
    {
        string arg = "1,2";
        string[] substrings = arg.Split(',');
        int sum = 0;

        foreach (string substring in substrings)
        {
            if (int.TryParse(substring, out int r))
                sum += r;
        }

        int res = StringCalculator.Calculate(arg);

        Assert.Equal(sum, res);
    } 
    [Fact]
    public void NegativeNumbersThrowException()
    {
        string arg = "-1";
        var exception = Assert.Throws<ArgumentException>(() => StringCalculator.Calculate(arg));
        Assert.Equal("Numbers cannot be negative", exception.Message);
    }
    [Fact]
    public void NumbersGreaterThan1000Ingored()
    {
        string arg = "1,1000000,2,60000";

        Assert.Equal(3,StringCalculator.Calculate(arg));
    }

    [Fact]
    public void NewLineDelimitedReturnSum()
    {
        string arg = "1\n2";

        Assert.Equal(3, StringCalculator.Calculate(arg));
    }

    [Fact]
    public void NumbersDelimitedEitherWayReturnSum()
    {
        string arg = "1\n3,5";

        Assert.Equal(9,StringCalculator.Calculate(arg));
    }

   
    [Fact]
    public void SingleCharDelimiterDefinedOnTHeFirstLine_ShouldBeValid()
    {
        string arg = "//#\n10#2";

        Assert.Equal(12, StringCalculator.Calculate(arg));
    }
    [Fact]
    public void MultipleCharDelimiterDefinedOnTHeFirstLine_ShouldBeValid()
    {
        string arg = "//[####]\n1####1000000####2####60000";

        Assert.Equal(3, StringCalculator.Calculate(arg));
    }
}

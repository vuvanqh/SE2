using ConsoleApp1;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void EmptyReturnsZero()
        {
            int res = StringCalculator.Calculate("");
            Assert.Equal(0, res);
        }
        [Fact]
        public void SingleNumberReturnsValue()
        {
            string input = "3";
            int res = StringCalculator.Calculate(input);
            Assert.Equal(3, res);
        }
        [Fact]
        public void TwoNumbersCommaDelimitedReturnSum()
        {
            string input = "3,1";
            int res = StringCalculator.Calculate(input);
            Assert.Equal(4, res);
        }

        [Fact]
        public void TwoNumbersNewlineDelimitedReturnSum()
        {
            string input = "3\n6";
            int res = StringCalculator.Calculate(input);
            Assert.Equal(9, res);
        }
        [Fact]
        public void ThreeNumbersDelimitedReturnSum()
        {
            string input1 = "3\n6\n7";
            int res1 = StringCalculator.Calculate(input1);
            Assert.Equal(16, res1);

            string input2 = "3\n5,7";
            int res2 = StringCalculator.Calculate(input2);
            Assert.Equal(15, res2);

            string input3 = "1,6,7";
            int res3 = StringCalculator.Calculate(input3);
            Assert.Equal(14, res3);
        }
        [Fact]
        public void NegativeNumbersThrowException()
        {
            string input = "-1";
            var ex = Assert.Throws<ArgumentException>(() => StringCalculator.Calculate(input));

        }

        [Fact]
        public void NumbersAbove1000AreIgnored()
        {
            string input = "1,1001";
            int res = StringCalculator.Calculate(input);
            Assert.Equal(1, res);
        }
        [Fact]
        public void SingleCharDelimiter()
        {
            string input1 = "//#\n1#100";
            int res1 = StringCalculator.Calculate(input1);
            Assert.Equal(101, res1); 
        }
        [Fact]
        public void MultiCharDelimiter()
        {
            string input2 = "//[###]\n1###1";
            int res2 = StringCalculator.Calculate(input2);
            Assert.Equal(2, res2);
        }
        [Fact]
        public void ManyCharCharDelimiters()
        {
            string input3 = "//[*][%]\n1*2%3";
            int res3 = StringCalculator.Calculate(input3);
            Assert.Equal(6, res3);
        }




    }
}
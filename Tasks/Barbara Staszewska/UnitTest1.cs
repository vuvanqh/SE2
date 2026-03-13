using Lab1;

namespace StringCalculatorTest
{
    public class UnitTest1
    {
        [Fact]
        public void EmptyString()
        {
            string arg = "";
            int res = StringCalculator.Calculate(arg);
            Assert.Equal(0, res);
        }

        [Fact]
        public void SingleNumber()
        {
            string arg = "1";
            int res = StringCalculator.Calculate(arg);
            Assert.Equal(1, res);
        }

        [Fact]
        public void CommaDelimitedSum()
        {
            string arg = "1,2";
            int res = StringCalculator.Calculate(arg);
            Assert.Equal(3, res);
        }

        [Fact]
        public void NewlineDelimitedSum()
        {
            string arg = "1\n2";
            int res = StringCalculator.Calculate(arg);
            Assert.Equal(3, res);
        }

        [Fact]
        public void ThreeNumbersSum()
        {
            string arg = "1,2\n3";
            int res = StringCalculator.Calculate(arg);
            Assert.Equal(6, res);
        }

        [Fact]
        public void Negative()
        {
            string arg = "-1";
            //string arg = "//[#][##]\n1#-1,1\n1##1";
            var exception = Assert.Throws<ArgumentException>(() => StringCalculator.Calculate(arg));
            Assert.Equal("Numbers cannot be negative", exception.Message);
        }

        [Fact]
        public void GreaterThan1000()
        {
            string arg = "1,1001,1";
            int res = StringCalculator.Calculate(arg);
            Assert.Equal(2, res);
        }

        [Fact]
        public void SingleCharDelimiter()
        {
            string arg = "//#\n1#1";
            int res = StringCalculator.Calculate(arg);
            Assert.Equal(2, res);
        }

        [Fact]
        public void MultiCharDelimiter()
        {
            string arg = "//[###]\n1###2###3";
            int res = StringCalculator.Calculate(arg);
            Assert.Equal(6,res);
        }

        [Fact]
        public void ManyDelimiters()
        {
            string arg = "//[#][##]\n1#1,1\n1##1";
            int res = StringCalculator.Calculate(arg);
            Assert.Equal(5, res);
        }

    }
}
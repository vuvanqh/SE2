using NUnit.Framework;
using Calculator;

namespace Calculator.Tests;

public class CalcTests
{
    [Test]
    public void EmptyString_Returns0()
    {
        var c = new Calc();
        Assert.That(c.StringCalculator(""), Is.EqualTo(0));
    }

    [Test]
    public void NumberString_ReturnsParsedInt()
    {
        var c = new Calc();
        Assert.That(c.StringCalculator("15"), Is.EqualTo(15));
    }

    [Test]
    public void NonNumber_ReturnsMinus1()
    {
        var c = new Calc();
        Assert.That(c.StringCalculator("abc"), Is.EqualTo(-1));
    }
    [Test]
    public void Number_Comma_Number()
    {
        var c = new Calc();
        Assert.That(c.StringCalculator("5,5"), Is.EqualTo(10));
    }
    [Test]
    public void Number_newline_Number()
    {
        var c = new Calc();
        Assert.That(c.StringCalculator("5\n5"), Is.EqualTo(10));
    }
    [Test]
    public void NonNumber_separator_number()
    {
        var c = new Calc();
        Assert.That(c.StringCalculator("abc,5"), Is.EqualTo(-1));
        Assert.That(c.StringCalculator("5,abc"), Is.EqualTo(-1));
        Assert.That(c.StringCalculator("abc\n5"), Is.EqualTo(-1));

    }
    [Test]
    public void Multiseparator()
    {
        var c = new Calc();
        Assert.That(c.StringCalculator("//[###]\n 5###5"), Is.EqualTo(10));

    }
    [Test]
    public void ManySingles()
    {
        var c = new Calc();
        Assert.That(c.StringCalculator("//[*][%]\n 5*5%5"), Is.EqualTo(15));

    }
    [Test]
    public void ManyMulti()
    {
        var c = new Calc();
        Assert.That(c.StringCalculator("//[^^^][%%%]\n 10^^^15%%%5"),Is.EqualTo(30));
    }
    [Test]
    public void ExceptionTest()
    {
        var c = new Calc();
        try
        {
            c.StringCalculator("//[^^^][%%%]\n -1^^^15%%%5");
        }
        catch (InvalidOperationException)
        {
            Assert.That(1, Is.EqualTo(1));
        }
    }

}
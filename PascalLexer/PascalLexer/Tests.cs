using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PascalLexer
{
  [TestFixture]
  public class Tests
  {
    private void LexTest(string text, List<Token> tokens)
    {
      try
      {
        Assert.That(new Lexer(text).Lex(), Is.EquivalentTo(tokens));
      }
      catch (LexingException e)
      {
        Console.WriteLine(text.Substring(0, e.Position));
        throw;
      }
    }
    
    [Test]
    public void Test01()
    {
      LexTest("var", new List<Token> {new ReservedWord(new TokenRange(0, 3))});
    }
    
    [Test]
    public void Test02()
    {
      LexTest("assembler", new List<Token> {new Modifier(new TokenRange(0, "assembler".Length))});
    }
    
    [Test]
    public void Test03()
    {
      LexTest("_sosiska", new List<Token> {new Identifier(new TokenRange(0, "_sosiska".Length))});
    }
    
    [Test]
    public void Test04()
    {
      LexTest("  \n  _sosiska;   \n\n", new List<Token> {new Identifier(new TokenRange(5, 13))});
    }
    
    [Test]
    public void Test05()
    {
      LexTest("// text\n123", new List<Token>
      {
        new SingleLineComment(new TokenRange(0, 7)),
        new Number(new TokenRange(8, 11))
      });
    }
    
    [Test]
    public void Test06()
    {
      var s = "(*asd//a\n*)";
      LexTest(s, new List<Token> {new MultiLineComment(new TokenRange(0, s.Length))});
    }
    
    [Test]
    public void Test07()
    {
      var s = "**";
      LexTest(s, new List<Token> {new Symbol(new TokenRange(0, s.Length))});
    }
    
    [Test]
    public void Test08()
    {
      var s = "/-*";
      LexTest(s, new List<Token>
      {
        new Symbol(new TokenRange(0, 1)),
        new Symbol(new TokenRange(1, 2)),
        new Symbol(new TokenRange(2, 3))
      });
    }
    
    [Test]
    public void Test09()
    {
      var s = "-%101010111";
      LexTest(s, new List<Token>
      {
        new Symbol(new TokenRange(0, 1)),
        new Number(new TokenRange(1, s.Length))
      });
    }
    
    [Test]
    public void Test10()
    {
      var s = "$67124";
      LexTest(s, new List<Token>
      {
        new Number(new TokenRange(0, s.Length))
      });
    }
    
    [Test]
    public void Test11()
    {
      var s = "123456";
      LexTest(s, new List<Token>
      {
        new Number(new TokenRange(0, s.Length))
      });
    }
    
    [Test]
    public void Test12()
    {
      var s = "-$DEADBEEF";
      LexTest(s, new List<Token>
      {
        new Symbol(new TokenRange(0, 1)),
        new Number(new TokenRange(1, s.Length))
      });
    }
    
    [Test]
    public void Test13()
    {
      var s = "123.456E12";
      LexTest(s, new List<Token>
      {
        new Number(new TokenRange(0, s.Length))
      });
    }
    
    [Test]
    public void Test14()
    {
      var s = "123E12";
      LexTest(s, new List<Token>
      {
        new Number(new TokenRange(0, s.Length))
      });
    }
    
    [Test]
    public void Test15()
    {
      var s = "123.456";
      LexTest(s, new List<Token>
      {
        new Number(new TokenRange(0, s.Length))
      });
    }
    
    [Test]
    public void Test16()
    {
      var s = "'text'";
      LexTest(s, new List<Token>
      {
        new CharacterString(new TokenRange(0, s.Length))
      });
    }
    
    [Test]
    public void Test17()
    {
      var s = "#13";
      LexTest(s, new List<Token>
      {
        new CharacterString(new TokenRange(0, s.Length))
      });
    }
    
    [Test]
    public void Test18()
    {
      var s = "'text'#13'moretext'";
      LexTest(s, new List<Token>
      {
        new CharacterString(new TokenRange(0, 6)),
        new CharacterString(new TokenRange(6, 9)),
        new CharacterString(new TokenRange(9, 19))
      });
    }
    
    [Test]
    public void Test19()
    {
      var s = @"var a: real;
begin
  var a := ReadReal('Введите a: ');
  var a2,a4,a8: real; // вспомогательные переменные
  var a2 := a * a;
  var a4 := a2 * a2;
  var a8 := a4 * a4;
  Println(a, 'в степени 8 =', a8);
end.";
      LexTest(s, new List<Token>
      {
        new ReservedWord(new TokenRange(0, 3)),
        new Identifier(new TokenRange(4, 5)),
        new Symbol(new TokenRange(5, 6)),
        new Identifier(new TokenRange(7, 11)),
        new ReservedWord(new TokenRange(14, 19)),
        new ReservedWord(new TokenRange(23, 26)),
        new Identifier(new TokenRange(27, 28)),
        new Symbol(new TokenRange(29, 31)),
        new Identifier(new TokenRange(32, 40)),
        new Symbol(new TokenRange(40, 41)),
        new CharacterString(new TokenRange(41, 54)),
        new Symbol(new TokenRange(54, 55)),
        new ReservedWord(new TokenRange(60, 63)),
        new Identifier(new TokenRange(64, 66)),
        new Symbol(new TokenRange(66, 67)),
        new Identifier(new TokenRange(67, 69)),
        new Symbol(new TokenRange(69, 70)),
        new Identifier(new TokenRange(70, 72)),
        new Symbol(new TokenRange(72, 73)),
        new Identifier(new TokenRange(74, 78)),
        new SingleLineComment(new TokenRange(80, 110)),
        new ReservedWord(new TokenRange(113, 116)),
        new Identifier(new TokenRange(117, 119)),
        new Symbol(new TokenRange(120, 122)),
        new Identifier(new TokenRange(123, 124)),
        new Symbol(new TokenRange(125, 126)),
        new Identifier(new TokenRange(127, 128)),
        new ReservedWord(new TokenRange(133, 136)),
        new Identifier(new TokenRange(137, 139)),
        new Symbol(new TokenRange(140, 142)),
        new Identifier(new TokenRange(143, 145)),
        new Symbol(new TokenRange(146, 147)),
        new Identifier(new TokenRange(148, 150)),
        new ReservedWord(new TokenRange(155, 158)),
        new Identifier(new TokenRange(159, 161)),
        new Symbol(new TokenRange(162, 164)),
        new Identifier(new TokenRange(165, 167)),
        new Symbol(new TokenRange(168, 169)),
        new Identifier(new TokenRange(170, 172)),
        new Identifier(new TokenRange(177, 184)),
        new Symbol(new TokenRange(184, 185)),
        new Identifier(new TokenRange(185, 186)),
        new Symbol(new TokenRange(186, 187)),
        new CharacterString(new TokenRange(188, 203)),
        new Symbol(new TokenRange(203, 204)),
        new Identifier(new TokenRange(205, 207)),
        new Symbol(new TokenRange(207, 208)),
        new ReservedWord(new TokenRange(211, 214)),
        new Symbol(new TokenRange(214, 215)),
      });
    }
  }
}
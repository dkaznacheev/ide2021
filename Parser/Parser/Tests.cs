using System.Text;
using NUnit.Framework;

namespace Parser
{
    public interface IExpressionVisitor
    {
        void Visit(Literal expression);
        void Visit(Variable expression);
        void Visit(BinaryExpression expression);
        void Visit(ParenExpression expression);
    }
    
    public interface IExpression
    {
        void Accept(IExpressionVisitor visitor);
    }

    public class Literal : IExpression
    {
        public Literal(string value)
        {
            Value = value;
        }

        public readonly string Value;
        
        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Variable : IExpression
    {
        public Variable(string name)
        {
            Name = name;
        }

        public readonly string Name;
        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    
    public class BinaryExpression : IExpression
    {
        public readonly IExpression FirstOperand;
        public readonly IExpression SecondOperand;
        public readonly string Operator;

        public BinaryExpression(IExpression firstOperand, IExpression secondOperand, string @operator)
        {
            FirstOperand = firstOperand;
            SecondOperand = secondOperand;
            Operator = @operator;
        }

        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    
    public class ParenExpression : IExpression
    {
        public ParenExpression(IExpression operand)
        {
            Operand = operand;
        }

        public readonly IExpression Operand;
        public void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class DumpVisitor : IExpressionVisitor
    {
        private readonly StringBuilder myBuilder;

        public DumpVisitor()
        {
            myBuilder = new StringBuilder();
        }

        public void Visit(Literal expression)
        {
            myBuilder.Append("Literal(" + expression.Value + ")");
        }

        public void Visit(Variable expression)
        {
            myBuilder.Append("Variable(" + expression.Name + ")");
        }

        public void Visit(BinaryExpression expression)
        {
            myBuilder.Append("Binary(");
            expression.FirstOperand.Accept(this);
            myBuilder.Append(expression.Operator);
            expression.SecondOperand.Accept(this);
            myBuilder.Append(")");
        }

        public void Visit(ParenExpression expression)
        {
            myBuilder.Append("Paren(");
            expression.Operand.Accept(this);
            myBuilder.Append(")");
        }

        public override string ToString()
        {
            return myBuilder.ToString();
        }
    }

    public readonly struct ParseResult<T>
    {
        public ParseResult(T expression, int index)
        {
            Expression = expression;
            Index = index;
        } 
        
        public T Expression { get; }
        public int Index { get; }
    }

    public class SimpleParser
    {
        private static ParseResult<Literal>? ParseLiteral(string text, int index)
        {
            if (index >= text.Length || !char.IsDigit(text[index]))
            {
                return null;
            }
            return new ParseResult<Literal>(new Literal(text[index].ToString()), index + 1);
        }
        
        private static ParseResult<Variable>? ParseVariable(string text, int index)
        {
            if (index >= text.Length || !char.IsLetter(text[index]))
            {
                return null;
            }
            return new ParseResult<Variable>(new Variable(text[index].ToString()), index + 1);
        }
        
        private static ParseResult<ParenExpression>? ParseParenExpression(string text, int index)
        {
            if (index >= text.Length || text[index] != '(')
            {
                return null;
            }
        
            var exprResult = ParseExpression(text, index + 1);
            if (!exprResult.HasValue)
            {
                return null;
            }

            if (exprResult.Value.Index >= text.Length || text[exprResult.Value.Index] != ')')
            {
                return null;
            }

            return new ParseResult<ParenExpression>(new ParenExpression(exprResult.Value.Expression),
                exprResult.Value.Index + 1);
        }

        private static ParseResult<BinaryExpression>? ParseMultipication(string text, int index)
        {
            if (index >= text.Length)
            {
                return null;
            }

            var left = ParseNonAdditionNorMultiplication(text, index);
            if (!left.HasValue || left.Value.Index >= text.Length)
            {
                return null;
            }

            var op = text[left.Value.Index];
            if (op != '*' && op != '/')
            {
                return null;
            }

            var right = ParseNonAddition(text, left.Value.Index + 1);
            if (!right.HasValue)
            {
                return null;
            }
            
            var lExpr = left.Value.Expression;
            var rExpr = right.Value.Expression;
            BinaryExpression expr;
            if (rExpr is BinaryExpression && "*/".Contains((rExpr as BinaryExpression).Operator))
            {
                var rBin = (rExpr as BinaryExpression);
                expr = new BinaryExpression(
                    new BinaryExpression(lExpr, rBin.FirstOperand, op.ToString()),
                    rBin.SecondOperand,
                    rBin.Operator);
            }
            else
            {
                expr = new BinaryExpression(
                    left.Value.Expression,
                    right.Value.Expression,
                    op.ToString());
            }
            
            return new ParseResult<BinaryExpression>(
                expr,
                right.Value.Index);
        }

        private static ParseResult<BinaryExpression>? ParseAddition(string text, int index)
        {
            if (index >= text.Length)
            {
                return null;
            }

            var left = ParseNonAddition(text, index);
            if (!left.HasValue || left.Value.Index >= text.Length)
            {
                return null;
            }

            var op = text[left.Value.Index];
            if (op != '+' && op != '-')
            {
                return null;
            }

            var right = ParseExpression(text, left.Value.Index + 1);
            if (!right.HasValue)
            {
                return null;
            }

            var lExpr = left.Value.Expression;
            var rExpr = right.Value.Expression;
            BinaryExpression expr;
            if (rExpr is BinaryExpression && "+-".Contains((rExpr as BinaryExpression).Operator))
            {
                var rBin = (rExpr as BinaryExpression);
                expr = new BinaryExpression(
                    new BinaryExpression(lExpr, rBin.FirstOperand, op.ToString()),
                    rBin.SecondOperand,
                    rBin.Operator);
            }
            else
            {
                expr = new BinaryExpression(
                    left.Value.Expression,
                    right.Value.Expression,
                    op.ToString());
            }
            
            return new ParseResult<BinaryExpression>(
                expr,
                right.Value.Index);
        }
        
        private static ParseResult<IExpression>? ParseNonAddition(string text, int index)
        {
            var parens = ParseParenExpression(text, index);
            if (parens.HasValue)
            {
                return new ParseResult<IExpression>(parens.Value.Expression, parens.Value.Index);
            }
            
            var multiplication = ParseMultipication(text, index);
            if (multiplication.HasValue)
            {
                return new ParseResult<IExpression>(multiplication.Value.Expression, multiplication.Value.Index); 
            }
            
            var literal = ParseLiteral(text, index);
            if (literal.HasValue)
            {
                return new ParseResult<IExpression>(literal.Value.Expression, literal.Value.Index); 
            }
            
            var variable = ParseVariable(text, index);
            if (variable.HasValue)
            {
                return new ParseResult<IExpression>(variable.Value.Expression, variable.Value.Index); 
            }

            return null;
        }

        private static ParseResult<IExpression>? ParseNonAdditionNorMultiplication(string text, int index)
        {
            var parens = ParseParenExpression(text, index);
            if (parens.HasValue)
            {
                return new ParseResult<IExpression>(parens.Value.Expression, parens.Value.Index);
            }
            
            var literal = ParseLiteral(text, index);
            if (literal.HasValue)
            {
                return new ParseResult<IExpression>(literal.Value.Expression, literal.Value.Index); 
            }
            
            var variable = ParseVariable(text, index);
            if (variable.HasValue)
            {
                return new ParseResult<IExpression>(variable.Value.Expression, variable.Value.Index); 
            }

            return null;
        }
        
        private static ParseResult<IExpression>? ParseExpression(string text, int index)
        {
            var addition = ParseAddition(text, index);
            if (addition.HasValue)
            {
                return new ParseResult<IExpression>(addition.Value.Expression, addition.Value.Index); 
            }
            
            var multiplication = ParseMultipication(text, index);
            if (multiplication.HasValue)
            {
                return new ParseResult<IExpression>(multiplication.Value.Expression, multiplication.Value.Index); 
            }
            
            var parens = ParseParenExpression(text, index);
            if (parens.HasValue)
            {
                return new ParseResult<IExpression>(parens.Value.Expression, parens.Value.Index);
            }
            
            var literal = ParseLiteral(text, index);
            if (literal.HasValue)
            {
                return new ParseResult<IExpression>(literal.Value.Expression, literal.Value.Index); 
            }
            
            var variable = ParseVariable(text, index);
            if (variable.HasValue)
            {
                return new ParseResult<IExpression>(variable.Value.Expression, variable.Value.Index); 
            }
            
            return null;
        }
        
        public static IExpression Parse(string text)
        {
            var result = ParseExpression(text, 0);
            if (result?.Index != text.Length)
            {
                return null;
            }
            return result?.Expression;
        }
    }
    
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var dumpVisitor = new DumpVisitor();
            new BinaryExpression(new Literal("1"), new Literal("2"), "+").Accept(dumpVisitor);
            Assert.AreEqual("Binary(Literal(1)+Literal(2))", dumpVisitor.ToString());
            Assert.Pass();
        }
        
        [Test]
        public void Test2()
        {
            var dumpVisitor = new DumpVisitor();
            SimpleParser.Parse("1+2").Accept(dumpVisitor);
            Assert.AreEqual("Binary(Literal(1)+Literal(2))", dumpVisitor.ToString());
            Assert.Pass();
        }
        
        [Test]
        public void Test3()
        {
            var dumpVisitor = new DumpVisitor();
            SimpleParser.Parse("1+2*3").Accept(dumpVisitor);
            Assert.AreEqual("Binary(Literal(1)+Binary(Literal(2)*Literal(3)))", dumpVisitor.ToString());
            Assert.Pass();
        }
        
        [Test]
        public void Test4()
        {
            var dumpVisitor = new DumpVisitor();
            SimpleParser.Parse("1*2+3").Accept(dumpVisitor);
            Assert.AreEqual("Binary(Binary(Literal(1)*Literal(2))+Literal(3))", dumpVisitor.ToString());
            Assert.Pass();
        }
        
        [Test]
        public void Test5()
        {
            var dumpVisitor = new DumpVisitor();
            SimpleParser.Parse("1*(2+3)").Accept(dumpVisitor);
            Assert.AreEqual("Binary(Literal(1)*Paren(Binary(Literal(2)+Literal(3))))", dumpVisitor.ToString());
            Assert.Pass();
        }
        
        [Test]
        public void Test6()
        {
            var dumpVisitor = new DumpVisitor();
            SimpleParser.Parse("1-2-3").Accept(dumpVisitor);
            Assert.AreEqual("Binary(Binary(Literal(1)-Literal(2))-Literal(3))", dumpVisitor.ToString());
            Assert.Pass();
        }
        
        [Test]
        public void Test7()
        {
            var dumpVisitor = new DumpVisitor();
            SimpleParser.Parse("1/2/3").Accept(dumpVisitor);
            Assert.AreEqual("Binary(Binary(Literal(1)/Literal(2))/Literal(3))", dumpVisitor.ToString());
            Assert.Pass();
        }
        
        [Test]
        public void Test8()
        {
            var dumpVisitor = new DumpVisitor();
            SimpleParser.Parse("(1+2)*(3+4)").Accept(dumpVisitor);
            Assert.AreEqual("Binary(Paren(Binary(Literal(1)+Literal(2)))*Paren(Binary(Literal(3)+Literal(4))))", 
                dumpVisitor.ToString());
            Assert.Pass();
        }
        
        [Test]
        public void Test9()
        {
            var dumpVisitor = new DumpVisitor();
            SimpleParser.Parse("(((x)))").Accept(dumpVisitor);
            Assert.AreEqual("Paren(Paren(Paren(Variable(x))))", dumpVisitor.ToString());
            Assert.Pass();
        }
        
        [Test]
        public void Test10()
        {
            var dumpVisitor = new DumpVisitor();
            SimpleParser.Parse("1-(2-3)").Accept(dumpVisitor);
            Assert.AreEqual("Binary(Literal(1)-Paren(Binary(Literal(2)-Literal(3))))", dumpVisitor.ToString());
            Assert.Pass();
        }
        
        [Test]
        public void Test11()
        {
            Assert.Null(SimpleParser.Parse("SHOULD NOT PARSE"));
            Assert.Pass();
        }
        
        [Test]
        public void Test12()
        {
            Assert.Null(SimpleParser.Parse("1-"));
            Assert.Pass();
        }
        
        [Test]
        public void Test13()
        {
            Assert.Null(SimpleParser.Parse("(1+2"));
            Assert.Pass();
        }
        [Test]
        public void Test14()
        {
            Assert.Null(SimpleParser.Parse("1+2)"));
            Assert.Pass();
        }
    }
    
}
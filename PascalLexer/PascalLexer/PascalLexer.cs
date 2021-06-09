using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PascalLexer
{
    public class TokenRange
    {
        public readonly int start;
        public readonly int end;

        public TokenRange(int start, int end)
        {
            this.start = start;
            this.end = end;
        }
    }
    
    public abstract class Token
    {
        public readonly TokenRange range;
    }

    public abstract class Comment : Token { }
    public class SingleLineComment: Comment { }
    public class MultiLineComment: Comment { }

    public class ReservedWord : Token { }
    public class Modifier : Token { }
    
    public class Identifier : Token { }

    public class Operator : Token { }

    public class Constant: Token { }
    public class Number: Constant { }
    public class Decimal: Number { }
    public class Octal: Number { }
    public class Hex: Number { }
    public class CharacterString: Constant { }


    enum LexerState
    {
        WHITESPACE,
        COMMENT,
        STRING,
        OCTAL,
        DECIMAL,
        HEX,
        IDENTIFIER,
        OPERATOR
    }
    
    public class PascalLexer
    {
        private string text;
        private int length;
        private List<Token> tokens = new List<Token>();
        private int pos = 0;

        public PascalLexer(string text)
        {
            this.text = text;
            this.length = text.Length;
        }

        private void readIdentOrReservedWord()
        {
            StringBuilder sb = new StringBuilder();
            while (pos < length && isIdentSymbol(text[pos]))
            {
                sb.Append(text[pos]);
            }            
        }

        private static bool isIdentStartSymbol(char c)
        {
            return c == '_' || c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '&';
        }

        public List<Token> lex()
        {
            while (pos < text.Length)
            {
                char c = text[pos];
                if (isIdentStartSymbol(c))
                {
                    readIdentOrReservedWord();
                }
            }
        }
    }
}
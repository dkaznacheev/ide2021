using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;

namespace PascalLexer
{
    public static class Reserved
    {
        public static readonly string[] ReservedWords = {
            "absolute",
            "and",
            "array",
            "asm",
            "begin",
            "case",
            "const",
            "constructor",
            "destructor",
            "div",
            "do",
            "downto",
            "else",
            "end",
            "file",
            "for",
            "function",
            "goto",
            "if",
            "implementation",
            "in",
            "inherited",
            "inline",
            "interface",
            "label",
            "mod",
            "nil",
            "not",
            "object",
            "of",
            "operator",
            "or",
            "packed",
            "procedure",
            "program",
            "record",
            "reintroduce",
            "repeat",
            "self",
            "set",
            "shl",
            "shr",
            "string",
            "then",
            "to",
            "type",
            "unit",
            "until",
            "uses",
            "var",
            "while",
            "with",
            "xor",
            "as",
            "class",
            "dispinterface",
            "except",
            "exports",
            "finalization",
            "finally",
            "initialization",
            "inline",
            "is",
            "library",
            "on",
            "out",
            "packed",
            "property",
            "raise",
            "resourcestring",
            "threadvar",
            "try"
        };
        
        public static readonly string[] Modifiers = {
            "absolute",
            "abstract",
            "alias",
            "assembler",
            "bitpacked",
            "break",
            "cdecl",
            "continue",
            "cppdecl",
            "cvar",
            "default",
            "deprecated",
            "dynamic",
            "enumerator",
            "experimental",
            "export",
            "external",
            "far",
            "far16",
            "forward",
            "generic",
            "helper",
            "implements",
            "index",
            "interrupt",
            "iocheck",
            "local",
            "message",
            "name",
            "near",
            "nodefault",
            "noreturn",
            "nostackframe",
            "oldfpccall",
            "otherwise",
            "overload",
            "override",
            "pascal",
            "platform",
            "private",
            "protected",
            "public",
            "published",
            "read",
            "register",
            "reintroduce",
            "result",
            "safecall",
            "saveregisters",
            "softfloat",
            "specialize",
            "static",
            "stdcall",
            "stored",
            "strict",
            "unaligned",
            "unimplemented",
            "varargs",
            "virtual",
            "winapi",
            "write"
        };

        public static readonly string[] TwoCharSymbols = {
            "<<", ">>", "**", "<>", "><", "<=", ">=", ":=", "+=", "-=", "*=", "/=", "(*", "*)", "(.", ".)"
        };
        
        public static readonly string[] OneCharSymbols = {
            "'", "+", "-", "*", "/", "=", "<", ">", "[", "]", ".", ",", "(", ")", ":", "^", "@", "{", "}", "$", "#", "&", "%"
        };

        public static readonly string BinaryDigits = "01";
        public static readonly string OctalDigits = "01234567";
        public static readonly string DecimalDigits = "0123456789";
        public static readonly string HexDigits = "0123456789ABCDEFabcdef";
    }
    
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

        protected Token(TokenRange range)
        {
            this.range = range;
        }
    }

    public abstract class Comment : Token
    {
        protected Comment(TokenRange range) : base(range)
        {
        }
    }
    public class SingleLineComment: Comment
    {
        public SingleLineComment(TokenRange range) : base(range)
        {
        }
    }
    public class MultiLineComment: Comment
    {
        public MultiLineComment(TokenRange range) : base(range)
        {
        }
    }

    public class ReservedWord : Token
    {
        public ReservedWord(TokenRange range) : base(range)
        {
        }
    }
    public class Modifier : Token
    {
        public Modifier(TokenRange range) : base(range)
        {
        }
    }
    
    public class Identifier : Token
    {
        public Identifier(TokenRange range) : base(range)
        {
        }
    }

    public class Symbol : Token
    {
        public Symbol(TokenRange range) : base(range)
        {
        }
    }

    public class Constant: Token
    {
        public Constant(TokenRange range) : base(range)
        {
        }
    }
    public class Number: Constant
    {
        public Number(TokenRange range) : base(range)
        {
        }
    }
    public class Decimal: Number
    {
        public Decimal(TokenRange range) : base(range)
        {
        }
    }
    public class Octal: Number
    {
        public Octal(TokenRange range) : base(range)
        {
        }
    }
    public class Hex: Number
    {
        public Hex(TokenRange range) : base(range)
        {
        }
    }
    public class CharacterString: Constant
    {
        public CharacterString(TokenRange range) : base(range)
        {
        }
    }


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

    public class LexingException : Exception
    {
        public int Position;

        public LexingException(int position)
        {
            Position = position;
        }
    }

    public class PascalLexer
    {
        private readonly string text;
        private readonly int length;
        private List<Token> tokens = new List<Token>();
        private int pos = 0;

        public PascalLexer(string text)
        {
            this.text = text;
            this.length = text.Length;
        }

        private void ReadIdentOrReservedWord()
        {
            StringBuilder sb = new StringBuilder();
            int startPos = pos;
            while (pos < length && IsIdentSymbol(text[pos]))
            {
                sb.Append(text[pos]);
                pos++;
            }

            if (pos >= length || !IsLineWhitespace(text[pos]) && text[pos] != ';')
            {
                throw new LexingException(pos);
            }
            
            Token token;
            if (Reserved.ReservedWords.Contains(sb.ToString()))
            {
                token = new ReservedWord(new TokenRange(startPos, pos));
            }
            else if (Reserved.Modifiers.Contains(sb.ToString()))
            {
                token = new Modifier(new TokenRange(startPos, pos));
            }
            else
            {
                token = new Identifier(new TokenRange(startPos, pos));
            }
            
            tokens.Add(token);
        }

        private void ReadSingleLineComment()
        {
            int startPos = pos;
            while (pos < length && text[pos] != '\n')
            {
                pos++;
            }
            tokens.Add(new SingleLineComment(new TokenRange(startPos, pos)));
        }

        private void ReadParenMultiLineComment()
        {
            int startPos = pos;
            while (pos < length - 1 && !(text[pos - 1] == '*' && text[pos] == ')'))
            {
                pos++;
            }
            // if we reached the end of text, but didn't find comment end, it's an error
            if (pos == length - 1 && !(text[pos] == '*' && text[pos + 1] == ')'))
            {
                throw new LexingException(pos);
            }

            tokens.Add(new MultiLineComment(new TokenRange(startPos, pos)));
        }
        
        private void ReadBraceMultiLineComment()
        {
            int startPos = pos;
            while (pos < length && text[pos] != '}')
            {
                pos++;
            }
            
            // if we reached the end of text, but didn't find comment end, it's an error
            if (pos == length && text[pos - 1] != '}')
            {
                throw new LexingException(pos);
            }
            tokens.Add(new MultiLineComment(new TokenRange(startPos, pos)));
        }

        private void ReadSymbol()
        {
            TokenRange range;
            if (IsTwoCharSymbol())
            {
                range = new TokenRange(pos, pos + 2);
                pos += 2;
            }
            else if (isOneCharSymbol())
            {
                range = new TokenRange(pos, pos + 1);
                pos++;
            }
            else
            {
                throw new LexingException(pos);
            }
            tokens.Add(new Symbol(range));
        }

        private void ReadNumber()
        {
            int startPos = pos;
            if (text[pos] == '-')
            {
                pos++;
            }

            if (pos >= length)
            {
                throw new LexingException(pos);
            }
            
            string allowedDigits;
            if (text[pos] == '%')
            {
                allowedDigits = Reserved.BinaryDigits;
            }
            else if (text[pos] == '&')
            {
                allowedDigits = Reserved.OctalDigits;
            }
            else if (text[pos] == '$')
            {
                allowedDigits = Reserved.HexDigits;
            }
            else if (text[pos] >= '0' && text[pos] <= '9')
            {
                allowedDigits = Reserved.OctalDigits;
            }
            else
            {
                throw new LexingException(pos);
            }

            if (!allowedDigits.Equals(Reserved.DecimalDigits))
            {
                pos++;
            }
            
            if (pos >= length)
            {
                throw new LexingException(pos);
            }

            bool readDigits = false;
            while (pos < length && allowedDigits.Contains(text[pos]))
            {
                readDigits = true;
                pos++;
            }

            if (!readDigits)
            {
                throw new LexingException(pos);
            }
            
            if (!allowedDigits.Equals(Reserved.DecimalDigits))
            {
                tokens.Add(new Number(new TokenRange(startPos, pos)));
                return;
            }

            if (text[pos] == '.')
            {
                pos++;
                readDigits = false;
                while (pos < length && allowedDigits.Contains(text[pos]))
                {
                    readDigits = true;
                    pos++;
                }
                if (!readDigits)
                {
                    throw new LexingException(pos);
                }
            }
            
            if (text[pos] == 'e' || text[pos] == 'E')
            {
                pos++;
                if (text[pos] == '-')
                {
                    pos++;
                }
                readDigits = false;
                while (pos < length && allowedDigits.Contains(text[pos]))
                {
                    readDigits = true;
                    pos++;
                }
                if (!readDigits)
                {
                    throw new LexingException(pos);
                }
            }
            
            tokens.Add(new Number(new TokenRange(startPos, pos)));
        }
        
        private bool IsTwoCharSymbol()
        {
            return pos < length - 1 && Reserved.TwoCharSymbols.Contains(text.Substring(pos, 2));
        }
        
        private bool isOneCharSymbol()
        {
            return pos < length && Reserved.OneCharSymbols.Contains(text.Substring(pos, 1));
        }
        
        private bool IsSingleLineCommentStart()
        {
            return pos < length - 1 && text[pos] == '/' && text[pos + 1] == '/';
        }
        
        private bool IsBraceMultiLineCommentStart()
        {
            return pos < length && text[pos] == '{';
        }
        
        private bool IsParenMultiLineCommentStart()
        {
            return pos < length - 1 && text[pos] == '(' && text[pos + 1] == '*';
        }

        private static bool IsSymbolStart(char c)
        {
            return "'+-*/=<>[].,():^@{}$#&%".Contains(c);
        }

        private static bool IsLineWhitespace(char c)
        {
            return c == ' ' || c == '\t';
        }

        private static bool IsIdentStartSymbol(char c)
        {
            return c == '_' || c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '&';
        }

        private static bool IsIdentSymbol(char c)
        {
            return c == '_' || c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9';
        }

        private static bool IsNumberStart(char c)
        {
            return "$%&".Contains(c) || c >= '0' && c <= '9';
        }
        
        

        public List<Token> Lex()
        {
            while (pos < text.Length)
            {
                var c = text[pos];
                if (IsIdentStartSymbol(c))
                {
                    ReadIdentOrReservedWord();
                } 
                else if (IsSingleLineCommentStart())
                {
                    ReadSingleLineComment();
                }
                else if (IsBraceMultiLineCommentStart())
                {
                    ReadBraceMultiLineComment();
                }
                else if (IsParenMultiLineCommentStart())
                {
                    ReadParenMultiLineComment();
                }
                else if (IsNumberStart(c))
                {
                    ReadNumber();
                }
                else if (IsSymbolStart(c))
                {
                    ReadSymbol();
                }
                else if (IsCharacterStringStart(c))
                {
                    ReadCharacterString();
                }
                else
                {
                    throw new LexingException(pos);
                }
            }
        }
    }
}
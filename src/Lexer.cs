namespace Prgfx.Fusion.Afx
{
    public class Lexer
    {
        protected string input;

        protected char currentChar;

        protected int charPos;

        public Lexer(string code)
        {
            input = code;
            currentChar = input.Length > 0 ? input[0] : '\0';
            charPos = 0;
        }

        public bool IsWhiteSpace()
        {
            return char.IsWhiteSpace(currentChar);
        }

        public bool IsAlpha()
        {
            return char.IsLetter(currentChar);
        }

        public bool IsAlphaNumeric()
        {
            return char.IsLetterOrDigit(currentChar);
        }

        public bool IsColon()
        {
            return currentChar == ':';
        }

        public bool IsDot()
        {
            return currentChar == '.';
        }

        public bool IsAt()
        {
            return currentChar == '@';
        }

        public bool IsMinus()
        {
            return currentChar == '-';
        }

        public bool IsUnderscore()
        {
            return currentChar == '_';
        }

        public bool IsEqualSign()
        {
            return currentChar == '=';
        }

        public bool IsOpeningBracket()
        {
            return currentChar == '<';
        }

        public bool IsClosingBracket()
        {
            return currentChar == '>';
        }

        public bool IsOpeningBrace()
        {
            return currentChar == '{';
        }

        public bool IsClosingBrace()
        {
            return currentChar == '}';
        }

        public bool IsForwardSlash()
        {
            return currentChar == '/';
        }

        public bool IsBackSlash()
        {
            return currentChar == '\\';
        }

        public bool IsSingleQuote()
        {
            return currentChar == '\'';
        }

        public bool IsDoubleQuote()
        {
            return currentChar == '\"';
        }

        public bool IsEnd()
        {
            return currentChar == '\0';
        }

        public void Rewind()
        {
            currentChar = input[--charPos];
        }

        public string Peek(int number)
        {
            if (charPos < input.Length - 1)
            {
                return input.Substring(charPos, number);
            }
            return null;
        }

        public char Consume()
        {
            var c = currentChar;
            if (charPos < input.Length - 1)
            {
                currentChar = input[++charPos];
            }
            else
            {
                currentChar = '\0';
            }
            return c;
        }
    }
}
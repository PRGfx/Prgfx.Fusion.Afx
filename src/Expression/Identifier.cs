using System.Text;

namespace Prgfx.Fusion.Afx.Expression
{
    public class Identifier
    {
        public static string Parse(Afx.Lexer lexer)
        {
            StringBuilder identifier = new StringBuilder();
            while (true)
            {
                if (lexer.IsAlphaNumeric()
                    || lexer.IsDot()
                    || lexer.IsColon()
                    || lexer.IsMinus()
                    || lexer.IsUnderscore()
                    || lexer.IsAt())
                {
                    identifier.Append(lexer.Consume());
                    continue;
                }
                if (lexer.IsEqualSign()
                    || lexer.IsWhiteSpace()
                    || lexer.IsClosingBracket()
                    || lexer.IsForwardSlash())
                {
                    return identifier.ToString();
                }
                var unexpectedChar = lexer.Consume();
                throw new AfxException($@"Unexpected character ""{unexpectedChar}"" in identifier ""{identifier.ToString()}""");
            }
        }
    }
}
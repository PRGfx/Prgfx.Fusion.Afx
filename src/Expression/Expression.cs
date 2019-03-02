using System.Text;

namespace Prgfx.Fusion.Afx.Expression
{
    public class Expression
    {
        public static string Parse(Afx.Lexer lexer)
        {
            var contents = new StringBuilder();
            var braceCount = 0;
            if (lexer.IsOpeningBrace())
            {
                lexer.Consume();
            }
            else
            {
                throw new AfxException("Expression without braces");
            }
            while (true)
            {
                if (lexer.IsEnd())
                {
                    throw new AfxException("Unfinished Expression \"" + contents.ToString() + "\"");
                }
                if (lexer.IsOpeningBrace())
                {
                    braceCount++;
                }
                if (lexer.IsClosingBrace())
                {
                    if (braceCount == 0)
                    {
                        lexer.Consume();
                        return contents.ToString();
                    }
                    braceCount--;
                }
                contents.Append(lexer.Consume());
            }
        }
    }
}
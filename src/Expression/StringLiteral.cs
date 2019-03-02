using System.Text;

namespace Prgfx.Fusion.Afx.Expression
{
    public class StringLiteral
    {
        public static string Parse(Afx.Lexer lexer)
        {
            char openingQuoteSign;
            char closingQuoteSign;
            var contents = new StringBuilder();
            var willBeEscaped = false;
            if (lexer.IsSingleQuote() || lexer.IsDoubleQuote())
            {
                openingQuoteSign = lexer.Consume();
            }
            else
            {
                throw new AfxException("Unquoted String literal");
            }
            while (true)
            {
                if (lexer.IsEnd())
                {
                    throw new AfxException($"Unfinished string literal \"{contents.ToString()}\"");
                }
                if (lexer.IsBackSlash() && !willBeEscaped)
                {
                    willBeEscaped = true;
                    lexer.Consume();
                    continue;
                }
                if (lexer.IsSingleQuote() || lexer.IsDoubleQuote())
                {
                    closingQuoteSign = lexer.Consume();
                    if (!willBeEscaped && openingQuoteSign == closingQuoteSign)
                    {
                        return contents.ToString();
                    }
                    contents.Append(closingQuoteSign);
                    willBeEscaped = false;
                    continue;
                }
                contents.Append(lexer.Consume());
                willBeEscaped = false;
            }
        }
    }
}
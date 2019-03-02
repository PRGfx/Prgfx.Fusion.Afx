namespace Prgfx.Fusion.Afx.Expression
{
    public class Spread
    {
        public static AstNode Parse(Afx.Lexer lexer)
        {
            if (lexer.IsOpeningBrace() && lexer.Peek(4) == "{...")
            {
                lexer.Consume();
                lexer.Consume();
                lexer.Consume();
                lexer.Consume();
            }
            else
            {
                throw new AfxException("Spread without braces");
            }
            string contents = string.Empty;
            int braceCount = 0;
            while (true)
            {
                if (lexer.IsEnd())
                {
                    throw new AfxException("Unifinished Spread");
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
                        return new AstNode()
                        {
                            Type = AstNodeType.Expression,
                            Payload = contents
                        };
                    }
                    braceCount--;
                }
                contents += lexer.Consume();
            }
        }
    }
}
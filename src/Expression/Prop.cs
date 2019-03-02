namespace Prgfx.Fusion.Afx.Expression
{
    public class Prop
    {

        public struct PropParsingResult
        {
            public string Identifier;
            public object Payload;
            public AstNodeType Type;
        }
        public static PropParsingResult Parse(Afx.Lexer lexer)
        {
            var identifier = Identifier.Parse(lexer);
            if (lexer.IsEqualSign())
            {
                lexer.Consume();
                if (lexer.IsSingleQuote() || lexer.IsDoubleQuote())
                {
                    return new PropParsingResult()
                    {
                        Type = AstNodeType.String,
                        Payload = StringLiteral.Parse(lexer),
                        Identifier = identifier
                    };
                }
                if (lexer.IsOpeningBrace())
                {
                    return new PropParsingResult()
                    {
                        Type = AstNodeType.Expression,
                        Payload = Expression.Parse(lexer),
                        Identifier = identifier
                    };
                }
                throw new AfxException($"Prop-Assignment \"{identifier}\" was not followed by quotes or braces");
            }
            else if (lexer.IsWhiteSpace() || lexer.IsForwardSlash() || lexer.IsClosingBracket())
            {
                return new PropParsingResult()
                {
                    Type = AstNodeType.Boolean,
                    Payload = true,
                    Identifier = identifier
                };
            }
            else
            {
                throw new AfxException($"Prop identifier \"{identifier}\" is neither assignment nor boolean");
            }
        }
    }
}
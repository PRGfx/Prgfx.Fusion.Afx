using Prgfx.Fusion.Afx;
using System.Collections.Generic;

namespace Prgfx.Fusion.Afx.Expression
{
    public class Node
    {
        public struct NodeParsingResult
        {
            public string Identifier;
            public AstNode[] Attributes;
            public AstNode[] Children;
            public bool SelfClosing;
        }
        public static NodeParsingResult Parse(Afx.Lexer lexer)
        {
            if (lexer.IsOpeningBracket())
            {
                lexer.Consume();
            }
            var identifier = Identifier.Parse(lexer);

            try
            {
                List<AstNode> attributes = new List<AstNode>();
                AstNode[] children = new AstNode[] { };

                if (lexer.IsWhiteSpace())
                {
                    while (lexer.IsWhiteSpace())
                    {
                        lexer.Consume();
                    }
                    while (!lexer.IsForwardSlash() && !lexer.IsClosingBracket())
                    {
                        if (lexer.IsOpeningBrace())
                        {
                            attributes.Add(new AstNode()
                            {
                                Type = AstNodeType.Spread,
                                Payload = Spread.Parse(lexer)
                            });
                        }
                        else
                        {
                            attributes.Add(new AstNode()
                            {
                                Type = AstNodeType.Prop,
                                Payload = Prop.Parse(lexer)
                            });
                        }
                        while (lexer.IsWhiteSpace())
                        {
                            lexer.Consume();
                        }
                    }
                }
                if (lexer.IsForwardSlash())
                {
                    lexer.Consume();
                    if (lexer.IsClosingBracket())
                    {
                        lexer.Consume();
                        return new NodeParsingResult()
                        {
                            Identifier = identifier,
                            Attributes = attributes.ToArray(),
                            Children = children,
                            SelfClosing = true
                        };
                    }
                    else
                    {
                        throw new AfxException($@"Self closing tag ""{identifier}"" missing closing bracket");
                    }
                }

                if (lexer.IsClosingBracket())
                {
                    lexer.Consume();
                }
                else
                {
                    throw new AfxException($@"Tag ""{identifier}"" did not end with closing bracket.");
                }

                children = NodeList.Parse(lexer);

                if (lexer.IsOpeningBracket())
                {
                    lexer.Consume();
                    if (lexer.IsForwardSlash())
                    {
                        lexer.Consume();
                    }
                    else
                    {
                        throw new AfxException($@"Opening-bracket for closing of tag ""{identifier}"" was not followed by slash.");
                    }
                }
                else
                {
                    throw new AfxException($@"Opening-bracket for closing of tag ""{identifier}"" expected.");
                }
                var closingIdentifier = Identifier.Parse(lexer);
                if (closingIdentifier != identifier)
                {
                    throw new AfxException($@"Closing-tag identifier ""{closingIdentifier}"" did not match opening-tag identifier ""{identifier}"".");
                }
                if (lexer.IsClosingBracket())
                {
                    lexer.Consume();
                    return new NodeParsingResult()
                    {
                        Identifier = identifier,
                        Attributes = attributes.ToArray(),
                        Children = children,
                        SelfClosing = false
                    };
                }
                else
                {
                    throw new AfxException($@"Closing tag ""{identifier}"" did not end with closing-bracket.");
                }
            }
            catch (AfxException e)
            {
                throw new DslException($@"<{identifier}> {e.Message}");
            }
        }
    }
}
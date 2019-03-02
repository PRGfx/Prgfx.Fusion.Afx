using System.Text;
using System.Collections.Generic;

namespace Prgfx.Fusion.Afx.Expression
{
    public class NodeList
    {

        public static AstNode[] Parse(Afx.Lexer lexer)
        {
            var currentText = new StringBuilder();
            var contents = new List<AstNode>();
            while (!lexer.IsEnd())
            {
                if (lexer.IsOpeningBracket())
                {
                    lexer.Consume();
                    if (lexer.IsForwardSlash())
                    {
                        lexer.Rewind();
                        if (currentText.Length > 0)
                        {
                            contents.Add(new AstNode()
                            {
                                Type = AstNodeType.Text,
                                Payload = currentText.ToString()
                            });
                        }
                        return contents.ToArray();
                    }
                    else
                    {
                        lexer.Rewind();
                        if (currentText.Length > 0)
                        {
                            contents.Add(new AstNode()
                            {
                                Type = AstNodeType.Text,
                                Payload = currentText.ToString()
                            });
                        }
                        contents.Add(new AstNode()
                        {
                            Type = AstNodeType.Node,
                            Payload = Node.Parse(lexer)
                        });
                        currentText.Clear();
                        continue;
                    }
                }
                if (lexer.IsOpeningBrace())
                {
                    if (currentText.Length > 0)
                    {
                        contents.Add(new AstNode()
                        {
                            Type = AstNodeType.Text,
                            Payload = currentText.ToString()
                        });
                    }
                    contents.Add(new AstNode()
                    {
                        Type = AstNodeType.Expression,
                        Payload = Expression.Parse(lexer)
                    });
                    currentText.Clear();
                    continue;
                }
                currentText.Append(lexer.Consume());
            }
            if (lexer.IsEnd() && currentText.Length > 0)
            {
                contents.Add(new AstNode()
                {
                    Type = AstNodeType.Text,
                    Payload = currentText.ToString()
                });
            }
            return contents.ToArray();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Prgfx.Fusion.Afx.Expression;

namespace Prgfx.Fusion.Afx
{
    public class AfxDsl : IFusionDsl
    {
        const string INDENTATION = "  ";

        public string Transpile(string code)
        {
            var parser = new Parser(code.Trim());
            var ast = parser.Parse();
            return AstNodeListToFusion(ast, "");
        }

        protected static string AstNodeListToFusion(AstNode[] astNodes, string indentation = "")
        {
            int index = 1;
            // ignore blank text if it is connected to a newline
            var payload = astNodes
                .ToList()
                .Select(a =>
                {
                    if (a.Type == AstNodeType.Text)
                    {
                        a.Payload = Regex.Replace(a.Payload.ToString(), @"[\s]*\n[\s]*", "");
                    }
                    return a;
                })
                .Where(a => a.Type != AstNodeType.Text || a.Payload.ToString().Length > 0)
                .ToArray();
            if (payload.Length == 0)
            {
                return string.Empty;
            }
            if (payload.Length == 1)
            {
                return AstToFusion(payload[0], indentation);
            }
            var fusion = new StringBuilder("Neos.Fusion:Array {\n");
            foreach (var astNode in payload)
            {
                var fusionName = $"item_{index}";
                if (astNode.Type == AstNodeType.Node)
                {
                    var astPayload = (Node.NodeParsingResult)astNode.Payload;
                    if (astPayload.Attributes.Length > 0)
                    {
                        foreach (var attribute in astPayload.Attributes)
                        {
                            if (attribute.Type == AstNodeType.Prop)
                            {
                                var attributePayload = (Prop.PropParsingResult)attribute.Payload;
                                if (attributePayload.Identifier == "@key")
                                {
                                    if (attributePayload.Type == AstNodeType.String)
                                    {
                                        fusionName = (string)attributePayload.Payload;
                                    }
                                    else
                                    {
                                        throw new AfxException($"@key only supports string payloads, {attributePayload.Type} was given");
                                    }
                                }
                            }
                        }
                    }
                }
                var nodeFusion = AstToFusion(astNode, indentation + AfxDsl.INDENTATION);
                if (nodeFusion != null)
                {
                    fusion.Append(indentation).Append(AfxDsl.INDENTATION).Append(fusionName).Append(" = ").Append(nodeFusion).Append('\n');
                    index++;
                }
            }
            fusion.Append(indentation).Append('}');
            return fusion.ToString();
        }

        protected static string AstToFusion(AstNode astNode, string indentation)
        {
            switch (astNode.Type)
            {
                case AstNodeType.Expression:
                    return AstExpressionToFusion((string)astNode.Payload, indentation);
                case AstNodeType.String:
                    return AstStringToFusion((string)astNode.Payload, indentation);
                case AstNodeType.Text:
                    return AstTextToFusion((string)astNode.Payload, indentation);
                case AstNodeType.Boolean:
                    return AstBooleanToFusion((bool)astNode.Payload, indentation);
                case AstNodeType.Node:
                    return AstNodeToFusion((Node.NodeParsingResult)astNode.Payload, indentation);
                default:
                    throw new AfxException($"ast type {astNode.Type} is unknown");
            }
        }

        private static string AstNodeToFusion(Node.NodeParsingResult payload, string indentation)
        {
            var tagName = payload.Identifier;
            var childrenPropertyName = "content";
            var attributes = payload.Attributes.Where(a =>
            {
                if (a.Type == AstNodeType.Prop)
                {
                    var attributePayload = (Prop.PropParsingResult)a.Payload;
                    if (attributePayload.Identifier == "@key" || attributePayload.Identifier == "@path")
                    {
                        return false;
                    }
                    if (attributePayload.Identifier == "@children")
                    {
                        if (attributePayload.Type == AstNodeType.String)
                        {
                            childrenPropertyName = (string)attributePayload.Payload;
                        }
                        else
                        {
                            throw new AfxException($"@children only supports string payloads {attributePayload.Type} found");
                        }
                        return false;
                    }
                }
                return true;
            });
            var pathChildren = new Dictionary<string, AstNode>();
            var contentChildren = new List<AstNode>();
            if (payload.Children.Length > 0)
            {
                foreach (var child in payload.Children)
                {
                    if (child.Type == AstNodeType.Node)
                    {
                        string path = null;
                        var childPayload = (Node.NodeParsingResult)child.Payload;
                        foreach (var attribute in childPayload.Attributes)
                        {
                            var attributePayload = (Prop.PropParsingResult)attribute.Payload;
                            if (attribute.Type == AstNodeType.Prop && attributePayload.Identifier == "@path")
                            {
                                if (attributePayload.Type == AstNodeType.String)
                                {
                                    path = (string)attributePayload.Payload;
                                }
                                else
                                {
                                    throw new AfxException($"@path only supports string payloads {attributePayload.Type} found");
                                }
                            }
                        }
                        if (path != null)
                        {
                            pathChildren.Add(path, child);
                            continue;
                        }
                    }
                    contentChildren.Add(child);
                }
            }

            var fusion = new StringBuilder();
            string attributePrefix;
            // Tag
            if (tagName.Contains(':'))
            {
                fusion.Append(tagName).Append(" {\n");
                attributePrefix = "";
            }
            else
            {
                fusion.Append("Neos.Fusion:Tag {\n");
                fusion.Append(indentation).Append(AfxDsl.INDENTATION).Append("tagName = '").Append(tagName).Append("'\n");
                attributePrefix = "attributes.";
                if (payload.SelfClosing)
                {
                    fusion.Append(indentation).Append(AfxDsl.INDENTATION).Append("selfClosingTag = true").Append("\n");
                }
            }
            // Attributes
            var metaAttributes = new List<AstNode>();
            var fusionAttributes = new List<AstNode>();
            var spreadsOrAttributeLists = new List<AstNode>();
            if (payload.Attributes.Length > 0)
            {
                var spreadIsPresent = false;
                foreach (var attribute in payload.Attributes)
                {
                    if (attribute.Type == AstNodeType.Prop)
                    {
                        var attributePayload = (Prop.PropParsingResult)attribute.Payload;
                        if (attributePayload.Identifier[0] == '@')
                        {
                            metaAttributes.Add(attribute);
                        }
                        else if (!spreadIsPresent)
                        {
                            fusionAttributes.Add(attribute);
                        }
                        else
                        {
                            var lastPos = spreadsOrAttributeLists.Count - 1;
                            if (lastPos >= 0)
                            {
                                var last = spreadsOrAttributeLists.Last();
                                if (last.Type == AstNodeType.PropList)
                                {
                                    ((List<AstNode>)last.Payload).Add(attribute);
                                }
                            }
                            else
                            {
                                spreadsOrAttributeLists.Add(new AstNode()
                                {
                                    Type = AstNodeType.PropList,
                                    Payload = new List<AstNode>() { attribute }
                                });
                            }
                        }
                    }
                    else if (attribute.Type == AstNodeType.Spread)
                    {
                        spreadsOrAttributeLists.Add(attribute);
                        spreadIsPresent = true;
                    }
                }

                if (fusionAttributes.Count > 0)
                {
                    fusion.Append(PropListToFusion(fusionAttributes.ToArray(), attributePrefix, indentation));
                }

                // starting with the first spread we render spreads as @apply expressions
                // and attributes as @apply of the respective propList
                var spreadIndex = 1;
                foreach (var attribute in spreadsOrAttributeLists)
                {
                    if (attribute.Type == AstNodeType.Spread)
                    {
                        var attributePayload = (AstNode)attribute.Payload;
                        if (attributePayload.Type == AstNodeType.Expression)
                        {
                            var spreadFusion = AstToFusion(attributePayload, indentation + AfxDsl.INDENTATION);
                            if (spreadFusion != null)
                            {
                                fusion.Append(indentation).Append(AfxDsl.INDENTATION)
                                    .Append(attributePrefix).Append("@apply.spread_").Append(spreadIndex).Append(" = ").AppendLine(spreadFusion);
                            }
                        }
                        else
                        {
                            throw new AfxException($"Spreads only support expression payloads {attributePayload.Type} found");
                        }
                    }
                    else if (attribute.Type == AstNodeType.PropList)
                    {
                        var attributePayload = (List<AstNode>)attribute.Payload;
                        fusion
                            .Append(indentation).Append(AfxDsl.INDENTATION)
                                .Append(attributePrefix).Append("@apply.spread_").Append(spreadIndex).AppendLine(" = Neos.Fusion:RawArray {")
                            .Append(PropListToFusion(attributePayload.ToArray(), "", indentation + AfxDsl.INDENTATION))
                            .Append(indentation).Append(AfxDsl.INDENTATION).AppendLine("}");
                    }
                    spreadIndex++;
                }

                if (metaAttributes.Count > 0)
                {
                    fusion.Append(PropListToFusion(metaAttributes.ToArray(), "", indentation));
                }
            }

            // Path Children
            if (pathChildren.Count > 0)
            {
                foreach (var kv in pathChildren)
                {
                    fusion.Append(indentation).Append(AfxDsl.INDENTATION).Append(kv.Key).Append(" = ").AppendLine(AstToFusion(kv.Value, indentation + AfxDsl.INDENTATION));
                }
            }

            // Content Children
            if (contentChildren.Count > 0)
            {
                var childFusion = AstNodeListToFusion(contentChildren.ToArray(), indentation + AfxDsl.INDENTATION);
                if (childFusion.Length > 0)
                {
                    fusion.Append(indentation).Append(AfxDsl.INDENTATION).Append(childrenPropertyName).Append(" = ").AppendLine(childFusion);
                }
            }

            fusion.Append(indentation).Append('}');

            return fusion.ToString();
        }

        private static string PropListToFusion(AstNode[] payload, string attributePrefix, string indentation)
        {
            var fusion = new StringBuilder();
            foreach (var attribute in payload)
            {
                if (attribute.Type == AstNodeType.Prop)
                {
                    var attributePayload = (Prop.PropParsingResult)attribute.Payload;
                    var propName = attributePayload.Identifier;
                    var propFusion = AstToFusion(
                        new AstNode()
                        {
                            Type = attributePayload.Type,
                            Payload = attributePayload.Payload
                        },
                        indentation + AfxDsl.INDENTATION);
                    if (propFusion != null)
                    {
                        fusion.Append(indentation).Append(AfxDsl.INDENTATION).Append(attributePrefix).Append(propName).Append(" = ").AppendLine(propFusion);
                    }
                }
            }
            return fusion.ToString();
        }

        private static string AstBooleanToFusion(bool payload, string indentation)
        {
            return "true";
        }

        private static string AstTextToFusion(string payload, string indentation)
        {
            return "'" + Regex.Replace(payload, @"[\000\010\011\012\015\032\042\047\134\140]", "\\$0") + "'";
        }

        private static string AstStringToFusion(string payload, string indentation)
        {
            return "'" + payload.Replace("'", "\\'") + "'";
        }

        private static string AstExpressionToFusion(string payload, string indentation)
        {
            return "${" + payload + "}";
        }

    }

    public class AfxException : System.Exception
    {
        public AfxException(string message) : base(message) { }
    }
}
namespace Prgfx.Fusion.Afx.Expression
{
    public enum AstNodeType
    {
        String,
        Expression,
        Boolean,
        Node,
        Prop,
        PropList,
        Text,
        Spread,
    }

    public struct AstNode
    {
        public AstNodeType Type;
        public object Payload;
    }
}
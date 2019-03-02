namespace Prgfx.Fusion.Afx
{
    class Parser
    {

        protected string code;

        protected Lexer lexer;

        public Parser(string code)
        {
            this.code = code;
            this.lexer = new Lexer(code);
        }

        public Expression.AstNode[] Parse()
        {
            return Expression.NodeList.Parse(lexer);
        }
    }

}
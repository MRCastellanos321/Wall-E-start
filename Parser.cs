namespace Compiler
{
    public class Parser
    {
        public List<Token> tokens;
        private int position = 0; //es separada de la de las otras clases

        public Parser(List<Token> Tokens)
        {
            tokens = Tokens;
        }
        public List<Statement> ParsePrograma()
        {
            var statements = new List<Statement>();
            while (Peek().type != TokenType.EOF)
            {
                statements.Add(CheckStatement(Peek()));
            }
            return statements;
        }
        private Token Peek()
        {
            if (position >= tokens.Count)
            {
                return tokens[tokens.Count - 1];
            }
            return tokens[position];
        }
        private Token IsTokenValid(Token token, TokenType type, int position)
        {
            if (type == token.type)
            {
                position++;
                return token;
            }
            //else logica de error para no valido
        }

        public Statement CheckStatement(Token token)
        {

        }

    }
}
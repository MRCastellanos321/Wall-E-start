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
        private Token IsTokenExpected(Token token, TokenType type, int position)
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
            //cambiar a un diccionario
            if (token.lexeme == "SpawnPoint")
            {
                position++;
                IsTokenExpected(tokens[position], TokenType.LEFT_PAREN, position);
                //esto no debe ser asi pq hay q ver algún tipo de lógica con lo de grouping expression
                //algo para poner la lista de parametros
            }
        }

        public Expr CheckExpression(Token token)
        {
            if (token.lexeme == )
            {
                position++;

            }

        }
        
        //lógica individual para cada una una vez encuentre un statement?
        public void ParseSpawnPoint() { }
        public void ParseBinaryExpression() { }
        public void ParseUnaryExpression() { }
        public void ParseGroupingExpression() { }
        public void ParseIsBrushColor() { }
        public void ParseActualX() { }
        public void ParseActualY() { }
        public void ParseIsCanvasColor() { }
        public void ParseGetCanvasSize() { }
        public void GetColorCount() { }
        public void IsColor() { }

    }

}

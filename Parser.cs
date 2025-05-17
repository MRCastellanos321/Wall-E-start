namespace Compiler
{
    public class Parser
    {
        public List<Token> tokens;
        private int position = 0; //es separada de la de las otras clases
        private readonly Dictionary<string, Func<Statement>> statementParsers;
        public Parser(List<Token> Tokens)
        {
            tokens = Tokens;
            statementParsers = new Dictionary<string, Func<Statement>>()
         {
            { "SpawnPoint", ParseSpawnPoint},
            {"IsCanvasColor", ParseIsCanvasColor},
            {"IsBrushSize", ParseIsBrushColor},
            { "ActualX", ParseActualX},
            { "ActualY", ParseActualY},
            {"GetCanvasSize", ParseGetCanvasSize},
            {"GetColorCount", ParseGetColorCount},
         };
        }
        public List<Statement> ParsePrograma()
        {
            var statements = new List<Statement>();
            while (Peek().type != TokenType.EOF)
            {
                statements.Add(ParseStatement());
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

        private Token Advance()
        {
            if (position < tokens.Count)
                return tokens[position++];
            return Peek();
        }

        private bool Match(TokenType type)
        {
            if (Peek().type == type)
            {
                Advance();
                return true;
            }
            return false;
        }

        private Token Consume(TokenType type, string errorMessage)
        {
            if (Peek().type == type)
            return Advance();
            throw new Exception($"Error en {Peek().line}. Se esperaba {errorMessage}, pero se encontró '{Peek().lexeme}'");
        }

        private Statement ParseStatement()
        {
            //aquí hay que hacer algo para comprobar cuando se habla de funciones y cuando no
            if (Peek().type == TokenType.)
            {
                string lexeme = Peek().lexeme;
                if (statementParsers.TryGetValue(lexeme, out Func<Statement> parseMethod))
                {
                    return parseMethod();
                }
            }
            // sino es función, es un expression statement
            //por aquí hay que comprobar por algún error?
            return ParseExpressionStatement();
        }
        public Statement ParseExpressionStatement()
        {
            Expr expr = ParseExpression();
            Consume(TokenType.SEMICOLON, "un ;");
            return new ExpressionStmt(expr);
        }
        public Expr ParseExpression()
        {

        }
        //lógica individual para cada una una vez encuentre un statement?
        public Statement ParseSpawnPoint()
        {
            Token funcToken = Consume(TokenType.SPAWN_POINT, "Spawn");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            List<Expr> parameters = new List<Expr>();
            if (!Match(TokenType.RIGHT_PAREN))
            {
                do
                {
                 parameters.Add(ParseExpression());
                } while (Match(TokenType.COMMA));
                Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
                if (parameters.Count != 2)
                {
                    throw new Exception($"Error de sintaxis en {funcToken.line}: Spawn requiere dos argumentos (int, int). Se recibieron {parameters.Count}.");
                }
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (!(parameters[i] is LiteralExpr literal))
                    {
                        throw new Exception($"Error de tipo en {funcToken.line}: el argumento {i + 1} de Spawn debe ser un literal.");
                    }
                    object value = literal.Value;

                    if (!(value is int))
                    {
                        throw new Exception($"Error de tipo en {funcToken.line}: el argumento {i + 1} de Spawn debe ser un entero.");
                    }
                }

            }
            return new CallFunction(TokenType.SPAWN_POINT, parameters);
        }
        public Statement ParseBinaryExpression() { }
        public Statement ParseUnaryExpression() { }
        public Statement ParseGroupingExpression() { }
        public Statement ParseIsBrushColor() { }
        public Statement ParseActualX() { }
        public Statement ParseActualY() { }
        public Statement ParseIsCanvasColor() { }
        public Statement ParseGetCanvasSize() { }
        public Statement ParseGetColorCount() { }
        public Statement ParseIsColor() { }

    }

}

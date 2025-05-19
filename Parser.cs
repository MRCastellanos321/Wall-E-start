using System.Data.Common;

namespace Compiler
{
    public class Parser
    {
        public List<Token> tokens;
        private int position = 0; //es separada de la de las otras clases
        private readonly Dictionary<string, Func<Statement>> commandParsers;
        private readonly Dictionary<string, Func<Expr>> exprFunctionParsers;
        private readonly Dictionary<string, string> colors;
        //  BLUE, RED, GREEN, YELLOW, PURPLE, BLACK, WHITE, GREY, TRANSPARENT,
        public Parser(List<Token> Tokens)
        {
            tokens = Tokens;
            commandParsers = new Dictionary<string, Func<Statement>>()
         {
            { "SpawnPoint", ParseSpawnPoint},
            { "Color", ParseColor},
            { "Size", ParseSize},
            { "DrawLine", ParseDrawLine},
            { "DrawRectangle", ParseDrawRectangle},
            { "DrawCirle", ParseDrawCircle},
         };
          exprFunctionParsers = new Dictionary<string, Func<Expr>>()
         {
            { "IsBrushColor", ParseIsBrushColor},
            { "IsCanvasColor", ParseIsCanvasColor},
            {"IsBrushSize", ParseIsBrushColor},
            { "ActualX", ParseActualX},
            { "ActualY", ParseActualY},
            {"GetCanvasSize", ParseGetCanvasSize },
            {"GetCanvasSize", ParseGetCanvasSize},
            {"GetColorCount", ParseGetColorCount},
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
            {
                return Advance();
            }
            throw new Exception($"Error en {Peek().line}. Se esperaba {errorMessage}, pero se encontró '{Peek().lexeme}'");
        }
        private Token LookAhead(int offset)
        {
            if (position + offset < tokens.Count)
            {
                return tokens[position + offset];
            }
            return tokens[tokens.Count - 1];
        }

        private Statement ParseStatement()
        {
            //aquí hay que hacer algo para comprobar cuando se habla de funciones y cuando no
            if (Lexer.keyWords.TryGetValue(Peek().lexeme, out TokenType type))
            {
                string lexeme = Peek().lexeme;
                if (commandParsers.TryGetValue(lexeme, out Func<Statement> parseMethod))
                {
                    return parseMethod();
                }
            }
            // sino es función, es una declaración de variable o está nombrando un loop?
            if (Peek().type == TokenType.IDENTIFIER && LookAhead(1).type == TokenType.ARROW)
            {
                return ParseAssignmentStatement();
            }
        }

        public Statement ParseAssignmentStatement()
        {
            Token ident = Consume(TokenType.IDENTIFIER, "un identificador en la asignación");
            Consume(TokenType.ARROW, "una flecha '<-' en la asignación");
            Expr value = ParseExpression(); // La expresión debe existir
            return new VarDeclaration(ident.lexeme, value);
        }

        public Expr ParseExpression()
        {
            Token token = Advance();
            if (token.type == TokenType.NUMBER || token.type == TokenType.STRING)
            {
                return new LiteralExpr(token.lexeme);
            }
            if (exprFunctionParsers.TryGetValue(token.lexeme, out Func<Expr> parseMethod))
            {
                if (LookAhead(1).type == TokenType.LEFT_PAREN)
                {
                    return parseMethod();
                }
                throw new Exception($"Error en {token.line}: la llamada a función {token.lexeme} debe seguirse de paréntesis");
            }
            if (token.type == TokenType.LEFT_PAREN)
            {
                Expr expr = ParseExpression();
                Consume(TokenType.RIGHT_PAREN, "un ) para cerrar la expresión");
                return new GroupingExpr(expr);
            }
            throw new Exception($"Error en {token.line}: Expresión no válida.");
        }
        public Statement ParseVarDeclaration()
        {
            Token ident = Consume(TokenType.IDENTIFIER, "un identificador para la variable");
            Expr initializer = null;
            if (Match(TokenType.ARROW))
            {
                //esto de compobar la función va en realidad dentro de Parse expresion
                if (commandParsers.TryGetValue(Peek().lexeme, out Func<Statement> parseMethod))
                {
                    //initializer = parseMethod(); tengo que hacer algo para cuando se iguala a una función
                }
                initializer = ParseExpression();
            }
            if (initializer != null)
            {
                return new VarDeclaration(ident.lexeme, initializer);
            }
            throw new Exception($"Error en {ident.line}: variable {ident.lexeme} no inicializada.");
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
            return new CallComand(TokenType.SPAWN_POINT, parameters);
        }
        public Statement ParseBinaryExpression()
        {
        }
        public Statement ParseUnaryExpression() { }
        public Statement ParseGroupingExpression() { }
        public Expr ParseIsBrushColor()
        {
            Token funcToken = Consume(TokenType.IS_BRUSH_COLOR, "IsBrushColor");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            List<Expr> parameters = new List<Expr>();
            if (!Match(TokenType.RIGHT_PAREN))
            {
                do
                {
                    parameters.Add(ParseExpression());
                } while (Match(TokenType.COMMA));
                Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
                if (parameters.Count != 1)
                {
                    throw new Exception($"Error de sintaxis en {funcToken.line}: IsColorBrush requiere 1 argumento (Color). Se recibieron {parameters.Count}.");
                }

                if (!(parameters[0] is LiteralExpr literal))
                {
                    throw new Exception($"Error de tipo en {funcToken.line}: el argumento de IsColorBrush debe ser un literal.");
                }

                if (!(parameters[1] is ))
                {
                    throw new Exception($"Error de tipo en {funcToken.line}: el argumento de IsColorBrush debe ser un color válido.");
                }
            }
        }
        public Expr ParseActualX() { }
        public Expr ParseActualY() { }
        public Expr ParseIsCanvasColor() { }
        public Expr ParseGetCanvasSize() { }
        public Expr ParseGetColorCount() { }
        public Expr ParseIsColor() { }
        public Statement ParseColor() { }
        public Statement ParseSize() { }
        public Statement ParseDrawLine() { }
        public Statement ParseDrawCircle() { }
        public Statement ParseDrawRectangle() { }

    }

}

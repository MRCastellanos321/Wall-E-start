
namespace Compiler
{
    public class Parser
    {
        public List<Token> tokens;
        private int position = 0;
        private readonly Dictionary<string, Func<Statement>> commandParsers;
        private readonly Dictionary<string, Func<Expr>> exprFunctionParsers;
        private readonly HashSet<string> colors;
        public Parser(List<Token> Tokens)
        {
            tokens = Tokens;
            commandParsers = new Dictionary<string, Func<Statement>>()
         {
            { "Spawn", ParseSpawnPoint},
            { "Color", ParseColor},
            { "Size", ParseSize},
            { "DrawLine", ParseDrawLine},
            { "DrawRectangle", ParseDrawRectangle},
            { "DrawCircle", ParseDrawCircle},
            {"Fill", ParseFill}
         };
            exprFunctionParsers = new Dictionary<string, Func<Expr>>()
         {
            { "IsBrushColor", ParseIsBrushColor},
            { "IsCanvasColor", ParseIsCanvasColor},
            {"IsBrushSize", ParseIsBrushColor},
            { "ActualX", ParseActualX},
            { "ActualY", ParseActualY},
            {"GetCanvasSize", ParseGetCanvasSize},
            {"GetColorCount", ParseGetColorCount},
         };
            colors = new HashSet<string> { "Blue", "Green", "Purple", "Yellow", "Grey", "Transparent", "White", "Red", "Black" };
        }
        public List<ASTNode> ParsePrograma()
        {
            var statements = new List<ASTNode>();
            while (Peek().type != TokenType.EOF)
            {
                statements.Add(ParseStatements());
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
        private Token Previous()
        {
            if (position - 1 < 0)
                return tokens[0];
            return tokens[position - 1];
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

        private void ConsumeNewLineAfterStatement()
        {
            while (Match(TokenType.NEW_LINE)) ;
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

        private ASTNode ParseStatements()
        {
            //después revisar que pasa cuando hay una función antes de EOF y luego no hay salto de línea
            if (Peek().type == TokenType.NEW_LINE)
            {
                ConsumeNewLineAfterStatement();
            }
            string lexeme = Peek().lexeme;
            if (commandParsers.TryGetValue(lexeme, out Func<Statement> parseMethod))
            {
                Statement func = parseMethod();
                ConsumeNewLineAfterStatement();
                return func;
            }
            if (exprFunctionParsers.TryGetValue(lexeme, out Func<Expr> exprParseMethod))
            {
                Statement exprFunc = parseMethod();
                ConsumeNewLineAfterStatement();
                return exprFunc;
            }

            if (Peek().type == TokenType.IDENTIFIER && LookAhead(1).type == TokenType.ARROW)
            {
                Statement varAsig = ParseAssignmentStatement();
                ConsumeNewLineAfterStatement();
                return varAsig;
            }
            if (Peek().type == TokenType.IDENTIFIER && LookAhead(1).type == TokenType.NEW_LINE)
            {
                Statement label = ParseLabelDeclaration();
                ConsumeNewLineAfterStatement();
                return label;

            }
            if (Peek().type == TokenType.GO_TO)
            {
                Statement goTo = ParseLabelStatement();
                ConsumeNewLineAfterStatement();
                return goTo;
            }

            throw new Exception($"Statement no identificado en la línea {Peek().line}");
        }

        public Statement ParseAssignmentStatement()
        {
            Token ident = Consume(TokenType.IDENTIFIER, "un identificador en la asignación");
            Consume(TokenType.ARROW, "una flecha '<-' en la asignación");
            Expr value = ParseExpression();
            //if (value is CallFunction || value is BinaryExpr || value is BinaryExpr || value is UnaryExpr || value is GroupingExpr  )
            return new VarDeclaration(ident.lexeme, value);
        }
        public Expr ParseExpression()
        {
            return ParseBinaryExpression();
        }
        public Expr ParseBinaryExpression()
        {
            Expr left = ParsePrimary();


            var binaryOperators = new[] {
        TokenType.EQUAL_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL,
        TokenType.GREATER, TokenType.GREATER_EQUAL,
        TokenType.PLUS, TokenType.MINUS,
        TokenType.MULTIPLY, TokenType.DIVIDE
        };

            while (binaryOperators.Contains(Peek().type))
            {
                Token op = Advance();
                Expr right = ParsePrimary();
                left = new BinaryExpr(left, op.lexeme, right);
            }

            return left;
        }
        private Expr ParsePrimary()
        {
            if (Peek().type == TokenType.NUMBER || Peek().type == TokenType.STRING)
            {
                Token tokenNumber = Peek();
                Advance();
                return new LiteralExpr(tokenNumber.literal);
            }

            if (exprFunctionParsers.TryGetValue(Peek().lexeme, out Func<Expr> parseMethod))
            {
                return parseMethod();
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                Advance();
                Expr expr = ParseExpression();
                Consume(TokenType.RIGHT_PAREN, " ')' luego de expresión");
                return new GroupingExpr(expr);
            }

            if (Peek().type == TokenType.IDENTIFIER)
            {
                if (Peek().lexeme.Contains('-'))
                {
                    throw new Exception($"Error en {Peek().line}: Identificador inválido");
                }
                //? 
                Token token = Advance();
                return new VariableExpr(token.lexeme);
            }
            if (Match(TokenType.MINUS))
            {
                Token op = Previous();
                Expr right = ParsePrimary();
                return new UnaryExpr(op.lexeme, right);
            }

            throw new Exception($"Error en {Peek().line}: Expresión no reconocida");
        }



        private Statement ParseLabelStatement()
        {
            Consume(TokenType.GO_TO, "GoTo");
            Consume(TokenType.LEFT_BRACKET, "corchete izquierdo '['");
            Token labelToken = Consume(TokenType.IDENTIFIER, "nombre de etiqueta");
            Consume(TokenType.RIGHT_BRACKET, "corchete derecho ']'");
            Consume(TokenType.LEFT_PAREN, "paréntesis izquierdo '('");
            //aquí seguramente hay que ver algo separado pq la condición tiene que ser bool
            Expr condition = ParseExpression();
            Consume(TokenType.RIGHT_PAREN, "paréntesis derecho ')'");
            return new GoToStatement(labelToken.lexeme, condition);
        }

        /*       private void ValidateVarIdentifier()
                {
                    if (Peek().lexeme.Contains('-'))
                    {
                        throw new Exception($"Error en {Peek().line}: el identificador de variable {Peek().lexeme} no puede contener el caracter '-'");
                    }

                }
                private void ValidateLabelIdentifier()

                {
                    if (Peek().lexeme.Contains('_'))
                    {
                        throw new Exception($"Error en {Peek().line}: el label {Peek().lexeme} no puede contener el caracter '_'");
                    }

                }*/
        private Statement ParseLabelDeclaration()
        {
            Token labelToken = Consume(TokenType.IDENTIFIER, "nombre de etiqueta");
            Consume(TokenType.NEW_LINE, "salto de línea");
            return new LabelDeclaration(labelToken.lexeme);
        }


        private void CheckParameters(int index, int count, List<Expr> parameters, string func)
        {
            for (int i = index; i < parameters.Count; i++)
            {
                if (!(parameters[i] is LiteralExpr literal))
                {
                    throw new Exception($"Error de tipo en {Peek().line}: el argumento {i + 1} de {func} debe ser un literal.");
                }
                object value = literal.Value;

                if (!(value is int))
                {
                    throw new Exception($"Error de tipo en {Peek().line}: el argumento {i + 1} de {func} debe ser un entero.");
                }
            }
        }
        private List<Expr> ParseParameters()
        {
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            List<Expr> parameters = new List<Expr>();
            do
            {
                parameters.Add(ParseExpression());
            } while (Match(TokenType.COMMA));
            //quizás debamos pensar algo aquí si hay un problema con las comas
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            return parameters;
        }
        //anadir una que revise el color y modificar esta para que revise la cuenta, ver si hay algo que hacer con respecto a la coma y error


        public Statement ParseSpawnPoint()
        {
            Token funcToken = Consume(TokenType.SPAWN_POINT, "Spawn");
            List<Expr> parameters = ParseParameters();
            if (parameters.Count != 2)
            {
                throw new Exception($"Error de sintaxis en {funcToken.line}: Spawn requiere dos argumentos (int, int). Se recibieron {parameters.Count}.");
            }
            CheckParameters(0, 2, parameters, "Spawn");
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallComand(TokenType.SPAWN_POINT, parameters);
        }
        public Expr ParseIsBrushColor()
        {
            Token funcToken = Consume(TokenType.IS_BRUSH_COLOR, "IsBrushColor");
            List<Expr> parameters = ParseParameters();
            if (parameters.Count != 1)
            {
                throw new Exception($"Error de sintaxis en {funcToken.line}: IsColorBrush requiere 1 argumento (Color). Se recibieron {parameters.Count}.");
            }
            if ((parameters[0] is LiteralExpr literal) && literal.Value is string colorValue)
            {
                if (!colors.Contains(colorValue))
                {
                    throw new Exception($"Error en {funcToken.line}: IsBrushColor debe recibir un color definido válido.");
                }
            }
            else
            {
                throw new Exception($"Error en {funcToken.line}: IsBrushColor debe recibir un string válido.");
            }
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallFunction(TokenType.IS_BRUSH_COLOR, parameters);
        }
        public Expr ParseActualX()
        {
            Consume(TokenType.GET_ACTUAL_X, "ActualX");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallFunction(TokenType.GET_ACTUAL_X, new List<Expr>());
        }

        public Expr ParseActualY()
        {
            Consume(TokenType.GET_ACTUAL_Y, "ActualY");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallFunction(TokenType.GET_ACTUAL_Y, new List<Expr>());
        }

        public Expr ParseGetCanvasSize()
        {
            Consume(TokenType.GET_CANVAS_SIZE, "GetCanvasSize");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallFunction(TokenType.GET_CANVAS_SIZE, new List<Expr>());
        }
        public Expr ParseGetColorCount()
        {
            Token funcToken = Consume(TokenType.GET_COLOR_COUNT, "GetColorCount");
            List<Expr> parameters = ParseParameters();
            if ((parameters[0] is LiteralExpr literal) && literal.Value is string colorValue)
            {
                if (!colors.Contains(colorValue))
                {
                    throw new Exception($"Error en {funcToken.line}: GetColorCount debe recibir un color definido válido como primer parámetro.");
                }
            }
            else
            {
                throw new Exception($"Error en {funcToken.line}: GetColorCount debe recibir un string válido como primer parámetro.");
            }
            CheckParameters(1, 5, parameters, "GetColorCount");
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallFunction(TokenType.GET_COLOR_COUNT, parameters);
        }
        //public Expr ParseIsColor() { }
        public Expr ParseIsCanvasColor()
        {
            Token funcToken = Consume(TokenType.IS_CANVAS_COLOR, "IsCanvasColor");
            List<Expr> parameters = ParseParameters();
            if (parameters.Count != 3)
            {
                throw new Exception($"Error en {funcToken.line}: IsCanvasColor requiere 3 parámetros (color, vertical, horizontal).");
            }
            if ((parameters[0] is LiteralExpr literal) && literal.Value is string colorValue)
            {
                if (!colors.Contains(colorValue))
                {
                    throw new Exception($"Error en {funcToken.line}: IsCanvasColor debe recibir un color definido válido como primer parámetro.");
                }
            }
            else
            {
                throw new Exception($"Error en {funcToken.line}: IsCanvasColor debe recibir un string válido como primer parámetro.");
            }
            CheckParameters(1, 3, parameters, "IsCanvasColor");
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallFunction(TokenType.IS_CANVAS_COLOR, parameters);
        }
        public Statement ParseColor()
        {
            Token funcToken = Consume(TokenType.COLOR, "Color");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Expr colorExpr = ParseExpression();
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");

            if ((colorExpr is LiteralExpr literal) && literal.Value is string colorValue)
            {
                if (!colors.Contains(colorValue))
                {
                    throw new Exception($"Error en {funcToken.line}: Color debe recibir un color definido válido.");
                }
            }
            else
            {
                throw new Exception($"Error en {funcToken.line}: Color debe recibir un string válido.");
            }

            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallComand(TokenType.COLOR, new List<Expr> { colorExpr });
        }
        public Statement ParseSize()
        {
            Token funcToken = Consume(TokenType.SIZE, "Size");
            List<Expr> parameters = ParseParameters();
            if (parameters.Count != 1) { throw new Exception($"Error en {funcToken.line}: Size requiere 1 parámetro (int)"); }
            CheckParameters(0, 1, parameters, "Size");
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallComand(TokenType.SIZE, new List<Expr> { parameters[0] });
        }

        public Statement ParseDrawLine()
        {

            Token funcToken = Consume(TokenType.DRAW_LINE, "DrawLine");
            List<Expr> parameters = ParseParameters();
            if (parameters.Count != 3)
            {
                throw new Exception($"Error en {funcToken.line}: DrawLine requiere 3 parámetros.");
            }
            CheckParameters(0, 3, parameters, "DrawLine");
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallComand(TokenType.DRAW_LINE, parameters);
        }

        public Statement ParseDrawCircle()
        {
            Token funcToken = Consume(TokenType.DRAW_CIRCLE, "DrawCircle");
            List<Expr> parameters = ParseParameters();
            if (parameters.Count != 3)
            {
                throw new Exception($"Error en {funcToken.line}: DrawCircle requiere 3 parámetros (dirX, dirY, radius).");
            }
            CheckParameters(0, 3, parameters, "DrawCircle");
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallComand(TokenType.DRAW_CIRCLE, parameters);
        }
        public Statement ParseDrawRectangle()
        {
            Token funcToken = Consume(TokenType.DRAW_RECTANGLE, "DrawRectangle");
            List<Expr> parameters = ParseParameters();
            if (parameters.Count != 5)
            {
                throw new Exception($"Error en {funcToken.line}: DrawRectangle requiere 5 parámetros (dirX, dirY, distance, width, height).");
            }
            CheckParameters(0, 5, parameters, "DrawRectangle");
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallComand(TokenType.DRAW_RECTANGLE, parameters);
        }
        public Statement ParseFill()
        {
            Consume(TokenType.FILL, "Fill");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallComand(TokenType.FILL, new List<Expr>());
        }
    }

}


namespace Compiler
{
    public class Parser
    {
        public List<Token> tokens;
        private int position = 0; //es separada de la de las otras clases
        private readonly Dictionary<string, Func<Statement>> commandParsers;
        private readonly Dictionary<string, Func<Expr>> exprFunctionParsers;
        private readonly Dictionary<string, string> colors;
        //  
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
            { "DrawCircle", ParseDrawCircle},
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

            colors = new Dictionary<string, string>()
         {
            {"Blue","Blue"},
            { "Red", "Red"},
            { "Green", "Green"},
            { "Yellow", "Yellow" },
            {"Purple","Purple"},
            {"Black","Black"},
            { "White","White"},
            {"Grey", "Grey"},
            { "Transparent", "Transparent" },
         };
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
            //aquí debe haber algo respecto al salto de línea
            string lexeme = Peek().lexeme;
            if (commandParsers.TryGetValue(lexeme, out Func<Statement> parseMethod))
            {
                return parseMethod();
            }
            else if (exprFunctionParsers.TryGetValue(lexeme, out Func<Expr> exprParseMethod))
            {
                return exprParseMethod();
            }

            else if (Peek().type == TokenType.IDENTIFIER && LookAhead(1).type == TokenType.ARROW)
            {
                return ParseAssignmentStatement();
            }
            else if (Peek().type == TokenType.IDENTIFIER && LookAhead(1).type == TokenType.NEW_LINE)
            {
                ParseLabelDeclaration();
            }
            else if (Peek().type == TokenType.GO_TO)
            {
                ParseLabelStatement();
            }

            throw new Exception($"Statement no identificado en la línea {Peek().line}");
        }

        public Statement ParseAssignmentStatement()
        {
            ValidateVarIdentifier();
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
            if (Peek().type == TokenType.NUMBER)
            {

                return new LiteralExpr(Peek().lexeme);
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

            ValidateLabelIdentifier();
            Token labelToken = Consume(TokenType.IDENTIFIER, "nombre de etiqueta");
            ValidateLabelIdentifier();

            Consume(TokenType.RIGHT_BRACKET, "corchete derecho ']'");
            Consume(TokenType.LEFT_PAREN, "paréntesis izquierdo '('");
            //aquí seguramente hay que ver algo separado pq la condición tiene que ser bool
            Expr condition = ParseExpression();
            Consume(TokenType.RIGHT_PAREN, "paréntesis derecho ')'");

            return new GoToStatement(labelToken.lexeme, condition);
        }
        private void ValidateVarIdentifier()
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

        }
        private Statement ParseLabelDeclaration()
        {
            ValidateLabelIdentifier();
            Token labelToken = Consume(TokenType.IDENTIFIER, "nombre de etiqueta");
            Consume(TokenType.NEW_LINE, "salto de línea después de etiqueta");
            return new LabelDeclaration(labelToken.lexeme);
        }
    



        public Statement ParseSpawnPoint()
        {
            Token funcToken = Consume(TokenType.SPAWN_POINT, "Spawn");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            List<Expr> parameters = new List<Expr>();
            do
            {
                parameters.Add(ParseExpression());
            } while (Match(TokenType.COMMA));
            //quizás debamos pensar algo aquí si hay un problema con las comas
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
            return new CallComand(TokenType.SPAWN_POINT, parameters);
        }
        public Expr ParseIsBrushColor()
        {
            Token funcToken = Consume(TokenType.IS_BRUSH_COLOR, "IsBrushColor");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            List<Expr> parameters = new List<Expr>();
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
            string parameterColor;
            if (!colors.TryGetValue(((LiteralExpr)parameters[0]).Value.ToString(), out parameterColor))
            {
                throw new Exception($"Error de tipo en {funcToken.line}: el argumento de IsColorBrush debe ser un color válido.");
            }
            return new CallFunction(TokenType.IS_BRUSH_COLOR, parameters);
        }
        public Expr ParseActualX()
        {
            Consume(TokenType.GET_ACTUAL_X, "ActualX");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            return new CallFunction(TokenType.GET_ACTUAL_X, new List<Expr>());
        }

        public Expr ParseActualY()
        {
            Consume(TokenType.GET_ACTUAL_Y, "ActualY");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            return new CallFunction(TokenType.GET_ACTUAL_Y, new List<Expr>());
        }

        public Expr ParseGetCanvasSize()
        {
            Consume(TokenType.GET_CANVAS_SIZE, "GetCanvasSize");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            return new CallFunction(TokenType.GET_CANVAS_SIZE, new List<Expr>());
        }
        public Expr ParseGetColorCount()
        {
            Token funcToken = Consume(TokenType.GET_COLOR_COUNT, "GetColorCount");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            List<Expr> parameters = new List<Expr>();

            if (!Match(TokenType.RIGHT_PAREN))
            {
                do
                {
                    parameters.Add(ParseExpression());
                } while (Match(TokenType.COMMA));

                Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");

                if (parameters.Count != 5)
                {
                    throw new Exception($"Error en {funcToken.line}: GetColorCount requiere 5 parámetros (color, x1, y1, x2, y2)");
                }

                if (!(parameters[0] is LiteralExpr) || !colors.ContainsKey(((LiteralExpr)parameters[0]).Value.ToString()))
                {
                    throw new Exception($"Error en {funcToken.line}: El primer parámetro debe ser un color válido");
                }
            }

            return new CallFunction(TokenType.GET_COLOR_COUNT, parameters);
        }
        public Expr ParseIsColor() { }

        //las funciones todavía no tienen comprobacion de int
        public Expr ParseIsCanvasColor()
        {
            Token funcToken = Consume(TokenType.IS_CANVAS_COLOR, "IsCanvasColor");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            List<Expr> parameters = new List<Expr>();

            if (!Match(TokenType.RIGHT_PAREN))
            {
                do
                {
                    parameters.Add(ParseExpression());
                } while (Match(TokenType.COMMA));

                Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");

                if (parameters.Count != 3)
                {
                    throw new Exception($"Error en {funcToken.line}: IsCanvasColor requiere 3 parámetros (color, vertical, horizontal).");
                }

                if (!(parameters[0] is LiteralExpr) || !colors.ContainsKey(((LiteralExpr)parameters[0]).Value.ToString()))
                {
                    throw new Exception($"Error en {funcToken.line}: El primer parámetro debe ser un color válido.");
                }
            }

            return new CallFunction(TokenType.IS_CANVAS_COLOR, parameters);
        }
        public Statement ParseColor()
        {
            Token funcToken = Consume(TokenType.COLOR, "Color");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Expr colorExpr = ParseExpression();
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");

            if (!(colorExpr is LiteralExpr literal) || !colors.ContainsKey(literal.Value.ToString()))
            {
                throw new Exception($"Error en {funcToken.line}: Color debe recibir un literal válido.");
            }

            return new CallComand(TokenType.COLOR, new List<Expr> { colorExpr });
        }
        public Statement ParseSize()
        {
            Token funcToken = Consume(TokenType.SIZE, "Size");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Expr sizeExpr = ParseExpression();
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");

            if (!(sizeExpr is LiteralExpr))
            {
                throw new Exception($"Error en {funcToken.line}: Size debe ser un entero.");
            }

            return new CallComand(TokenType.SIZE, new List<Expr> { sizeExpr });
        }

        public Statement ParseDrawLine()
        {

            Token funcToken = Consume(TokenType.DRAW_LINE, "DrawLine");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            List<Expr> parameters = new List<Expr>();
            if (!Match(TokenType.RIGHT_PAREN))
            {
                do
                {
                    parameters.Add(ParseExpression());
                } while (Match(TokenType.COMMA));

                Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");

                if (parameters.Count != 3)
                {
                    throw new Exception($"Error en {funcToken.line}: DrawLine requiere 3 parámetros.");
                }
            }
            else
            {
                throw new Exception($"Error en {funcToken.line}: DrawLine requiere 3 parámetros.");
            }

            return new CallComand(TokenType.DRAW_LINE, parameters);
        }

        public Statement ParseDrawCircle()
        {
            Token funcToken = Consume(TokenType.DRAW_CIRCLE, "DrawCircle");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            List<Expr> parameters = new List<Expr>();

            if (!Match(TokenType.RIGHT_PAREN))
            {
                do
                {
                    parameters.Add(ParseExpression());
                } while (Match(TokenType.COMMA));

                Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");

                if (parameters.Count != 3)
                {
                    throw new Exception($"Error en {funcToken.line}: DrawCircle requiere 3 parámetros (dirX, dirY, radius).");
                }
            }
            else
            {
                throw new Exception($"Error en {funcToken.line}: DrawCircle requiere 3 parámetros.");
            }

            return new CallComand(TokenType.DRAW_CIRCLE, parameters);
        }
        public Statement ParseDrawRectangle()
        {
            Token funcToken = Consume(TokenType.DRAW_RECTANGLE, "DrawRectangle");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            List<Expr> parameters = new List<Expr>();

            if (!Match(TokenType.RIGHT_PAREN))
            {
                do
                {
                    parameters.Add(ParseExpression());
                } while (Match(TokenType.COMMA));

                Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");

                if (parameters.Count != 5)
                {
                    throw new Exception($"Error en {funcToken.line}: DrawRectangle requiere 5 parámetros (dirX, dirY, distance, width, height).");
                }
            }
            else
            {
                throw new Exception($"Error en {funcToken.line}: DrawRectangle requiere 5 parámetros (dirX, dirY, distance, width, height).");
            }
            return new CallComand(TokenType.DRAW_RECTANGLE, parameters);
        }

    }

}

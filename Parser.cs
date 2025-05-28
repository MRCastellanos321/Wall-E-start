
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
            { "GetActualX", ParseActualX},
            { "GetActualY", ParseActualY},
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
                Expr exprFunc = exprParseMethod();
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
            Token ident = Consume(TokenType.IDENTIFIER, "identificador");
            Consume(TokenType.ARROW, "<-");
            Expr value = ParseExpression();
            string type = DetermineExpressionType(value);
            return new VarDeclaration(ident.lexeme, value, type);
        }

        private string DetermineExpressionType(Expr expr)
        {
            if (expr is LiteralExpr literal)
                return (literal.Value is bool) ? "bool" : "number";
            else if (expr is BinaryExpr binary)
                return (binary.Operator == "&&" || binary.Operator == "||") ? "bool" : "number";
            else
                return "number"; //las funciones a las q puede llamar retornan eso
        }
        /*
        public Expr ParseExpression()
        {
            return ParseBinaryExpression();
        }
*/
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
            if (Peek().type == TokenType.NUMBER || Peek().type == TokenType.STRING || Peek().type == TokenType.BOOLEAN)
            {
                Token token = Peek();
                Advance();
                return new LiteralExpr(token.literal);
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


        public Expr ParseExpression()
        {
            return ParseLogicalOr(); // Inicia con la menor precedencia
        }

        private Expr ParseLogicalOr()
        {
            Expr left = ParseLogicalAnd();
            while (Match(TokenType.OR))
            {
                Token op = Previous();
                Expr right = ParseLogicalAnd();
                left = new BinaryExpr(left, op.lexeme, right);
            }
            return left;
        }

        private Expr ParseLogicalAnd()
        {
            Expr left = ParseEquality();
            while (Match(TokenType.AND))
            {
                Token op = Previous();
                Expr right = ParseEquality();
                left = new BinaryExpr(left, op.lexeme, right);
            }
            return left;
        }

        private Expr ParseEquality()
        {
            Expr left = ParseComparison();
            while (Match(TokenType.EQUAL_EQUAL) || Match(TokenType.BANG_EQUAL))
            {
                Token op = Previous();
                Expr right = ParseComparison();
                left = new BinaryExpr(left, op.lexeme, right);
            }
            return left;
        }

        private Expr ParseComparison()
        {
            Expr left = ParseTerm();
            while (Match(TokenType.GREATER) || Match(TokenType.GREATER_EQUAL) ||
                   Match(TokenType.LESS) || Match(TokenType.LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = ParseTerm();
                left = new BinaryExpr(left, op.lexeme, right);
            }
            return left;
        }

        private Expr ParseTerm()
        {
            Expr left = ParseFactor();
            while (Match(TokenType.PLUS) || Match(TokenType.MINUS))
            {
                Token op = Previous();
                Expr right = ParseFactor();
                left = new BinaryExpr(left, op.lexeme, right);
            }
            return left;
        }

        private Expr ParseFactor()
        {
            Expr left = ParsePower();
            while (Match(TokenType.MULTIPLY) || Match(TokenType.DIVIDE) || Match(TokenType.MODULO))
            {
                Token op = Previous();
                Expr right = ParsePower();
                left = new BinaryExpr(left, op.lexeme, right);
            }
            return left;
        }

        private Expr ParsePower()
        {
            Expr left = ParsePrimary();
            if (Match(TokenType.POWER))
            {
                Token op = Previous();
                Expr right = ParsePower();
                left = new BinaryExpr(left, op.lexeme, right);
            }
            return left;
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
            if (!IsBooleanExpression(condition))
            {
                throw new Exception($"La condición en GoTo debe ser booleana (línea {labelToken.line})");
            }
            Consume(TokenType.RIGHT_PAREN, "paréntesis derecho ')'");
            return new GoToStatement(labelToken.lexeme, condition);
        }
        private bool IsBooleanExpression(Expr expr)
        {
            return expr is BinaryExpr binary &&
                   (binary.Operator == "&&" || binary.Operator == "||" ||
                    binary.Operator == "==" || binary.Operator == "!=" ||
                    binary.Operator == "<" || binary.Operator == ">" ||
                    binary.Operator == "<=" || binary.Operator == ">=");
        }
        private Statement ParseLabelDeclaration()
        {
            Token labelToken = Consume(TokenType.IDENTIFIER, "nombre de etiqueta");
            Consume(TokenType.NEW_LINE, "salto de línea");
            return new LabelDeclaration(labelToken.lexeme);
        }



        private void CheckColor(List<Expr> parameters, string func, int line)
        {
            if ((parameters[0] is LiteralExpr literal) && literal.Value is string colorValue)
            {
                if (!colors.Contains(colorValue))
                {
                    throw new Exception($"Error en {line}: {func} debe recibir un color definido válido.");
                }
            }
            else
            {
                throw new Exception($"Error en {line}: {func} debe recibir un string válido.");
            }
        }
        private void CheckParameters(int index, List<Expr> parameters, string func, int line)
        {
            for (int i = index; i < parameters.Count; i++)
            {
                if (!(parameters[i] is LiteralExpr) && !(parameters[i] is VariableExpr))
                {
                    throw new Exception($"Error de tipo en {line}: el argumento {i + 1} de {func} debe ser un literal o variable.");
                }

                if (parameters[i] is LiteralExpr literal)
                {
                    if (!(literal.Value is int))
                    {
                        throw new Exception($"Error de tipo en {line}: el argumento {i + 1} de {func} debe ser un entero.");
                    }
                }
               //si es solo variable expr no tengo contexto aquí y no sé si le estoy pasand bool o int pq el parser no lleva cuenta de si ha sido declarada o no
            }
        }
        private List<Expr> ParseParameters(int count, int line, string func)
        {
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            List<Expr> parameters = new List<Expr>();
            do
            {
                parameters.Add(ParsePrimary());
            } while (Match(TokenType.COMMA));
            //quizás debamos pensar algo aquí si hay un problema con las comas
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            if (parameters.Count != count)
            {
                throw new Exception($"Error de sintaxis en {line}: {func} requiere {count} argumento(s). Se recibieron {parameters.Count}.");
            }
            return parameters;
        }
        //anadir una que revise el color y modificar esta para que revise la cuenta, ver si hay algo que hacer con respecto a la coma y error


        public Statement ParseSpawnPoint()
        {
            Token funcToken = Consume(TokenType.SPAWN_POINT, "Spawn");
            List<Expr> parameters = ParseParameters(2, funcToken.line, "Spawn");
            CheckParameters(0, parameters, "Spawn", funcToken.line);
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallComand(TokenType.SPAWN_POINT, parameters);
        }
        public Expr ParseIsBrushColor()
        {
            Token funcToken = Consume(TokenType.IS_BRUSH_COLOR, "IsBrushColor");
            List<Expr> parameters = ParseParameters(1, funcToken.line, "IsBrushColor");
            CheckColor(parameters, "IsBrushColor", funcToken.line);
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
            List<Expr> parameters = ParseParameters(5, funcToken.line, "GetColorCount");
            CheckColor(parameters, "GetColorCount", funcToken.line);
            CheckParameters(1, parameters, "GetColorCount", funcToken.line);
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallFunction(TokenType.GET_COLOR_COUNT, parameters);
        }
        //public Expr ParseIsColor() { }
        public Expr ParseIsCanvasColor()
        {
            Token funcToken = Consume(TokenType.IS_CANVAS_COLOR, "IsCanvasColor");
            List<Expr> parameters = ParseParameters(3, funcToken.line, "IsCanvasColor");
            CheckColor(parameters, "IsCanvasColor", funcToken.line);
            CheckParameters(1, parameters, "IsCanvasColor", funcToken.line);
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallFunction(TokenType.IS_CANVAS_COLOR, parameters);
        }
        public Statement ParseColor()
        {
            Token funcToken = Consume(TokenType.COLOR, "Color");
            List<Expr> parameters = ParseParameters(1, funcToken.line, "Color");
            CheckColor(parameters, "Color", funcToken.line);
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallComand(TokenType.COLOR, parameters);
        }
        public Statement ParseSize()
        {
            Token funcToken = Consume(TokenType.SIZE, "Size");
            List<Expr> parameters = ParseParameters(1, funcToken.line, "Size");
            CheckParameters(0, parameters, "Size", funcToken.line);
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallComand(TokenType.SIZE, new List<Expr> { parameters[0] });
        }

        public Statement ParseDrawLine()
        {

            Token funcToken = Consume(TokenType.DRAW_LINE, "DrawLine");
            List<Expr> parameters = ParseParameters(3, funcToken.line, "DrawLine");
            CheckParameters(0, parameters, "DrawLine", funcToken.line);
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallComand(TokenType.DRAW_LINE, parameters);
        }

        public Statement ParseDrawCircle()
        {
            Token funcToken = Consume(TokenType.DRAW_CIRCLE, "DrawCircle");
            List<Expr> parameters = ParseParameters(3, funcToken.line, "DrawCircle");
            CheckParameters(0, parameters, "DrawCircle", funcToken.line);
            Consume(TokenType.NEW_LINE, "Se esperaba salto de línea");
            return new CallComand(TokenType.DRAW_CIRCLE, parameters);
        }
        public Statement ParseDrawRectangle()
        {
            Token funcToken = Consume(TokenType.DRAW_RECTANGLE, "DrawRectangle");
            List<Expr> parameters = ParseParameters(5, funcToken.line, "DrawRectangle");
            CheckParameters(0, parameters, "DrawRectangle", funcToken.line);
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

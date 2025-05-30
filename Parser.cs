
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
            Expr value;
            try
            {
                value = ParseExpression();
            }
            catch { throw new Exception($"Error en {Peek().line}: Expresión de inicialización de variable no reconocida"); }
            if (!IsValidAssignmentExpression(value, out string type))
            {
                throw new Exception($"Error en {Peek().line}: Expresión de inicialización de variable no reconocida, se esperaba booleana o numérica");
            }
           // string type = DetermineExpressionType(value);
            Consume(TokenType.NEW_LINE, "salto de línea luego de asignación de variable");
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
                Expr expr = ParseExpression();
                Consume(TokenType.RIGHT_PAREN, " ')' luego de expresión");
                return new GroupingExpr(expr);
            }

            if (Peek().type == TokenType.IDENTIFIER)
            {
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

        private bool IsComparableExpression1(Expr expr)
        {
            if (IsValidNumericExpression(expr)) return true;
            else if (expr is LiteralExpr literal)
            {
                if (literal.Value is string strValue)
                {
                    return strValue == "true" || strValue == "false";
                }
                return false;
            }
            if (expr is VariableExpr) return true;

            return false;
        }
        private bool IsComparableExpression2(Expr expr)
        {
            if (IsNumericExpression(expr)) return true;
            else if (expr is LiteralExpr literal)
            {
                if (literal.Value is string strValue)
                {
                    return strValue == "true" || strValue == "false";
                }
                return false;
            }
            if (expr is VariableExpr) return true;

            return false;
        }
        private bool IsBooleanExpression(Expr expr)
        {
            if (expr is BinaryExpr binary)
            {
                switch (binary.Operator)
                {
                    case "&&":
                    case "||":
                        return IsBooleanExpression(binary.Left) &&
                               IsBooleanExpression(binary.Right);

                    case "==":
                    case "!=":
                        return IsComparableExpression2(binary.Left) &&
                               IsComparableExpression2(binary.Right);

                    case "<":
                    case ">":
                    case "<=":
                    case ">=":
                        return IsNumericExpression(binary.Left) &&
                               IsNumericExpression(binary.Right);

                    default:
                        return false;
                }
            }
            else if (expr is GroupingExpr grouping)
            {
                return IsBooleanExpression(grouping.Expression);
            }
            else if (expr is LiteralExpr literal)
            {
                if (literal.Value is string strValue)
                {
                    return strValue == "true" || strValue == "false";
                }
                return false;
            }
            else if (expr is VariableExpr)
            {
                // Variables pueden ser booleanas
                return true;
            }
            else if (expr is UnaryExpr unary && unary.Operator == "!")
            {
                return IsBooleanExpression(unary.Right);
            }

            return false;
        }
        private bool IsNumericExpression(Expr expr)
        {
             if (expr is LiteralExpr literal)
            {
                return literal.Value is int;
            }
            else if (expr is VariableExpr || expr is CallFunction)
            {
                return true;
            }
            else if (expr is BinaryExpr binary)
            {
                string[] validOps = { "+", "-", "*", "/", "**", "%" };

                if (!validOps.Contains(binary.Operator))
                    return false;

                return IsValidNumericExpression(binary.Left) &&
                       IsValidNumericExpression(binary.Right);
            }
            else if (expr is GroupingExpr grouping)
            {
                return IsValidNumericExpression(grouping.Expression);
            }
            else if (expr is UnaryExpr unary && unary.Operator == "-")
            {
                return IsValidNumericExpression(unary.Right);
            }
            return false;
        }
        //esta es para los asigment y permite llamar a funciones


        //esta es para las funciones y que no se pueda incluir funciones en sus llamados

        //buscar mejorar despues pa no repetir codigo
        private bool IsValidNumericExpression(Expr expr)
        {
            if (expr is LiteralExpr literal)
            {
                return literal.Value is int;
            }
            else if (expr is VariableExpr)
            {
                return true;
            }
            else if (expr is BinaryExpr binary)
            {
                string[] validOps = { "+", "-", "*", "/", "**", "%" };

                if (!validOps.Contains(binary.Operator))
                    return false;

                return IsValidNumericExpression(binary.Left) &&
                       IsValidNumericExpression(binary.Right);
            }
            else if (expr is GroupingExpr grouping)
            {
                return IsValidNumericExpression(grouping.Expression);
            }
            else if (expr is UnaryExpr unary && unary.Operator == "-")
            {
                return IsValidNumericExpression(unary.Right);
            }
            return false;
        }
        private bool IsValidAssignmentExpression(Expr expr, out string type)

        {
            //la asig o es booleana o es numerica, permite funciones
            type = "Desconocido";
            if (IsNumericExpression(expr))
            {
                type = "number";
                return true;
            }
            if (IsBooleanExpression(expr))
            {
                type = "bool";
                return true;
            }

            return false;
        }
        private bool IsValidBooleanExpression(Expr expr)
        {
            if (expr is BinaryExpr binary)
            {
                switch (binary.Operator)
                {
                    case "&&":
                    case "||":
                        return IsValidBooleanExpression(binary.Left) &&
                               IsValidBooleanExpression(binary.Right);

                    case "==":
                    case "!=":
                        return IsComparableExpression1(binary.Left) &&
                               IsComparableExpression1(binary.Right);

                    case "<":
                    case ">":
                    case "<=":
                    case ">=":
                        return IsValidNumericExpression(binary.Left) &&
                               IsValidNumericExpression(binary.Right);

                    default:
                        return false;
                }
            }
            else if (expr is GroupingExpr grouping)
            {
                return IsValidBooleanExpression(grouping.Expression);
            }
            else if (expr is LiteralExpr literal)
            {
                if (literal.Value is string strValue)
                {
                    return strValue == "true" || strValue == "false";
                }
                return false;
            }
            else if (expr is UnaryExpr unary && unary.Operator == "!")
            {
                return IsValidBooleanExpression(unary.Right);
            }
            else if (expr is VariableExpr)
            {
                return true;
            }

            return false;
        }
        //lo de is valid boolean expesrion es para que en lugar de is boolean expresion, cuando se esten revisando asig si permirta llamadas a funcion

        private Statement ParseLabelDeclaration()
        {
            Token labelToken = Consume(TokenType.IDENTIFIER, "nombre de etiqueta");
            Consume(TokenType.NEW_LINE, "salto de línea");
            return new LabelDeclaration(labelToken.lexeme);
        }
        private Statement ParseLabelStatement()

        {
            Consume(TokenType.GO_TO, "GoTo");
            Consume(TokenType.LEFT_BRACKET, "corchete izquierdo '[' luego del GoTo");
            Token labelToken = Consume(TokenType.IDENTIFIER, "nombre de etiqueta");
            Consume(TokenType.RIGHT_BRACKET, "corchete derecho ']'");

            Consume(TokenType.LEFT_PAREN, "paréntesis izquierdo '(' para abrir la condición del GoTo");


            Expr condition;
            try
            {
                condition = ParseExpression();
            }
            catch
            {
                throw new Exception($"Error en {Peek().line}: Condición de GoTo no válida ");
            }
            if (!IsBooleanExpression(condition))
            {
                throw new Exception($"La condición en GoTo debe ser booleana y válida (línea {labelToken.line})");
            }

            Consume(TokenType.RIGHT_PAREN, "paréntesis derecho ')' para cerrar la condición del GoTo");
            Consume(TokenType.NEW_LINE, "salto de línea tras la condición del GoTo");
            return new GoToStatement(labelToken.lexeme, condition);
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
                if (!IsValidNumericExpression(parameters[i]))
                {
                    throw new Exception($"Error en línea {line}:El argumento {i + 1} de {func} debe ser una expresión numérica (no se permiten booleanos ni funciones)");
                }
            }
        }
        /*
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
       */
        private List<Expr> ParseParameters(int expectedCount, int line, string funcName)
        {
            Consume(TokenType.LEFT_PAREN, "paréntesis izquierdo '('");
            List<Expr> parameters = new List<Expr>();
            if (Match(TokenType.RIGHT_PAREN))
            {
                ValidateParameterCount(expectedCount, 0, line, funcName);
                return parameters;
            }
            do
            {
                try
                {
                    parameters.Add(ParseExpression());
                }
                catch
                {
                    if (Peek().type == TokenType.NEW_LINE)
                    {
                        throw new Exception($"Error en {line}: no hay parámetros tras la última coma en {funcName}");
                    }
                    throw new Exception($"Error en {line}: Expresión no reconocida en {funcName}, parámetro {parameters.Count + 1}");
                }
                if (Match(TokenType.COMMA))
                {
                    if (Peek().type == TokenType.COMMA)
                    {
                        throw new Exception($"Error en línea {line}: Coma adicional después del parámetro.");
                    }
                    if (Peek().type == TokenType.RIGHT_PAREN)
                    {
                        throw new Exception($"Error en línea {line}:Se espera un parámetro después de la coma.");
                    }
                }
                else if (Peek().type == TokenType.RIGHT_PAREN)
                {
                    break;
                }
                else
                {
                    throw new Exception($"Error en línea {Peek().line}: Se esperaba un paréntesis derecho ')' después de parámetro(s).");
                }

            } while (true);

            Consume(TokenType.RIGHT_PAREN, "paréntesis derecho ')' después de parámetro(s)");
            ValidateParameterCount(expectedCount, parameters.Count, line, funcName);

            return parameters;
        }
        private void ValidateParameterCount(int expected, int actual, int line, string funcName)
        {
            if (actual != expected)
            {
                throw new Exception($"Error en línea {line}: {funcName} requiere {expected} parámetro(s). Se recibieron {actual}.");
            }
        }






        public Statement ParseSpawnPoint()
        {
            Token funcToken = Consume(TokenType.SPAWN_POINT, "Spawn");
            List<Expr> parameters = ParseParameters(2, funcToken.line, "Spawn");
            CheckParameters(0, parameters, "Spawn", funcToken.line);
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función Spawn");
            return new CallComand(TokenType.SPAWN_POINT, parameters);
        }
        public Expr ParseIsBrushColor()
        {
            Token funcToken = Consume(TokenType.IS_BRUSH_COLOR, "IsBrushColor");
            List<Expr> parameters = ParseParameters(1, funcToken.line, "IsBrushColor");
            CheckColor(parameters, "IsBrushColor", funcToken.line);
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función IsBrushColor");
            return new CallFunction(TokenType.IS_BRUSH_COLOR, parameters);
        }
        public Expr ParseActualX()
        {
            Consume(TokenType.GET_ACTUAL_X, "ActualX");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función ActualX");
            return new CallFunction(TokenType.GET_ACTUAL_X, new List<Expr>());
        }

        public Expr ParseActualY()
        {
            Consume(TokenType.GET_ACTUAL_Y, "ActualY");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función ActualY");
            return new CallFunction(TokenType.GET_ACTUAL_Y, new List<Expr>());
        }

        public Expr ParseGetCanvasSize()
        {
            Consume(TokenType.GET_CANVAS_SIZE, "GetCanvasSize");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función GetCanvasSize");
            return new CallFunction(TokenType.GET_CANVAS_SIZE, new List<Expr>());
        }
        public Expr ParseGetColorCount()
        {
            Token funcToken = Consume(TokenType.GET_COLOR_COUNT, "GetColorCount");
            List<Expr> parameters = ParseParameters(5, funcToken.line, "GetColorCount");
            CheckColor(parameters, "GetColorCount", funcToken.line);
            CheckParameters(1, parameters, "GetColorCount", funcToken.line);
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función GetColorCount");
            return new CallFunction(TokenType.GET_COLOR_COUNT, parameters);
        }
        //public Expr ParseIsColor() { }
        public Expr ParseIsCanvasColor()
        {
            Token funcToken = Consume(TokenType.IS_CANVAS_COLOR, "IsCanvasColor");
            List<Expr> parameters = ParseParameters(3, funcToken.line, "IsCanvasColor");
            CheckColor(parameters, "IsCanvasColor", funcToken.line);
            CheckParameters(1, parameters, "IsCanvasColor", funcToken.line);
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función IsCanvasColor");
            return new CallFunction(TokenType.IS_CANVAS_COLOR, parameters);
        }
        public Statement ParseColor()
        {
            Token funcToken = Consume(TokenType.COLOR, "Color");
            List<Expr> parameters = ParseParameters(1, funcToken.line, "Color");
            CheckColor(parameters, "Color", funcToken.line);
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función Color");
            return new CallComand(TokenType.COLOR, parameters);
        }
        public Statement ParseSize()
        {
            Token funcToken = Consume(TokenType.SIZE, "Size");
            List<Expr> parameters = ParseParameters(1, funcToken.line, "Size");
            CheckParameters(0, parameters, "Size", funcToken.line);
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función Size");
            return new CallComand(TokenType.SIZE, new List<Expr> { parameters[0] });
        }

        public Statement ParseDrawLine()
        {

            Token funcToken = Consume(TokenType.DRAW_LINE, "DrawLine");
            List<Expr> parameters = ParseParameters(3, funcToken.line, "DrawLine");
            CheckParameters(0, parameters, "DrawLine", funcToken.line);
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función DrawLine");
            return new CallComand(TokenType.DRAW_LINE, parameters);
        }

        public Statement ParseDrawCircle()
        {
            Token funcToken = Consume(TokenType.DRAW_CIRCLE, "DrawCircle");
            List<Expr> parameters = ParseParameters(3, funcToken.line, "DrawCircle");
            CheckParameters(0, parameters, "DrawCircle", funcToken.line);
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función DrawCircle");
            return new CallComand(TokenType.DRAW_CIRCLE, parameters);
        }
        public Statement ParseDrawRectangle()
        {
            Token funcToken = Consume(TokenType.DRAW_RECTANGLE, "DrawRectangle");
            List<Expr> parameters = ParseParameters(5, funcToken.line, "DrawRectangle");
            CheckParameters(0, parameters, "DrawRectangle", funcToken.line);
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función DrawRectangle");
            return new CallComand(TokenType.DRAW_RECTANGLE, parameters);
        }
        public Statement ParseFill()
        {
            Consume(TokenType.FILL, "Fill");
            Consume(TokenType.LEFT_PAREN, "un paréntesis izquierdo");
            Consume(TokenType.RIGHT_PAREN, "un paréntesis derecho");
            Consume(TokenType.NEW_LINE, "salto de línea luego de llamado a función Fill");
            return new CallComand(TokenType.FILL, new List<Expr>());
        }
    }

}

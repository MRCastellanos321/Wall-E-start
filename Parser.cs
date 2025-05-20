using System.Data.Common;
using System.Drawing;

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

        private ASTNode ParseStatement()
        {
            if (Lexer.keyWords.TryGetValue(Peek().lexeme, out TokenType type))
            {
                string lexeme = Peek().lexeme;
                if (commandParsers.TryGetValue(lexeme, out Func<Statement> parseMethod))
                {
                    return parseMethod();
                }
                else if (exprFunctionParsers.TryGetValue(lexeme, out Func<Expr> exprParseMethod))
                {
                    return exprParseMethod();
                }
            }
            else if (Peek().type == TokenType.IDENTIFIER && LookAhead(1).type == TokenType.ARROW)
            {
                return ParseAssignmentStatement();
            }
            //aquí falta la lógica del loop
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
            Token token = Peek();
            if (token.type == TokenType.NUMBER || token.type == TokenType.STRING)
            {
                return new LiteralExpr(token.lexeme);
            }
            if (exprFunctionParsers.TryGetValue(token.lexeme, out Func<Expr> exprParseMethod))
            {
                return exprParseMethod();
            }
            if (token.type == TokenType.LEFT_PAREN)
            {
                Expr expr = ParseExpression();
                Consume(TokenType.RIGHT_PAREN, "un ) para cerrar la expresión");
                return new GroupingExpr(expr);
            }
            throw new Exception($"Error en {token.line}: Expresión asignada no válida.");
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
                string parameterColor;
                if (!colors.TryGetValue(((LiteralExpr)parameters[0]).Value.ToString(), out parameterColor))
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

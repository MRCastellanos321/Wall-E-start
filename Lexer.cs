
namespace Compiler
{
    class Lexer
    {
        private string sourceCode;
        private List<Token> tokens = new List<Token>();
        private int position = 0;
        private int start;
        private int line = 1;

        Lexer(String sourceCode)
        {
            this.sourceCode = sourceCode;
        }
        private bool IsAtEnd()
        {
            return position >= sourceCode.Count();
        }

        private char Peek()
        {
            if (IsAtEnd())
            {
                return '\0';
            }
            return sourceCode[position];
        }

        private char Advance()
        {
            char current = Peek();
            position++;

            if (current == '\n')
            {
                line++;
            }
            return current;
        }

        private void SkipWhitespace()
        {
            while (Peek().Equals(' '))
            {
                Advance();
            }
        }

        public static readonly Dictionary<string, TokenType> keyWords = new Dictionary<string, TokenType>()
        {

          {"Spawn", TokenType.SPAWN_POINT},
          {"Color", TokenType.COLOR},
          {"Size", TokenType.SIZE},
          {"DrawLine", TokenType.DRAW_LINE},
          {"DrawCircle", TokenType.DRAW_CIRCLE},
          {"DrawRectangle", TokenType.DRAW_RECTANGLE},
          { "Fill", TokenType.FILL},
          { "GoTo", TokenType.GO_TO},
          {"IsColor", TokenType.IS_COLOR},
          {"IsBrushSize", TokenType.IS_BRUSH_SIZE},
          {"IsCanvasColor", TokenType.IS_CANVAS_COLOR},
          {"IsBrushColor" ,TokenType.IS_BRUSH_COLOR},
          { "GetActualX", TokenType.GET_ACTUAL_X},
          {"GetActualY", TokenType.GET_ACTUAL_Y},
          {"GetCanvasSize", TokenType.GET_CANVAS_SIZE},
          {"GetColorCount", TokenType.GET_COLOR_COUNT},
          {"true", TokenType.BOOLEAN},
          {"false", TokenType.BOOLEAN},

        };
        public IEnumerable<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                start = position;
                Tokenizar();
            }
            tokens.Add(new Token(TokenType.EOF, "", "", line));
            return tokens;
        }
        private void Tokenizar()
        {
            SkipWhitespace();
            char currentChar = Advance();
            switch (currentChar)
            {
                case '/': tokens.Add(new Token(TokenType.DIVIDE, "/", "/", line)); Advance(); break;
                case '(': tokens.Add(new Token(TokenType.LEFT_PAREN, "(", "(", line)); Advance(); break;
                case ')': tokens.Add(new Token(TokenType.RIGHT_PAREN, ")", ")", line)); Advance(); break;
                case '[': tokens.Add(new Token(TokenType.LEFT_BRACKET, "[", "[", line)); Advance(); break;
                case ']': tokens.Add(new Token(TokenType.RIGHT_BRACKET, "]", "]", line)); Advance(); break;
                case '{': tokens.Add(new Token(TokenType.LEFT_BRACE, "{", "{", line)); Advance(); break;
                case '}': tokens.Add(new Token(TokenType.RIGHT_BRACE, "}", "}", line)); Advance(); break;
                case ',': tokens.Add(new Token(TokenType.COMMA, ",", ",", line)); Advance(); break;
                case '.': tokens.Add(new Token(TokenType.DOT, ".", ".", line)); Advance(); break;
                case '-': tokens.Add(new Token(TokenType.MINUS, "-", "-", line)); Advance(); break;
                case '+': tokens.Add(new Token(TokenType.PLUS, "+", "+", line)); Advance(); break;
                case ';': tokens.Add(new Token(TokenType.SEMICOLON, ";", ";", line)); Advance(); break;
                case '*': tokens.Add(new Token(TokenType.MULTIPLY, "*", "*", line)); Advance(); break;
                case '"': ReadString(); break;
                case '\n': tokens.Add(new Token(TokenType.NEW_LINE, "\n", "\n", line)); Advance(); break;
                // case '"': tokens.Add(new Token(TokenType.DOUBLE_COMMA, '"', '"', line)); Advance(); break;
                case '<':
                    if (position + 1 < sourceCode.Length && sourceCode[position + 1] == '=') { tokens.Add(new Token(TokenType.LESS_EQUAL, "<=", "<=", line)); Advance(); break; }
                    else if (sourceCode[position + 1] == '-') { tokens.Add(new Token(TokenType.ARROW, "<-", "<-", line)); Advance(); Advance(); break; }
                    else { tokens.Add(new Token(TokenType.LESS, "<", "<", line)); Advance(); }
                    break;
                case '>':
                    if (position + 1 < sourceCode.Length && sourceCode[position + 1] == '=') { tokens.Add(new Token(TokenType.GREATER_EQUAL, ">=", ">=", line)); Advance(); Advance(); break; }
                    else { tokens.Add(new Token(TokenType.GREATER, ">", ">", line)); Advance(); }
                    break;
                case '=':
                    if (position + 1 < sourceCode.Length && sourceCode[position + 1] == '=') { tokens.Add(new Token(TokenType.EQUAL_EQUAL, "==", "==", line)); Advance(); Advance(); break; }
                    else { tokens.Add(new Token(TokenType.EQUAL, "=", "=", line)); Advance(); }
                    break;


                default:
                    if (IsDigit(currentChar)) { CheckNumber(); }
                    else if (IsAlpha(currentChar) || currentChar == '_') { CheckAlpha(); }
                    else { throw new Exception($"Caracter {currentChar} no reconocido en la línea {line}"); }
                    break;
            }

        }
        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }
        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }
        private void CheckNumber()
        {
            while (IsDigit(Peek()))
            {
                Advance();
            }
            string numberString = sourceCode.Substring(start, position - start);
            int value = int.Parse(numberString);
            tokens.Add(new Token(TokenType.NUMBER, numberString, value, line));
        }
        private void CheckAlpha()
        {
            while (IsAlpha(Peek()) || IsDigit(Peek()) || Peek() == '_' || Peek() == '-')
            { Advance(); }
            string textString = sourceCode.Substring(start, position - start);
            TokenType type;
            if (!keyWords.TryGetValue(textString, out type))
            {
                type = TokenType.IDENTIFIER;

            }
            tokens.Add(new Token(type, textString, textString, line));
            ScanIdentifier(tokens[tokens.Count - 1]);
        }
        private void ScanIdentifier(Token token)
        {
            string text = token.lexeme;
            bool hasUnderscore = text.Contains('_');
            bool hasMinus = text.Contains('-');

            if (hasUnderscore && hasMinus)
            {
                throw new Exception($"Identificador '{text}' no puede contener _ y - simultáneamente (línea {token.line})");
            }
            char firstChar = text[0];
            if (char.IsDigit(firstChar) || firstChar == '-')
            {
                throw new Exception($"Identificador '{text}' no puede comenzar con número o guión (línea {token.line})");
            }
        }
        private void ReadString(){}
    }
}



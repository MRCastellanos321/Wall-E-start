

namespace Compiler
{
    class Lexer
    {
        private string sourceCode;
        private List<Token> tokens = new List<Token>();
        private int position = 0;
        private int start;
        private int line = 1;

        public Lexer(string sourceCode)
        {
            this.sourceCode = sourceCode;
        }
        private bool IsAtEnd()
        {
            return position >= sourceCode.Count() - 1;
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
            while (Peek().Equals(' ') && !IsAtEnd())
            {
                if (sourceCode[position] == '\n')
                {
                    line++;
                }
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
                Tokenizar();
            }
            tokens.Add(new Token(TokenType.EOF, "", "", line));
            return tokens;
        }
        private void Tokenizar()
        {
            SkipWhitespace();
            start = position;
            if (IsAtEnd()) { return; }
            char currentChar = Advance();
            switch (currentChar)
            {
                case '/': tokens.Add(new Token(TokenType.DIVIDE, "/", "/", line)); break;
                case '(': tokens.Add(new Token(TokenType.LEFT_PAREN, "(", "(", line)); break;
                case ')': tokens.Add(new Token(TokenType.RIGHT_PAREN, ")", ")", line)); break;
                case '[': tokens.Add(new Token(TokenType.LEFT_BRACKET, "[", "[", line)); break;
                case ']': tokens.Add(new Token(TokenType.RIGHT_BRACKET, "]", "]", line)); break;
                case '{': tokens.Add(new Token(TokenType.LEFT_BRACE, "{", "{", line)); break;
                case '}': tokens.Add(new Token(TokenType.RIGHT_BRACE, "}", "}", line)); break;
                case ',': tokens.Add(new Token(TokenType.COMMA, ",", ",", line)); break;
                case '.': tokens.Add(new Token(TokenType.DOT, ".", ".", line)); break;
                case '-': tokens.Add(new Token(TokenType.MINUS, "-", "-", line)); break;
                case '+': tokens.Add(new Token(TokenType.PLUS, "+", "+", line)); break;
                case ';': tokens.Add(new Token(TokenType.SEMICOLON, ";", ";", line)); break;
                case '\n': tokens.Add(new Token(TokenType.NEW_LINE, "salto de línea", "salto de línea", line - 1)); break;
                case '"': ReadString(); break;
                case '\0': return;
                case '%':
                    tokens.Add(new Token(TokenType.MODULO, "%", "%", line)); break;
                case '*':
                    if (Peek() == '*')
                    { tokens.Add(new Token(TokenType.POWER, "**", "**", line)); Advance(); break; }
                    else { tokens.Add(new Token(TokenType.MULTIPLY, "*", "*", line)); break; }
                case '&':
                    if (Peek() == '&') { tokens.Add(new Token(TokenType.AND, "&&", "&&", line)); Advance(); break; }
                    else { throw new Exception("Caracter & no puede ir solo"); }

                case '|':
                    if (Peek() == '|') { tokens.Add(new Token(TokenType.OR, "||", "||", line)); Advance(); break; }
                    else { throw new Exception("Caracter | no puede ir solo"); }

                case '<':
                    if (Peek() == '=') { tokens.Add(new Token(TokenType.LESS_EQUAL, "<=", "<=", line)); Advance(); break; }
                    else if (Peek() == '-') {  tokens.Add(new Token(TokenType.ARROW, "<-", "<-", line)); Advance(); break; }
                    else {  tokens.Add(new Token(TokenType.LESS, "<", "<", line)); }
                    break;
                case '>':
                    if (Peek() == '=') { tokens.Add(new Token(TokenType.GREATER_EQUAL, ">=", ">=", line)); Advance(); break; }
                    else { tokens.Add(new Token(TokenType.GREATER, ">", ">", line)); }
                    break;
                case '=':
                    if (Peek() == '=') { tokens.Add(new Token(TokenType.EQUAL_EQUAL, "==", "==", line)); Advance(); break; }
                    else { tokens.Add(new Token(TokenType.EQUAL, "=", "=", line)); }
                    break;


                default:
                    if (currentChar == '\0') { return; }
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
            while (!IsAtEnd() && IsDigit(Peek()))
            {
                Advance();
            }
            string numberString = sourceCode.Substring(start, position - start);
            int value = int.Parse(numberString);
            tokens.Add(new Token(TokenType.NUMBER, numberString, value, line));
        }
        private void CheckAlpha()
        {
            while (!IsAtEnd() && (IsAlpha(Peek()) || IsDigit(Peek()) || Peek() == '_'))
            { Advance(); }
            string textString = sourceCode.Substring(start, position - start);
            TokenType type;
            if (textString == "true" || textString == "false")
            {
                type = TokenType.BOOLEAN;
            }
            else if (!keyWords.TryGetValue(textString, out type))
            {
                type = TokenType.IDENTIFIER;
            }
            tokens.Add(new Token(type, textString, textString, line));
        }
        private void ReadString()
        {
            while (Peek() != '"')
            {
                if (Peek() == '\n' || IsAtEnd())
                {
                    throw new Exception($"String sin cerrar en línea {line}");
                }
                Advance();
            }
            Advance();
            string value = sourceCode.Substring(start + 1, position - start - 2);
            tokens.Add(new Token(TokenType.STRING, value, value, line));
        }
    }
}


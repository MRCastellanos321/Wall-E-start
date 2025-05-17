
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

        private readonly Dictionary<string, TokenType> keyWords = new Dictionary<string, TokenType>()
        {
          {"SpawnPoint" ,TokenType.SPAWN_POINT},
          {"IsBrushColor" ,TokenType.IS_BRUSH_COLOR},
          {"Spawn", TokenType.SPAWN_POINT},
          {"Color", TokenType.COLOR},
          {"GoTo", TokenType.GO_TO},
          {"IsColor", TokenType.IS_COLOR},
          {"IsBrushSize", TokenType.IS_BRUSH_SIZE},
          {"IsCanvasColor", TokenType.IS_CANVAS_COLOR},
          {"GetActualX", TokenType.GET_ACTUAL_X},
          {"GetActualY", TokenType.GET_ACTUAL_Y},
          {"GetCanvasSize", TokenType.GET_CANVAS_SIZE},
          {"GetColorCount", TokenType.GET_COLOR_COUNT},

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
            //aquí falta toda la lógica de los labels
            var tokens = new List<Token>();

            while (position < sourceCode.Length)
            {
                SkipWhitespace();
                char currentChar = Peek();
                switch (currentChar)
                {
                    case '(': tokens.Add(new Token(TokenType.LEFT_PAREN, "(", "(", line)); break;
                    case ')': tokens.Add(new Token(TokenType.RIGHT_PAREN, ")", ")", line)); break;
                    case '{': tokens.Add(new Token(TokenType.LEFT_BRACE, "{", "{", line)); break;
                    case '}': tokens.Add(new Token(TokenType.RIGHT_BRACE, "}", "}", line)); break;
                    case ',': tokens.Add(new Token(TokenType.COMMA, ",", ",", line)); break;
                    case '.': tokens.Add(new Token(TokenType.DOT, ".", ".", line)); break;
                    case '-': tokens.Add(new Token(TokenType.MINUS, "-", "-", line)); break;
                    case '+': tokens.Add(new Token(TokenType.PLUS, "+", "+", line)); break;
                    case ';': tokens.Add(new Token(TokenType.SEMICOLON, ";", ";", line)); break;
                    case '*': tokens.Add(new Token(TokenType.MULTIPLY, "*", "*", line)); break;
                    case '<': if (sourceCode[position + 1] == '=') { tokens.Add(new Token(TokenType.LESS_EQUAL, "<=", "<=", line)); Advance(); break; } 
                    else { tokens.Add(new Token(TokenType.LESS, "<", "<", line)); } break;
                    case '>': if (sourceCode[position + 1] == '=') { tokens.Add(new Token(TokenType.GREATER_EQUAL, ">=", ">=", line)); Advance(); break;} 
                    else { tokens.Add(new Token(TokenType.GREATER, ">", ">", line)); } break;
                    case '=': if (sourceCode[position + 1] == '=') { tokens.Add(new Token(TokenType.EQUAL_EQUAL, "==", "==", line)); Advance(); break; } 
                    else { tokens.Add(new Token(TokenType.EQUAL, "=", "=", line)); } break;

                    default:
                        if (IsDigit(currentChar)) { CheckNumber(); }
                        else if (IsAlpha(currentChar)) { CheckAlpha(); } //falta ver q hacer con respecto a etiquetas
                        else
                        {
                            tokens.Add(new Token(TokenType.IDENTIFIER, sourceCode.Substring(start, position - start), sourceCode.Substring(start, position - start), line));
                        }
                        break;
                }
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
            while (IsAlpha(Peek()))
            { Advance(); }
            string textString = sourceCode.Substring(start, position - start);
            TokenType type;
            if (!keyWords.TryGetValue(textString, out type))
            {
                type = TokenType.IDENTIFIER;
            }
            tokens.Add(new Token(type, textString, textString, line));
        }
    }
}



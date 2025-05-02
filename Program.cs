namespace Compiler
{
    public enum TokenType
    {
        
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, MULTIPLY,
        
        BANG, BANG_EQUAL,
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,
       
        IDENTIFIER, STRING, NUMBER,
       
        AND, CLASS, ELSE, FALSE, IF, OR,
        RETURN, TRUE, VAR, SPAWN_POINT, IS_BRUSH_COLOR, COLOR, GO_TO, IS_COLOR, IS_BRUSH_SIZE, IS_CANVAS_COLOR,
        GET_ACTUAL_X, GET_ACTUAL_Y, GET_CANVAS_SIZE, GET_COLOR_COUNT,

        
        BLUE, RED, GREEN, YELLOW, PURPLE, BLACK, WHITE, GREY, TRANSPARENT,
        EOF
    }
    public class Token
    {
        public TokenType type;
        public string lexeme;
        public Object literal;
        public int line;
        public Token(TokenType type, string lexeme, Object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }
        public string toString()
        {
            return type + " " + lexeme + " " + literal;
        }
       public static void Main(string[] args)
        {
            Console.WriteLine("hi");
        }
    }

}
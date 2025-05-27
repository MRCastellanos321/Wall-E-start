namespace Compiler
{
    public enum TokenType
    {

        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE, LEFT_BRACKET, RIGHT_BRACKET, COMMA, DOT,
        MINUS, PLUS, SEMICOLON, SLASH, MULTIPLY, NEW_LINE, DIVIDE, DOUBLE_COMMA, BOOLEAN, MODULO, POWER,

        BANG, BANG_EQUAL,
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,
        ARROW,

        IDENTIFIER, STRING, NUMBER,

        AND, IF, OR,
        TRUE, VAR, SPAWN_POINT, IS_BRUSH_COLOR, COLOR, GO_TO, IS_COLOR, IS_BRUSH_SIZE, IS_CANVAS_COLOR,
        GET_ACTUAL_X, GET_ACTUAL_Y, GET_CANVAS_SIZE, GET_COLOR_COUNT, DRAW_LINE, DRAW_RECTANGLE, DRAW_CIRCLE, FILL, SIZE,
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
    }
}



namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            // Ejemplo 1: Código básico con todas las características
            string testCode = @"
                Spawn(50, 50)
                Color(""Red"")
                Size(3)
                DrawLine(1, 0, 10)
                counter <- 0
                loop-start
                DrawCircle(1, 1, 2)
                counter <- counter + 1
                Fill()
            ";
            /*  string testCode = @"
                 Spawn(50, 50)
                 Color(""Red"")
                 Size(3)
                 DrawLine(1, 0, 10)
                 counter <- 0
                 loop_start
                 GoTo [loop_end] (counter >= 5)
                 DrawCircle(1, 1, 2)
                 counter <- counter + 1
                 GoTo [loop_start] (true)
                 loop_end
                 Fill()
             ";*/

            /* // Ejemplo 2: Expresiones complejas
              string testExpressions = @"
                  x <- (5 + 3) * 2
                  y <- GetActualX() % 4
                  valid <- x > y && IsBrushColor(""Red"")
              ";*/

            RunCompiler(testCode);

        }

        static void RunCompiler(string sourceCode)
        {
            try
            {
                Lexer lexer = new Lexer(sourceCode);
                List<Token> tokens = lexer.ScanTokens().ToList();

                foreach (var token in tokens)
                {
                    Console.WriteLine($"{token.type} '{token.lexeme}' {token.line}");
                }

                Parser parser = new Parser(tokens);
                List<ASTNode> ast = parser.ParsePrograma();
                //foreach(ASTNode node in ast){ Console.WriteLine node}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nERROR: {ex.Message}");
            }
        }
    }
}
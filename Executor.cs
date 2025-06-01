using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Compiler
{
    public class Executor : IExprVisitor<object>, IStmtVisitor<object>
    {
        private Dictionary<string, object> variables = new Dictionary<string, object>();
        private Point wallEPosition;
        private Color brushColor = Color.Transparent;
        private int brushSize = 1;
        // private Bitmap canvas;
        private Stack<Point> positionStack = new Stack<Point>();
        /*
                public Executor(int canvasSize)
                {
                    canvas = new Bitmap(canvasSize, canvasSize);
                    using (Graphics g = Graphics.FromImage(canvas))
                    {
                        g.Clear(Color.White);
                    }
                }

                public Bitmap GetCanvas()
                {
                    return canvas;
                }
        */
        public object VisitBinaryExpr(BinaryExpr expr)
        {
            object left = expr.Left.Accept(this);
            object right = expr.Right.Accept(this);

            switch (expr.Operator)
            {
                case "+":
                    return (double)left + (double)right;
                case "-":
                    return (double)left - (double)right;
                case "*":
                    return (double)left * (double)right;
                case "/":
                    return (double)left / (double)right;
                case "**":
                    return Math.Pow((double)left, (double)right);
                case "%":
                    return (double)left % (double)right;
                case "&&":
                    return (bool)left && (bool)right;
                case "||":
                    return (bool)left || (bool)right;
                case "==":
                    return left.Equals(right);
                case "!=":
                    return !left.Equals(right);
                case "<":
                    return (double)left < (double)right;
                case ">":
                    return (double)left > (double)right;
                case "<=":
                    return (double)left <= (double)right;
                case ">=":
                    return (double)left >= (double)right;
                default:
                    throw new Exception("Operador desconocido: " + expr.Operator);
            }
        }

        public object VisitLiteralExpr(LiteralExpr expr)
        {
            return expr.Value;
        }

        public object VisitGroupingExpr(GroupingExpr expr)
        {
            return expr.Expression.Accept(this);
        }

        public object VisitUnaryExpr(UnaryExpr expr)
        {
            object right = expr.Right.Accept(this);
            if (expr.Operator == "-")
            {
                return -(double)right;
            }
            else if (expr.Operator == "!")
            {
                return !(bool)right;
            }
            else throw new Exception("Error en unario");

        }

        public object VisitVariableExpr(VariableExpr expr)
        {
            return variables[expr.Identifier];
        }

        public object VisitCallFunction(CallFunction expr)
        {
            switch (expr.Name)
            {
                case TokenType.GET_ACTUAL_X:
                    return (double)wallEPosition.X;
                case TokenType.GET_ACTUAL_Y:
                    return (double)wallEPosition.Y;
                case TokenType.GET_CANVAS_SIZE:
                //     return (double)canvas.Width;
                default:
                    throw new Exception("Función no implementada: " + expr.Name);
            }
        }

        public object VisitCallComand(CallComand stmt)
        {
            switch (stmt.Name)
            {
                case TokenType.SPAWN_POINT:
                    //aquí hay que ver pq a las function estoy dejando q se les pase expresiones combinadas
                    //   int x = (int)(double)stmt.Parameters[0].Accept(this);
                    //    int y = (int)(double)stmt.Parameters[1].Accept(this);
                    //     wallEPosition = new Point(x, y);
                    break;

                case TokenType.COLOR:
                    //implementar todos los demas
                    break;
            }
            throw new Exception("Commando desconocido");
        }

        public object VisitVarDeclaration(VarDeclaration stmt)
        {
            object value = stmt.Initializer.Accept(this);
            variables[stmt.Name] = value;
            return null;
        }

        public object VisitGoToStatement(GoToStatement stmt)
        {
            // Implementación de saltos
            return null;
        }

        public object VisitLabelDeclaration(LabelDeclaration stmt)
        {
            // deberíamos annadir esto a un diccionario para saber si se la llama despues
            return null;
        }

        public void Execute(List<ASTNode> nodes)
        {
            foreach (ASTNode node in nodes)
            {
                if (node is Statement)
                {
                    Statement stmt = (Statement)node;
                    stmt.Accept(this);
                }
                else if (node is Expr)
                {
                    Expr expr = (Expr)node;
                    expr.Accept(this);
                }
            }
        }
    }
}
/*Ok, lee bien el estado actual del parser.  Tengo un problema con el parseo de expresiones. Aunque el código actual funciona, tengo dos funciones para iscomparableexpression, el isnumericExpression, is Valid boolean expresion, porque a mis funciones no se les puede pasar entre los parámetros u llamado a funcion o combinacion 3 * CallFuncion, etc, pero a mis variable expresion sí, por tanto ahora msimo tengo código repetido que me dificulta el trabajo. Las diferencias entre estas funciones y las otras es que isNumeric expression revisa variable || callfuntion, y como las otras tienen que llamar en algun momento a isNumericExpresion o IsValidExpression, las cambié tambien. Dime una forma de reducirlo a una sola función de cada una y que me permita controlar el contexto de si es desde una función o desde una variable sin hacer cabios mayores*/
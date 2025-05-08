using System.Diagnostics;

namespace Compiler
{
       #region 
       public interface IExprVisitor
       {//el q luego va a leer todo esto
              T VisitBinaryExpr<T>(BinaryExpr expr);
              T VisitLiteralExpr<T>(LiteralExpr expr);
              T VisitGroupingExpr<T>(GroupingExpr expr);
              T VisitUnaryExpr<T>(UnaryExpr expr);
       }

       // 
       public abstract class Expr
       {
              // Esto es para q todo el mudno tenga el accept
              public abstract T Accept<T>(IExprVisitor visitor);
       }

       //este es para operaciones suma etc
       public class BinaryExpr : Expr
       {
              public Expr Left { get; }
              public string Operator { get; } //esto aquí es token o string?

              public Expr Right { get; }

              public BinaryExpr(Expr left, string oper, Expr right)
              {
                     Left = left;
                     Operator = oper;
                     Right = right;
              }

              public override T Accept<T>(IExprVisitor visitor)
              {
                     return visitor.VisitBinaryExpr<T>(this);
                     //esto es para q acept la instancia de la clase
              }

       }

       // Aqui van numeros y strings
       public class LiteralExpr : Expr
       {
              public object Value { get; }

              public LiteralExpr(object value)
              {

                     Value = value;
              }

              public override T Accept<T>(IExprVisitor visitor)
              {
                     return visitor.VisitLiteralExpr<T>(this);
              }

       }

       //este es el nodo para parentesis llaves etc q luego va a contener los nodos de o que hay
       public class GroupingExpr : Expr
       {
              public Expr Expression { get; }

              public GroupingExpr(Expr expression)
              {
                     Expression = expression;
              }

              public override T Accept<T>(IExprVisitor visitor)
              {
                     return visitor.VisitGroupingExpr<T>(this);
              }

       }

       //    Esto es para cuando aparce -int    
       public class UnaryExpr : Expr
       {
              public string Operator { get; }
              public Expr Right { get; }

              public UnaryExpr(string oper, Expr right)
              {
                     Operator = oper;
                     Right = right;
              }

              public override T Accept<T>(IExprVisitor visitor)
              {
                     return visitor.VisitUnaryExpr<T>(this);
              }

       }
       #endregion


       public abstract class Statement
       {
              public abstract T Accept<T>(IStmtVisitor visitor);
       }

       public interface IStmtVisitor
       {
              T VisitCallFunction<T>(CallFunction statement);
              T VisitIfStatement<T>(IfStatement statement);
              T VisitVarDeclaration<T>(VarDeclaration statement);
              T VisitWhileStatement<T>(WhileStatement statement);
       }

       public class CallFunction : Statement
       {
              //este va a ser el nodo de las funciones, q tiene q tener dentro una lista de nodos para los parámetros, y lo q hay dentro de la función ademas
              public Token Name { get; }
              public List<Token> Parameters { get; }
              public List<Statement> Body { get; }

              public CallFunction(Token name, List<Token> parameters, List<Statement> body)
              {
                     Name = name;
                     Parameters = parameters;
                     Body = body;
              }

              public override T Accept<T>(IStmtVisitor visitor)
              {
                     return visitor.VisitCallFunction<T>(this);
              }

       }

       public class IfStatement : Statement
       {
              //este va a unir el if y el else. El branch else aquí puede ser nulo. Habra q especificar algo más?
              public Expr Condition { get; }
              public Statement ThenBranch { get; }
              public Statement ElseBranch { get; }
              public IfStatement(Expr condition, Statement thenBranch, Statement elseBranch)
              {
                     Condition = condition;
                     ThenBranch = thenBranch;
                     ElseBranch = elseBranch;
              }

              public override T Accept<T>(IStmtVisitor visitor)
              {
                     return visitor.VisitIfStatement<T>(this);
              }
       }
       //queda ver lo del if, asignacion de variables, etc, en statements

       public class VarDeclaration : Statement
       {
              public Token Name { get; }
              public Expr Initializer { get; }  // el pdf permit null?

              public VarDeclaration(Token name, Expr initializer)
              {
                     Name = name;
                     Initializer = initializer;
              }

              public override T Accept<T>(IStmtVisitor visitor)
              {
                     return visitor.VisitVarDeclaration<T>(this);
              }

       }
//esto no es en realidad para While sino parael GoTo del label, despué hay que revisr mejor
       public class WhileStatement : Statement
       {
              public Expr Condition { get; }
              public Statement Body { get; }
              public WhileStatement(Expr condition, Statement body)
              {
                     Condition = condition;
                     Body = body;
              }

              public override T Accept<T>(IStmtVisitor visitor)
              {
                     return visitor.VisitWhileStatement<T>(this);
              }
       }

}

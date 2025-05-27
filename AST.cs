using System.Diagnostics;

namespace Compiler
{
       #region 
       public interface IExprVisitor<T>
       {//el q luego va a leer todo esto
              T VisitBinaryExpr(BinaryExpr expr);
              T VisitLiteralExpr(LiteralExpr expr);
              T VisitGroupingExpr(GroupingExpr expr);
              T VisitUnaryExpr(UnaryExpr expr);
              T VisitCallFunction(CallFunction expr);
              T VisitVariableExpr(VariableExpr expr);
       }
       public abstract class ASTNode
       {

       }
       public abstract class Expr : ASTNode
       {
              // Esto es para q todo el mudno tenga el accept
              public abstract T Accept<T>(IExprVisitor<T> visitor);
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

              public override T Accept<T>(IExprVisitor<T> visitor)
              {
                     return visitor.VisitBinaryExpr(this);
                     //esto es para q acept la instancia de la clase
              }

              /*// Ejemplo para BinaryExpr:
              public override T Accept<T>(IExprVisitor<T> visitor)
              {
                     return visitor.VisitBinaryExpr(this);
              }

// Ejemplo para CallComand:
public override T Accept<T>(IStmtVisitor<T> visitor)
{
    return visitor.VisitCallComand<T>(this);
}*/
       }

       // Aqui van numeros y strings
       public class LiteralExpr : Expr
       {
              public object Value { get; }

              public LiteralExpr(object value)
              {

                     Value = value;
              }

              public override T Accept<T>(IExprVisitor<T> visitor)
              {
                     return visitor.VisitLiteralExpr(this);
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

              public override T Accept<T>(IExprVisitor<T> visitor)
              {
                     return visitor.VisitGroupingExpr(this);
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

              public override T Accept<T>(IExprVisitor<T> visitor)
              {
                     return visitor.VisitUnaryExpr(this);
              }

       }
       public class VariableExpr : Expr
       {
              public string Identifier { get; }
              public VariableExpr(string identifier)
              {
                     Identifier = identifier;
              }

              public override T Accept<T>(IExprVisitor<T> visitor)
              {
                     return visitor.VisitVariableExpr(this);
              }

       }
       #endregion


       public abstract class Statement : ASTNode
       {
              public abstract T Accept<T>(IStmtVisitor<T> visitor);
       }

       public interface IStmtVisitor<T>
       {
              T VisitCallComand(CallComand statement);
              T VisitVarDeclaration(VarDeclaration statement);
              T VisitGoToStatement(GoToStatement statement);
              T VisitLabelDeclaration(LabelDeclaration statment);
       }

       public class CallFunction : Expr
       {
              //este va a ser el nodo de las funciones, q tiene q tener dentro una lista de nodos para los parámetros, y lo q hay dentro de la función ademas
              public TokenType Name { get; }
              public List<Expr> Parameters { get; }

              //debeía poner algo de lógica del tipo de retorno aquí?

              public CallFunction(TokenType name, List<Expr> parameters)
              {
                     Name = name;
                     Parameters = parameters;
                     //las funciones del pdf son en realidad comandos y no tienen nada dentro
              }

              public override T Accept<T>(IExprVisitor<T> visitor)
              {
                     return visitor.VisitCallFunction(this);
              }

       }

       public class CallComand : Statement
       {

              public TokenType Name { get; }
              public List<Expr> Parameters { get; }

              public CallComand(TokenType name, List<Expr> parameters)
              {
                     Name = name;
                     Parameters = parameters;
                     //las funciones del pdf son en realidad comandos y no tienen nada dentro
              }

              public override T Accept<T>(IStmtVisitor<T> visitor)
              {
                     return visitor.VisitCallComand(this);
              }

       }

       /*   public class IfStatement : Statement
                {
                       //este va a unir el if y el else. El branch else aquí puede ser nulo. Habra q especificar algo más?
                       //el pdf permite incluir if y else?
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
         */
       public class VarDeclaration : Statement
       {
              public string Name { get; }
              public Expr Initializer { get; }  // el pdf permit null?

              public VarDeclaration(string name, Expr initializer)
              {
                     Name = name;
                     Initializer = initializer;
              }

              public override T Accept<T>(IStmtVisitor<T> visitor)
              {
                     return visitor.VisitVarDeclaration(this);
              }

       }
       /*
       public class ExpressionStmt : Statement
       {
              public Expr Expression;
              public ExpressionStmt(Expr expression)
              {
                     Expression = expression;
              }
              public override T Accept<T>(IStmtVisitor visitor)
              {
                     return visitor.VisitExprStatement<T>(this);
              }
       }*/
       //esto no es en realidad para While sino parael GoTo del label, despué hay que revisr mejor
       public class GoToStatement : Statement
       {
              public string Location { get; }

              public Expr Condition { get; }
              public GoToStatement(string location, Expr condition)
              {
                     Location = location;
                     Condition = condition;
              }

              public override T Accept<T>(IStmtVisitor<T> visitor)
              {
                     return visitor.VisitGoToStatement(this);
              }
       }
       public class LabelDeclaration : Statement
       {
              public string LabelName { get; }

              public LabelDeclaration(string labelName)
              {
                     LabelName = labelName;
              }

              public override T Accept<T>(IStmtVisitor<T> visitor)
              {
                     return visitor.VisitLabelDeclaration(this);
              }

       }
}
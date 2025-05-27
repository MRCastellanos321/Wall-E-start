using System;
using System.Text;

namespace Compiler
{
    public class AstPrinter : IExprVisitor<string>, IStmtVisitor<string>
    {
        private int indentLevel = 0;

        public string Print(List<ASTNode> nodes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var node in nodes)
            {
                if (node is Expr expr)
                    sb.AppendLine(expr.Accept(this));
                else if (node is Statement stmt)
                    sb.AppendLine(stmt.Accept(this));
            }
            return sb.ToString();
        }

        private string Indent()
        {
            return new string(' ', indentLevel * 2);
        }

        public string VisitBinaryExpr(BinaryExpr expr)
        {
            return $"{Indent()}(Binario: {expr.Operator}\n" +
                   $"{expr.Left.Accept(this)}\n" +
                   $"{expr.Right.Accept(this)})";
        }

        public string VisitLiteralExpr(LiteralExpr expr)
        {
            return $"{Indent()}(Literal: {expr.Value})";
        }

        public string VisitGroupingExpr(GroupingExpr expr)
        {
            return $"{Indent()}(Agrupación:\n{expr.Expression.Accept(this)})";
        }

        public string VisitUnaryExpr(UnaryExpr expr)
        {
            return $"{Indent()}(Unario: {expr.Operator}\n{expr.Right.Accept(this)})";
        }

        public string VisitVariableExpr(VariableExpr expr)
        {
            return $"{Indent()}(Variable: {expr.Identifier})";
        }

        public string VisitCallFunction(CallFunction expr)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Indent()}(Llamada a {expr.Name}:\n");
            indentLevel++;
            foreach (var param in expr.Parameters)
            {
                sb.AppendLine(param.Accept(this));
            }
            indentLevel--;
            sb.Append($"{Indent()})");
            return sb.ToString();
        }

        public string VisitCallComand(CallComand stmt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Indent()}(Comando {stmt.Name}:\n");
            indentLevel++;
            foreach (var param in stmt.Parameters)
            {
                sb.AppendLine(param.Accept(this));
            }
            indentLevel--;
            sb.Append($"{Indent()})");
            return sb.ToString();
        }

        public string VisitVarDeclaration(VarDeclaration stmt)
        {
            return $"{Indent()}(Declarar '{stmt.Name}':\n{stmt.Initializer.Accept(this)})";
        }

        public string VisitGoToStatement(GoToStatement stmt)
        {
            return $"{Indent()}(GoTo {stmt.Location}:\n{stmt.Condition.Accept(this)})";
        }

        public string VisitLabelDeclaration(LabelDeclaration stmt)
        {
            return $"{Indent()}(Label: {stmt.LabelName})";
        }
    }
}
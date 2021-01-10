using System.Collections.Generic;

namespace CSLox
{
	public abstract class Expr
	{
		public interface IVisitor<R>
		{
			R VisitAssignExpr ( Assign expr );
			R VisitBinaryExpr ( Binary expr );
			R VisitCallExpr ( Call expr );
			R VisitGetExpr ( Get expr );
			R VisitSetExpr ( Set expr );
			R VisitSuperExpr ( Super expr );
			R VisitThisExpr ( This expr );
			R VisitGroupingExpr ( Grouping expr );
			R VisitLiteralExpr ( Literal expr );
			R VisitLogicalExpr ( Logical expr );
			R VisitUnaryExpr ( Unary expr );
			R VisitVariableExpr ( Variable expr );
		}

		public class Assign : Expr
		{
			public Assign ( Token name, Expr value )
			{
				this.name = name;
				this.value = value;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitAssignExpr(this);
			}

			public Token name;
			public Expr value;
		}

		public class Binary : Expr
		{
			public Binary ( Expr left, Token oper, Expr right )
			{
				this.left = left;
				this.oper = oper;
				this.right = right;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitBinaryExpr(this);
			}

			public Expr left;
			public Token oper;
			public Expr right;
		}

		public class Call : Expr
		{
			public Call ( Expr callee, Token paren, IList<Expr> arguments )
			{
				this.callee = callee;
				this.paren = paren;
				this.arguments = arguments;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitCallExpr(this);
			}

			public Expr callee;
			public Token paren;
			public IList<Expr> arguments;
		}

		public class Get : Expr
		{
			public Get ( Expr obj, Token name )
			{
				this.obj = obj;
				this.name = name;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitGetExpr(this);
			}

			public Expr obj;
			public Token name;
		}

		public class Set : Expr
		{
			public Set ( Expr obj, Token name, Expr value )
			{
				this.obj = obj;
				this.name = name;
				this.value = value;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitSetExpr(this);
			}

			public Expr obj;
			public Token name;
			public Expr value;
		}

		public class Super : Expr
		{
			public Super ( Token keyword, Token method )
			{
				this.keyword = keyword;
				this.method = method;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitSuperExpr(this);
			}

			public Token keyword;
			public Token method;
		}

		public class This : Expr
		{
			public This ( Token keyword )
			{
				this.keyword = keyword;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitThisExpr(this);
			}

			public Token keyword;
		}

		public class Grouping : Expr
		{
			public Grouping ( Expr expression )
			{
				this.expression = expression;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitGroupingExpr(this);
			}

			public Expr expression;
		}

		public class Literal : Expr
		{
			public Literal ( object value )
			{
				this.value = value;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitLiteralExpr(this);
			}

			public object value;
		}

		public class Logical : Expr
		{
			public Logical ( Expr left, Token oper, Expr right )
			{
				this.left = left;
				this.oper = oper;
				this.right = right;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitLogicalExpr(this);
			}

			public Expr left;
			public Token oper;
			public Expr right;
		}

		public class Unary : Expr
		{
			public Unary ( Token oper, Expr right )
			{
				this.oper = oper;
				this.right = right;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitUnaryExpr(this);
			}

			public Token oper;
			public Expr right;
		}

		public class Variable : Expr
		{
			public Variable ( Token name )
			{
				this.name = name;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitVariableExpr(this);
			}

			public Token name;
		}

		public abstract R Accept<R>(IVisitor<R> visitor);
	}
}

using System.Collections.Generic;

namespace CSLox
{
	public abstract class Stmt
	{
		public interface IVisitor<R>
		{
			R VisitBlockStmt ( Block stmt );
			R VisitExpressionStmt ( Expression stmt );
			R VisitPrintStmt ( Print stmt );
			R VisitVarStmt ( Var stmt );
		}

		public class Block : Stmt
		{
			public Block ( IList<Stmt> statements )
			{
				this.statements = statements;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitBlockStmt(this);
			}

			public IList<Stmt> statements;
		}

		public class Expression : Stmt
		{
			public Expression ( Expr expression )
			{
				this.expression = expression;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitExpressionStmt(this);
			}

			public Expr expression;
		}

		public class Print : Stmt
		{
			public Print ( Expr expression )
			{
				this.expression = expression;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitPrintStmt(this);
			}

			public Expr expression;
		}

		public class Var : Stmt
		{
			public Var ( Token name, Expr initilizer )
			{
				this.name = name;
				this.initilizer = initilizer;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitVarStmt(this);
			}

			public Token name;
			public Expr initilizer;
		}

		public abstract R Accept<R>(IVisitor<R> visitor);
	}
}

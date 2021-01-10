using System.Collections.Generic;

namespace CSLox
{
	public abstract class Stmt
	{
		public interface IVisitor<R>
		{
			R VisitBlockStmt ( Block stmt );
			R VisitClassStmt ( Class stmt );
			R VisitFunctionStmt ( Function stmt );
			R VisitIfStmt ( If stmt );
			R VisitExpressionStmt ( Expression stmt );
			R VisitPrintStmt ( Print stmt );
			R VisitReturnStmt ( Return stmt );
			R VisitVarStmt ( Var stmt );
			R VisitWhileStmt ( While stmt );
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

		public class Class : Stmt
		{
			public Class ( Token name, Expr.Variable superClass, IList<Stmt.Function> methods )
			{
				this.name = name;
				this.superClass = superClass;
				this.methods = methods;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitClassStmt(this);
			}

			public Token name;
			public Expr.Variable superClass;
			public IList<Stmt.Function> methods;
		}

		public class Function : Stmt
		{
			public Function ( Token name, IList<Token> parameters, IList<Stmt> body )
			{
				this.name = name;
				this.parameters = parameters;
				this.body = body;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitFunctionStmt(this);
			}

			public Token name;
			public IList<Token> parameters;
			public IList<Stmt> body;
		}

		public class If : Stmt
		{
			public If ( Expr condition, Stmt thenBranch, Stmt elseBranch )
			{
				this.condition = condition;
				this.thenBranch = thenBranch;
				this.elseBranch = elseBranch;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitIfStmt(this);
			}

			public Expr condition;
			public Stmt thenBranch;
			public Stmt elseBranch;
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

		public class Return : Stmt
		{
			public Return ( Token keyword, Expr value )
			{
				this.keyword = keyword;
				this.value = value;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitReturnStmt(this);
			}

			public Token keyword;
			public Expr value;
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

		public class While : Stmt
		{
			public While ( Expr condition, Stmt body )
			{
				this.condition = condition;
				this.body = body;
			}

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitWhileStmt(this);
			}

			public Expr condition;
			public Stmt body;
		}

		public abstract R Accept<R>(IVisitor<R> visitor);
	}
}

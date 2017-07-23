using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class UnaryOp:Types
    {
        Token token, op, name;
        Types expr;
        ParameterList plist;

        public UnaryOp(Token op, Types expr)
        {
            this.op = this.token = op;
            this.expr = expr;
        }
        public UnaryOp(Token op, Token name, ParameterList plist = null)
        {
            this.op = this.token = op;
            this.name = name;
            this.plist = plist;
        }

        public String Op { get { return Variable.GetOperatorStatic(op.type); } }
        public Types  Expr { get { return expr; } }
        public Token Name { get { return name; } }

        public override string Compile(int tabs = 0)
        {
            string tbs = DoTabs(tabs);
            string o = Variable.GetOperatorStatic(op.type);
            if (o == "new")
            {
                return tbs+"new " + name.Value + "(" + plist?.Compile(0) + ")";
            }
            if(o == "return")
            {
                return tbs + "return " + expr.Compile(0) + ";";
            }
            return tbs + Variable.GetOperatorStatic(op.type) + expr.Compile(0);
        }

        public override void Semantic()
        {
            
        }

        public override int Visit()
        {
            if (op.type == Token.Type.PLUS)
                return +expr.Visit();
            else if (op.type == Token.Type.MINUS)
                return -expr.Visit();
            return 0;
        }
    }
}

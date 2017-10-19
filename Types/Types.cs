using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public abstract class Types
    {
        //public abstract Token Token { get; }
        public abstract Token getToken();
        public string assignTo = "";
        public bool endit = true;
        public bool inParen = false;
        public Block assingBlock;
        public abstract int Visit();
        public abstract string Compile(int tabs = 0);
        public abstract string InterpetSelf();
        public abstract void Semantic();
        public string DoTabs(int tabs)
        {
            string r = "";
            for(int i=0; i < tabs; i++) { r += "  "; }
            return r;
        }
        public Variable TryVariable()
        {
            if (this is Number)
                return new Variable(((Number)this).getToken(), this.assingBlock, new Token(Token.Type.CLASS, "int"));
            if (this is CString)
                return new Variable(((CString)this).getToken(), this.assingBlock, new Token(Token.Type.CLASS, "string"));
            if (this is BinOp)
                return new Variable(new Token(Token.Type.NULL, ""), this.assingBlock, ((BinOp)this).OutputType);
            if(this is UnaryOp && ((UnaryOp)this).Op == "new")
                return new Variable(new Token(Token.Type.NULL, ""), this.assingBlock, ((UnaryOp)this).Name);
            if (this is UnaryOp && ((UnaryOp)this).Op == "call")
            {
                if(((UnaryOp)this).Name.type == Token.Type.LAMBDA)
                    return new Variable(new Token(Token.Type.NULL, ""), this.assingBlock, new Token(Token.Type.LAMBDA, "lambda"));
                return new Variable(new Token(Token.Type.NULL, ""), this.assingBlock, ((UnaryOp)this).Name);
            }
            if (this is UnaryOp && (((UnaryOp)this).Op == "-" || ((UnaryOp)this).Op == "++"))
                return new Variable(new Token(Token.Type.ID, (((UnaryOp)this).Expr).TryVariable().Value), this.assingBlock, new Token(Token.Type.CLASS, "int"));
            if (this is Generic)
                return new Variable(new Token(Token.Type.STRING, ((Generic)this).Name), this.assingBlock, new Token(Token.Type.CLASS, "object"));
            if (this is Lambda)
                return new Variable(new Token(Token.Type.STRING, ((Lambda)this).RealName), this.assingBlock, new Token(Token.Type.LAMBDA, "lambda"));
            if (this is Class)
                return new Variable(new Token(Token.Type.CLASS, ((Class)this).getName()), this.assingBlock, new Token(Token.Type.CLASS, ((Class)this).getName()));
            if (this is Null)
                return new Variable(new Token(Token.Type.NULL, ""), this.assingBlock, new Token(Token.Type.NULL, "null"));
            return (Variable)this;
        }
    }
}

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
        public Types assingToType = null;
        public Token assingToToken = null;
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
            if (this is Variable variable)
                return variable;

            switch (this)
            {
                case Number _:                    
                    return new Variable(((Number)this).getToken(), this.assingBlock, new Token(Token.Type.CLASS, "int"));
                case CString _:
                    return new Variable(((CString)this).getToken(), this.assingBlock, new Token(Token.Type.CLASS, "string"));
                case Assign asign when asign.Left is Variable va:
                    return va;
                case BinOp _:
                    return new Variable(new Token(Token.Type.NULL, ""), this.assingBlock, ((BinOp)this).OutputType);
                case UnaryOp _ when ((UnaryOp)this).Op == "new":
                    return new Variable(new Token(Token.Type.NULL, ""), this.assingBlock, ((UnaryOp)this).Name);
                case UnaryOp _ when ((UnaryOp)this).Op == "..":
                    return new Variable(new Token(Token.Type.NULL, ""), this.assingBlock, new Token(Token.Type.RANGE, "range"));
                case UnaryOp _ when ((UnaryOp)this).Op == "call":
                    if(((UnaryOp)this).Name.type == Token.Type.LAMBDA)
                        return new Variable(new Token(Token.Type.NULL, ""), this.assingBlock, new Token(Token.Type.LAMBDA, "lambda"));
                    return new Variable(new Token(Token.Type.NULL, ""), ((UnaryOp)this).Block, ((UnaryOp)this).OutputType);
                case UnaryOp _ when (((UnaryOp)this).Op == "-" || ((UnaryOp)this).Op == "++"):
                    return new Variable(new Token(Token.Type.ID, (((UnaryOp)this).Expr).TryVariable().Value), this.assingBlock, new Token(Token.Type.CLASS, "int"));
                case Generic _:
                    return new Variable(new Token(Token.Type.STRING, ((Generic)this).Name), this.assingBlock, new Token(Token.Type.CLASS, "object"));
                case Lambda _:
                    return new Variable(new Token(Token.Type.STRING, ((Lambda)this).RealName), this.assingBlock, new Token(Token.Type.LAMBDA, "lambda"));
                case Class _:
                    return new Variable(new Token(Token.Type.CLASS, ((Class)this).getName()), this.assingBlock, new Token(Token.Type.CLASS, ((Class)this).getName()));
                case Interface _:
                    return new Variable(new Token(Token.Type.CLASS, ((Interface)this).getName()), this.assingBlock, new Token(Token.Type.CLASS, ((Interface)this).getName()));
                case Null _:
                    return new Variable(new Token(Token.Type.NULL, ""), this.assingBlock, new Token(Token.Type.NULL, "null"));
                case Properties prop:
                    return (Variable)prop.variable;
                case Error _:
                    return new Variable(new Token(Token.Type.ID, ""), this.assingBlock);
            }

            return (Variable)this;
        }
    }
}

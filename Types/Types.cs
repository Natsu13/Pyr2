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
        public Block assingBlock;
        public abstract int Visit();
        public abstract string Compile(int tabs = 0);
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
            if (this is Null)
                return new Variable(new Token(Token.Type.NULL, ""), this.assingBlock, new Token(Token.Type.NULL, "null"));
            return (Variable)this;
        }
    }
}

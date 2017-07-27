using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class TypeObject
    {        
        public string Name { get; }
        public virtual bool SupportOp(string op)
        {
            if (op == "==" || op == "!=")
                return true;
            return false;
        }
        public virtual object Operator(string op, object first, object second)
        {
            return null;
        }
        public virtual bool SupportSecond(object second, object secondAsVariable)
        {
            return true;
        }
        public virtual Token OutputType(string op, object first, object second)
        {
            return new Token(Token.Type.CLASS, "object");
        }
    }
}

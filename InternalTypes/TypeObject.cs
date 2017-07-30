using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class TypeObject
    {
        public string Name { get { return "object"; } }
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
            if ((op == "==" || op == "!="))
                return new Token(Token.Type.BOOL, "bool");
            return new Token(Token.Type.CLASS, "object");
        }
        public virtual string ClassNameForLanguage()
        {
            if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                return "Object";
            return Name;
        }
    }
}

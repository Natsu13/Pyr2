using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class TypeInt:TypeObject
    {
        public new string Name { get { return "int"; } }

        public override Token OutputType(string op, object first, object second)
        {
            if ((first is Number || (first is Variable && ((Variable)first).Type == "int")) &&
                (second is Number || (second is Variable && ((Variable)second).Type == "int")) &&
                (op == "==" || op == "!="))
                return new Token(Token.Type.BOOL, "bool");
            return new Token(Token.Type.INTEGER, "int");
        }
        public override bool SupportOp(string op)
        {
            return true;
        }
        public override bool SupportSecond(object second, object secondAsVariable)
        {
            int output = 0;
            if (second is Number || (second is CString && Int32.TryParse(((CString)second).Value, out output)))
                return true;
            return false;
        }
        public override object Operator(string op, object first, object second)
        {
            int f = (int)first;
            if(second is int)
            {
                if (op == "+") return f + (int)second;
                if (op == "-") return f - (int)second;
                if (op == "/") return f / (int)second;
                if (op == "*") return f * (int)second;
            }

            return default(object);
        }
        public override string ClassNameForLanguage()
        {
            if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                return "Number";
            return Name;
        }
    }
}

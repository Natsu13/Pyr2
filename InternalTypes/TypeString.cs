using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class TypeString:TypeObject
    {
        public new string Name { get { return "string"; } }

        public override Token OutputType(string op, object first, object second)
        {
            if ((first is CString  || (first  is Variable && ((Variable)first).Type == "string")) && 
                (second is CString || (second is Variable && ((Variable)second).Type == "string")) && 
                op == "+")
                return new Token(Token.Type.STRING, "string");
            if ((first is CString || (first is Variable && ((Variable)first).Type == "string")) &&
                (second is CString || (second is Variable && ((Variable)second).Type == "string")) &&
                (op == "==" || op == "!="))
                return new Token(Token.Type.BOOL, "bool");
            return new Token(Token.Type.STRING, "string");
        }

        public override bool SupportOp(string op)
        {
            if(op == "+" || op == "==" || op == "!=")
                return true;
            return false;
        }
        public override bool SupportSecond(object second, object secondAsVariable)
        {
            return true;
        }
        public override object Operator(string op, object first, object second)
        {
            string f = first.ToString();
            if (op == "+") return f + second.ToString();
            return default(object);
        }
    }
}

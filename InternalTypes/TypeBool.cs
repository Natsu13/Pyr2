using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class TypeBool:TypeObject
    {
        public new string Name { get { return "bool"; } }
        public override Token OutputType(string op, object first, object second)
        {
            return new Token(Token.Type.BOOL, "bool");
        }
        public override bool SupportOp(string op)
        {
            if (op == "==" || op == "&&" || op == "||" || op == "!=")
                return true;
            return false;
        }
        public override bool SupportSecond(object second, object secondAsVariable)
        {
            if (second is Token && (((Token)second).type == Token.Type.TRUE || ((Token)second).type == Token.Type.FALSE))
                return true;
            if (((Variable)secondAsVariable).getType().type == Token.Type.BOOL)
                return true;
            return false;
        }
        public override object Operator(string op, object first, object second)
        {
            Token left = (Token)first;
            if (!(second is Token)) return false;
            Token right = (Token)second;
            if(op == "&&")
            {
                if (left.type == Token.Type.TRUE && right.type == Token.Type.TRUE)
                    return true;
            }
            else if(op == "||")
            {
                if (left.type == Token.Type.TRUE || right.type == Token.Type.TRUE)
                    return true;
            }

            return false;
        }
    }
}

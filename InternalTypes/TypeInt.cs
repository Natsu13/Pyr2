using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class TypeInt:TypeObject
    {
        public override bool SupportOp(string op)
        {
            return true;
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
    }
}

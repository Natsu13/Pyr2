using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class TypeString:TypeObject
    {
        public override bool SupportOp(string op)
        {
            if(op == "+")
                return true;
            return false;
        }
        public override object Operator(string op, object first, object second)
        {
            string f = first.ToString();
            if (op == "+") return f + second.ToString();
            return default(object);
        }
    }
}

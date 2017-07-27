using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class TypeNull : TypeObject
    {
        public new string Name { get { return "null"; } }

        public override bool SupportOp(string op)
        {
            return true;
        }
        public override object Operator(string op, object first, object second)
        {
            int f = 0;
            if (second is int)
            {
                if (op == "+") return f + (int)second;
                if (op == "-") return f - (int)second;
                if (op == "/") return f / (int)second;
                if (op == "*") return f * (int)second;
            }
            else
            {
                if (op == "+") return f.ToString() + second.ToString();
            }

            return default(object);
        }
    }
}

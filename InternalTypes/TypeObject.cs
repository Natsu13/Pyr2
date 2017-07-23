using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class TypeObject
    {        
        public virtual bool SupportOp(string op)
        {
            throw new NotImplementedException();
        }
        public virtual object Operator(string op, object first, object second)
        {
            throw new NotImplementedException();
        }
    }
}

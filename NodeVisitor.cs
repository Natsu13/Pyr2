using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class NodeVisitor
    {
        public NodeVisitor()
        {
            
        }

        public int Visit(Types node)
        {
            return node.Visit();
        }
    }
}

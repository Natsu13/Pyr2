using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public abstract class Types
    {
        public string assignTo = "";
        public Block assingBlock;
        public abstract int Visit();
        public abstract string Compile(int tabs = 0);
        public abstract void Semantic();
        public string DoTabs(int tabs)
        {
            string r = "";
            for(int i=0; i < tabs; i++) { r += "  "; }
            return r;
        }
    }
}

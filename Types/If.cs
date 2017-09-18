using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class If:Types
    {
        Dictionary<Types, Block> conditions = new Dictionary<Types, Block>();

        public If(Dictionary<Types, Block> conditions)
        {
            this.conditions = conditions;
        }

        public override string Compile(int tabs = 0)
        {
            tabs++;
            string tbs = DoTabs(tabs+1);
            string ret = "";
            bool first = true;

            tabs+=2;
            foreach (KeyValuePair<Types, Block> c in conditions)
            {
                c.Value.Parent = assingBlock;
                if (first)
                {                    
                    first = false;
                    ret += "if(" + c.Key.Compile() + ") {\n" + c.Value.Compile(tabs) + tbs + "}\n";
                }else if(c.Key is NoOp)
                {
                    ret += tbs + "else {\n" + c.Value.Compile(tabs) + tbs + "}\n";
                }
                else
                {
                    ret += tbs + "else if(" + c.Key.Compile() + ") {\n" + c.Value.Compile(tabs) + tbs + "}\n";
                }
            }
            return ret.Substring(0,ret.Length - 1);
        }
        public override Token getToken() { return null; }

        public override void Semantic()
        {
            foreach(KeyValuePair<Types, Block> c in conditions)
            {
                if(!(c.Key is NoOp))
                    c.Key.Semantic();
                c.Value.assingBlock = assingBlock;
                c.Value.Semantic();
            }
        }

        public override int Visit()
        {
            return 0;
        }
    }
}

﻿using System;
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
            string tbs = DoTabs(tabs);
            string ret = "";
            bool first = true;
            
            tabs++;
            foreach (var c in conditions)
            {
                c.Value.Parent = assingBlock;
                if(c.Key != null)
                    c.Key.endit = false;
                if (first)
                {                    
                    first = false;
                    ret += "if(" + c.Key?.Compile() + ") {\n" + c.Value.Compile(tabs) + DoTabs(tabs-2) + "  }\n";
                }else if(c.Key is NoOp)
                {
                    ret += tbs + "else {\n" + c.Value.Compile(tabs) + DoTabs(tabs-2) + "  }\n";
                }
                else
                {
                    ret += tbs + "else if(" + c.Key?.Compile() + ") {\n" + c.Value.Compile(tabs) + DoTabs(tabs-2) + "  }\n";
                }
            }
            return ret.Substring(0,ret.Length - 1);
        }
        public override Token getToken() { return null; }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }

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

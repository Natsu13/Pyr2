﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class Interface:Types
    {
        Token name;
        Block block;
        List<Token> parents;
        public bool isExternal = false;
        public Token _external;

        public Interface(Token name, Block block, List<Token> parents)
        {
            this.name = name;
            this.block = block;
            this.block.blockAssignTo = name.Value;
            this.assingBlock = block;
            this.parents = parents;
        }
        public Block Block { get { return block; } }
        public override string Compile(int tabs = 0)
        {
            if (!isExternal)
            {
                string tbs = DoTabs(tabs);
                string ret = tbs + "var " + name.Value + " = function(){";
                if (block.variables.Count != 0 || parents.Count != 0) ret += "\n";
                foreach (Token parent in parents)
                {
                    ret += tbs + "\t" + parent.Value + ".call(this);\n";
                }
                foreach (KeyValuePair<string, Assign> var in block.variables)
                {
                    //var.Key + " => ["+var.Value.GetType()+"] " + var.Value.GetVal()
                    if(var.Value.Right is Null)
                        ret += tbs + "\tthis." + var.Key + " = '';\n";
                    else if (var.Value.GetType() == "string")
                        ret += tbs + "\tthis." + var.Key + " = " + var.Value.Compile() + ";\n";
                    else
                        ret += tbs + "\tthis." + var.Key + " = " + var.Value.GetVal() + ";\n";
                }
                ret += tbs + "}\n";
                ret += block.Compile(tabs);
                return ret;
            }
            return "";
        }

        public override Token getToken() { return null; }

        public override void Semantic()
        {
            foreach (KeyValuePair<string, Assign> var in block.variables)
            {
                var.Value.Semantic();
            }
            block.Semantic();
        }

        public override int Visit()
        {
            return 0;
        }
    }
}

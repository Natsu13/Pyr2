using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Class : Types
    {
        Token name;
        Block block;
        List<Token> parents;

        public Class(Token name, Block block, List<Token> parents)
        {
            this.name = name;
            this.block = block;
            this.block.blockAssignTo = name.Value;
            this.assingBlock = block;
            this.parents = parents;
        }
        public override string Compile(int tabs = 0)
        {
            string tbs = DoTabs(tabs);
            string ret = tbs + "var " + name.Value + " = function(){";
            if (block.variables.Count != 0 || parents.Count != 0) ret += "\n";
            foreach(Token parent in parents)
            {
                ret += tbs + "\t" + parent.Value + ".call(this);\n";
            }
            foreach(KeyValuePair<string, Assign> var in block.variables)
            {
                //var.Key + " => ["+var.Value.GetType()+"] " + var.Value.GetVal()
                if (var.Value.GetType() == "string")
                    ret += tbs + "\tthis." + var.Key + " = NotImplemented();";
                else
                    ret += tbs + "\tthis." + var.Key + " = " + var.Value.GetVal() + ";\n";
            }
            ret += tbs + "}\n";
            ret += block.Compile(tabs);
            return ret;
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

    public class Class<T>:Types where T:new()
    {
        string name;
        T intr;

        public Class(string name)
        {
            this.name = name;
            this.intr = new T();
        }       

        public T Inter { get { return intr; } }
        public override Token getToken() { return null; }

        public override string Compile(int tabs = 0)
        {
            return "";
        }

        public override void Semantic()
        {
            
        }

        public override int Visit()
        {
            return 0;
        }        
    }
}

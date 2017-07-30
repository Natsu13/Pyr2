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
        public Block block;
        List<Token> parents;
        public bool isExternal = false;
        public Token _external;
        public string JSName = "";

        public Class(Token name, Block block, List<Token> parents)
        {
            this.name = name;
            this.block = block;
            this.block.blockAssignTo = name.Value;
            this.assingBlock = block;
            this.parents = parents;
        }

        public bool haveParent(Token name)
        {
            return parents.Contains(name);
        }

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
                    if (var.Value.GetType() == "string")
                        ret += tbs + "\tthis." + var.Key + " = " + var.Value.Compile() + ";";
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

        public Token OutputType(string op, object a, object b)
        {
            if(block.SymbolTable.Find("operator " + op))
            {
                Types t = block.SymbolTable.Get("operator " + op);
                if(t is Function f)
                {
                    return f.Returnt;
                }
            }
            return new Token(Token.Type.VOID, "void");
        }
        public bool SupportOp(string op)
        {
            if (block.SymbolTable.Find("operator " + op))
            {
                Types t = block.SymbolTable.Get("operator " + op);
                if (t is Function f)
                {
                    return true;
                }
            }
            return false;
        }
        public bool SupportSecond(string op, object second, object secondAsVariable)
        {
            if (block.SymbolTable.Find("operator " + op))
            {
                Types t = block.SymbolTable.Get("operator " + op);
                if (t is Function f)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class Class<T>:Types where T:new()
    {
        string name;
        List<Token> parents;
        T intr;
        Block block;        

        public Class(Interpreter interpreter, Block block, string name, List<Token> parents)
        {
            this.name = name;
            this.parents = parents;
            this.intr = new T();
            this.block = new Block(interpreter);
            this.block.Parent = block;
        }

        public bool haveParent(string name)
        {
            foreach(Token t in parents)
            {
                if (t.Value == name)
                    return true;
            }
            return false;
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

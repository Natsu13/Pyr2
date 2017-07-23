using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Block:Types
    {
        public List<Types> children = new List<Types>();
        public Dictionary<string, Assign> variables = new Dictionary<string, Assign>();
        Interpreter interpret;
        public String blockAssignTo = "";
        Block parent = null;
        SymbolTable symbolTable;

        public Block(Interpreter interpret)
        {
            this.interpret = interpret;
            symbolTable = new SymbolTable(interpret, this);
        }        
        public Block Parent { get { return parent; } set { parent = value; } }
        public Interpreter Interpret { get { return this.interpret; } }
        public SymbolTable SymbolTable { get { return symbolTable; } }

        public override string Compile(int tabs = 0)
        {
            string tbs = DoTabs(tabs);
            string ret = "";
            foreach (Types child in children)
            {
                child.assignTo = blockAssignTo;
                string p = child.Compile((tabs > 0?tabs-1:tabs));
                if(p != "")
                    ret += tbs + p + "\n";
            }
            return ret;
        }

        public override int Visit()
        {
            foreach(Types child in children)
            {
                child.Visit();
            }
            return 0;
        }

        public override void Semantic()
        {
            foreach (Types child in children)
            {
                child.Semantic();
            }
        }
    }
}

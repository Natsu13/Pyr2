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
        public enum BlockType { NONE, FUNCTION, CLASS };
        BlockType type = BlockType.NONE;

        public Block(Interpreter interpret)
        {
            this.interpret = interpret;
            symbolTable = new SymbolTable(interpret, this);
        }        
        public Block Parent { get { return parent; } set { parent = value; } }
        public BlockType Type { get { return type; } set { type = value; } }
        public Interpreter Interpret { get { return this.interpret; } }
        public SymbolTable SymbolTable { get { return symbolTable; } }

        public void CheckReturnType(string type)
        {
            foreach (Types child in children)
            {
                if (!(child is UnaryOp)) continue;
                UnaryOp uop = (UnaryOp)child;
                if(uop.Op == "return")
                {
                    if (uop.Expr is Variable) {
                        if(((Variable)uop.Expr).Type == "dynamic")
                        {
                            Assign ava = FindVariable(((Variable)uop.Expr).Value);
                            if (ava == null)
                            {
                                Interpreter.semanticError.Add(new Error("Variable " + ((Variable)uop.Expr).Value + " not exists", Interpreter.ErrorType.ERROR));
                                continue;
                            }
                            if(ava.GetType() != type)
                                Interpreter.semanticError.Add(new Error("Variable " + ((Variable)uop.Expr).Value + " with type " + ava.GetType() + " can't be converted to " + type, Interpreter.ErrorType.ERROR));
                        }                            
                        else if (((Variable)uop.Expr).Type != type)
                            Interpreter.semanticError.Add(new Error("Variable " + ((Variable)uop.Expr).Value + " with type " + ((Variable)uop.Expr).Type + " can't be converted to " + type, Interpreter.ErrorType.ERROR));
                    }
                    else if((uop.Expr is Number && type != "int") || (uop.Expr is CString && type != "string"))
                        Interpreter.semanticError.Add(new Error("Variable " + ((Variable)uop.Expr).Value + " with type " + ((Variable)uop.Expr).Type + " can't be converted to " + type, Interpreter.ErrorType.ERROR));
                }
            }
        }

        public Assign FindVariable(string name)
        {
            if (variables.ContainsKey(name))
                return variables[name];
            else
            {
                if (parent != null)
                    return parent.FindVariable(name);
                return null;
            }
        }

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

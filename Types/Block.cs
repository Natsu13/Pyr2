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
        public string blockAssignTo = "";
        public string blockClassTo = "";
        Block parent = null;
        SymbolTable symbolTable;        
        public enum BlockType { NONE, FUNCTION, CLASS, CONDITION, INTERFACE, FOR, WHILE, PROPERTIES, LAMBDA };
        BlockType type = BlockType.NONE;
        public bool _isInConstructor = false;
        public Import import = null;
        List<_Attribute> attributes = new List<_Attribute>();
        Token token = null;

        public bool isInConstructor
        {
            get {
                if (parent != null && !_isInConstructor)
                    return parent.isInConstructor;
                return _isInConstructor;
            }
            set { _isInConstructor = value; }
        }

        public Block(Interpreter interpret, bool first = false, Token token = null)
        {
            this.interpret = interpret;
            symbolTable = new SymbolTable(interpret, this, first);
        }        
        public Block Parent { get { return parent; } set { parent = value; } }
        public BlockType Type { get { return type; } set { type = value; } }
        public Interpreter Interpret { get { return this.interpret; } }
        public SymbolTable SymbolTable { get { return symbolTable; } }
        public List<_Attribute> Attributes { get { return attributes; } set { attributes = value; } }
        
        public bool isType(BlockType type)
        {
            if (this.type == type)
                return true;
            if (parent != null && parent.isType(type))
                return true;
            return false;
        }

        public bool isAttribute(string name, bool one = false)
        {
            foreach(var attr in attributes)
            {
                if (attr.GetName(true) == name)
                    return true;
            }
            if (!one && parent != null && parent.isAttribute(name))
                return true;
            return false;
        }

        public string getClass()
        {
            if (blockClassTo != "")
                return blockClassTo;
            if (parent != null)
                return parent.getClass();
            return "";
        }

        public override Token getToken(){ return token; }
        public void setToken(Token t) { token = t; }

        public void CheckReturnType(string type, bool isNull)
        {
            foreach (Types child in children)
            {
                if (!(child is UnaryOp)) continue;
                UnaryOp uop = (UnaryOp)child;
                if(uop.Op == "return")
                {
                    if (isNull)
                    {
                        if (uop.Expr != null)
                        {
                            Function asfunc = (Function)SymbolTable.Get(assignTo);
                            Interpreter.semanticError.Add(new Error("#400 Because your function " + assignTo + "(" + asfunc.ParameterList.List() + ") return void you can't return value", Interpreter.ErrorType.ERROR, uop.getToken()));                            
                        }
                        continue;
                    }
                    if (uop.Expr is Variable) {
                        if (((Variable)uop.Expr).Type == "auto")
                        {
                            string newname = ((Variable)uop.Expr).Value;
                            if (((Variable)uop.Expr).Value == "this")
                                continue;
                            if(((Variable)uop.Expr).Value.Split('.')[0] == "this")
                                newname = parent.assignTo + "." + string.Join(".", ((Variable)uop.Expr).Value.Split('.').Skip(1));                            
                            Types avaq = SymbolTable.Get(newname);
                            Assign ava = null;
                            if (!(avaq is Error))
                                ava = (Assign)avaq;
                            if ((((Variable)uop.Expr).Value == "true" || ((Variable)uop.Expr).Value == "false") && type == "bool")
                                continue;
                            if (ava == null)
                            {
                                Interpreter.semanticError.Add(new Error("#401 Variable " + ((Variable)uop.Expr).Value + " not exists", Interpreter.ErrorType.ERROR, ((Variable)uop.Expr).getToken()));
                                continue;
                            }
                            if (type != null && ava.GetType() != type)
                                Interpreter.semanticError.Add(new Error("#402 Variable " + ((Variable)uop.Expr).Value + " with type " + ava.GetType() + " can't be converted to " + type, Interpreter.ErrorType.ERROR, ((Variable)uop.Expr).getToken()));
                            if (type == null) type = ava.GetType();
                        }
                        else if (type != null && ((Variable)uop.Expr).Type != type)
                            Interpreter.semanticError.Add(new Error("#403 Variable " + ((Variable)uop.Expr).Value + " with type " + ((Variable)uop.Expr).Type + " can't be converted to " + type, Interpreter.ErrorType.ERROR, ((Variable)uop.Expr).getToken()));
                        else if (type == null)
                            type = ((Variable)uop.Expr).Type;
                    }
                    else if(type != null && (uop.Expr is Number && type != "int"))
                        Interpreter.semanticError.Add(new Error("#404 int can't be converted to " + type, Interpreter.ErrorType.ERROR, ((Number)uop.Expr).getToken()));
                    else if(type != null && (uop.Expr is CString && type != "string"))
                        Interpreter.semanticError.Add(new Error("#405 string can't be converted to " + type, Interpreter.ErrorType.ERROR, ((CString)uop.Expr).getToken()));
                    else if(type == null)
                    {
                        if (uop.Expr is Number) type = "int";
                        if (uop.Expr is CString) type = "string";
                    }                        
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

        public string Compile(int tabs = 0, bool noAssign = false)
        {
            string tbs = DoTabs(tabs);
            string ret = "";
            foreach (Types child in children)
            {
                if (child == null) continue;
                if (noAssign && child is Assign) continue;
                child.assignTo = (assingBlock == null?blockAssignTo:assingBlock.assignTo);    
                if(!(child is Class))
                    child.assingBlock = this;
                string p = child.Compile(0);
                if (tabs != 0)
                {
                    var returntab = "";
                    if (p != "")
                    {
                        foreach (var s in p.Split('\n'))
                        {
                            returntab += (returntab == "" ? s : (s.Trim() == s ? tbs + s : s)) + "\n";
                        }

                        p = returntab.Substring(returntab.Length - 1) == "\n" ? returntab.Substring(0, returntab.Length - 1) : returntab;
                    }

                    if (p != "" && p.Substring(p.Length - 2, 1) != "\n")
                        ret += tbs + p + "\n";
                    else if(p != "")
                        ret += p + "\n";
                }
                else
                {
                    if (p != "" && p.Substring(p.Length - 2, 1) != "\n")
                        ret += tbs + p + "\n";
                    else
                        ret += p + "\n";
                }
            }
            return ret;
        }
        public override string Compile(int tabs = 0)
        {
            return Compile(tabs, false);
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
            if (isAttribute("Obsolete", true))
            {
                Interpreter.semanticError.Add(new Error("#499 Block of code is marked as Obsolete!", Interpreter.ErrorType.WARNING, getToken()));
            }
            foreach (Types child in children)
            {
                if (child == null) continue;
                child.Semantic();
            }
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

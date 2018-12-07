using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class Block:Types
    {
        public List<Types> children = new List<Types>();
        public Dictionary<string, Assign> variables = new Dictionary<string, Assign>();
        Interpreter interpret;
        public string blockAssignTo = "";
        public string blockClassTo = "";
        SymbolTable symbolTable => interpret.SymbolTable;        
        public enum BlockType { NONE, FUNCTION, CLASS, CONDITION, INTERFACE, FOR, WHILE, PROPERTIES, LAMBDA, COMPONENT };
        BlockType type = BlockType.NONE;
        public bool _isInConstructor = false;
        public Import import = null;
        List<_Attribute> attributes = new List<_Attribute>();
        Token token = null;
        private bool _first;
        private static int _idCounter = 0;
        public int _id = _idCounter++;

        public Block _blockParent = null;
        public Block BlockParent
        {
            get
            {
                return _blockParent ?? (parent as Block);
            }
            set
            {
                if (value is Block)
                    _blockParent = value;
                else
                    parent = value;
            }
        }

        public bool isInConstructor
        {
            get {
                if (parent != null && !_isInConstructor && parent is Block)
                    return ((Block)parent).isInConstructor;
                return _isInConstructor;
            }
            set { _isInConstructor = value; }
        }

        /*Serialization to JSON object for export*/
        [JsonParam("BlockType")] public int _BlockType => (int)type;
        [JsonParam] public List<_Attribute> Attributes { get => attributes; set => attributes = value; }
        [JsonParam] public List<Types> Childrens => children;
        //[JsonParam] public Dictionary<string, JObject> Variables => variables.ToDictionary(x => x.Key, x => JsonParam.ToJson(x.Value));
        [JsonParam] public bool IsInConstructor => isInConstructor;
        [JsonParam] public bool First => _first;

        public override void FromJson(JObject o)
        {
            interpret = Interpreter.CurrentStaticInterpreter;
            //symbolTable = new SymbolTable(interpret, this, (bool)o["First"]);
            type = (BlockType)(int)o["BlockType"];
            attributes = JsonParam.FromJsonArray<_Attribute>((JArray) o["Attributes"]);
            children = JsonParam.FromJsonArray<Types>((JArray) o["Childrens"]);
            isInConstructor = (bool) o["IsInConstructor"];
            foreach (var typese in children.ToList())
            {
                if (typese is NoOp)
                    children.Remove(typese);
                else
                    typese.assingBlock = this;                
            }
        }
        public Block() { }

        public Block(Interpreter interpret, bool first = false, Token token = null)
        {
            this.interpret = interpret;
            this._first = first;
        }        
        public BlockType Type { get { return type; } set { type = value; } }
        public Interpreter Interpret { get { return this.interpret; } }
        public SymbolTable SymbolTable { get { return symbolTable; } }        
        
        public bool isType(BlockType type)
        {
            if (this.type == type)
                return true;
            if (BlockParent != null && BlockParent.isType(type))
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
            if (!one && BlockParent != null && BlockParent.isAttribute(name))
                return true;
            return false;
        }

        public string getClass()
        {
            if (blockClassTo != "")
                return blockClassTo;
            if (parent != null)
                return ((Block)parent).getClass();
            return "";
        }

        public override Token getToken()
        {
            /*
            return token == null ? 
                    (new Token(Token.Type.STRING, 
                        (!string.IsNullOrEmpty(blockAssignTo) ? 
                            blockAssignTo : 
                            "block"+_id )
                        )
                    ) : token;  
                        */
            if (assingToType != null)
            {
                var ast = assingToType.getToken();
                if (ast != null)
                    return ast;
            }

            return (new Token(Token.Type.STRING,
                (!string.IsNullOrEmpty(blockAssignTo) ? blockAssignTo : "block" + _id)
            ));
            //return new Token(Token.Type.STRING, "block_" + blockAssignTo + "_" + _id);
        }
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
                        {
                            var notType = true;
                            var cla = assingBlock.SymbolTable.Get(((Variable) uop.Expr).Type);
                            if (cla is Class clac)
                            {
                                if (!clac.haveParent(type))
                                    notType = false;
                            }
                            else if (cla is Interface clai)
                            {
                                if (!clai.haveParent(type))
                                    notType = false;
                            }
                            if(!notType)
                                Interpreter.semanticError.Add(new Error("#403 Variable " + ((Variable)uop.Expr).Value + " with type " + ((Variable)uop.Expr).Type + " can't be converted to " + type, Interpreter.ErrorType.ERROR, ((Variable)uop.Expr).getToken()));
                        }
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
                return ((Block)parent)?.FindVariable(name);
            }
        }

        public Block GetBlock(Block.BlockType type, List<Block.BlockType> nottype = null)
        {
            if (type == this.type)
                return this;
            
            if (parent != null)
            {               
                if(((Block)parent).type == type)
                    return ((Block)parent);
                if (nottype == null || !nottype.Contains(((Block)parent).type))
                    return ((Block)parent).GetBlock(type, nottype);
            }            

            return null;
        }

        public void Add(Block block)
        {
            if (block == null)
                return;
            //symbolTable.Add(block);
            foreach (var blockVariable in (block.variables).ToList())
            {
                variables[blockVariable.Key] = blockVariable.Value;
            }
        }
        
        public string Compile(int tabs = 0, bool noAssign = false, bool componentSetFirst = false)
        {
            var tbs = DoTabs(tabs);
            var ret = new StringBuilder();
            foreach (Types child in children)
            {                
                if (child == null) continue;
                if (noAssign && child is Assign) continue;
                child.assignTo = (assingBlock == null?blockAssignTo:assingBlock.assignTo);    
                if (child is Component chc && componentSetFirst)
                    chc.IsStart = true;
                /*
                if(!(child is Class) && !(child is _Enum))
                    child.assingBlock = this;
                */
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
                        ret.Append(tbs + p + "\n");
                    else if(p != "")
                        ret.Append(p + "\n");
                }
                else
                {
                    if (p != "" && p.Substring(p.Length - 2, 1) != "\n")
                        ret.Append(tbs + p + "\n");
                    else
                        ret.Append(p + "\n");
                }
            }

            ret.Append(tbs + "{" + ret + tbs + "}"); 

            return ret.ToString();
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

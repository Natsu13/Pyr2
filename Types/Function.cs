using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Function:Types
    {
        Token name;
        Block block;
        ParameterList paraml;
        Token returnt;
        public bool isStatic = false;
        public Token _static;
        public bool isExternal = false;
        public Token _external;
        public bool isDynamic = false;
        public Token _dynamic;        
        public bool isExtending = false;
        public string extendingClass = "";
        public bool isOperator = false;
        public bool isConstructor = false;
        public Token _constuctor;
        public bool returnAsArray = false;
        public List<_Attribute> attributes = new List<_Attribute>();
        public List<string> returnGeneric = new List<string>();
        List<string> genericArguments = new List<string>();

        bool parentNotDefined = false;
        bool parentIsNotClassOrInterface = false;        

        public Function(Token name, Block _block, ParameterList paraml, Token returnt, Interpreter interpret, Block parent_block = null)
        {
            this.name = name;
            if (name.Value.Contains("."))
            {
                string[] _name = name.Value.Split('.');
                this.name = new Token(Token.Type.ID, _name.Last<string>(), name.Pos, name.File);
                string _className = string.Join(".", _name.Take(_name.Length - 1));
                extendingClass = _className;
                if(_block == null)
                {
                    _block = new Block(interpret);
                    _block.Parent = parent_block;
                }
                if (_block.SymbolTable.Find(_className))
                {
                    isExtending = true;
                    Types t = _block.SymbolTable.Get(_className);
                    Type tg = _block.SymbolTable.GetType(_className);
                    if (t is Class c)
                    {
                        c.assingBlock.SymbolTable.Add(this.name.Value, this);
                        if (c.JSName != "")
                            extendingClass = c.JSName;
                    }
                    else if (t is Interface i)
                    {
                        i.assingBlock.SymbolTable.Add(this.name.Value, this);
                    }
                    else if(tg != null)
                    {
                        dynamic dt = t;
                        extendingClass = dt.Inter.ClassNameForLanguage();
                    }
                    else parentIsNotClassOrInterface = true;
                }
                else parentNotDefined = true;
            }            
            this.block = _block;
            if (_block != null)
            {
                this.block.assignTo = name.Value;
                this.block.assingBlock = this.block;
            }
            //this.block.blockAssignTo = name.Value;
            this.paraml = paraml;
            this.paraml.assingBlock = this.block;
            this.returnt = returnt;

            if (this.name.Value == extendingClass || (_block?.Parent != null && this.name.Value == _block?.Parent.assignTo))
            {
                isConstructor = true;
                if(block != null)
                    block.isInConstructor = true;
            }
        }

        public void AddGenericArg(string name)
        {
            genericArguments.Add(name);
        }
        public void SetGenericArgs(List<string> list)
        {
            genericArguments = list;
        }
        public List<string> GenericArguments { get { return genericArguments; } }

        public ParameterList ParameterList { get { return paraml; } }
        public override Token getToken() { return null; }
        public Token Returnt { get { return returnt; } }
        public Block Block { get { return block; } }

        public string _hash = "";
        public string getHash()
        {
            if (assingBlock == null) assingBlock = block;
            if (isOperator || isConstructor || assingBlock.SymbolTable.GetAll(name.Value)?.Count > 1)
            {
                if (_hash == "")
                    _hash = string.Format("{0:X8}", (name.Value + paraml.List() + block?.GetHashCode()).GetHashCode());
                return _hash;
            }
            return "";
        }
        public string Name {
            get {
                string hash = getHash();
                if (isConstructor)
                    return "constructor_" + hash;
                return name.Value + (hash != "" ? "_" + hash : "");
            }
        }
        public string RealName { get { return name.Value; } }

        public string Return()
        {
            if (returnt == null)
                return "auto";
            return returnt?.Value + (returnGeneric.Count > 0 ? "<" + string.Join(", ", returnGeneric) + ">" : "") + (returnAsArray ? "[]" : "");
        }

        public override string Compile(int tabs = 0)
        {
            string functionOpname = "";
            string ret = "";

            string addCode = "";
            if (attributes?.Where(x => x.GetName(true) == "Debug").Count() > 0)
            {
                if(Interpreter._DEBUG)
                    Debugger.Break();
                addCode = "debugger;";
            }

            if (!isExternal)
            {
                Class c = null;
                Interface i_ = null;
                string fromClass = "";
                string tbs = DoTabs(tabs);
                if (isExtending)
                {
                    if (assingBlock.SymbolTable.Find(extendingClass))
                    {
                        var ex = assingBlock.SymbolTable.Get(extendingClass, false, true);
                        if(ex is Import)
                        {
                            extendingClass = ((Import)ex).GetName() + "." + extendingClass;
                        }
                    }
                    fromClass = extendingClass;
                    if (isStatic || isConstructor)
                    {
                        ret += tbs + extendingClass + "." + Name + " = function(" + paraml.Compile(0) + "){" + (block != null ? "\n" : "");
                        functionOpname = extendingClass + "." + Name;
                    }
                    else
                    {
                        ret += tbs + extendingClass + ".prototype." + Name + " = function(" + paraml.Compile(0) + "){" + (block != null ? "\n" : "");
                        functionOpname = extendingClass + ".prototype." + Name;
                    }
                }
                else if (assignTo == "")
                {
                    ret += tbs + "function " + Name + "(" + paraml.Compile(0);
                    if(genericArguments.Count > 0)
                    {
                        int q = 0;
                        foreach (string generic in genericArguments)
                        {
                            if (q != 0) ret += ", ";
                            else if (paraml.Parameters.Count > 0) { ret += ", "; }
                            ret += "generic$" + generic;
                            q++;
                        }
                    }
                    ret += "){" + (block != null ? "\n" : "");
                    functionOpname = Name;
                }
                else
                {
                    fromClass = assignTo;
                    string hash_name = "";
                    Types fg = assingBlock.assingToType;
                    if (fg is Class fgc)
                        hash_name = fgc.getName();
                    if (fg is Interface fgi)
                        hash_name = fgi.getName();

                    if (isStatic || isConstructor)
                    {                        
                        if (fg is Class)
                            c = (Class)fg;
                        if (fg is Interface)
                            i_ = (Interface)fg;

                        if (isConstructor && c.GenericArguments.Count > 0)
                        {
                            paraml.assingBlock = assingBlock;
                            ret += tbs + hash_name + "." + Name + " = function(" + paraml.Compile(0);
                            functionOpname = hash_name + "." + Name;
                            bool f = true;
                            if (paraml.Parameters.Count > 0) f = false;
                            foreach (string generic in c.GenericArguments)
                            {
                                if (!f) ret += ", ";
                                f = false;
                                ret += "generic$" + generic;
                            }
                            ret += "){" + (block != null ? "\n" : "");
                        }
                        else
                        {
                            ret += tbs + hash_name + "." + Name + " = function(" + paraml.Compile(0);
                            if(genericArguments.Count > 0)
                            {
                                int q = 0;
                                foreach (string generic in genericArguments)
                                {
                                    if (q != 0) ret += ", ";
                                    else if (paraml.Parameters.Count > 0) { ret += ", "; }
                                    ret += "generic$" + generic;
                                    q++;
                                }
                            }
                            ret += "){" + (block != null ? "\n" : "");
                            functionOpname = hash_name + "." + Name;
                        }
                    }
                    else
                    {
                        ret += tbs + hash_name + ".prototype." + Name + " = function(" + paraml.Compile(0);
                        if(genericArguments.Count > 0)
                        {
                            int q = 0;
                            foreach (string generic in genericArguments)
                            {
                                if (q != 0) ret += ", ";
                                else if (paraml.Parameters.Count > 0) { ret += ", "; }
                                ret += "generic$" + generic;
                                q++;
                            }
                        }
                        ret += "){" + (block != null ? "\n" : "");
                        functionOpname = hash_name + ".prototype." + Name;
                    }
                }

                if(addCode != "")
                    ret += tbs + "  " + addCode + "\n";

                //if (genericArguments.Count != 0) ret += "\n";
                foreach (string generic in genericArguments)
                {
                    block.SymbolTable.Add(generic, new Generic(this, block, generic) { assingBlock = block });
                }

                if(isConstructor)
                {
                    if (block == null) ret += "\n";
                    ret += tbs + "  var $this = Object.create(" + fromClass + ".prototype);\n";
                    ret += tbs + "  " + fromClass + ".call($this);\n";
                    if(c != null)
                    {
                        foreach (string generic in c.GenericArguments)
                        {                            
                            ret += tbs + "  $this.generic$" + generic + " = generic$" + generic+";\n";
                        }
                    }
                }

                foreach (Types t in paraml.Parameters)
                {                    
                    if (t is Assign a)
                    {
                        ret += tbs + "  if(typeof "+a.Left.Compile()+" == \"undefined\") " + a.Left.Compile()+" = " + a.Right.Compile()+";\n";
                    }
                }

                if (block != null)
                    ret += block.Compile(tabs + 1);
                if (isConstructor)
                {
                    ret += tbs + "  return $this;\n";
                }
                ret += tbs + "}\n";
                
                if(functionOpname.Split('.').Count() > 1)
                    ret += tbs + functionOpname + "$META = function(){\n";
                else
                    ret += tbs + "var " + functionOpname + "$META = function(){\n";
                ret += tbs + "  return {";
                ret += "\n" + tbs + "    type: '"+(isConstructor?"constructor":"function")+"'" + (attributes.Count > 0 ? ", " : "");
                if (attributes.Count > 0)
                {
                    ret += "\n" + tbs + "    attributes: {";
                    int i = 0;
                    foreach (_Attribute a in attributes)
                    {
                        ret += "\n" + tbs + "      " + a.GetName() + ": " + a.Compile() + ((attributes.Count - 1) == i ? "" : ", ");
                        i++;
                    }
                    ret += "\n" + tbs + "    },";
                }
                ret += "\n" + tbs + "  };\n";
                ret += tbs + "};\n";
                
            }
            return ret;
        }
        public override int Visit()
        {
            return 0;
        }

        public override void Semantic()
        {
            if (parentNotDefined)
                Interpreter.semanticError.Add(new Error("#700 Parent for extending function by " + extendingClass + "." + name.Value + "(" + paraml.List() + ") is not found", Interpreter.ErrorType.ERROR, name));
            if(parentIsNotClassOrInterface)
                Interpreter.semanticError.Add(new Error("#701 You can extend only Class or Interface", Interpreter.ErrorType.ERROR, name));
            if (isStatic && assingBlock.Type != Block.BlockType.INTERFACE && assingBlock.Type != Block.BlockType.CLASS && !isExtending)
                Interpreter.semanticError.Add(new Error("#799 Static modifier outside class or interface is useless", Interpreter.ErrorType.WARNING, _static));
            else if(isStatic && assingBlock.Type == Block.BlockType.INTERFACE)
                Interpreter.semanticError.Add(new Error("#702 Illegal modifier for the interface static "+assingBlock.assignTo+"."+name.Value+"("+paraml.List()+")", Interpreter.ErrorType.ERROR, _static));
            //else if(!isStatic && isConstructor)
            //    Interpreter.semanticError.Add(new Error("Constructor " + name.Value + "(" + paraml.List() + ") of class " + assingBlock.assignTo + " must be static", Interpreter.ErrorType.ERROR, _constuctor));
            ParameterList.Semantic();
            if (block == null && assingBlock.Type != Block.BlockType.INTERFACE && !isExternal)
            {
                Interpreter.semanticError.Add(new Error("#703 The body of function " + (assingBlock.assignTo == "" ? "" : assingBlock.assignTo + ".") + name.Value + "(" + paraml.List() + ") must be defined", Interpreter.ErrorType.ERROR, name));
            }
            else if (!isExternal && block != null)
            {
                block.Semantic();
                block.CheckReturnType(returnt?.Value, (returnt?.type == Token.Type.VOID ? true : false));
            }
            foreach (_Attribute a in attributes)
            {
                a.Semantic();
            }
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

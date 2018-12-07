using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class Function:Types
    {
        Token name;
        public Block block;
        public ParameterList paraml;
        Token returnt;
        public bool isStatic = false;
        public Token _static;
        public bool isExternal = false;
        public Token _external;
        public bool isDynamic = false;
        public Token _dynamic;
        public bool isInline = false;
        public Token _inline;
        public bool isExtending = false;
        public string extendingClass = "";
        public bool isOperator = false;
        public bool isConstructor = false;
        public Token _constuctor;
        public bool returnAsArray = false;
        public List<_Attribute> attributes = new List<_Attribute>();
        public List<string> returnGeneric = new List<string>();
        public List<string> genericArguments = new List<string>();
        public int inlineId = 0;
        public Types assigmentInlineVariable = null;
        public static int inlineIdCounter = 1;

        private Interpreter interpret;

        readonly bool parentNotDefined = false;
        readonly bool parentIsNotClassOrInterface = false;

        private List<Token> returnTuple = null;

        /*Serialization to JSON object for export*/
        [JsonParam("Name")] public string RealName => name.Value;
        [JsonParam] public List<string> GenericArguments => genericArguments;
        [JsonParam] public ParameterList ParameterList => paraml;
        [JsonParam] public Token Returnt => returnt;
        [JsonParam] public Block Block => block;
        [JsonParam] public string ExtendingClass => extendingClass;
        [JsonParam] public bool IsConstructor => isConstructor;
        [JsonParam] public string CacheName => Name;

        private string _cacheName = null;
 
        public override void FromJson(JObject o)
        {
            name = Token.FromJson(o["Name"]);
            genericArguments = JsonParam.FromJsonArrayBase<string>((JArray)o["GenericArguments"]);
            paraml = JsonParam.FromJson<ParameterList>(o["ParameterList"]);
            returnt = Token.FromJson(o["Returnt"]);
            block = JsonParam.FromJson<Block>(o["Block"]);
            assingBlock = block;
            extendingClass = o["ExtendingClass"].ToString();
            isConstructor = (bool) o["IsConstructor"];
            _cacheName = o["CacheName"].ToString();
        }
        public Function() { }

        public Function(Token name, Block _block, ParameterList paraml, List<Token> returnTuple, Interpreter interpret, Block parent_block = null):this(name, _block,paraml,(Token)null,interpret,parent_block)
        {
            this.returnTuple = returnTuple;
        }
        public Function(Token name, Block _block, ParameterList paraml, Token returnt, Interpreter interpret, Block parent_block = null)
        {
            this.interpret = interpret;
            this.name = name;
            
            if(_block != null)
                _block.parent = this;

            if (name.Value.Contains("."))
            {
                string[] _name = name.Value.Split('.');
                this.name = new Token(Token.Type.ID, _name.Last<string>(), name.Pos, name.File);
                string _className = string.Join(".", _name.Take(_name.Length - 1));
                extendingClass = _className;
                if(_block == null)
                {
                    _block = new Block(interpret);
                    _block.BlockParent = parent_block;
                }
                Types t = _block.SymbolTable.Get(_className);
                if (!(t is Error))
                {
                    isExtending = true;
                    
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
            if (this.block != null)
            {
                this.block.assingToType = this;
                this.block.assignTo = name.Value;
                this.block.assingBlock = this.block;
            }               
            //this.block.blockAssignTo = name.Value;
            this.paraml = paraml;
            this.paraml.assingBlock = this.block;
            this.returnt = returnt;

            if (this.name.Value == extendingClass || (_block?.BlockParent != null && this.name.Value == _block?.BlockParent.assignTo && _block?.BlockParent.Type == Block.BlockType.CLASS))
            {
                isConstructor = true;
                if(block != null)
                    block.isInConstructor = true;
            }
        }

        public override string ToString()
        {
            return "<Function\""+Name+"\">";
        }

        public void AddGenericArg(string name)
        {
            genericArguments.Add(name);
        }
        public void SetGenericArgs(List<string> list)
        {
            genericArguments = list;
        }
        
        public override Token getToken() { return name; }        

        public string _hash = "";
        public string getHash()
        {
            if (assingBlock == null) assingBlock = block;
            if (isOperator || isConstructor || assingBlock.SymbolTable.GetAll(assingBlock.blockAssignTo+"::"+name.Value)?.Count > 1)
            {
                if (_hash == "")
                    _hash = $"{(name.Value + paraml.List() + block?.GetHashCode()).GetHashCode():X8}";
                return _hash;
            }
            return "";
        }
        public string Name {
            get
            {
                if (_cacheName != null) return _cacheName;
                string hash = getHash();
                if (isConstructor)
                    return "constructor_" + hash;
                return name.Value + (hash != "" ? "_" + hash : "");
            }
        }

        public string Return()
        {
            if (returnt == null)
                return "auto";
            return returnt?.Value + (returnGeneric.Count > 0 ? "<" + string.Join(", ", returnGeneric) + ">" : "") + (returnAsArray ? "[]" : "");
        }

        public string functionOpname = "";
        public override string Compile(int tabs = 0)
        {
            functionOpname = "";
            var ret = new StringBuilder();

            string addCode = "";
            if (attributes?.Where(x => x.GetName(true) == "Debug").Count() > 0)
            {
                if(Interpreter._DEBUG)
                    Debugger.Break();
                addCode = "debugger;";
            }

            if (!isExternal)
            {
                string python_fun = "";
                Class c = null;
                Interface i_ = null;
                string fromClass = "";
                string tbs = DoTabs(tabs);
                int tmpc = 0;

                if(Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                {
                    tmpc = block.Interpret.tmpcount++;
                    //if(tabs == -1)
                    //    tbs = DoTabs(tabs+2);
                }

                if (isExtending)
                {
                    var ex = assingBlock.SymbolTable.Get(extendingClass, false, true);
                    if (ex is Import import)
                    {
                        extendingClass = import.GetName() + "." + extendingClass;
                    }
                    fromClass = extendingClass;
                    if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                    {
                        if (isStatic || isConstructor)
                        {
                            ret.Append(tbs + extendingClass + "." + Name + " = function(" + paraml.Compile(0) + "){" + (block != null ? "\n" : ""));
                            functionOpname = extendingClass + "." + Name;
                        }
                        else
                        {
                            ret.Append(tbs + extendingClass + ".prototype." + Name + " = function(" + paraml.Compile(0) + "){" + (block != null ? "\n" : ""));
                            functionOpname = extendingClass + ".prototype." + Name;
                        }
                    }
                    else if(Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                    {                        
                        python_fun = extendingClass + "." + Name + " = extending_function__"+tmpc+";\n";     
                        ret.Append(tbs + "def extending_function__"+tmpc+"(" + paraml.Compile(0) + "):" + (block != null ? "\n" : ""));
                        functionOpname = extendingClass + "." + Name;
                    }
                }
                else if (assignTo == "")
                {
                    if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                        ret.Append(tbs + "def " + Name + "(" + paraml.Compile(0));
                    else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                        ret.Append(tbs + "function " + Name + "(" + paraml.Compile(0));

                    if(genericArguments.Count > 0)
                    {
                        int q = 0;
                        foreach (string generic in genericArguments)
                        {
                            if (q != 0) ret.Append(", ");
                            else if (paraml.Parameters.Count > 0) { ret.Append(", "); }
                            if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                                ret.Append("generic__" + generic);
                            else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                                ret.Append("generic$" + generic);
                            q++;
                        }
                    }

                    if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                        ret.Append("):" + (block != null ? "\n" : ""));
                    else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                        ret.Append("){" + (block != null ? "\n" : ""));                    
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

                            if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                            {
                                python_fun = hash_name + "." + Name + " = extending_function__" + tmpc + ";\n";
                                ret.Append(tbs + "def extending_function__" + tmpc + "(" + paraml.Compile(0));
                            }
                            else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                                ret.Append(tbs + hash_name + "." + Name + " = function(" + paraml.Compile(0));
                            
                            functionOpname = hash_name + "." + Name;
                            bool f = !(paraml.Parameters.Count > 0);
                            foreach (string generic in c.GenericArguments)
                            {
                                if (!f) ret.Append(", ");
                                f = false;
                                if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                                    ret.Append("generic__" + generic);
                                else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                                    ret.Append("generic$" + generic);
                            }
                            if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                                ret.Append("):" + (block != null ? "\n" : ""));
                            else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                                ret.Append("){" + (block != null ? "\n" : ""));
                        }
                        else
                        {
                            if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                            {
                                python_fun = hash_name + "." + Name + " = extending_function__" + tmpc + ";\n";
                                ret.Append(tbs + "def extending_function__" + tmpc + "(" + paraml.Compile(0));
                            }
                            else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                                ret.Append(tbs + hash_name + "." + Name + " = function(" + paraml.Compile(0));
                            
                            if(genericArguments.Count > 0)
                            {
                                int q = 0;
                                foreach (string generic in genericArguments)
                                {
                                    if (q != 0) ret.Append(", ");
                                    else if (paraml.Parameters.Count > 0) { ret.Append(", "); }
                                    if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                                        ret.Append("generic__" + generic);
                                    else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                                        ret.Append("generic$" + generic);
                                    q++;
                                }
                            }
                            if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                                ret.Append("):" + (block != null ? "\n" : ""));
                            else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                                ret.Append("){" + (block != null ? "\n" : ""));

                            functionOpname = hash_name + "." + Name;
                        }
                    }
                    else
                    {
                        if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                        {
                            python_fun = hash_name + "." + Name + " = extending_function__" + tmpc + ";\n";
                            ret.Append(tbs + "def extending_function__" + tmpc + "(" + paraml.Compile(0));
                        }
                        else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                            ret.Append(tbs + hash_name + ".prototype." + Name + " = function(" + paraml.Compile(0));

                        if(genericArguments.Count > 0)
                        {
                            int q = 0;
                            foreach (string generic in genericArguments)
                            {
                                if (q != 0) ret.Append(", ");
                                else if (paraml.Parameters.Count > 0) { ret.Append(", "); }
                                if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                                    ret.Append("generic__" + generic);
                                else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                                    ret.Append("generic$" + generic);
                                q++;
                            }
                        }
                        if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                            ret.Append("):" + (block != null ? "\n" : ""));
                        else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                            ret.Append("){" + (block != null ? "\n" : ""));
                        functionOpname = hash_name + ".prototype." + Name;
                    }
                }

                if(addCode != "")
                    ret.Append(tbs + "  " + addCode + "\n");

                //if (genericArguments.Count != 0) ret += "\n";
                foreach (var generic in genericArguments)
                {
                    block?.SymbolTable.Add(generic, new Generic(this, block, generic) { assingBlock = block }, parent: assingBlock);
                }

                if(isConstructor)
                {
                    if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                    {
                        if (block == null) ret.Append("\n");
                        if (c != null)
                        {
                            foreach (string generic in c.GenericArguments)
                            {
                                ret.Append(tbs + "  self.generic__" + generic + " = generic__" + generic + ";\n");
                            }
                        }
                    }
                    else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                    {
                        if (block == null) ret.Append("\n");
                        ret.Append(tbs + "  var $this = Object.create(" + fromClass + ".prototype);\n");
                        ret.Append(tbs + "  " + fromClass + ".call($this);\n");
                        if (c != null)
                        {
                            foreach (string generic in c.GenericArguments)
                            {
                                ret.Append(tbs + "  $this.generic$" + generic + " = generic$" + generic + ";\n");
                            }
                        }
                    }
                }

                if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                {
                    foreach (Types t in paraml.Parameters)
                    {
                        if (t is Assign a)
                        {
                            ret.Append(tbs + "  if(" + a.Left.Compile() + " == void 0) " + a.Left.Compile() + " = " + a.Right.Compile() + ";\n");
                        }
                    }
                }

                
                ret.Append(block?.Compile(tabs + 1, componentSetFirst: true));
                
                if (isConstructor && Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                {
                    ret.Append(tbs + "  return $this;\n");
                }                

                if(Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                {
                    ret.Append("\n" + python_fun);
                }
                else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                {
                    ret.Append(tbs + "}\n");

                    if (Interpreter._DEBUG)
                    {
                        if (functionOpname.Split('.').Count() > 1)
                            ret.Append(tbs + functionOpname + "$META = function(){\n");
                        else
                            ret.Append(tbs + "var " + functionOpname + "$META = function(){\n");
                        ret.Append(tbs + "  return {");
                        ret.Append("\n" + tbs + "    type: '" + (isConstructor ? "constructor" : "function") + "'" + (attributes.Count > 0 ? ", " : ""));
                        if (attributes.Count > 0)
                        {
                            ret.Append("\n" + tbs + "    attributes: {");
                            int i = 0;
                            foreach (_Attribute a in attributes)
                            {
                                ret.Append("\n" + tbs + "      " + a.GetName() + ": " + a.Compile() + ((attributes.Count - 1) == i ? "" : ", "));
                                i++;
                            }

                            ret.Append("\n" + tbs + "    },");
                        }

                        ret.Append("\n" + tbs + "  };\n");
                        ret.Append(tbs + "};\n");
                    }
                }

            }
            else
            {
                var useBlock = block ?? assingBlock;
                foreach (var generic in genericArguments)
                {
                    useBlock.SymbolTable.Add(generic, new Generic(this, useBlock, generic) { assingBlock = useBlock });
                }
            }
            return ret.ToString();
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

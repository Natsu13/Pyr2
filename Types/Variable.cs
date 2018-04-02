using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Variable : Types
    {
        Token token;
        string value;
        Token dateType;
        Block block;
        public TypeObject   _class;
        public Class        class_;
        public Interface    inter_;
        public Generic      genei_;
        Types key;
        bool isKey = false;
        public List<string> genericArgs = new List<string>();
        public Token asDateType = null;
        public bool isis = true;
        public bool isArray = false;
        public int arraySize = 0;
        public bool isDateType = false;
        bool getFoundButBadArgs = false;
        List<string> generic = new List<string>();

        public Variable(Token token, Types block, Token dateType = null, List<string> generic = null)
        {
            this.block = (Block)block;
            this.token = token;
            this.value = token.Value;
            if (dateType == null)
                dateType = new Token(Token.Type.AUTO, "auto");
            this.dateType = dateType;
            if(generic != null)
                this.generic = generic;
            
            if (this.dateType.Value != "auto" && this.block != null)
            {
                if (this.block.SymbolTable.FindInternal(dateType.Value))
                {
                    Type type = this.block.SymbolTable.GetType(dateType.Value);
                    _class = (TypeObject)Activator.CreateInstance(type);
                }
                else if (this.block.SymbolTable.Find(dateType.Value))
                {
                    Types fic = this.block.SymbolTable.Get(dateType.Value);
                    if (fic is Class)
                        class_ = (Class)fic;
                    else if (fic is Interface)
                        inter_ = (Interface)fic;
                }
            }
            if(_class == null && class_ == null && inter_ == null)
            {
                _class = new TypeObject();
            }
        }        
        public void   setType(Token _type) {
            this.dateType = _type;
            if (this.dateType.Value != "auto")
            {                
                if (this.block.SymbolTable.FindInternal(dateType.Value))
                {
                    Type type = this.block.SymbolTable.GetType(dateType.Value);
                    _class = (TypeObject)Activator.CreateInstance(type);
                }
                else if (this.block.SymbolTable.Find(dateType.Value))
                {
                    _class = null;
                    Types fic = this.block.SymbolTable.Get(dateType.Value);
                    if (fic is Class)
                        class_ = (Class)fic;
                    else if (fic is Interface)
                        inter_ = (Interface)fic;
                }
            }
        }
        public Token  getType() { return dateType; }
        public string Value { get { return value; } }
        public string Type { get { return dateType.Value; } }
        public Block  Block { get { return block; } }
        public override Token getToken() { return token; }
        public Token  getDateType() { return dateType; }
        public bool   IsKey { get { return isKey; } }
        public Types  Key { get { return key; } }
        public void MadeArray(bool isArray) { isArray = true; }
        public List<string> GenericList { get { return generic; } }

        public Token AsDateType
        {
            set
            {                
                Token dateType = value;
                if (this.block.SymbolTable.Find(dateType.Value))
                {
                    _class = null;
                    Types fic = this.block.SymbolTable.Get(dateType.Value);
                    if (fic is Class)
                    {
                        class_ = (Class)fic;
                        dateType = class_.Name;
                    }
                    else if (fic is Interface)
                    {
                        inter_ = (Interface)fic;
                        dateType = inter_.Name;
                    }
                }
                asDateType = value;
            }
            get { return asDateType; }
        }

        public void setKey(Types key)
        {
            this.key = key;
            isKey = true;
        }

        public void Check()
        {
            string newname = this.value;
            if (newname.Split('.')[0] == "this" && this.value != "this")
            {                
                newname = block.getClass() + "." + string.Join(".", newname.Split('.').Skip(1));
            }
            if(Value == "this")
            {
                Types s;
                if(this.assingBlock?.Parent != null)
                    s = this.assingBlock.SymbolTable.Get(this.assingBlock.Parent.assignTo.Split('.')[0]);
                else
                    s = this.assingBlock?.SymbolTable.Get(this.assingBlock.assignTo.Split('.')[0]);

                if(s is Class)
                    this.dateType = ((Class)s).Name;
                else if (s is Interface si)
                    this.dateType = si.Name;
                else if (s is Function sf && sf.Returnt != null)
                    this.dateType = ((Function) s).Returnt;
            }
            if (this.dateType.Value == "auto")
            {
                Types fvar = this.block.FindVariable(newname);
                if (this.block.SymbolTable.Find(newname))
                {
                    Types t = this.block.SymbolTable.Get(newname);
                    if(t is Generic)
                    {
                        this.dateType = new Token(Token.Type.CLASS, "object");
                    }
                    else if (t is Properties prop)
                    {
                        this.dateType = prop.variable.TryVariable().dateType;
                    }
                    else if (t is Function tf)
                    {
                        this.dateType = tf.Returnt;
                    }
                    else if (t is Assign && ((Assign)t).Right is UnaryOp)
                    {
                        if (((UnaryOp)((Assign)t).Right).Op == "call")
                        {
                            Token fname = ((UnaryOp)((Assign)t).Right).Name;
                            Types sd = this.block.SymbolTable.Get(fname.Value);
                            if (!(sd is Error))
                            {
                                Function f = (Function)sd;
                                this.dateType = f.Returnt;
                            }
                        }else
                            this.dateType = ((Variable)(((Assign)t).Left)).dateType;
                    }
                    else if (t is Variable)
                    {
                        this.dateType = ((Variable) t).dateType;
                    }
                    else if (t is Class)
                        this.dateType = ((Class) t).Name;
                    else
                        this.dateType = ((Variable)(((Assign)t).Left)).dateType;
                }
                else if (fvar != null)
                {
                    this.dateType = ((Variable)((Assign)fvar).Left).dateType;
                }
                else if (this.token.type == Token.Type.TRUE || this.token.type == Token.Type.FALSE)
                {
                    this.dateType = new Token(Token.Type.BOOL, "bool");
                }
                else
                {
                    if (this.block.SymbolTable.Find(this.block.assignTo))
                    {
                        Function asfunc;
                        Types qq = this.block.SymbolTable.Get(this.block.assignTo);
                        if (!(qq is Function))
                            asfunc = (Function)this.block.SymbolTable.Get("constructor " + this.block.assignTo);
                        else
                            asfunc = (Function)qq;

                        Variable var = asfunc.ParameterList.Find(this.value);
                        if (var != null)
                        {
                            this.dateType = var.dateType;
                        }
                    }                    
                }
            }
            if (this.dateType == null)
                this.dateType = new Token(Token.Type.AUTO, "auto");
            if (this.dateType.Value != "auto")
            {
                if (this.block.SymbolTable.FindInternal(dateType.Value))
                {
                    Type type = this.block.SymbolTable.GetType(dateType.Value);
                    _class = (TypeObject)Activator.CreateInstance(type);
                }
                else if (this.block.SymbolTable.Find(dateType.Value) && class_ == null)
                {
                    Types fic = this.block.SymbolTable.Get(dateType.Value);
                    if (fic is Class)
                        class_ = (Class)fic;
                    else if (fic is Interface)
                        inter_ = (Interface)fic;
                    else if (fic is Generic)
                        genei_ = (Generic)fic;
                }
            }
        }

        public override string Compile(int tabs = 0)
        {
            return CompileHard(tabs, null);
        }

        public string CompileHard(int tabs = 0, Types ti = null)
        {
            if (key != null)
                key.endit = false;

            string vname = "";

            string nameclass = "";
            if (class_ != null)
                nameclass = (class_.JSName != "" ? class_.JSName : class_.Name.Value);
            else if (inter_ != null)
                nameclass = inter_.Name.Value;
            else if (_class != null)
                nameclass = _class.Name;

            if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
            {
                var t__ = "this" + (Value == "this" ? "" : ".");
                if (assingBlock != null && assingBlock.isType(Block.BlockType.PROPERTIES))
                    t__ = "this.$self" + (Value == "this" ? "" : ".");
                if (value.Split('.')[0] != "this")
                    t__ = "";
                if (value.Contains("."))
                    t__ = value.Split('.')[0] + ".";

                var not = Value;
                var withouthis = Value;
                if (value.Contains("."))
                    withouthis = string.Join(".", value.Split('.').Skip(1));

                if (Value.Split('.')[0] == "this")
                {
                    not = t__ + string.Join(".", value.Split('.').Skip(1));
                    vname = string.Join(".", value.Split('.').Skip(1)) + (isKey ? "[" + key.Compile() + "]" : "");

                    if (block.isInConstructor)
                    {
                        if (block.SymbolTable.Get(this.value) is Properties)
                        {
                            if (asDateType != null)
                                return DoTabs(tabs) + (inParen ? "(" : "") + "($this.Property$" + vname + ".get().constructor.name == '" + nameclass + "' ? $this.Property$" + vname + ".get() : alert('Variable " + vname + " is not type " + asDateType.Value + "'))" + (inParen ? ")" : "");
                            return DoTabs(tabs) + (inParen ? "(" : "") + "$this.Property$" + string.Join(".", value.Split('.').Skip(1)) + ".get()" + (isKey ? "[" + key.Compile() + "]" : "") + (inParen ? ")" : "");
                        }
                        if (asDateType != null)
                            return DoTabs(tabs) + (inParen ? "(" : "") + "($this." + vname + ".constructor.name == '" + nameclass + "' ? $this." + vname + " : alert('Variable " + vname + " is not type " + asDateType.Value + "'))" + (inParen ? ")" : "");
                        return DoTabs(tabs) + (inParen ? "(" : "") + "$this." + string.Join(".", value.Split('.').Skip(1)) + (isKey ? "[" + key.Compile() + "]" : "") + (inParen ? ")" : "");
                    }
                }

                Types t = ti ?? block.SymbolTable.Get(value, Type);
                if (t is Generic)
                    vname = t__ + "generic$" + withouthis + (isKey ? "[" + key.Compile() + "]" : "");
                else if (t is Assign ta && ta.Left is Variable tav && tav.Type != "object")
                {
                    if(tav.Type == "auto")
                        tav.Check();
                    Types tavt = block.SymbolTable.Get(tav.Type, genericArgs: tav.genericArgs.Count);
                    if (tavt is Delegate)
                    {
                        vname = t__ + "delegate$" + withouthis + (isKey ? "[" + key.Compile() + "]" : "");
                    }
                    else
                    {
                        vname = not + (isKey ? "[" + key.Compile() + "]" : "");
                    }
                }
                else if (t is Properties)
                {
                    if (value.Split('.')[0] == "this")
                    {
                        vname = t__ + ".Property$" + string.Join(".", value.Split('.').Skip(1)) + ".get()" + (isKey ? "[" + key.Compile() + "]" : "");
                    }
                    else
                        vname = t__ + ".Property$" + Value + ".get()" + (isKey ? "[" + key.Compile() + "]" : "");
                }
                else
                {
                    vname = not + (isKey ? "[" + key.Compile() + "]" : "");
                }
                if (dateType.Value == "auto")
                    Check();
                if (class_ != null && isKey)
                {
                    ParameterList plist = new ParameterList(false);
                    plist.Parameters.Add(key);
                    Types oppq = class_.block.SymbolTable.Get("operator " + Variable.GetOperatorNameStatic(Token.Type.GET), plist);
                    if (!(oppq is Error))
                    {
                        Function opp = (Function)oppq;
                        if (!opp.isExternal)
                            vname = not + "." + opp.Name + "(" + key.Compile() + ")";
                    }
                    else if (oppq is Error && ((Error)oppq).Message == "Found but arguments are bad!") getFoundButBadArgs = true;
                }
                if (asDateType != null)
                    return DoTabs(tabs) + (inParen ? "(" : "") + "(" + vname + ".constructor.name == '" + nameclass + "' ? " + vname + " : alert('Variable " + vname + " is not type " + asDateType.Value + "'))" + (inParen ? ")" : "");
                return DoTabs(tabs) + (inParen ? "(" : "") + vname + (inParen ? ")" : "");
            }
            else if(Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
            {
                var t__ = "self" + (Value == "this" ? "" : ".");
                if (assingBlock != null && assingBlock.isType(Block.BlockType.PROPERTIES))
                    t__ = "self.__self" + (Value == "this" ? "" : ".");
                if (value.Split('.')[0] != "this")
                    t__ = "";
                if (value.Contains("."))
                    t__ = value.Split('.')[0] + ".";

                var not = Value;
                var withouthis = Value;
                if (value.Contains("."))
                    withouthis = string.Join(".", value.Split('.').Skip(1));

                if (Value.Split('.')[0] == "this")
                {
                    not = t__ + string.Join(".", value.Split('.').Skip(1));
                    vname = string.Join(".", value.Split('.').Skip(1)) + (isKey ? "[" + key.Compile() + "]" : "");

                    if (block.isInConstructor)
                    {
                        if (block.SymbolTable.Get(this.value) is Properties)
                        {
                            if (asDateType != null)
                                return DoTabs(tabs) + (inParen ? "(" : "") + "(self.Property__" + vname + ".get().constructor.name == '" + nameclass + "' ? self.Property__" + vname + ".get() : alert('Variable " + vname + " is not type " + asDateType.Value + "'))" + (inParen ? ")" : "");
                            return DoTabs(tabs) + (inParen ? "(" : "") + "self.Property__" + string.Join(".", value.Split('.').Skip(1)) + ".get()" + (isKey ? "[" + key.Compile() + "]" : "") + (inParen ? ")" : "");
                        }
                        if (asDateType != null)
                            return DoTabs(tabs) + (inParen ? "(" : "") + "(self." + vname + ".constructor.name == '" + nameclass + "' ? self." + vname + " : alert('Variable " + vname + " is not type " + asDateType.Value + "'))" + (inParen ? ")" : "");
                        return DoTabs(tabs) + (inParen ? "(" : "") + "self." + string.Join(".", value.Split('.').Skip(1)) + (isKey ? "[" + key.Compile() + "]" : "") + (inParen ? ")" : "");
                    }
                }

                Types t = block.SymbolTable.Get(this.value);
                if (t is Generic)
                    vname = t__ + "generic__" + withouthis + (isKey ? "[" + key.Compile() + "]" : "");
                else if (t is Assign ta && ta.Left is Variable tav && tav.Type != "object")
                {
                    Types tavt = block.SymbolTable.Get(tav.Type, genericArgs: tav.genericArgs.Count);
                    if (tavt is Delegate)
                    {
                        vname = t__ + "delegate__" + withouthis + (isKey ? "[" + key.Compile() + "]" : "");
                    }
                    else
                    {
                        vname = not + (isKey ? "[" + key.Compile() + "]" : "");
                    }
                }
                else if (t is Properties)
                {
                    if (value.Split('.')[0] == "this")
                    {
                        vname = t__ + ".Property__" + string.Join(".", value.Split('.').Skip(1)) + ".get()" + (isKey ? "[" + key.Compile() + "]" : "");
                    }
                    else
                        vname = t__ + ".Property__" + Value + ".get()" + (isKey ? "[" + key.Compile() + "]" : "");
                }
                else
                {
                    vname = not + (isKey ? "[" + key.Compile() + "]" : "");
                }
                if (dateType.Value == "auto")
                    Check();
                if (class_ != null && isKey)
                {
                    ParameterList plist = new ParameterList(false);
                    plist.Parameters.Add(key);
                    Types oppq = class_.block.SymbolTable.Get("operator " + Variable.GetOperatorNameStatic(Token.Type.GET), plist);
                    if (!(oppq is Error))
                    {
                        Function opp = (Function)oppq;
                        if (!opp.isExternal)
                            vname = not + "." + opp.Name + "(" + key.Compile() + ")";
                    }
                    else if (oppq is Error && ((Error)oppq).Message == "Found but arguments are bad!") getFoundButBadArgs = true;
                }
                if (asDateType != null)
                    return DoTabs(tabs) + (inParen ? "(" : "") + "(" + vname + ".constructor.name == '" + nameclass + "' ? " + vname + " : alert('Variable " + vname + " is not type " + asDateType.Value + "'))" + (inParen ? ")" : "");
                return DoTabs(tabs) + (inParen ? "(" : "") + vname + (inParen ? ")" : "");
            }

            return "";
        }

        public override string ToString()
        {
            return "Variable("+Type+", Value: "+Value+")";
        }

        public override int Visit()
        {
            if (block.variables.ContainsKey(value))
            {
                int val = Int32.Parse(block.variables[value].GetVal());
                return val;
            }
            return 0;
        }
        public static string GetOperatorStatic(Token.Type op)
        {
            string o = "";
            switch (op)
            {
                case Token.Type.EQUAL:
                    o = "==";
                    break;
                case Token.Type.NOTEQUAL:
                    o = "!=";
                    break;
                case Token.Type.AND:
                    o = "&&";
                    break;
                case Token.Type.OR:
                    o = "||";
                    break;
                case Token.Type.MORE:
                    o = ">";
                    break;
                case Token.Type.LESS:
                    o = "<";
                    break;
                case Token.Type.PLUS:
                    o = "+";
                    break;
                case Token.Type.INC:
                    o = "++";
                    break;
                case Token.Type.MINUS:
                    o = "-";
                    break;
                case Token.Type.DEC:
                    o = "--";
                    break;
                case Token.Type.DIV:
                    o = "/";
                    break;
                case Token.Type.MUL:
                    o = "*";
                    break;
                case Token.Type.DOT:
                    o = ".";
                    break;
                case Token.Type.NEW:
                    o = "new";
                    break;
                case Token.Type.RETURN:
                    o = "return";
                    break;
                case Token.Type.CALL:
                    o = "call";
                    break;
                case Token.Type.GET:
                    o = "get";
                    break;
                case Token.Type.IS:
                    o = "is";
                    break;
                case Token.Type.RANGE:
                    o = "..";
                    break;
            }
            return o;
        }
        public string GetOperator(Token.Type op)
        {
            return GetOperatorStatic(op);
        }
        public string GetOperatorName(Token.Type op)
        {
            return GetOperatorNameStatic(op);
        }
        public static string GetOperatorNameStatic(Token.Type op)
        {
            string o = "";
            if (op == Token.Type.EQUAL)     o = "equal";
            if (op == Token.Type.NOTEQUAL)  o = "equal";
            if (op == Token.Type.AND)       o = "and";
            if (op == Token.Type.OR)        o = "or";
            if (op == Token.Type.MORE)      o = "compareTo";
            if (op == Token.Type.LESS)      o = "compareTo";

            if (op == Token.Type.DOT)       o = "dot";
            if (op == Token.Type.PLUS)      o = "plus";
            if (op == Token.Type.INC)       o = "inc";            
            if (op == Token.Type.MINUS)     o = "minus";
            if (op == Token.Type.DEC)       o = "dec";
            if (op == Token.Type.DIV)       o = "divide";
            if (op == Token.Type.MUL)       o = "multiple";
            if (op == Token.Type.NEW)       o = "new";
            if (op == Token.Type.RETURN)    o = "return";
            if (op == Token.Type.CALL)      o = "invoke";
            if (op == Token.Type.GET)       o = "get";
            if (op == Token.Type.IS)        o = "is";
            if (op == Token.Type.RANGE)     o = "range";
            return o;
        }        
        public Token OutputType(Token.Type op, object first, object second)
        {
            if (_class != null && (class_ == null && inter_ == null && genei_ == null))
                return _class.OutputType(GetOperator(op), first, second);
            else if (class_ != null)
                return class_.OutputType(GetOperatorName(op), first, second);
            else if (inter_ != null)
                return inter_.OutputType(GetOperatorName(op), first, second);
            else if (genei_ != null)
                return genei_.OutputType(GetOperatorName(op), first, second);
            return null;
        }
        public bool SupportOp(Token.Type op)
        {
            object o = null;
            if (_class != null && (class_ == null && inter_ == null && genei_ == null))
                o = _class.SupportOp(GetOperator(op));
            else if (class_ != null)
                o = class_.SupportOp(GetOperatorName(op));
            else if (inter_ != null)
                o = inter_.SupportOp(GetOperatorName(op));
            else if (genei_ != null)
                o = genei_.SupportOp(GetOperatorName(op));
            return (o is null ? false : (bool)o);
        }
        public bool SupportSecond(Token.Type op, object second, object secondAsVariable)
        {
            object o = null;
            if (_class != null && (class_ == null && inter_ == null && genei_ == null))
                o = _class.SupportSecond(second, secondAsVariable);
            else if (class_ != null)
                o = class_.SupportSecond(GetOperatorName(op), second, secondAsVariable);
            else if (inter_ != null)
                o = inter_.SupportSecond(GetOperatorName(op), second, secondAsVariable);
            else if (genei_ != null)
                o = genei_.SupportSecond(GetOperatorName(op), second, secondAsVariable);
            return (o is null ? false : (bool)o);
        }          
        public object Operator(Token.Type op, object first, object second)
        {
            if(_class != null)
                return _class?.Operator(GetOperator(op), first, second);
            return "";
        }
        public object GetClass() { return _class; }

        public override void Semantic()
        {
            if (this.dateType.Value == "auto")
                Check();
            if (isKey && !SupportOp(Token.Type.GET))
                Interpreter.semanticError.Add(new Error("#302 Date type "+dateType.Value+" not support 'get' operator", Interpreter.ErrorType.ERROR, token));
            if(isKey && getFoundButBadArgs)
                Interpreter.semanticError.Add(new Error("#303 Date type " + dateType.Value + " support 'get' operator, but arguments are wrong", Interpreter.ErrorType.ERROR, token));
            if(block.SymbolTable.Get(this.value) is Properties p)
            {
                if(p.Getter == null)
                    Interpreter.semanticError.Add(new Error("#802 Properties "+this.value+" don't define getter!", Interpreter.ErrorType.ERROR, token));
            }
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

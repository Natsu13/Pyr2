using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class Variable : Types
    {
        Token token;
        public string value;
        public Token dateType;
        public Block block;
        public TypeObject   _class;
        public Class        class_;
        public Interface    inter_;
        public Generic      genei_;
        public Types key;
        public bool isKey = false;
        public List<string> genericArgs = new List<string>();
        public Token asDateType = null;
        public bool isis = true;
        public bool isArray = false;
        public int  arraySize = 0;
        public bool isDateType = false;
        private bool isVal;
        public bool asvar = false;
        public bool getFoundButBadArgs = false;
        List<string> generic = new List<string>();

        /*Serialization to JSON object for export*/
        [JsonParam] public Token Token => token;        
        [JsonParam] public Token DateType => dateType;
        [JsonParam] public bool IsVal { get => isVal; set => isVal = value; }
        [JsonParam] public bool IsKey => isKey;
        [JsonParam] public Types Key => key;        
        [JsonParam] public List<string> GenericList => generic;

        public Block Block => block;

        public override void FromJson(JObject o)
        {
            token = Token.FromJson(o["Token"]);
            dateType = Token.FromJson(o["dateType"]);
            isVal = (bool) o["IsVal"];
            isKey = (bool) o["IsKey"];
            key = JsonParam.FromJson(o["Key"]);
            generic = JsonParam.FromJsonArrayBase<string>((JArray) o["GenericList"]);
        }
        public Variable() { }

        public Variable(Token token, Types block, Token dateType = null, List<string> generic = null, bool isVal = false)
        {
            this.isVal = isVal;
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
                else 
                {
                    var fic = this.block.SymbolTable.Get(dateType.Value);
                    if (!(fic is Error))
                    {                        
                        if (fic is Class)
                            class_ = (Class) fic;
                        else if (fic is Interface)
                            inter_ = (Interface) fic;
                    }
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
                else
                {
                    var fic = block.SymbolTable.Get(dateType.Value);
                    if (!(fic is Error))
                    {
                        _class = null;                        
                        if (fic is Class)
                            class_ = (Class) fic;
                        else if (fic is Interface)
                            inter_ = (Interface) fic;
                    }
                }
            }
        }
        public void MadeArray(bool isArray) { this.isArray = isArray; }
        public Token  getType() => dateType;
        public string Value => value;
        public string Type => dateType.Value;
        public override Token getToken() => token;
        public Token  GetDateType() => dateType;        

        public Token AsDateType
        {
            set
            {                
                Token dateType = value;
                var fic = this.block.SymbolTable.Get(dateType.Value);
                if (!(fic is Error))
                {
                    _class = null;                    
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

        public static bool IsItPrimitive(string name)
        {
            return new[] {"int", "string", "bool", "float"}.Contains(name);
        }

        public bool IsPrimitive
        {
            get
            {
                if(dateType.Value == "auto")
                    Check();

                return new[] {"int", "string", "bool", "float"}.Contains(dateType.Value);
            }
        }

        private int hash = 0;
        public void Check()
        {
            if (hash == GetHashCode())
                return;
            hash = GetHashCode();

            string newname = this.value;
            if (newname.Split('.')[0] == "this" && this.value != "this")
            {                
                newname = block.getClass() + "." + string.Join(".", newname.Split('.').Skip(1));
            }
            if(Value == "this")
            {
                Types s;
                if(this.assingBlock?.BlockParent != null)
                    s = this.assingBlock.SymbolTable.Get(this.assingBlock.BlockParent.assignTo.Split('.')[0]);
                else
                    s = this.assingBlock?.SymbolTable.Get(this.assingBlock.assignTo.Split('.')[0]);

                if(s is Class)
                    this.dateType = ((Class)s).Name;
                else if (s is Interface si)
                    this.dateType = si.Name;
                else if (s is Function sf && sf.Returnt != null)
                    this.dateType = ((Function) s).Returnt;
            }
            if (newname.Contains("."))
            {
                var prep = newname.Split('.');
                for (var i = 0; i < prep.Length - 1; i++)
                {
                    var get = this.block.SymbolTable.Get(prep[i]);
                    if(get != null)
                    {
                        newname = string.Join(".", prep.Skip(1));
                        this.block = get.assingBlock;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (this.dateType.Value == "auto")
            {
                if (this.block == null)
                    this.block = assingBlock;     
                
                if (this.token.type == Token.Type.TRUE || this.token.type == Token.Type.FALSE)
                {
                    dateType = new Token(Token.Type.BOOL, "bool");
                }                                            
                else
                {
                    var fnd = block?.SymbolTable.Get(newname, assingBlock);
                    if (!(fnd is Error))
                    {
                        var t = fnd;
                        if (t is Generic)
                        {
                            dateType = new Token(Token.Type.CLASS, "object");
                        }
                        else if (t is Properties prop)
                        {
                            dateType = prop.variable.TryVariable().dateType;
                        }
                        else if (t is Function tf)
                        {
                            dateType = tf.Returnt;
                        }
                        else if (t is Assign && ((Assign) t).Right is UnaryOp)
                        {
                            if (((UnaryOp) ((Assign) t).Right).Op == "call")
                            {
                                Token fname = ((UnaryOp) ((Assign) t).Right).Name;
                                Types sd = this.block.SymbolTable.Get(fname.Value, assingBlock);
                                if (!(sd is Error))
                                {
                                    Function f = (Function) sd;
                                    dateType = f.Returnt;
                                }
                            }
                            else
                                dateType = ((Variable) (((Assign) t).Left)).dateType;
                        }
                        else if (t is Variable)
                        {
                            dateType = ((Variable) t).dateType;
                        }
                        else if (t is Class)
                            dateType = ((Class) t).Name;
                        else if (t is Assign ta)
                        {
                            var right = ta.Right;
                            if (right is CString)
                                dateType = new Token(Token.Type.CLASS, "string");
                            else if (right is Number && ((Number) right).isReal)
                                dateType = new Token(Token.Type.CLASS, "float");
                            else if (right is Number)
                                dateType = new Token(Token.Type.CLASS, "int");
                            else if (right.getToken().Value == "true" || right.getToken().Value == "false")
                                dateType = new Token(Token.Type.CLASS, "bool");
                            else
                                dateType = ((Variable) (((Assign) t).Left)).dateType;
                        }
                        else if (t is Number tn)
                            dateType = new Token(Token.Type.CLASS, "int");
                        else if (t is _Enum te)
                            dateType = new Token(Token.Type.CLASS, "Enum");
                        else if (t != null)
                            dateType = ((Variable) ((Assign) t).Left).dateType;
                    }
                    else
                    {
                        var qq = block.SymbolTable.Get(block.assignTo, assingBlock);
                        if (!(qq is Error))
                        {
                            Function asfunc;
                            if (!(qq is Function))
                                asfunc = (Function) block.SymbolTable.Get("constructor " + this.block.assignTo, assingBlock);
                            else
                                asfunc = (Function) qq;

                            Variable var = asfunc.ParameterList.Find(this.value);
                            if (var != null)
                            {
                                dateType = var.dateType;
                            }
                        }
                    }
                }
            }
            if (dateType == null)
                dateType = new Token(Token.Type.AUTO, "auto");
            if (dateType.Value != "auto")
            {
                if (block.SymbolTable.FindInternal(dateType.Value))
                {
                    Type type = this.block.SymbolTable.GetType(dateType.Value);
                    _class = (TypeObject)Activator.CreateInstance(type);
                }
                else 
                {
                    var fic = this.block.SymbolTable.Get(dateType.Value, assingBlock);
                    if (class_ == null && !(fic is Error))
                    {                        
                        if (fic is Class @class)
                            class_ = @class;
                        else if (fic is Interface)
                            inter_ = (Interface) fic;
                        else if (fic is Generic)
                            genei_ = (Generic) fic;
                    }
                }
            }
        }

        public override string Compile(int tabs = 0)
        {
            return CompileHard(tabs, null);
        }

        public string GetAssignTo()
        {
            if (assignTo != "")
                return assignTo;
            if(assingBlock?.assignTo != "")
                return assingBlock?.assignTo;
            return assingBlock?.assingBlock?.assignTo;
        }
        
        public string CompileHard(int tabs = 0, Types ti = null)
        {
            if (key != null)
                key.endit = false;

            string vname = "";
            string overidename = "";

            string nameclass = "";
            if (class_ != null)
                nameclass = (class_.JSName != "" ? class_.JSName : class_.Name.Value);
            else if (inter_ != null)
                nameclass = inter_.Name.Value;
            else if (_class != null)
                nameclass = _class.Name;

            if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
            {
                var fasign = block.SymbolTable.Get(this.value);
                if (fasign is Assign fass)
                {
                    if (fass.Left is Variable fasv && fasv.GetHashCode() != GetHashCode() && !(fass.Right is Null))
                    {
                        if (fasv.IsVal && fasv.IsPrimitive)
                        {
                            fass.Right.endit = endit;
                            return fass.Right.Compile(tabs);
                        }
                    }
                }

                var split = Value.Split(new[] {'.'}, 1);
                var usingBlock = assingBlock ?? block;
                var _usingBlock = usingBlock?.GetBlock(Block.BlockType.FUNCTION, new List<Block.BlockType> {Block.BlockType.CLASS, Block.BlockType.INTERFACE});
                if (_usingBlock != null)
                {
                    usingBlock = _usingBlock;
                    if (usingBlock.SymbolTable.Get(usingBlock.assignTo) is Function ass && ass.isInline && ass.inlineId > 0)
                    {
                        if (split[0] == "this")
                        {
                            overidename = split[0] + "." + ass.Name + "$" + ass.inlineId + "$" + value;
                        }
                        else
                        {
                            overidename = ass.Name + "$" + ass.inlineId + "$" + value;
                        }
                    }
                }
                var t__ = (assingBlock != null && assingBlock.isInConstructor ? "$this" : "this") + (Value == "this" ? "" : ".");
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
                    var ths = Value == "this" ? "this" : "this.";
                    not = (t__ == "this." && usingBlock.isInConstructor ? "$" + ths : ths) + string.Join(".", value.Split('.').Skip(1));
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
                    //Datetype
                    //((Assign)t).Left.assingBlock.SymbolTable.Get("T1")
                    if(tav.Type == "auto")
                        tav.Check();
                    Types tavt = block.SymbolTable.Get(tav.Type, genericArgs: tav.genericArgs.Count);
                    if (tavt is Delegate)
                    {
                        if(t__ != "")
                            vname = (assingBlock != null && assingBlock.isInConstructor ? "$this" : "this") + ".delegate$" + withouthis + (isKey ? "[" + key.Compile() + "]" : "");
                        else
                            vname = "delegate$" + withouthis + (isKey ? "[" + key.Compile() + "]" : "");
                    }
                    else
                    {
                        vname = (overidename != "" ? overidename : not) + (isKey ? "[" + key.Compile() + "]" : "");
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
                if (class_ != null && isKey && !class_.isExternal)
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
                case Token.Type.NEG:
                    o = "!";
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
            if (op == Token.Type.EQUAL)          o = "equal";
            else if (op == Token.Type.NOTEQUAL)  o = "equal";
            else if (op == Token.Type.AND)       o = "and";
            else if (op == Token.Type.OR)        o = "or";
            else if (op == Token.Type.MORE)      o = "compareTo";
            else if (op == Token.Type.LESS)      o = "compareTo";
             
            else if (op == Token.Type.DOT)       o = "dot";
            else if (op == Token.Type.PLUS)      o = "plus";
            else if (op == Token.Type.INC)       o = "inc";            
            else if (op == Token.Type.MINUS)     o = "minus";
            else if (op == Token.Type.DEC)       o = "dec";
            else if (op == Token.Type.NEG)       o = "negation";
            else if (op == Token.Type.DIV)       o = "divide";
            else if (op == Token.Type.MUL)       o = "multiple";
            else if (op == Token.Type.NEW)       o = "new";
            else if (op == Token.Type.RETURN)    o = "return";
            else if (op == Token.Type.CALL)      o = "invoke";
            else if (op == Token.Type.GET)       o = "get";
            else if (op == Token.Type.IS)        o = "is";
            else if (op == Token.Type.RANGE)     o = "range";
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
            if (Type == "int" && second is int)
                return (int)first + (int)second;
            return "";
        }
        public object GetClass() { return _class; }

        public override void Semantic()
        {
            if (this.dateType.Value == "auto")
                Check();
            //if(block.SymbolTable.Get(this.value) is Assign va && va.Left is Variable vav && vav.isArray && !isKey)
                //Interpreter.semanticError.Add(new Error("#302 Date type "+dateType.Value+" is defined as Array but you dont access it throught index", Interpreter.ErrorType.ERROR, token));
            if (isKey && !SupportOp(Token.Type.GET) && !(block.SymbolTable.Get(this.value) is Assign va && va.Left is Variable vav && vav.isArray))
                Interpreter.semanticError.Add(new Error("#302 Date type "+dateType.Value+" not support 'get' operator", Interpreter.ErrorType.ERROR, token));
            if(isKey && getFoundButBadArgs)
                Interpreter.semanticError.Add(new Error("#303 Date type " + dateType.Value + " support 'get' operator, but arguments are wrong", Interpreter.ErrorType.ERROR, token));
            if(block.SymbolTable.Get(this.value) is Properties p)
            {
                if(p.Getter == null)
                    Interpreter.semanticError.Add(new Error("#802 Properties "+this.value+" don't define getter!", Interpreter.ErrorType.ERROR, token));
            }

            var t = value.Split('.');
            if (t.Length > 1)
            {
                var block = assingBlock.SymbolTable.Get(t[0]);
                if (block is Assign bloa)
                {
                    if (bloa.Right is UnaryOp bluo && bluo.Op == "call")
                    {
                        var funct = bluo.assingBlock.SymbolTable.Get(bluo.Name.Value);
                        if (funct is Function fun)
                        {
                            var ret = fun.Returnt.Value;
                            var retclass = fun.assingBlock.SymbolTable.Get(ret, genericArgs: fun.returnGeneric.Count);
                            if (!(retclass is Error))
                            {                                
                                var myvar = retclass.assingBlock.SymbolTable.Get(t[1]);
                                if (myvar is Error)
                                {
                                    if (retclass is Class rtc)
                                        Interpreter.semanticError.Add(new Error("#3x0 Variable " + value + " in class " + rtc.Name.Value + " not exists!", Interpreter.ErrorType.ERROR, token));
                                    else if (retclass is Interface rti)
                                        Interpreter.semanticError.Add(new Error("#3x1 Variable " + value + " in interface " + rti.Name.Value + " not exists!", Interpreter.ErrorType.ERROR, token));
                                }
                            }                            
                        }
                    }
                }
            }

            if (!((parent is ParameterList pl) && pl.declare))
            {
                if (block.SymbolTable.Get(value, assingBlock) is Error)
                {
                    Interpreter.semanticError.Add(new Error("#3x2 Variable " + token.Value + " not exists", Interpreter.ErrorType.ERROR, getToken()));
                }
            }
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

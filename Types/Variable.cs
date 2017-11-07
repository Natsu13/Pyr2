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

        public Variable(Token token, Types block, Token dateType = null)
        {
            this.block = (Block)block;
            this.token = token;
            this.value = token.Value;
            if (dateType == null)
                dateType = new Token(Token.Type.AUTO, "auto");
            this.dateType = dateType;
            
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
                        class_ = (Class)fic;
                    else if (fic is Interface)
                        inter_ = (Interface)fic;
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
            /*
            if(Value.Split('.')[0] != "this")
            {
                if (this.block.SymbolTable.Find(newname))
                {
                    Types __t = this.block.SymbolTable.Get(newname);
                    if(__t is Assign _a)
                    {
                        if(_a.Left is Variable _av)
                        {
                            if (_av.Type == "auto")
                                newname = "this." + newname;
                        }
                    }else if(__t is Variable _v)
                    {
                        if(_v.Type == "auto")
                            newname = "this." + newname;
                    }
                }
            }
            */
            if (newname.Split('.')[0] == "this")
            {                
                newname = block.getClass() + "." + string.Join(".", newname.Split('.').Skip(1));
            }
            if(Value == "this")
            {
                Types s = this.assingBlock.SymbolTable.Get(this.assingBlock.assignTo.Split('.')[0]);
                if(s is Class)
                    this.dateType = ((Class)this.assingBlock.SymbolTable.Get(this.assingBlock.assignTo.Split('.')[0])).Name;
            }
            if (this.dateType.Value == "auto")
            {
                Types fvar = this.block.FindVariable(newname);
                if (this.block.SymbolTable.Find(newname))
                {
                    if(this.block.SymbolTable.Get(newname) is Generic)
                    {
                        this.dateType = new Token(Token.Type.CLASS, "object");
                    }
                    else if (this.block.SymbolTable.Get(newname) is Properties prop)
                    {
                        this.dateType = prop.variable.TryVariable().dateType;
                    }
                    else if (((Assign)this.block.SymbolTable.Get(newname)).Right is UnaryOp)
                    {
                        if (((UnaryOp)((Assign)this.block.SymbolTable.Get(newname)).Right).Op == "call")
                        {
                            Token fname = ((UnaryOp)((Assign)this.block.SymbolTable.Get(newname)).Right).Name;
                            Types sd = this.block.SymbolTable.Get(fname.Value);
                            if (!(sd is Error))
                            {
                                Function f = (Function)sd;
                                this.dateType = f.Returnt;
                            }
                        }else
                            this.dateType = ((Variable)(((Assign)this.block.SymbolTable.Get(newname)).Left)).dateType;
                    }
                    else
                        this.dateType = ((Variable)(((Assign)this.block.SymbolTable.Get(newname)).Left)).dateType;
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
            if(key != null)
                key.endit = false;

            string vname = "";

            string nameclass = "";
            if (class_ != null)
                nameclass = (class_.JSName != "" ? class_.JSName : class_.Name.Value);
            else if (inter_ != null)
                nameclass = inter_.Name.Value;
            else if (_class != null)
                nameclass = _class.Name;

            var t__ = "this" + (Value == "this"?"":".");
            if (assingBlock != null && assingBlock.isType(Block.BlockType.PROPERTIES))
                t__ = "this.$self" + (Value == "this"?"":".");

            var not = Value;

            if (Value.Split('.')[0] == "this")
            {
                not = t__ + string.Join(".", value.Split('.').Skip(1));
                vname = string.Join(".", value.Split('.').Skip(1)) + (isKey ? "[" + key.Compile() + "]" : "");

                if (block.isInConstructor)
                {
                    if(block.SymbolTable.Get(this.value) is Properties)
                    {
                        if (asDateType != null)
                            return DoTabs(tabs) + (inParen ? "(" : "") + "($this.Property$" + vname + "_getter().constructor.name == '" + nameclass + "' ? $this.Property$" + vname + "_getter() : alert('Variable " + vname + " is not type " + asDateType.Value + "'))" + (inParen ? ")" : "");
                        return DoTabs(tabs) + (inParen ? "(" : "") + "$this.Property$" + string.Join(".", value.Split('.').Skip(1)) + "_getter()" + (isKey ? "["+key.Compile()+"]" : "") + (inParen ? ")" : "");
                    }
                    if (asDateType != null)
                        return DoTabs(tabs) + (inParen ? "(" : "") + "($this." + vname + ".constructor.name == '" + nameclass + "' ? $this." + vname + " : alert('Variable " + vname + " is not type " + asDateType.Value + "'))" + (inParen ? ")" : "");
                    return DoTabs(tabs) + (inParen ? "(" : "") + "$this." + string.Join(".", value.Split('.').Skip(1)) + (isKey ? "["+key.Compile()+"]" : "") + (inParen ? ")" : "");
                }
            }

            if (block.SymbolTable.Get(this.value) is Generic)
                vname = t__ + "generic$" + Value + (isKey ? "[" + key.Compile() + "]" : "");
            else if (block.SymbolTable.Get(this.value) is Properties)
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
                else if(oppq is Error && ((Error)oppq).Message == "Found but arguments are bad!") getFoundButBadArgs = true;
            }
            if (asDateType != null)
                return DoTabs(tabs) + (inParen ? "(" : "") + "(" + vname + ".constructor.name == '" + nameclass + "' ? "+vname+ " : alert('Variable " + vname + " is not type " + asDateType.Value + "'))" + (inParen ? ")" : "");
            return DoTabs(tabs) + (inParen ? "(" : "") + vname + (inParen ? ")" : "");
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
            if (op == Token.Type.EQUAL)     o = "==";
            if (op == Token.Type.NOTEQUAL)  o = "!=";
            if (op == Token.Type.AND)       o = "&&";
            if (op == Token.Type.OR)        o = "||";
            if (op == Token.Type.MORE)      o = ">";
            if (op == Token.Type.LESS)      o = "<";

            if (op == Token.Type.PLUS)      o = "+";
            if (op == Token.Type.INC)       o = "++";
            if (op == Token.Type.MINUS)     o = "-";
            if (op == Token.Type.DEC)       o = "--";
            if (op == Token.Type.DIV)       o = "/";
            if (op == Token.Type.MUL)       o = "*";
            if (op == Token.Type.NEW)       o = "new";
            if (op == Token.Type.RETURN)    o = "return";
            if (op == Token.Type.CALL)      o = "call";
            if (op == Token.Type.GET)       o = "get";
            if (op == Token.Type.IS)        o = "is";
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
                Interpreter.semanticError.Add(new Error("Date type "+dateType.Value+" not support 'get' operator", Interpreter.ErrorType.ERROR, token));
            if(isKey && getFoundButBadArgs)
                Interpreter.semanticError.Add(new Error("Date type " + dateType.Value + " support 'get' operator, but arguments are wrong", Interpreter.ErrorType.ERROR, token));
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

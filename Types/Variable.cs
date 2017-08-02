﻿using System;
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
        Types key;
        bool isKey = false;

        public Variable(Token token, Types block, Token dateType = null)
        {
            this.block = (Block)block;
            this.token = token;
            this.value = token.Value;
            if (dateType == null)
                dateType = new Token(Token.Type.AUTO, "auto");
            this.dateType = dateType;
            
            if (this.dateType.Value != "auto")
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

        public void setKey(Types key)
        {
            this.key = key;
            isKey = true;
        }

        public void Check()
        {
            string newname = this.value;
            if (Value.Split('.')[0] == "this")
            {                
                newname = block.getClass() + "." + string.Join(".", value.Split('.').Skip(1));
            }
            if (this.dateType.Value == "auto")
            {
                Types fvar = this.block.FindVariable(newname);
                if (this.block.SymbolTable.Find(newname))
                {
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
                        Function asfunc = (Function)this.block.SymbolTable.Get(this.block.assignTo);
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
                }
            }
        }
        
        public override string Compile(int tabs = 0)
        {
            if(key != null)
                key.endit = false;
            if (Value.Split('.')[0] == "this")
            {
                if (block.isInConstructor)
                {
                    return DoTabs(tabs) + "$this." + string.Join(".", value.Split('.').Skip(1)) + (isKey ? "["+key.Compile()+"]" : "");
                }
            }
            return DoTabs(tabs) + Value + (isKey ? "[" + key.Compile() + "]" : "");
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
            if (_class != null && (class_ == null && inter_ == null))
                return _class.OutputType(GetOperator(op), first, second);
            else if (class_ != null)
                return class_.OutputType(GetOperatorName(op), first, second);
            else if (inter_ != null)
                return inter_.OutputType(GetOperatorName(op), first, second);
            return null;
        }
        public bool SupportOp(Token.Type op)
        {
            object o = null;
            if (_class != null && (class_ == null && inter_ == null))
                o = _class.SupportOp(GetOperator(op));
            else if (class_ != null)
                o = class_.SupportOp(GetOperatorName(op));
            else if (inter_ != null)
                o = inter_.SupportOp(GetOperatorName(op));
            return (o is null ? false : (bool)o);
        }
        public bool SupportSecond(Token.Type op, object second, object secondAsVariable)
        {
            object o = null;
            if (_class != null)
                o = _class.SupportSecond(second, secondAsVariable);
            else if (class_ != null)
                o = class_.SupportSecond(GetOperatorName(op), second, secondAsVariable);
            else if (inter_ != null)
                o = inter_.SupportSecond(GetOperatorName(op), second, secondAsVariable);
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
            if (isKey && !SupportOp(Token.Type.GET))
            {
                Interpreter.semanticError.Add(new Error("Date type "+dateType.Value+" not support 'get' operator", Interpreter.ErrorType.ERROR, token));
            }
        }
    }
}

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
        TypeObject _class;

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
            }
            if(_class == null)
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

        public void Check()
        {
            if (this.dateType.Value == "auto")
            {
                Types fvar = this.block.FindVariable(this.value);
                if (this.block.SymbolTable.Find(this.value))
                {
                    this.dateType = ((Variable)(((Assign)this.block.SymbolTable.Get(this.value)).Left)).dateType;
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
            }
        }
        
        public override string Compile(int tabs = 0)
        {
            return DoTabs(tabs) + Value;
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
            if (op == Token.Type.EQUAL) o = "==";
            if (op == Token.Type.NOTEQUAL) o = "!=";
            if (op == Token.Type.AND) o = "&&";
            if (op == Token.Type.OR) o = "||";

            if (op == Token.Type.PLUS) o = "+";
            if (op == Token.Type.MINUS) o = "-";
            if (op == Token.Type.DIV) o = "/";
            if (op == Token.Type.MUL) o = "*";
            if (op == Token.Type.NEW) o = "new";
            if (op == Token.Type.RETURN) o = "return";
            if (op == Token.Type.CALL) o = "call";
            return o;
        }
        public string GetOperator(Token.Type op)
        {
            return GetOperatorStatic(op);
        }
        public Token OutputType(Token.Type op, object first, object second)
        {
            return _class?.OutputType(GetOperator(op), first, second);
        }
        public bool SupportOp(Token.Type op)
        {
            object o = _class?.SupportOp(GetOperator(op));
            return (o is null ? false : (bool)o);
        }
        public bool SupportSecond(object second, object secondAsVariable)
        {
            object o = _class?.SupportSecond(second, secondAsVariable);
            return (o is null ? false : (bool)o);
        }          
        public object Operator(Token.Type op, object first, object second)
        {
            return _class?.Operator(GetOperator(op), first, second);
        }

        public override void Semantic()
        {
            
        }
    }
}

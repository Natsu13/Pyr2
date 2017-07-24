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
                dateType = new Token(Token.Type.DYNAMIC, "dynamic");
            this.dateType = dateType;
            if (this.dateType.Value != "dynamic")
            {
                if (this.block.SymbolTable.FindInternal(dateType.Value))
                {
                    Type type = this.block.SymbolTable.GetType(dateType.Value);
                    _class = (TypeObject)Activator.CreateInstance(type);
                }
            }
        }        
        public void   setType(Token _type) {
            this.dateType = _type;
            if (this.dateType.Value != "dynamic")
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
        public Token Token { get { return token; } }

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
            else
                throw new Exception("Variable " + value + " is not declared!");
        }
        public static string GetOperatorStatic(Token.Type op)
        {
            string o = "";
            if (op == Token.Type.PLUS) o = "+";
            if (op == Token.Type.MINUS) o = "-";
            if (op == Token.Type.DIV) o = "/";
            if (op == Token.Type.MUL) o = "*";
            if (op == Token.Type.NEW) o = "new";
            if (op == Token.Type.RETURN) o = "return";
            return o;
        }
        public string GetOperator(Token.Type op)
        {
            return GetOperatorStatic(op);
        }
        public bool SupportOp(Token.Type op)
        {
            return _class.SupportOp(GetOperator(op));
        }
        public object Operator(Token.Type op, object first, object second)
        {
            return _class.Operator(GetOperator(op), first, second);
        }

        public override void Semantic()
        {
            
        }
    }
}

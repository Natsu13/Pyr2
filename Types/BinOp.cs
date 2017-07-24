using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class BinOp: Types
    {
        Types left, right;
        Token op, token;
        Block block;
        public BinOp(Types left, Token op, Types right, Block block)
        {
            this.left = left;
            this.op = this.token = op;
            this.right = right;
            this.block = block;
        }

        public override string Compile(int tabs = 0)
        {
            right.assingBlock = block;
            left.assingBlock = block;
            return left.Compile(0) + Variable.GetOperatorStatic(op.type) + right.Compile(0); 
        }

        public override void Semantic()
        {
            if (left is Number)
            {
                Variable v = new Variable(((Number)left).Token, block, new Token(Token.Type.CLASS, "int"));
                if (!v.SupportOp(op.type))
                {
                    Interpreter.semanticError.Add(new Error("Varible type 'int' not support operator " + v.GetOperator(op.type), Interpreter.ErrorType.ERROR, ((Number)left).Token));
                }
            }
            else if (left is CString)
            {
                Variable v = new Variable(((CString)left).Token, block, new Token(Token.Type.CLASS, "string"));
                if (!v.SupportOp(op.type))
                {
                    Interpreter.semanticError.Add(new Error("Varible type 'int' not support operator " + v.GetOperator(op.type), Interpreter.ErrorType.ERROR, ((Number)left).Token));
                }
            }
        }

        public override int Visit()
        {
            if(left is Number)
            {
                Variable v = new Variable(((Number)left).Token, block, new Token(Token.Type.CLASS, "int"));
                if (v.SupportOp(op.type))
                {
                    return (int)v.Operator(op.type, ((Number)left).Value, ((Number)right).Value);
                }
                else
                {
                    block.Interpret.Error("Varible type 'int' not support operator "+v.GetOperator(op.type));
                }
            }
            else if (left is CString)
            {
                Variable v = new Variable(((CString)left).Token, block, new Token(Token.Type.CLASS, "int"));
                if (v.SupportOp(op.type))
                {
                    return (int)v.Operator(op.type, ((CString)left).Value, ((CString)right).Value);
                }
                else
                {
                    block.Interpret.Error("Varible type 'int' not support operator " + v.GetOperator(op.type));
                }
            }
            else if (op.type == Token.Type.PLUS)
                return left.Visit() + right.Visit();
            else if(op.type == Token.Type.MINUS)
                return left.Visit() - right.Visit();
            else if(op.type == Token.Type.MUL)
                return left.Visit() * right.Visit();
            else if(op.type == Token.Type.DIV)
                return left.Visit() / right.Visit();
            return 0;
        }
    }
}

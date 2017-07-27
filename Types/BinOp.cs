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
        Token outputType;
        public BinOp(Types left, Token op, Types right, Block block)
        {
            this.left = left;
            this.op = this.token = op;
            this.right = right;
            this.block = block;
        }
        
        public override Token getToken() { return Token.Combine(this.left.getToken(), this.right.getToken()); }

        public override string Compile(int tabs = 0)
        {
            right.assingBlock = block;
            left.assingBlock = block;

            if (right is Variable) ((Variable)right).Check();
            if (left is Variable) ((Variable)left).Check();

            if (left is Number)
            {
                Variable v = new Variable(((Number)left).getToken(), block, new Token(Token.Type.CLASS, "int"));
                outputType = v.OutputType(op.type, left, right);
            }
            else if (left is CString)
            {
                Variable v = new Variable(((CString)left).getToken(), block, new Token(Token.Type.CLASS, "string"));
                outputType = v.OutputType(op.type, left, right);
            }
            else if(left is Variable)
            {
                outputType = ((Variable)left).OutputType(op.type, left, right);
            }
            return left.Compile(0) + " "+ Variable.GetOperatorStatic(op.type) + " " + right.Compile(0); 
        }

        public override void Semantic()
        {
            left.Semantic();
            right.Semantic();

            Variable v = left.TryVariable();
            Variable r = right.TryVariable();

            this.outputType = v.OutputType(op.type, v, r);

            if (!v.SupportOp(op.type))
            {
                Interpreter.semanticError.Add(new Error("Varible type '" + v.Type + "' not support operator " + Variable.GetOperatorStatic(op.type), Interpreter.ErrorType.ERROR, left.getToken()));
            }
            if (!v.SupportSecond(right, r))
            {
                Interpreter.semanticError.Add(new Error("Operator " + Variable.GetOperatorStatic(op.type) + " cannot be applied for '" + v.Type + "' and '" + r.Type + "'", Interpreter.ErrorType.ERROR, op));
            }
        }

        public Token OutputType { get { return outputType; } }

        public override int Visit()
        {
            if(left is Number)
            {
                Variable v = new Variable(((Number)left).getToken(), block, new Token(Token.Type.CLASS, "int"));
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
                Variable v = new Variable(((CString)left).getToken(), block, new Token(Token.Type.CLASS, "int"));
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

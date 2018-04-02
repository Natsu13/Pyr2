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
        Token rtok;
        public BinOp(Types left, Token op, Types right, Block block)
        {
            this.left = left;
            this.op = this.token = op;
            this.right = right;
            this.block = this.assingBlock = block;            
        }
        public BinOp(Types left, Token op, Token right, Block block)
        {
            this.left = left;
            this.op = this.token = op;
            this.right = null;
            this.block = this.assingBlock = block;
            this.rtok = right;
        }

        public override Token getToken() { return Token.Combine(this.left.getToken(), this.right.getToken()); }

        public override string Compile(int tabs = 0)
        {
            if (right != null)
            {
                right.assingBlock = assingBlock;
                right.endit = false;
            }
            left.assingBlock = assingBlock;
            left.endit = false;

            if (right is Variable) ((Variable)right).Check();
            if (left is Variable) ((Variable)left).Check();                      

            Variable v = null;

            string o = Variable.GetOperatorStatic(op.type);
            if (o == "is")
            {
                right = block.SymbolTable.Get(rtok.Value);
                string vname = left.TryVariable().Value;

                string classname = "";
                if (right is Class) classname = ((Class)right).getName();
                else if (right is Interface) classname = ((Interface)right).getName();
                else if (right is Generic) classname = ((Generic)right).Name;
                else classname = right.TryVariable().Value;

                string rt = "";
                if (right is Generic)
                    rt = "(" + vname + ".constructor.name == this.generic$" + classname + " ? true : false)";
                else
                    rt = "("+vname + ".constructor.name == '" + classname + "' ? true : false)";

                outputType = new Token(Token.Type.BOOL, "bool");
                return (inParen ? "(" : "") + rt + (inParen ? ")" : "");
            }
            if (left is Number)
            {
                v = new Variable(((Number)left).getToken(), block, new Token(Token.Type.CLASS, "int"));
                outputType = v.OutputType(op.type, left, right);
            }
            else if (left is CString)
            {
                v = new Variable(((CString)left).getToken(), block, new Token(Token.Type.CLASS, "string"));
                outputType = v.OutputType(op.type, left, right);
                if (right is UnaryOp ruop)
                    ruop.isInString = true;
            }
            else if(left is Variable)
            {
                v = ((Variable)left);
                if (v.getDateType().Value == "auto")
                    v.Check();
                outputType = ((Variable)left).OutputType(op.type, left, right);
            }
            else if(left is UnaryOp leuo)
            {
                if(leuo.Op == "call")
                {
                    if (leuo.usingFunction == null)
                        leuo.Compile();
                    if (leuo.usingFunction != null)
                    {
                        Function f = leuo.usingFunction;
                        outputType = f.Returnt;
                    }
                }
                if (leuo.Op == "..")
                {
                    if (assingBlock?.SymbolTable.Get("Range") != null)
                    {
                        outputType = ((Class)assingBlock?.SymbolTable.Get("Range")).Name;
                    }                    
                }
                if(op.Value == "dot" && right is UnaryOp riuo)
                {
                    riuo.Block = assingBlock?.SymbolTable.Get(outputType.Value).assingBlock;
                }
                else if(op.Value == "dot" && right is Variable riva)
                {                    
                    if(assingBlock.SymbolTable.Find(leuo.OutputType.Value))
                    {
                        Types t = ((Class)assingBlock.SymbolTable.Get(leuo.OutputType.Value)).Block.SymbolTable.Get(riva.Value);
                        if(t is Assign ta)
                        {
                            if (ta.Left is Variable tav)
                                riva.setType(tav.getDateType());
                        }
                        else if(t is Variable tv)
                        {
                            riva.setType(tv.getDateType());
                        }
                        outputType = riva.getDateType();
                    }
                }
                v = left.TryVariable();
            }
            else if(left is BinOp)
            {
                left.Compile();
                v = left.TryVariable();
            }
            else
            {
                left.Compile();
                v = left.TryVariable();
            }
            if ((v._class != null && v.class_ == null) || (v.class_ != null && v.class_.JSName != ""))
            {
                if (op.Value == "dot")
                    return (inParen ? "(" : "") + left.Compile(0) + Variable.GetOperatorStatic(op.type) + right.Compile(0) + (inParen ? ")" : "");
                else
                    return (inParen ? "(" : "") + left.Compile(0) + " " + Variable.GetOperatorStatic(op.type) + " " + right.Compile(0) + (inParen ? ")" : "");
            }
            else if(v.class_ != null)
            {                
                if(op.Value == "dot")
                {
                    return (inParen ? "(" : "") + left.Compile(0) + "." + right.Compile(0) + (inParen ? ")" : "");
                }
                Types oppq = v.class_.block.SymbolTable.Get("operator " + Variable.GetOperatorNameStatic(op.type));
                if (oppq is Error)                                    
                    return "";                
                Function opp = (Function)oppq;
                if (op.type == Token.Type.NOTEQUAL)
                    return (inParen ? "(" : "") + "!(" + left.Compile(0) + "." + opp.Name + "(" + right.Compile(0) + "))" + (inParen ? ")" : "");
                else if (op.type == Token.Type.MORE || op.type == Token.Type.LESS)
                {
                    if (op.type == Token.Type.MORE)
                        return (inParen ? "(" : "") + left.Compile(0) + "." + opp.Name + "(" + right.Compile(0) + ") > 0" + (inParen ? ")" : "");
                    else
                        return (inParen ? "(" : "") + left.Compile(0) + "." + opp.Name + "(" + right.Compile(0) + ") < 0" + (inParen ? ")" : "");
                }
                else
                    return (inParen ? "(" : "") + left.Compile(0) + "." + opp.Name + "(" + right.Compile(0) + ")" + (inParen ? ")" : "");
            }
            return "";
        }

        public override void Semantic()
        {            
            left.Semantic();
            if(op.type == Token.Type.DOT)
            {
                if(left is UnaryOp && ((UnaryOp)left).Op == "call" && right is Variable)
                {
                    if(((UnaryOp)left).usingFunction != null)
                    {
                        Function f = ((UnaryOp)left).usingFunction;
                        ((Variable)right).setType(f.Returnt);
                    }
                }
            }
            right.Semantic();

            Variable v = left.TryVariable();
            Variable r = right.TryVariable();

            if(left is BinOp bi)
            {
                if(bi.op.Value == "dot")
                {
                    
                }
            }
            if(op.Value == "dot" && left is UnaryOp leuop && right is Variable riva)
            {
                if(assingBlock.SymbolTable.Find(leuop.OutputType.Value))
                {
                    Types t = ((Class)assingBlock.SymbolTable.Get(leuop.OutputType.Value)).Block.SymbolTable.Get(riva.Value);
                    if(t is Assign ta)
                    {
                        if (ta.Left is Variable tav)
                            riva.setType(tav.getDateType());
                    }
                    else if(t is Variable tv)
                    {
                        riva.setType(tv.getDateType());
                    }
                }
            }

            if (op.Value == "dot")
                this.outputType = right.TryVariable().getDateType();
            else
                this.outputType = v.OutputType(op.type, v, r);

            if (!v.SupportOp(op.type))
            {
                Interpreter.semanticError.Add(new Error("#300 Varible type '" + v.Type + "' not support operator " + Variable.GetOperatorStatic(op.type), Interpreter.ErrorType.ERROR, left.getToken()));
            }
            else if (!v.SupportSecond(op.type, right, r))
            {
                Interpreter.semanticError.Add(new Error("#301 Operator " + Variable.GetOperatorStatic(op.type) + " cannot be applied for '" + v.Type + "' and '" + r.Type + "'", Interpreter.ErrorType.ERROR, op));
            }
        }        

        public Token OutputType {
            get {
                if (outputType != null && outputType.Value != "auto" && outputType.Value != "object")
                    return outputType;

                if(op.Value == "dot" && left is UnaryOp leuop && right is Variable riva)
                {
                    if(assingBlock != null && assingBlock.SymbolTable.Find(leuop.OutputType.Value))
                    {
                        Types t = ((Class)assingBlock.SymbolTable.Get(leuop.OutputType.Value)).Block.SymbolTable.Get(riva.Value);
                        if(t is Assign ta)
                        {
                            if (ta.Left is Variable tav)
                                riva.setType(tav.getDateType());
                        }
                        else if(t is Variable tv)
                        {
                            riva.setType(tv.getDateType());
                        }
                        this.outputType = right.TryVariable().getDateType();
                        return this.outputType;
                    }
                }

                if(left is UnaryOp leuo)
                {
                    if(leuo.OutputType.Value == "object")
                    {
                        if (leuo.Op == "call")
                        { 
                            if (leuo.usingFunction == null)
                                leuo.Compile();
                            if(leuo.usingFunction != null)
                            {
                                leuo.OutputType = leuo.usingFunction.Returnt;
                            }
                        }
                    }
                }
                if(op.Value == "dot")
                {
                    if (right is Variable && ((Variable)right).Type == "auto")
                        ((Variable)right).setType(((UnaryOp)left).OutputType);
                }
                Variable v = left.TryVariable();
                Variable r = right.TryVariable();

                if(left is Variable) v.Check();
                if(right is Variable) r.Check();

                this.outputType = v.OutputType(op.type, v, r);

                return outputType;
            }
        }

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
                    block.Interpret.Error("#113 Varible type 'int' not support operator "+v.GetOperator(op.type));
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
                    block.Interpret.Error("#114 Varible type 'int' not support operator " + v.GetOperator(op.type));
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

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

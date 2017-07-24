using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Assign:Types
    {
        Types left, right;
        Token op, token;
        public Assign(Types left, Token op, Types right)
        {
            this.left = left;
            this.op = this.token = op;
            this.right = right;
            string name = ((Variable)left).Value;
            ((Variable)left).Block.variables[name] = this;
        }

        public override int Visit()
        {            
            return 0; 
        }

        public new string GetType()
        {
            return ((Variable)left).Type;
        }

        public string GetVal()
        {
            return right.Visit().ToString();
        }

        public override string Compile(int tabs = 0)
        {

            if (left is Variable)
            {
                if (((Variable)left).Block.blockAssignTo != "") return "";
                right.assingBlock = ((Variable)left).Block;
                return DoTabs(tabs) + "var " + left.Compile(0) + " = " + right.Compile(0) + ";";
            }
            else
                return DoTabs(tabs) + left.Compile(0) + " = " + right.Compile(0) + ";";
        }

        public override void Semantic()
        {
            if (left is Variable)
            {
                if (((Variable)left).Type == "dynamic")
                {
                    if (right is Variable)
                        ((Variable)left).setType(((Variable)right).getType());
                }
                else
                {
                    if (right is Variable && ((Variable)left).Type != ((Variable)right).Type)
                    {
                        if (((Variable)right).Type == "dynamic")
                        {
                            Block rblock = ((Variable)right).Block;
                            if (rblock.SymbolTable.Find(rblock.assignTo))
                            {
                                Function asfunc = (Function)rblock.SymbolTable.Get(rblock.assignTo);
                                Variable var = asfunc.ParameterList.Find(((Variable)right).Value);
                                if (var != null)
                                {
                                    right = var;
                                }
                                else
                                    Interpreter.semanticError.Add(new Error("Variable " + ((Variable)right).Value + " not exist!", Interpreter.ErrorType.ERROR, ((Variable)right).Token));
                            }
                            if (right is Variable && ((Variable)left).Type != ((Variable)right).Type)
                                Interpreter.semanticError.Add(new Error("Variable " + ((Variable)left).Value + " with type " + ((Variable)left).Type + " can't be converted to " + ((Variable)right).Type, Interpreter.ErrorType.ERROR, ((Variable)left).Token));
                        }
                        else
                            Interpreter.semanticError.Add(new Error("Variable " + ((Variable)left).Value + " with type " + ((Variable)left).Type + " can't be converted to " + ((Variable)right).Type, Interpreter.ErrorType.ERROR, ((Variable)left).Token));
                    }
                    else if (right is Number && ((Variable)left).Type != "int")
                        Interpreter.semanticError.Add(new Error("Variable " + ((Variable)left).Value + " with type " + ((Variable)left).Type + " can't be converted to int", Interpreter.ErrorType.ERROR, ((Variable)left).Token));
                    else if (right is CString)
                    {
                        if(((Variable)left).Type != "string")
                            Interpreter.semanticError.Add(new Error("Variable " + ((Variable)left).Value + " with type " + ((Variable)left).Type + " can't be converted to string", Interpreter.ErrorType.ERROR, ((Variable)left).Token));                        
                        else
                            ((CString)right).Semantic();
                    }
                    else if (right is UnaryOp && ((UnaryOp)right).Op == "new")
                    {
                        if (((Variable)left).Type != ((UnaryOp)right).Name.Value)
                            Interpreter.semanticError.Add(new Error("Variable " + ((Variable)left).Value + " with type " + ((Variable)left).Type + " can't be converted to " + ((UnaryOp)right).Name.Value, Interpreter.ErrorType.ERROR, ((Variable)left).Token));
                    }
                }
            }
        }
    }
}

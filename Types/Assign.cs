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
        bool isDeclare = false;
        bool isMismash = false;
        bool isRedeclared = false;
        string originlDateType = "";
        public Assign(Types left, Token op, Types right, Block current_block = null)
        {
            this.left = left;
            this.op = this.token = op;
            this.right = right;        
            if (left is Variable) {
                left.Semantic();

                string name = ((Variable)left).Value;
                if (!((Variable)left).Block.variables.ContainsKey(name))
                {
                    isDeclare = true;
                    ((Variable)left).Block.variables[name] = this;
                }
                else if (((Variable)left).Type != "auto")
                {
                    Variable v = ((Variable)((Variable)left).Block.FindVariable(name).Left);
                    if (!Interpreter._REDECLARATION && v.getType().Value != ((Variable)this.left).getType().Value)
                    {
                        originlDateType = v.getType().Value;
                        isRedeclared = true;
                    }
                }
                else
                {
                    Variable v = ((Variable)((Variable)left).Block.variables[name].Left);
                    if (v.getType().Value != ((Variable)this.left).getType().Value)
                    {
                        originlDateType = v.getType().Value;
                        isMismash = true;
                    }
                }
                if (((Variable)left).Value.Contains("."))
                    isDeclare = false;
            }            
            left.assingBlock = current_block;
        }

        public override Token getToken() { return null; }

        public Types Left { get { return left; } } 
        public Types Right { get { return right; } }        

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
            if ((((Variable)left).Block.Type == Block.BlockType.CONDITION && ((Variable)left).Block.Parent.variables.ContainsKey(((Variable)left).Value)))
                isDeclare = false;
            if (left is Variable)
            {
                if (((Variable)left).Block.blockAssignTo != "") return "";
                    right.assingBlock = ((Variable)left).Block;
                if (right is UnaryOp)
                    ((UnaryOp)right).endit = false;
                return DoTabs(tabs) + (isDeclare?"var ":"") + left.Compile(0) + " = " + right.Compile(0) + ";";
            }
            else
                return DoTabs(tabs) + left.Compile(0) + " = " + right.Compile(0) + ";";
        }

        public override void Semantic()
        {            
            if (left is Variable)
            {
                if (((Variable)left).Type == "auto")
                {
                    Types t = ((Variable)left).assingBlock.SymbolTable.Get(((Variable)left).Value);
                    if(t != null)
                        ((Variable)left).setType(((Variable)((Assign)t).Left).getType());
                    else if (right is Variable && (((Variable)right).getToken().type == Token.Type.TRUE || ((Variable)right).getToken().type == Token.Type.FALSE))
                        ((Variable)left).setType(new Token(Token.Type.BOOL, "bool"));
                    else if (right is Variable)
                        ((Variable)left).setType(((Variable)right).getType());
                    else if(right is Number)
                        ((Variable)left).setType(new Token(Token.Type.INTEGER, "int"));
                    else if (right is CString)
                        ((Variable)left).setType(new Token(Token.Type.STRING, "string"));                        
                }
                else
                {
                    if (right is Variable && ((Variable)left).Type != ((Variable)right).Type)
                    {
                        if (((Variable)right).Type == "auto")
                        {
                            if (right is Variable && ((Variable)right).getToken().type == Token.Type.TRUE)
                                ((Variable)right).setType(new Token(Token.Type.BOOL, "bool"));
                            else if (right is Variable && ((Variable)right).getToken().type == Token.Type.FALSE)
                                ((Variable)right).setType(new Token(Token.Type.BOOL, "bool"));
                            else
                            {
                                Block rblock = ((Variable)right).Block;
                                if (rblock.SymbolTable.Find(rblock.assignTo))
                                {
                                    Function asfunc = (Function)rblock.SymbolTable.Get(rblock.assignTo);
                                    Variable var = asfunc.ParameterList.Find(((Variable)right).Value);
                                    Types vaq = (Types)rblock.SymbolTable.Get(((Variable)right).Value);
                                    if(vaq != null)
                                    {
                                        if(vaq is Assign)
                                        {
                                            right = vaq;
                                        }
                                        if(vaq is Error)
                                            Interpreter.semanticError.Add(new Error("Variable " + ((Variable)right).Value + " not exist!", Interpreter.ErrorType.ERROR, ((Variable)right).getToken()));
                                    }
                                    else if (var != null)
                                    {
                                        right = var;
                                    }
                                    else
                                        Interpreter.semanticError.Add(new Error("Variable " + ((Variable)right).Value + " not exist!", Interpreter.ErrorType.ERROR, ((Variable)right).getToken()));
                                }
                            }
                            if (right is Variable && ((Variable)left).Type != ((Variable)right).Type)
                                Interpreter.semanticError.Add(new Error("Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to '" + ((Variable)right).Type + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                        }
                        else
                            Interpreter.semanticError.Add(new Error("Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to '" + ((Variable)right).Type + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                    }
                    else if (right is Number && ((Variable)left).Type != "int")
                        Interpreter.semanticError.Add(new Error("Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to 'int'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                    else if (right is CString)
                    {
                        if(((Variable)left).Type != "string")
                            Interpreter.semanticError.Add(new Error("Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to 'string'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));                        
                        else
                            ((CString)right).Semantic();
                    }
                    else if (right is UnaryOp && ((UnaryOp)right).Op == "new")
                    {
                        if (((Variable)left).Type != ((UnaryOp)right).Name.Value)
                            Interpreter.semanticError.Add(new Error("Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to " + ((UnaryOp)right).Name.Value + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                    }
                }

                if (isMismash && originlDateType != ((Variable)left).getType().Value)
                {
                    Interpreter.semanticError.Add(new Error("Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to '" + originlDateType + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                }

                if (isRedeclared)
                {
                    Interpreter.semanticError.Add(new Error("Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' is alerady declared as '" + originlDateType+ "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                }
            }

            left.Semantic();
            right.Semantic();
        }
    }
}

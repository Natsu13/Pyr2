using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public List<_Attribute> attributes;

        public bool isStatic = false;
        public Token _static = null;

        public Assign(Types left, Token op, Types right, Block current_block = null)
        {
            this.left = left;
            left.assingBlock = current_block;
            this.op = this.token = op;
            this.right = right;        
            if (left is Variable) {
                //left.Semantic();

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
        }

        public override string InterpetSelf()
        {
            return "new Assign("+left.InterpetSelf()+", "+token.InterpetSelf()+ ", "+right.InterpetSelf()+", "+ left.assingBlock.InterpetSelf()+ ")";
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
            string addCode = "";
            if (attributes?.Where(x => x.GetName(true) == "Debug").Count() > 0)
            {
                if(Interpreter._DEBUG)
                    Debugger.Break();
                addCode = "debugger;";
            }
            if (isStatic) return "";
            string addName = "";
<<<<<<< HEAD
            if (left is Variable variable)
            {
                if (assingBlock.Parent != null && assingBlock.Parent.SymbolTable.Find(variable.Value))
                    isDeclare = false;

                if (variable.Block.Type == Block.BlockType.CONDITION && variable.Block.Parent.variables.ContainsKey(variable.Value))
                    isDeclare = false;
            }

            right.assingToType = left;
=======
            if (left is Variable)
            {
                if (assingBlock.Parent != null && assingBlock.Parent.SymbolTable.Find(((Variable)left).Value))
                    isDeclare = false;
>>>>>>> 0c640203808d4ca5c25cb372dd6d91da202c18f8

                if ((((Variable)left).Block.Type == Block.BlockType.CONDITION && ((Variable)left).Block.Parent.variables.ContainsKey(((Variable)left).Value)))
                    isDeclare = false;
            }
            Types maybeIs = assingBlock.SymbolTable.Get(left.TryVariable().Value);
            Types maybeIs2 = null;
            string rightCompiled = "";
            if (right is Lambda)
            {
                rightCompiled = ((Lambda)right).Compile(0);
                addName = "lambda$";
            }
            else
                maybeIs2 = assingBlock.SymbolTable.Get(right.TryVariable().Value);
            
            if(left is Variable)
                right.assingBlock = ((Variable)left).Block;
            if (right is UnaryOp)
                ((UnaryOp)right).endit = false;

            if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
            {
<<<<<<< HEAD
                if (assingBlock.SymbolTable.Find(left.TryVariable().Value) && maybeIs is Properties)
                {
                    string[] varname = left.TryVariable().Value.Split('.');
                    if (assingBlock.isInConstructor || assingBlock.isType(Block.BlockType.PROPERTIES))
                        varname[0] = "$this";
                    return DoTabs(tabs) + addCode + varname[0] + ".Property$" + string.Join(".", varname.Skip(1)) + ".set(" + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ");";
                }
                else if (maybeIs2 != null && assingBlock.SymbolTable.Find(right.TryVariable().Value) && maybeIs2 is Properties)
                {
                    string[] varname = right.TryVariable().Value.Split('.');
                    if (assingBlock.isInConstructor || assingBlock.isType(Block.BlockType.PROPERTIES))
                        varname[0] = "$this";
                    return DoTabs(tabs) + addCode + (isDeclare ? "var " : "") + addName + left.Compile(0) + " = " + varname[0] + ".Property$" + string.Join(".", varname.Skip(1)) + ".get();";
                }
                else if (left is Variable)
                {
                    string tbs = DoTabs(tabs);
                    string ret = "";
                    if (addName == "lambda$")
                    {
                        string var = left.Compile(0);
                        if (var.Contains("delegate$"))
                        {
                            ret = tbs + addCode + (isDeclare ? "var " : "") + var + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
                        }
                        else
                        {
                            string[] spli = var.Split('.');
                            ret = tbs + addCode + (isDeclare ? "var " : "") + string.Join(".", spli.Take(spli.Length - 1)) + ".delegate$" + spli.Skip(spli.Length - 1).First() + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
                        }
                    }
                    else
                        ret = tbs + addCode + (isDeclare ? "var " : "") + addName + left.Compile(0) + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
                    return ret;
                }
                else
                    return DoTabs(tabs) + addName + left.Compile(0) + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
            }
            else if(Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
            {
                if (assingBlock.SymbolTable.Find(left.TryVariable().Value) && maybeIs is Properties)
                {
                    string[] varname = left.TryVariable().Value.Split('.');
                    if (assingBlock.isInConstructor || assingBlock.isType(Block.BlockType.PROPERTIES))
                        varname[0] = "self";
                    return DoTabs(tabs) + addCode + varname[0] + ".Property__" + string.Join(".", varname.Skip(1)) + ".set(" + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ");";
                }
                else if (maybeIs2 != null && assingBlock.SymbolTable.Find(right.TryVariable().Value) && maybeIs2 is Properties)
                {
                    string[] varname = right.TryVariable().Value.Split('.');
                    if (assingBlock.isInConstructor || assingBlock.isType(Block.BlockType.PROPERTIES))
                        varname[0] = "self";
                    return DoTabs(tabs) + addCode + addName + left.Compile(0) + " = " + varname[0] + ".Property__" + string.Join(".", varname.Skip(1)) + ".get();";
                }
                else if (left is Variable)
                {
                    string tbs = DoTabs(tabs);
                    string ret = "";
                    if (addName == "lambda$")
                    {
                        string var = left.Compile(0);
                        if (var.Contains("delegate$"))
                        {
                            ret = tbs + addCode + var + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
                        }
                        else
                        {
                            string[] spli = var.Split('.');
                            ret = tbs + addCode + string.Join(".", spli.Take(spli.Length - 1)) + ".delegate$" + spli.Skip(spli.Length - 1).First() + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
                        }
                    }
                    else
                        ret = tbs + addCode + addName + left.Compile(0) + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
                    return ret;
                }
                else
                    return DoTabs(tabs) + addName + left.Compile(0) + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
            }
            return "";
=======
                string[] varname = left.TryVariable().Value.Split('.');
                if (assingBlock.isInConstructor || assingBlock.isType(Block.BlockType.PROPERTIES))
                    varname[0] = "$this";
                return DoTabs(tabs) + addCode + varname[0] + ".Property$" + string.Join(".", varname.Skip(1)) + ".set(" + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ");";
            }
            else if (maybeIs2 != null && assingBlock.SymbolTable.Find(right.TryVariable().Value) && maybeIs2 is Properties)
            {
                string[] varname = right.TryVariable().Value.Split('.');
                if (assingBlock.isInConstructor || assingBlock.isType(Block.BlockType.PROPERTIES))
                    varname[0] = "$this";
                return DoTabs(tabs) + addCode + (isDeclare?"var ":"") + addName + left.Compile(0) + " = " + varname[0] + ".Property$" + string.Join(".", varname.Skip(1)) + ".get();";
            }
            else if (left is Variable)
            {
                string tbs = DoTabs(tabs);
                string ret = "";
                if(addName == "lambda$")
                {
                    string var = left.Compile(0);
                    if (var.Contains("delegate$"))
                    {
                        ret = tbs + addCode + (isDeclare ? "var " : "") + var + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
                    }
                    else
                    {
                        string[] spli = var.Split('.');
                        ret = tbs + addCode + (isDeclare ? "var " : "") + string.Join(".", spli.Take(spli.Length - 1)) + ".delegate$" + spli.Skip(spli.Length - 1).First() + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
                    }
                }
                else                    
                    ret = tbs + addCode + (isDeclare?"var ":"") + addName + left.Compile(0) + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled)  + ";";                
                return ret;
            }
            else
                return DoTabs(tabs) + addName + left.Compile(0) + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled)  + ";";
>>>>>>> 0c640203808d4ca5c25cb372dd6d91da202c18f8
        }

        public override void Semantic()
        {
            if (assingBlock?.SymbolTable.Get(left.TryVariable().Value) is Properties ps)
            {
                if(ps.Setter == null)
                    Interpreter.semanticError.Add(new Error("#800 Propertie " + ((Properties)left).variable.TryVariable().Value + " don't define setter!", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
            }
            if (!(right is Lambda) && assingBlock?.SymbolTable.Get(right.TryVariable().Value) is Properties pg)
            {
                if(pg.Getter == null)
                    Interpreter.semanticError.Add(new Error("#801 Propertie " + ((Properties)left).variable.TryVariable().Value + " don't define getter!", Interpreter.ErrorType.ERROR, right.TryVariable().getToken()));
            }
            if (left is Variable)
            {
                if (((Variable)left).Type == "auto")
                {
                    string newname = ((Variable)left).Value;
                    if(newname.Split('.')[0] == "this")
                    {
                        newname = (assingBlock.assignTo == "" ? assingBlock.getClass() : assingBlock.assignTo) + "." + string.Join(".", ((Variable)left).Value.Split('.').Skip(1));
                    }
                    Types t = ((Variable)left).assingBlock?.SymbolTable.Get(newname);
                    if (t != null)
                    {
                        if(t is Generic && !(right is Null))
                        {
                            Variable ri = right.TryVariable();
                            Interpreter.semanticError.Add(new Error("#102 Variable " + ((Variable)left).Value + " is generic and '"+ ri.Value + "' can't be converted to '"+ ri.getDateType().Value + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                        }
                        if (((Variable)left).Value.Split('.')[0] == "this")
                        {
                            string left_type = "auto";
                            if (t is Assign ava)
                            {
                                left_type = ava.GetType();
                            }
                            else if (t is Properties prop)
                            {
                                string ty = prop.variable.TryVariable().Type;
                                if (ty == "auto")
                                    prop.variable.TryVariable().Check();
                                left_type = prop.variable.TryVariable().Type;
                            }
                            string type = "auto";
                            if (right is CString) type = "string";
                            else if (right is Number) type = "int";
                            else if (right is Variable)
                            {
                                type = ((Variable)right).Type;
                                if (type == "auto")
                                {
                                    ((Variable)right).Check();
                                    type = ((Variable)right).Type;
                                }
                            }
                            else if (right is BinOp) type = ((BinOp)right).OutputType.Value;
                            else if (right is UnaryOp) type = ((UnaryOp)right).OutputType.Value;

                            if (left_type != type)
                            {
                                Interpreter.semanticError.Add(new Error("#101 Variable " + ((Variable)left).Value + " with type '" + left_type + "' can't be implicitly converted to '" + type + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                            }
                        }
                        else
                        {
                            ((Variable)left).setType(Left.TryVariable().getType());
                        }
                    }
                    else if (right is Variable && (((Variable)right).getToken().type == Token.Type.TRUE || ((Variable)right).getToken().type == Token.Type.FALSE))
                        ((Variable)left).setType(new Token(Token.Type.BOOL, "bool"));
                    else if (right is Variable)
                        ((Variable)left).setType(((Variable)right).getType());
                    else if (right is Number)
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
                            right.TryVariable().Check();
                            if (((Variable)right).Type != "auto")
                            { 
                                //check fixed it o.o    
                            }
                            else if (right is Variable && ((Variable)right).getToken().type == Token.Type.TRUE)
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
                                    if (vaq != null)
                                    {
                                        if (vaq is Assign)
                                        {
                                            right = vaq;
                                        }
                                        if (vaq is Error)
                                            Interpreter.semanticError.Add(new Error("#103 Variable " + ((Variable)right).Value + " not exist!", Interpreter.ErrorType.ERROR, ((Variable)right).getToken()));
                                    }
                                    else if (var != null)
                                    {
                                        right = var;
                                    }
                                    else
                                        Interpreter.semanticError.Add(new Error("#104 Variable " + ((Variable)right).Value + " not exist!", Interpreter.ErrorType.ERROR, ((Variable)right).getToken()));
                                }
                            }
                            if (right is Variable && ((Variable)left).Type != ((Variable)right).Type)
                                Interpreter.semanticError.Add(new Error("#105 Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to '" + ((Variable)right).Type + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                        }
                        else
                            Interpreter.semanticError.Add(new Error("#106 Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to '" + ((Variable)right).Type + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                    }
                    else if (right is Number && ((Variable)left).Type != "int")
                        Interpreter.semanticError.Add(new Error("#107 Variable " + ((Variable)left).Value + " can't be implicitly converted to 'int' with type '" + ((Variable)left).Type + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                    else if (right is CString)
                    {
                        if(((Variable)left).Type != "string")
                            Interpreter.semanticError.Add(new Error("#108 Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to 'string'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));                        
                        else
                            ((CString)right).Semantic();
                    }
                    else if (right is UnaryOp && ((UnaryOp)right).Op == "new")
                    {
                        if (((Variable)left).Type != ((UnaryOp)right).Name.Value)
                            Interpreter.semanticError.Add(new Error("#109 Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to '" + ((UnaryOp)right).Name.Value + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                    }
                    else if (right is BinOp && ((BinOp)right).OutputType.Value != left.TryVariable().Type)
                    {
                        Interpreter.semanticError.Add(new Error("#114 Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to '" + ((BinOp)right).OutputType.Value + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                    }
                }

                if (isMismash && originlDateType != ((Variable)left).getType().Value)
                {
                    Interpreter.semanticError.Add(new Error("#110 Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to '" + originlDateType + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                }

                if (isRedeclared)
                {
                    Interpreter.semanticError.Add(new Error("#111 Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' is alerady declared as '" + originlDateType+ "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                }
            }

            left.Semantic();
            right.Semantic();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class Assign:Types
    {
        public Types left;
        public Types right;
        Token op, token;
        public bool isDeclare = false;
        bool isMismash = false;
        bool isRedeclared = false;
        string originlDateType = "";
        public List<_Attribute> attributes;
        public bool isNull;

        public bool isStatic = false;
        public Token _static = null;

        /*Serialization to JSON object for export*/
        [JsonParam] public Types Left => left;
        [JsonParam] public Types Right { get => right; set => right = value; }
        [JsonParam] public Token Op => op;
        //[JsonParam] public Block AssignBlock => assingBlock;
        [JsonParam] public bool IsNull => isNull;
        
        public override void FromJson(JObject o)
        {
            left = JsonParam.FromJson(o["Left"]);
            right = JsonParam.FromJson(o["Right"]);
            op = Token.FromJson(o["Op"]);
            isNull = (bool) o["IsNull"];
        }
        public Assign() { }
        

        public Assign(Types left, Token op, Types right, Block current_block = null, bool isNull = false, bool isVal = false)
        {
            this.isNull = isNull;
            assingBlock = current_block;
            this.left = left;
            left.assingBlock = current_block;
            this.op = this.token = op;
            this.right = right;
            if (left is Variable) {
                //left.Semantic();
                ((Variable) left).IsVal = isVal;                
                string name = ((Variable)left).Value;
                bool notfound = current_block != null && !(current_block.SymbolTable.Get(name) is Error);
                if (current_block != null && !current_block.variables.ContainsKey(name) && !notfound)
                {
                    isDeclare = true;
                    //current_block.variables[name] = this;
                    current_block.SymbolTable.Add(left.getToken().Value, this, parent: assingBlock);
                }
                else if (!((Variable)left).Block.variables.ContainsKey(name) && !notfound)
                {
                    isDeclare = true;
                    ((Variable)left).Block.variables[name] = this;
                }
                else if (((Variable)left).Type != "auto")
                {
                    var vl = ((Variable)left);
                    var fn = vl.Block.SymbolTable.Get(name, vl.Type);
                    if (fn is Assign)
                    {
                        var v = ((Variable)(((Assign)vl.Block.SymbolTable.Get(name, vl.Type)).Left));
                        if (!Interpreter._REDECLARATION && v.getType().Value != vl.getType().Value)
                        {
                            if (v.GetAssignTo() == assingBlock?.assignTo)
                            {
                                originlDateType = v.getType().Value;
                                isRedeclared = true;
                            }
                        }
                    }
                }
                else
                {
                    var v = (Variable)((Assign)current_block?.SymbolTable.Get(name))?.Left;
                    if (v?.getType().Value != ((Variable)this.left).getType().Value && ((Variable)this.left).getType().Value != "auto")
                    {
                        originlDateType = v?.getType().Value;
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

        public override Token getToken() { return this.token; }        

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
            if (attributes != null && (attributes?.Where(x => x.GetName(true) == "Debug")).Any())
            {
                if(Interpreter._DEBUG)
                    Debugger.Break();
                addCode = "debugger;";
            }
            if (isStatic) return "";
            string addName = "";
            if (left is Variable variable)
            {
                var fvar = assingBlock.SymbolTable.Get(variable.Value);
                if (assingBlock.BlockParent != null && !(fvar is Error))
                {
                    if(fvar.GetHashCode() != GetHashCode())
                        isDeclare = false;
                }

                if (variable.Block.Type == Block.BlockType.CONDITION && variable.Block.BlockParent.variables.ContainsKey(variable.Value))
                    isDeclare = false;
            }

            right.assingToType = left;

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

            if (left is Variable variable1)
            {
                right.assingBlock = variable1.Block;
                if (variable1.IsPrimitive && variable1.IsVal)
                    return "";
                //((Variable)left).Check();
            }

            //$[<code>]
            var returnAssigment =  "";
            var rightcompile = "";

            if (right is UnaryOp unaryOp)
            {
                unaryOp.assignToParent = this;
                unaryOp.endit = false;
            }

            while (true) { 
                if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                {
                    Function usingFunc = null;
                    var inlineIdUsing = 0;
                    var isInline = false;                    
                    if (right is UnaryOp riuOp && riuOp.Op == "call")
                    {
                        usingFunc = riuOp.FindUsingFunction();
                        if (usingFunc is Function ass && ass.isInline)
                        {
                            ass.inlineId = inlineIdUsing = Function.inlineIdCounter++;
                            isInline = true;
                        }
                    }
                    rightcompile = (rightCompiled == "" ? right?.Compile(0) ?? "" : rightCompiled);

                    var ltryvar = left.TryVariable();
                    var rtryvar = right.TryVariable();
                    var fleft = assingBlock.SymbolTable.Get(ltryvar.Value);

                    if (!(fleft is Error) && maybeIs is Properties)
                    {
                        string[] varname = ltryvar.Value.Split('.');
                        if (assingBlock.isInConstructor || assingBlock.isType(Block.BlockType.PROPERTIES))
                            varname[0] = "$this";
                        returnAssigment = varname[0] + ".Property$" + string.Join(".", varname.Skip(1)) + ".set($[<code>])";
                    }
                    else if (maybeIs2 != null && !(assingBlock.SymbolTable.Get(rtryvar.Value) is Error) && maybeIs2 is Properties)
                    {
                        string[] varname = rtryvar.Value.Split('.');
                        if (assingBlock.isInConstructor || assingBlock.isType(Block.BlockType.PROPERTIES))
                            varname[0] = "$this";                        
                        returnAssigment = (isDeclare ? "var " : "") + addName + left.Compile(0) + " = $[<code>];";
                        rightcompile = varname[0] + ".Property$" + string.Join(".", varname.Skip(1)) + ".get()";
                    }
                    else if (left is Variable)
                    {
                        if (((Variable) left).Type == "auto")
                        {
                            ((Variable) left).Check();
                            if (((Variable) left).Type == "auto")
                            {
                                if (right is Variable rvar)
                                {
                                    rvar.Check();
                                    if (rvar.Type != "auto")
                                        ((Variable) left).setType(rvar.GetDateType());
                                }
                                else if(right is BinOp)
                                    ((Variable) left).setType(((BinOp)right).OutputType);
                                else if (right is CString)
                                    ((Variable) left).setType(new Token(Token.Type.STRING, "string"));
                                else if (right is Number rin)
                                {
                                    if(rin.isReal)
                                        ((Variable) left).setType(new Token(Token.Type.REAL, "float"));
                                    else
                                        ((Variable) left).setType(new Token(Token.Type.INTEGER, "int"));
                                }
                            }
                        }
                        string tbs = DoTabs(tabs);
                        //string ret = "";
                        if (addName == "lambda$")
                        {
                            string var = left.Compile(0);
                            if (var.Contains("delegate$"))
                            {
                                //ret = tbs + addCode + (isDeclare ? "var " : "") + var + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
                                returnAssigment = (isDeclare ? "var " : "") + var + " = $[<code>];";
                            }
                            else
                            {
                                string[] spli = var.Split('.');
                                //ret = tbs + addCode + (isDeclare ? "var " : "") + string.Join(".", spli.Take(spli.Length - 1)) + ".delegate$" + spli.Skip(spli.Length - 1).First() + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
                                returnAssigment = (isDeclare ? "var " : "") + string.Join(".", spli.Take(spli.Length - 1)) + ".delegate$" + spli.Skip(spli.Length - 1).First() + " = $[<code>];";                                                           
                            }
                        }
                        else {
                            if (isNull)
                            {
                                //ret = tbs + addCode + (isDeclare ? "var " : "") + addName + left.Compile(0) + ";";
                                returnAssigment = (isDeclare ? "var " : "") + addName + left.Compile(0) + ";";
                                rightcompile = "";
                            }
                            else
                            {
                                //ret = tbs + addCode + (isDeclare ? "var " : "") + addName + left.Compile(0) + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
                                returnAssigment = (isDeclare ? "var " : "") + addName + left.Compile(0) + " = $[<code>];";
                            }
                        }
                        //return ret;
                    }
                    else 
                    { 
                        //return DoTabs(tabs) + addName + left.Compile(0) + " = " + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ";";
                        returnAssigment = addName + left.Compile(0) + " = $[<code>];";
                        rightcompile = (rightCompiled == "" ? right.Compile(0) : rightCompiled);
                    }

                    if (isInline)
                    {
                        var ret = "";
                        var varname = usingFunc.Name + "$" + inlineIdUsing + "$return";
                        var rightCode = rightcompile.Substring(rightcompile.Length - 1, 1) == "\n" ? rightcompile.Substring(0, rightcompile.Length - 1) : rightcompile;
                        rightCode = (rightCode.Substring(0, 1) == "\n" ? rightCode.Substring(1) : rightCode);
                        if (usingFunc.Block.children.Count == 2 && usingFunc.ParameterList.IsAllPrimitive)
                        {
                            rightCode = (rightCode.Substring(rightCode.Length - 1, 1) == "\n" ? rightCode.Substring(0, rightCode.Length - 1) : rightCode);
                            rightCode = new StringBuilder(rightCode).Replace(varname + " = ", "").ToString();
                            ret = new StringBuilder(returnAssigment).Replace("$[<code>]", rightCode.Substring(0, rightCode.Length - 1)).ToString();
                        }
                        else
                        {                            
                            ret = DoTabs(tabs) + "var " + varname + " = '';\n";
                            ret += rightCode;
                            ret += new StringBuilder(returnAssigment).Replace("$[<code>]", varname);
                        }

                        return ret;
                    }
                }
                else if(Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                {
                    if (!(assingBlock.SymbolTable.Get(left.TryVariable().Value) is Error) && maybeIs is Properties)
                    {
                        string[] varname = left.TryVariable().Value.Split('.');
                        if (assingBlock.isInConstructor || assingBlock.isType(Block.BlockType.PROPERTIES))
                            varname[0] = "self";
                        return DoTabs(tabs) + addCode + varname[0] + ".Property__" + string.Join(".", varname.Skip(1)) + ".set(" + (rightCompiled == "" ? right.Compile(0) : rightCompiled) + ");";
                    }
                    else if (maybeIs2 != null && !(assingBlock.SymbolTable.Get(right.TryVariable().Value) is Error) && maybeIs2 is Properties)
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

                break;
            }
            return DoTabs(tabs) + new StringBuilder(returnAssigment).Replace("$[<code>]", rightcompile);
        }

        public override void Semantic()
        {
            Semantic(false);
        }       

        public void Semantic(bool isParam = false)
        { 
            var lfv = left.TryVariable().Value;
            var leftFromSymbolTable = assingBlock?.SymbolTable.Get(lfv, assingBlock);
            if(leftFromSymbolTable is Error)
            {
                var parent = assingBlock.SymbolTable.Get(GetAssingTo(Block.BlockType.CLASS));
                if(parent is Class pc)
                {
                    var _parent = pc.GetParent();
                    leftFromSymbolTable = _parent.assingBlock.SymbolTable.Get(lfv);
                }
            }
            if (leftFromSymbolTable is Properties ps)
            {
                if(ps.Setter == null)
                    Interpreter.semanticError.Add(new Error("#800 Propertie " + ((Properties)left).variable.TryVariable().Value + " don't define setter!", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
            }
            if (!(right is Lambda) && assingBlock?.SymbolTable.Get(right.TryVariable().Value) is Properties pg)
            {
                if(pg.Getter == null)
                    Interpreter.semanticError.Add(new Error("#801 Propertie " + ((Properties)left).variable.TryVariable().Value + " don't define getter!", Interpreter.ErrorType.ERROR, right.TryVariable().getToken()));
            }
            if (left is Variable leftVariable)
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
                            Interpreter.semanticError.Add(new Error("#102 Variable " + ((Variable)left).Value + " is generic and '"+ ri.Value + "' can't be converted to '"+ ri.GetDateType().Value + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
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

                            //TODO: maybe check better xD
                            if (left_type != type && left_type != "object")
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
                                var fnd = rblock.SymbolTable.Get(rblock.assignTo);
                                if (!(fnd is Error))
                                {
                                    Function asfunc = (Function)fnd;
                                    Variable var = asfunc.ParameterList.Find(((Variable)right).Value);
                                    Types vaq = (Types)rblock.SymbolTable.Get(((Variable)right).Value);
                                    if (vaq != null)
                                    {
                                        if (vaq is Assign)
                                        {
                                            right = vaq;
                                        }

                                        if (vaq is Error)
                                        {
                                            if (right is Variable rv)
                                            {
                                                var cls = assingBlock.SymbolTable.Get(this.assingBlock.getClass());
                                                if (cls is Class clsc)
                                                {
                                                    var parent = clsc.GetParent();
                                                    vaq = parent.assingBlock.SymbolTable.Get(((Variable) right).Value);
                                                }
                                            }
                                            if(vaq is Error)
                                                Interpreter.semanticError.Add(new Error("#103 Variable " + ((Variable) right).Value + " not exist!", Interpreter.ErrorType.ERROR, ((Variable) right).getToken()));                                            
                                        }
                                    }
                                    else if (var != null)
                                    {
                                        right = var;
                                    }
                                    else
                                        Interpreter.semanticError.Add(new Error("#104 Variable " + ((Variable)right).Value + " not exist!", Interpreter.ErrorType.ERROR, ((Variable)right).getToken()));
                                }
                            }
                            if (right is Variable && ((Variable)left).Type != ((Variable)right).Type && ((Variable)left).Type != ((Variable)right).AsDateType.Value)
                                Interpreter.semanticError.Add(new Error("#105 Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to '" + ((Variable)right).Type + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                        }
                        else
                            Interpreter.semanticError.Add(new Error("#106 Variable " + ((Variable)left).Value + " with type '" + ((Variable)left).Type + "' can't be implicitly converted to '" + ((Variable)right).Type + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                    }
                    else if (right is Number && ((Variable)left).Type != "int" && ((Variable)left).Type != "float")
                        Interpreter.semanticError.Add(new Error("#107 Variable " + ((Variable)left).Value + " can't be implicitly converted to '"+((Variable)left).Type+"' with type '" + ((Variable)left).Type + "'", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
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

                if(leftFromSymbolTable is Error && left is Variable)
                    Interpreter.semanticError.Add(new Error("#10x Variable " + ((Variable)left).Value + " not exist!", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));

                if (!isDeclare && !isParam)
                {
                    if (leftFromSymbolTable is Assign leftFromSymbolTableVar)
                    {
                        if (leftFromSymbolTableVar.left is Variable leftvarass && leftvarass.IsVal)
                        {
                            Interpreter.semanticError.Add(new Error("#1xx Variable " + ((Variable)left).Value + " is declared as val and it can't be reasigned!", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                        }
                    }

                    if (leftFromSymbolTable is Variable leftvarFromSymbolTable)
                    {
                        if (leftvarFromSymbolTable.IsVal)
                        {
                            Interpreter.semanticError.Add(new Error("#1xx Variable " + ((Variable)left).Value + " is declared as val and it can't be reasigned!", Interpreter.ErrorType.ERROR, ((Variable)left).getToken()));
                        }
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

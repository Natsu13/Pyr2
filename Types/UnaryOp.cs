using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{    
    public class UnaryOp:Types
    {        
        Token token, op, name;
        Types expr;
        List<Types> exptList = new List<Types>();
        ParameterList plist;
        Block block;        
        Token arraySizeVariable = null;
        Types arraySizeVariableTypes = null;
        bool isArray = false;
        int arraySize = -1;                      
        bool founded = false;

        public bool post = false;
        public List<string> genericArgments = new List<string>();
        public bool asArgument = false;
        public bool isInString = false;
        public Types assignToParent = null;

        public UnaryOp(Token op, Types expr, Block block = null)
        {
            this.op = this.token = op;
            this.expr = expr;
            this.block = block;
        }

        public UnaryOp(Token op, Token name, ParameterList plist = null, Block block = null, bool endit = false)
        {
            this.op = this.token = op;
            this.name = name;
            this.plist = plist;
            this.block = block;
            this.endit = endit;
        }

        public UnaryOp(Token op, List<Types> exptList, Block block = null)
        {
            this.op = this.token = op;
            this.exptList = exptList;
            this.block = block;
        }
        
        public override Token getToken() { return token; }
        public string Op { get { return Variable.GetOperatorStatic(op.type); } }
        public Types  Expr { get { return expr; } }
        public Token Name { get { return name; } }
        public void MadeArray(int size) { isArray = true; arraySize = size; }
        public void MadeArray(Token name) { isArray = true; arraySizeVariable = name; }
        public void MadeArray(Types name) { isArray = true; arraySizeVariableTypes = name; }
        public Block Block { get { return block; } set { block = value; } }

        public Function usingFunction = null;

        private Types _myt = null;
        private string newname = "";
        private bool isDynamic = false;
        private string generic = "";
        public Function FindUsingFunction()
        {
            string o = Variable.GetOperatorStatic(op.type);
            if (o != "call")
                return null;

            //if (usingFunction != null)
            //    return usingFunction;

            string nwnam = name.Value;
            Types myfunc = null;
            isDynamic = false;
            if (!block.SymbolTable.Find(name.Value, true))
            {
                nwnam = name.Value.Split('.')[0];
                if (name.Value.Split('.')[0] == "this")
                    nwnam = string.Join(".", name.Value.Split('.').Skip(1));
                Types q = block.SymbolTable.Get(nwnam);

                if (q is Class @class && @class.isDynamic) { isDynamic = true; }
                if (q is Interface @interface && @interface.isDynamic) { isDynamic = true; }
                if (q is Error && assignToParent != null && assignToParent is UnaryOp auop)
                {
                    var output = auop.OutputType;
                    var outClass = assingBlock.SymbolTable.Get(output.Value);
                    if (outClass is Class outcl)
                    {
                        var finbd = outcl.Get(nwnam);
                        if (finbd is Function finf)
                        {
                            name = finf.getToken();
                            myfunc = finf;
                        }
                    }
                    //assignToParent.assingBlock.SymbolTable.Get(nwnam);
                }
                if(!isDynamic && (!(q is Function) && !(q is Assign) && myfunc == null))
                    return null;
            }

            generic = "";
            bool fir = true;
            string gname = "";
            if(genericArgments.Count == 0)
            {
                var funcs = (myfunc == null ? block.SymbolTable.GetAll(name.Value) : new List<Types>(){ myfunc });
                if (funcs == null)
                    funcs = block.SymbolTable.GetAll(nwnam);
                if (funcs != null && funcs.Count == 1)
                {
                    var func = funcs[0];
                    if (func != null && func is Function funct)
                    {
                        if (funct.GenericArguments.Count > 0)
                        {
                            if (funct.ParameterList.parameters.Count == plist.parameters.Count || funct.ParameterList.allowMultipel)
                            {
                                int i = 0;
                                foreach (var parameter in funct.ParameterList.parameters)
                                {
                                    var vleft = parameter.TryVariable();
                                    var vright = plist.parameters[i++].TryVariable();     
                                    if(vright.Type == "auto")
                                        vright.Check();
                                    if(vleft.Type == vright.Type)
                                        continue;
                                    if (funct.GenericArguments.Contains(vleft.Type))
                                    {
                                        genericArgments.Add(vright.Type);
                                        if (genericArgments.Count == funct.GenericArguments.Count)
                                            break;
                                    }                                        
                                }
                            }
                        }
                    }
                }
            }
            foreach(string g in genericArgments)
            {
                if (!fir) generic += ", ";
                fir = false;
                Types gf = block.SymbolTable.Get(g);
                if (gf is Class) gname = ((Class)gf).getName();
                if (gf is Interface) gname = ((Interface)gf).getName();
                generic += "'"+ gname + "'";
            }

            if (plist != null && plist.assingToType == null)
            {
                plist.assingToToken = Name;
            }

            List<Types> allf = (myfunc == null ? block.SymbolTable.GetAll(nwnam) : new List<Types>(){ myfunc });
            Types t = null;
            if (allf != null && allf.Count > 1)
            {
                foreach (Types q in allf)
                {
                    ParameterList p = null;
                    if (q is Function)
                        p = ((Function)q).ParameterList;
                    if (q is Lambda)
                        p = ((Lambda)q).ParameterList;
                    
                    if (p.assingBlock == null)
                        p.assingBlock = assingBlock;

                    if (p.Compare(plist))
                    {
                        t = q;
                    }
                }

            }else
                t = allf[0];                
            
            newname = nwnam;
            if (t is Function _f)
            {
                newname = _f.Name;
                usingFunction = _f;
            }

            _myt = t;

            return usingFunction;
        }

        public override string Compile(int tabs = 0)
        {
            if(expr != null)
                expr.assingBlock = assingBlock;
            string tbs = DoTabs(tabs);
            string o = Variable.GetOperatorStatic(op.type);
            Types myfunc = null;
            if(o == "call")
            {
                FindUsingFunction();

                var t = _myt;

                string before = "";
                if(name.Value.Split('.')[0] == "this")
                {
                    if (block != null && block.isInConstructor)
                        before = "$this.";
                    else
                        before = "this.";
                }

                if (t is Assign && ((Assign)t).Right is Lambda)
                {
                    string args = "()";
                    if (plist != null)
                        args = "(" + plist.Compile() + ")";
                    if(asArgument)
                        return tbs + (inParen ? "(" : "") + "lambda$" + name.Value + (inParen ? ")" : "") + (endit ? ";" : "");
                    return tbs + (inParen ? "(" : "") + "lambda$" + name.Value + args + (inParen ? ")" : "") + (endit ? ";" : "");
                }

                if (t is Assign ta && ta.Right is UnaryOp tau)
                {
                    if (tau.Op == "new" && tau.isArray)
                    {
                        var func = name.Value.Split('.')[1];                        
                        var functio = (Function)assingBlock.SymbolTable.Get("Array."+func);
                        return tbs + (inParen ? "(" : "") + before + newname + "." + functio.Name + "(" + plist.Compile(0, usingFunction?.ParameterList) + (plist.Parameters.Count > 0 && generic != "" ?", ":"") + generic + ")" + (inParen ? ")" : "") + (endit ? ";" : "");
                    }
                }
                if (t is Assign && block?.SymbolTable.Get(((Variable)((Assign)t).Left).Type) is Delegate)
                {
                    string args = "()";
                    if (plist != null)
                        args = "(" + plist.Compile() + ")";
                    if(asArgument)
                        return tbs + (inParen ? "(" : "") + "delegate$" + name.Value + (inParen ? ")" : "") + (endit ? ";" : "");
                    if(isInString)
                        return tbs + (inParen ? "(" : "") + "function(){ delegate$" + name.Value + args + "; }" + (inParen ? ")" : "") + (endit ? ";" : "");
                    return tbs + (inParen ? "(" : "") + before + "delegate$" + newname + args + (inParen ? ")" : "") + (endit ? ";" : "");
                }
                if (name.Value == "js")
                {
                    string pl = plist.Compile();
                    return pl.Substring(1, pl.Length - 2).Replace("\\", "");
                }
                if (name.Value.Contains("."))
                {
                    string[] nnaml = name.Value.Split('.');
                    string nname = "";
                    var vario = block?.SymbolTable.Get(string.Join(".", nnaml.Take(nnaml.Length - 1)));
                    if(!(vario is Error) && vario is Properties prop)
                    {
                        var type = block?.SymbolTable.Get(((Variable)prop.variable).Type+"."+nnaml[nnaml.Length - 1]);
                        nname = string.Join(".", nnaml.Take(nnaml.Length - 2)) + ".Property$" + nnaml[nnaml.Length - 2] + ".get()." + ((Function)type)?.Name;
                        founded = true;
                    }
                    else if (isDynamic)
                        nname = name.Value;
                    else
                        nname = string.Join(".", nnaml.Take(nnaml.Length - 1)) + "." + ((Function)t)?.Name;
                    if (nname.Split('.')[0] == "this")
                        nname = string.Join(".", nname.Split('.').Skip(1));

                    if (usingFunction != null && usingFunction.isInline)
                    {
                        int tmpc = usingFunction.inlineId > 0 ? usingFunction.inlineId : (usingFunction.inlineId = Function.inlineIdCounter++);

                        if (assignToParent is Assign astpa)
                        {
                            usingFunction.assigmentInlineVariable = assignToParent;
                        }
                        var ret = "\n";
                        var i = 0;

                        Dictionary<Assign, Types> defaultVal = new Dictionary<Assign, Types>();

                        foreach (var par in usingFunction.ParameterList.Parameters)
                        {
                            if (i >= plist.Parameters.Count)
                            {
                                if (par is Assign para)
                                {                                    
                                    //ret += tbs + "var " + newname + "$" + tmpc + "$" + par.TryVariable().Value + " = " + para.Right.Compile() + ";\n";
                                }
                            }
                            else
                            {
                                //ret += tbs + "var "+newname+"$"+tmpc+"$"+par.TryVariable().Value+" = " + plist.Parameters[i].Compile() + ";\n";
                                if (par is Assign para)
                                {      
                                    defaultVal.Add(para, para.Right);
                                    para.Right = plist.Parameters[i];
                                }
                            }                            
                            i++;
                        }

                        ret += usingFunction.Block.Compile(tabs);

                        foreach (var typese in defaultVal)
                        {
                            typese.Key.Right = typese.Value;
                        }

                        usingFunction.inlineId = 0;
                        usingFunction.assigmentInlineVariable = null;
                        return ret;
                    }

                    if (plist == null)
                        return tbs + (inParen ? "(" : "") + before + nname + "("+generic+")" + (inParen ? ")" : "") + (endit ? ";" : "");
                    if (plist.assingBlock == null)
                        plist.assingBlock = block;
                    return tbs + (inParen ? "(" : "") + before + nname + "(" + plist.Compile(0, usingFunction?.ParameterList) + (plist.Parameters.Count > 0 && generic != ""?", ":"") + generic + ")" + (inParen ? ")" : "") + (endit ? ";" : "");
                }
                else if (usingFunction != null && usingFunction.isInline)
                {
                    int tmpc = usingFunction.inlineId > 0 ? usingFunction.inlineId : (usingFunction.inlineId = Function.inlineIdCounter++);

                    if (assignToParent is Assign astpa)
                    {
                        usingFunction.assigmentInlineVariable = assignToParent;
                    }
                    var ret = "\n";
                    var i = 0;
                    foreach (var par in usingFunction.ParameterList.Parameters)
                    {
                        ret += tbs + "var "+newname+"$"+tmpc+"$"+par.TryVariable().Value+" = " + plist.Parameters[i].Compile() + ";\n";
                        i++;
                    }

                    ret += usingFunction.Block.Compile(tabs);

                    usingFunction.inlineId = 0;
                    usingFunction.assigmentInlineVariable = null;
                    return ret;
                }
                else
                {
                    if(asArgument)
                        return tbs + (inParen ? "(" : "") + before + newname + (inParen ? ")" : "") + (endit ? ";" : "");
                    if (plist == null)
                        return tbs + (inParen ? "(" : "") + before + newname + "(" + generic + ")" + (inParen ? ")" : "") + (endit ? ";" : "");
                    return tbs + (inParen ? "(" : "") + before + newname + "(" + plist.Compile(0, usingFunction?.ParameterList) + (plist.Parameters.Count > 0 && generic != "" ?", ":"") + generic + ")" + (inParen ? ")" : "") + (endit ? ";" : "");
                }
            }
            if (o == "new")
            {
                Types t = assingBlock.SymbolTable.Get(name.Value);
                if (t is Error) return "";
                if (t is Import) {
                    t = ((Import)t).Block.SymbolTable.Get(name.Value);
                    if (t is Error) return "";
                }
                string rt;

                string _name = "";
                if(t is Class)
                {
                    if (((Class)t).JSName != "") _name = ((Class)t).JSName;
                    else _name = ((Class)t).Name.Value;
                }                
                
                if (t is Class && ((Class)t).assingBlock.SymbolTable.Find("constructor " + _name))
                {
                    string generic = "";
                    bool fir = true;
                    string gname = "";
                    foreach(string g in genericArgments)
                    {
                        if (!fir) generic += ", ";
                        fir = false;
                        Types gf = block.SymbolTable.Get(g);
                        if (gf is Class) gname = "'"+((Class)gf).getName()+"'";
                        if (gf is Interface) gname = "'"+((Interface)gf).getName()+"'";
                        if (gf is Generic) gname = "generic$"+g;
                        generic += gname;
                    }
                    Function f = (Function)(((Class)t).assingBlock.SymbolTable.Get("constructor " + _name));
                    if (isArray)
                    {
                        Variable va = null;
                        if (arraySizeVariable != null && block.SymbolTable.Find(arraySizeVariable.Value))
                        {
                            va = (Variable)block.SymbolTable.Get(arraySizeVariable.Value);
                        }

                        string arrayS = "";
                        if (arraySizeVariable != null && va != null && arraySizeVariableTypes == null)
                            arrayS = va.Value;
                        else if (arraySizeVariableTypes != null)
                            arrayS = arraySizeVariableTypes.Compile();
                        else
                            arrayS = arraySize.ToString();

                        rt = tbs + "new Array(" + arrayS + ").fill(" + _name + "." + f.Name + "(" + plist?.Compile();
                        if (plist != null && plist.Parameters.Count > 0)
                            rt += ", ";
                        rt += generic;
                        rt += "))";
                    }
                    else
                    {
                        if (t.assingBlock.Interpret.FindImport(string.Join(".", name.Value.Split('.').Take(name.Value.Split('.').Length - 1))))
                        {
                            Import im = t.assingBlock.Interpret.GetImport(string.Join(".", name.Value.Split('.').Take(name.Value.Split('.').Length - 1)));
                            if(im.As != null)
                                rt = tbs + im.As + "." + _name + "." + f.Name + "(" + plist?.Compile(0, f.ParameterList, plist);
                            else if(string.Join(".", name.Value.Split('.').Take(name.Value.Split('.').Length - 1)) != name.Value)
                                rt = tbs + string.Join(".", name.Value.Split('.').Take(name.Value.Split('.').Length - 1)) + "." + _name + "." + f.Name + "(" + plist?.Compile(0, f.ParameterList, plist);
                            else
                                rt = tbs + _name + "." + f.Name + "(" + plist?.Compile(0, f.ParameterList, plist);
                        }
                        else
                            rt = tbs + _name + "." + f.Name + "(" + plist?.Compile(0, f.ParameterList, plist);
                        if (plist != null && (plist.Parameters.Count > 0 || f.ParameterList.Parameters.Count > 0) && generic != "")
                            rt += ", ";
                        rt += generic;
                        rt += ")";
                    }
                }
                else
                {
                    if(t is Generic)
                    {
                        TypeObject obj = new TypeObject();
                        _name = obj.ClassNameForLanguage();
                        if (block.isInConstructor)
                            _name = "window[$this.generic$"+((Generic)t).Name+"]";
                        else
                            _name = "window[this.generic$" + ((Generic)t).Name + "]";
                    }
                    if (isArray)
                    {
                        string generic = "", gname = "";
                        bool fir = true;
                        foreach(string g in genericArgments)
                        {
                            if (!fir) generic += ", ";
                            fir = false;
                            Types gf = block.SymbolTable.Get(g);
                            if (gf is Class) gname = "'"+((Class)gf).getName()+"'";
                            if (gf is Interface) gname = "'"+((Interface)gf).getName()+"'";
                            if (gf is Generic) gname = (block.isInConstructor?"$this.":"this.")+"generic$"+g;
                            generic += gname;
                        }
                        Variable va = null;
                        if (arraySizeVariable != null && block.SymbolTable.Find(arraySizeVariable.Value))
                        {
                            va = ((Variable)((Assign)block.SymbolTable.Get(arraySizeVariable.Value)).Left);
                        }

                        string arrayS = "";
                        if (arraySizeVariable != null && va != null && arraySizeVariableTypes == null)
                            arrayS = va.Value;
                        else if (arraySizeVariableTypes != null)
                            arrayS = arraySizeVariableTypes.Compile();
                        else
                            arrayS = arraySize.ToString();

                        var tv = t.TryVariable();
                        if (tv.IsPrimitive)
                        {
                            var type = tv.GetDateType().Value;
                            var primitiveFill = "";
                            if(type == "string"){ primitiveFill = "\"\""; }
                            if(type == "int") { primitiveFill = "0"; }
                            if(type == "float") { primitiveFill = "0.0"; }
                            if(type == "bool") { primitiveFill = "false"; }

                            rt = tbs + "new Array(" + arrayS + ").fill(" + primitiveFill + ")";
                        }
                        else
                        {
                            rt = tbs + "new Array(" + arrayS + ").fill(new " + _name + "(" + plist?.Compile();
                            if (plist != null && plist.Parameters.Count > 0) rt += ", ";
                            rt += generic;
                            rt += "))";
                        }
                    }
                    else
                    {
                        rt = "";
                        if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                        {
                            if (t.assingBlock.Interpret.FindImport(name.Value.Split('.').First()))
                            {
                                rt = tbs + name.Value.Split('.').First() + "." + _name + "(" + plist?.Compile() + ")";
                            }
                            else
                                rt = tbs + _name + "(" + plist?.Compile() + ")";
                        }
                        else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                        {
                            if (t.assingBlock.Interpret.FindImport(name.Value.Split('.').First()))
                            {
                                rt = tbs + "new " + name.Value.Split('.').First() + "." + _name + "(" + plist?.Compile() + ")";
                            }
                            else
                                rt = tbs + "new " + _name + "(" + plist?.Compile() + ")";
                        }
                    }
                }
                return (inParen ? "(" : "") + rt + (inParen ? ")" : "");
            }
            if(o == "return")
            {                              
                if (expr != null)
                    expr.endit = false;

                var usingBlock = assingBlock ?? block;
                if (usingBlock != null && (usingBlock = usingBlock.GetBlock(Block.BlockType.FUNCTION, new List<Block.BlockType>{ Block.BlockType.CLASS, Block.BlockType.INTERFACE })) != null)
                {
                    if (usingBlock.SymbolTable.Get(usingBlock.assignTo) is Function ass && ass.isInline && ass.inlineId > 0)
                    {
                        if (ass.assigmentInlineVariable == null)
                        {
                            if (expr != null)
                                return tbs + expr.Compile();
                        }
                        else if(ass.assigmentInlineVariable is Assign assv)
                        {
                            if (expr != null)
                                return tbs + ass.Name + "$" + ass.inlineId + "$return = " + expr.Compile() + ";";
                        }
                    }
                }

                if (expr == null)
                    return tbs + "return;";

                var compiled = expr.Compile();
                if (compiled.Contains("\n"))
                    return compiled;                
                return tbs + "return " + expr.Compile() + ";";
            }

            if (o == "..")
            {
                var fun = block.SymbolTable.Get("Range.range") as Function;
                return fun.assignTo + "." + fun.Name + "(" + exptList[0].Compile() + ", " + exptList[1].Compile() + ")";
            }

            if (post)
                return tbs + (inParen ? "(" : "") + expr.Compile() + Variable.GetOperatorStatic(op.type) + (inParen ? ")" : "") + (endit ? ";" : "");
            else
                return tbs + (inParen ? "(" : "") + Variable.GetOperatorStatic(op.type) + expr.Compile() + (inParen ? ")" : "") + (endit ? ";" : "");
        }

        Token _outputtype = null;
        public Token OutputType
        {
            get
            {                
                string o = Variable.GetOperatorStatic(op.type);
                if (o == "new")
                {
                    return name;
                }
                if (o == "..")
                {
                    return new Token(Token.Type.CLASS, "Range");
                }
                return _outputtype ?? new Token(Token.Type.CLASS, "object");
            }
            set
            {
                _outputtype = value;
            }
        }

        public override void Semantic()
        {
            string o = Variable.GetOperatorStatic(op.type);
            if (o == "call")
            {
                if (founded) return;
                if (asArgument) return;
                Types t = null;               

                if (usingFunction is Function && usingFunction.attributes.Where(qt => ((_Attribute)qt).GetName(true) == "Obsolete").Count() > 0)
                    Interpreter.semanticError.Add(new Error("#704 Function " + name.Value + " is Obsolete", Interpreter.ErrorType.ERROR, name));

                Dictionary<string, Types> genericArgsTypes = new Dictionary<string, Types>();                
                if (usingFunction is Function _f)
                {
                    t = _f;
                    if (_f.assignTo != "")
                    {
                        Types q = null;

                        if (name.Value.Split('.')[0] != "this")
                        {
                            if (block.SymbolTable.Get(name.Value.Split('.')[0]) is Assign asign)
                            {
                                if (asign.Right is UnaryOp)
                                {
                                    Token mytoken = ((UnaryOp)(asign.Right)).Name;
                                    q = block.SymbolTable.Get(mytoken.Value);
                                }
                                else if(asign.Right is Null)
                                {
                                    q = block.SymbolTable.Get(asign.Left.TryVariable().Type);
                                }
                            }
                        }

                        if(q == null)
                            q = block.SymbolTable.Get(_f.assignTo);

                        if (name.Value.Split('.')[0] != "this")
                        {
                            Types x = block.SymbolTable.Get(name.Value.Split('.')[0]);
                            if (q is Class __c && x is Assign)
                            {
                                Assign __a = (Assign)x;
                                for (int i = 0; i < __c.GenericArguments.Count; i++)
                                {
                                    Types xp = block.SymbolTable.Get(((Variable)__a.Left).genericArgs[i]);
                                    genericArgsTypes[__c.GenericArguments[i]] = xp;
                                }
                            }
                            if(q is Interface __i && x is Assign)
                            {
                                Assign __a = (Assign)x;
                                for (int i = 0; i < __i.GenericArguments.Count; i++)
                                {
                                    Types xp = block.SymbolTable.Get(((Variable)__a.Left).genericArgs[i]);
                                    genericArgsTypes[__i.GenericArguments[i]] = xp;
                                }
                            }
                        }
                    }
                }

                if(plist.assingBlock == null)
                    plist.assingBlock = assingBlock;
                plist.Semantic(usingFunction?.ParameterList, usingFunction?.RealName);

                if (block.Parent?.Parent == null)
                    Interpreter.semanticError.Add(new Error("#000 Expecting a top level declaration", Interpreter.ErrorType.ERROR, name));

                string nwnam = name.Value.Split('.')[0];
                if (name.Value.Split('.')[0] == "this")
                    nwnam = string.Join(".", name.Value.Split('.').Skip(1));

                if (block.assingBlock != null && (!block.assingBlock.SymbolTable.Find(name.Value) && !block.assingBlock.SymbolTable.Find(nwnam)))
                {
                    t = block.assingBlock.SymbolTable.Get(nwnam);
                    if (t is Class || t is Interface)
                    {
                        if (t is Class && ((Class)t).isDynamic) { return; }
                        if (t is Interface && ((Interface)t).isDynamic) { return; }
                    }
                    if (t is Error)
                        Interpreter.semanticError.Add(new Error("#707-1 Function with name " + name.Value + " not found", Interpreter.ErrorType.ERROR, name));
                    else if (!block.assingBlock.SymbolTable.Find(name.Value) && t is Assign)
                    {
                        if(name.Value.Contains(".") && !block.assingBlock.SymbolTable.Find(string.Join(".", name.Value.Split('.').Skip(1))))
                            Interpreter.semanticError.Add(new Error("#707-2 Function with name " + name.Value + " not found", Interpreter.ErrorType.ERROR, name));
                        else if(!name.Value.Contains("."))
                            Interpreter.semanticError.Add(new Error("#707-3 Function with name " + name.Value + " not found", Interpreter.ErrorType.ERROR, name));
                    }
                }

                plist.GenericTUsage = genericArgsTypes;

                List<Types> allf = block.SymbolTable.GetAll(name.Value);
                Types tt = null;
                string possible = "";
                foreach(KeyValuePair<string, Types> kvp in genericArgsTypes)
                {
                    if(kvp.Value is Class)
                        possible += "\n\t" +kvp.Key + " as " + ((Class)kvp.Value).Name.Value;
                }
                var plst = plist.GenericTUsage;
                if (allf != null && allf.Count > 1)
                {
                    foreach (Types q in allf)
                    {
                        plist.GenericTUsage = plst;
                        ParameterList p = null;
                        if (q is Function)
                        {
                            int i = 0;
                            foreach (string g in genericArgments)
                            {
                                plist.GenericTUsage.Add(((Function)q).GenericArguments[i], block.SymbolTable.Get(g));
                                i++;
                            }

                            p = ((Function)q).ParameterList;
                            Function qf = ((Function)q);
                            possible += "\n\t" + qf.RealName + "(" + qf.ParameterList.List() + ")";
                        }
                        if (q is Lambda)
                        {
                            p = ((Lambda)q).ParameterList;
                            Lambda ql = (Lambda)q;
                            possible += "\n\t" + ql.RealName + "(" + ql.ParameterList.List() + ")";
                        }

                        if (p.Compare(plist))
                        {
                            tt = q;
                        }
                    }
                    if (tt == null)
                    {
                        Interpreter.semanticError.Add(new Error("#705 Function with name " + name.Value + "("+plist.List()+") has been found but parameters are wrong. Here is possible solutions:" + possible, Interpreter.ErrorType.ERROR, name));
                    }
                }
                else
                {                
                    if(t is Function tf)
                    {                        
                        t = block.SymbolTable.Get(name.Value);
                        if (t is Function)
                        {
                            int i = 0;
                            foreach (string g in genericArgments)
                            {
                                plist.GenericTUsage.Add(((Function)t).GenericArguments[i], block.SymbolTable.Get(g));
                                i++;
                            }
                        }
                        possible = "";
                        foreach (KeyValuePair<string, Types> kvp in genericArgsTypes)
                        {
                            if (kvp.Value is Class)
                                possible = "\n\t" + kvp.Key + " as " + ((Class)kvp.Value).Name.Value;
                        }
                        if (tf.ParameterList.assingBlock == null)
                            tf.ParameterList.assingBlock = tf.Block;
                        if (tf.ParameterList.assingBlock == null)
                            tf.ParameterList.assingBlock = assingBlock;
                        if (!tf.ParameterList.Compare(plist))
                        {
                            ParameterList p = null;
                            
                            if (t is Function)
                            {
                                p = ((Function)t).ParameterList;
                                Function qf = ((Function)t);
                                possible += "\n\t" + qf.RealName + "(" + qf.ParameterList.List() + ")";
                                int i = 0;
                                foreach(Types typs in tf.ParameterList.parameters)
                                {
                                    if (typs is Variable)
                                    {
                                        Types realtype = t.assingBlock.SymbolTable.Get(((Variable)typs).Type, false, false, ((Variable)typs).GenericList.Count);
                                        if (realtype is Delegate)
                                        {
                                            Function usefun = null;
                                            if (plist.parameters[i] is UnaryOp fuop)
                                            {
                                                if (fuop.usingFunction != null)
                                                    usefun = fuop.usingFunction;
                                            }
                                            else if(plist.parameters[i] is Variable fuva)
                                            {
                                                if (assingBlock.SymbolTable.Get(fuva.Value) is Function fuvafu)
                                                    usefun = fuvafu;
                                            }
                                            int state = ((Delegate)realtype).CompareTo((Variable)typs, usefun, plist);
                                            if (state > 0)
                                                possible += "\n\t\tDelegate problems: " + ((Delegate)realtype).GetError(state);
                                        }
                                    }
                                    i++;
                                }
                            }
                            if (t is Lambda)
                            {
                                p = ((Lambda)t).ParameterList;
                                Lambda ql = (Lambda)t;
                                possible += "\n\t" + ql.RealName + "(" + ql.ParameterList.List() + ")";
                            }
                            Interpreter.semanticError.Add(new Error("#706 Function with name " + name.Value + "("+plist.List()+") has been found but parameters are wrong. Here is possible solution:" + possible, Interpreter.ErrorType.ERROR, name));
                        }
                    }
                }
                //if (plist != null && !asArgument) plist.Semantic();
            }
            if(o == "new")
            {
                if (plist != null) plist.Semantic();
                if (!assingBlock.SymbolTable.Find(name.Value))
                {
                    Interpreter.semanticError.Add(new Error("#603 Class '" + name.Value + "' not found", Interpreter.ErrorType.ERROR, name));
                }
                else
                {
                    Types t = assingBlock.SymbolTable.Get(name.Value);
                    if (t is Class && ((Class)t).GenericArguments.Count > 0)
                    {
                        if(((Class)t).GenericArguments.Count != genericArgments.Count)
                        {
                            Interpreter.semanticError.Add(new Error("#200 You must specify all generic types when creating instance of class '" + name.Value + "'", Interpreter.ErrorType.ERROR, name));
                        }
                    }
                    else if(t is Class && ((Class)t).GenericArguments.Count == 0)
                    {
                        if(genericArgments.Count > 0)
                            Interpreter.semanticError.Add(new Error("#2xx Class '" + name.Value + "' is not Generic!", Interpreter.ErrorType.ERROR, name));
                    }
                    if (t is Class && ((Class)t).attributes.Where(qt => ((_Attribute)qt).GetName(true) == "Obsolete").Count() > 0)
                        Interpreter.semanticError.Add(new Error("#708 Class " + name.Value + " is Obsolete", Interpreter.ErrorType.ERROR, name));
                }
            }
            if (o == "return")
            {
                if (expr != null)
                    expr.Semantic();
            }
        }

        public override string ToString()
        {
            return "UnaryOp("+Name+", Output: "+OutputType+", Expr: "+Expr+", Op: "+Op+")";
        }

        public override int Visit()
        {
            if (op.type == Token.Type.PLUS)
                return +expr.Visit();
            else if (op.type == Token.Type.MINUS)
                return -expr.Visit();
            return 0;
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

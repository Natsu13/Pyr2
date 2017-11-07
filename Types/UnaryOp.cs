using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class UnaryOp:Types
    {
        Token token, op, name;
        Types expr;
        ParameterList plist;
        Block block;
        public bool post = false;
        public List<string> genericArgments = new List<string>();
        bool isArray = false;
        int arraySize = -1;
        public bool asArgument = false;
        Token arraySizeVariable = null;
        Types arraySizeVariableTypes = null;

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
        
        public override Token getToken() { return token; }
        public String Op { get { return Variable.GetOperatorStatic(op.type); } }
        public Types  Expr { get { return expr; } }
        public Token Name { get { return name; } }
        public void MadeArray(int size) { isArray = true; arraySize = size; }
        public void MadeArray(Token name) { isArray = true; arraySizeVariable = name; }
        public void MadeArray(Types name) { isArray = true; arraySizeVariableTypes = name; }

        Function usingFunction = null;

        public override string Compile(int tabs = 0)
        {
            if(expr != null)
                expr.assingBlock = assingBlock;
            string tbs = DoTabs(tabs);
            string o = Variable.GetOperatorStatic(op.type);
            if(o == "call")
            {
                string nwnam = name.Value;
                bool isDynamic = false;
                if (!block.SymbolTable.Find(name.Value))
                {
                    nwnam = name.Value.Split('.')[0];
                    if (name.Value.Split('.')[0] == "this")
                        nwnam = string.Join(".", name.Value.Split('.').Skip(1));
                    Types q = block.SymbolTable.Get(nwnam);

                    if (q is Class && ((Class)q).isDynamic) { isDynamic = true; }
                    if (q is Interface && ((Interface)q).isDynamic) { isDynamic = true; }
                    if(!isDynamic && !(q is Function))
                        return "";
                }
                List<Types> allf = block.SymbolTable.GetAll(nwnam);
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

                        if (p.Compare(plist))
                        {
                            t = q;
                        }
                    }

                }else
                    t = block.SymbolTable.Get(nwnam);
                
                string newname = nwnam;
                if (t is Function _f)
                {
                    newname = _f.Name;
                    usingFunction = _f;
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
                if (name.Value == "js")
                {
                    string pl = plist.Compile();
                    return pl.Substring(1, pl.Length - 2).Replace("\\", "") + (pl.Substring(pl.Length - 2, 1) != ";"?"":"");
                }
                if (name.Value.Contains("."))
                {
                    string[] nnaml = name.Value.Split('.');
                    string nname = "";
                    if (isDynamic)
                        nname = name.Value;
                    else
                        nname = string.Join(".", nnaml.Take(nnaml.Length - 1)) + "." + ((Function)t)?.Name;
                    if (plist == null)
                        return tbs + (inParen ? "(" : "") + nname + "()" + (inParen ? ")" : "") + (endit ? ";" : "");
                    return tbs + (inParen ? "(" : "") + nname + "(" + plist.Compile() + ")" + (inParen ? ")" : "") + (endit ? ";" : "");
                }
                else
                {
                    if(asArgument)
                        return tbs + (inParen ? "(" : "") + newname + (inParen ? ")" : "") + (endit ? ";" : "");
                    if (plist == null)
                        return tbs + (inParen ? "(" : "") + newname + "()" + (inParen ? ")" : "") + (endit ? ";" : "");
                    return tbs + (inParen ? "(" : "") + newname + "(" + plist.Compile() + ")" + (inParen ? ")" : "") + (endit ? ";" : "");
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
                        if (gf is Class) gname = ((Class)gf).getName();
                        if (gf is Interface) gname = ((Interface)gf).getName();
                        generic += "'"+ gname + "'";
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
                        if (t.assingBlock.Interpret.FindImport(name.Value.Split('.').First()))
                        {
                            Import im = t.assingBlock.Interpret.GetImport(name.Value.Split('.').First());
                            if(im.As != null)
                                rt = tbs + im.As + "." + _name + "." + f.Name + "(" + plist?.Compile();
                            else if(name.Value.Split('.').First() != name.Value)
                                rt = tbs + name.Value.Split('.').First() + "." + _name + "." + f.Name + "(" + plist?.Compile();
                            else
                                rt = tbs + _name + "." + f.Name + "(" + plist?.Compile();
                        }
                        else
                            rt = tbs + _name + "." + f.Name + "(" + plist?.Compile();
                        if (plist != null && plist.Parameters.Count > 0 && generic != "")
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

                        rt = tbs + "new Array(" + arrayS + ").fill(new " + _name + "(" + plist?.Compile() + "))";
                    }
                    else
                    {
                        if (t.assingBlock.Interpret.FindImport(name.Value.Split('.').First()))
                        {
                            rt = tbs + "new " + name.Value.Split('.').First() + "." + _name + "(" + plist?.Compile() + ")";

                        }
                        else
                            rt = tbs + "new " + _name + "(" + plist?.Compile() + ")";
                    }
                }
                return (inParen ? "(" : "") + rt + (inParen ? ")" : "");
            }
            if(o == "return")
            {
                if (expr == null)
                    return tbs + "return;";
                expr.endit = false;

                return tbs + "return " + expr.Compile() + ";";
            }
            if (post)
                return tbs + (inParen ? "(" : "") + expr.Compile() + Variable.GetOperatorStatic(op.type) + (inParen ? ")" : "") + (endit ? ";" : "");
            else
                return tbs + (inParen ? "(" : "") + Variable.GetOperatorStatic(op.type) + expr.Compile() + (inParen ? ")" : "") + (endit ? ";" : "");
        }

        public Token OutputType
        {
            get
            {
                string o = Variable.GetOperatorStatic(op.type);
                if (o == "new")
                {
                    return name;
                }
                return new Token(Token.Type.CLASS, "object");
            }
        }

        public override void Semantic()
        {
            string o = Variable.GetOperatorStatic(op.type);
            if (o == "call")
            {
                if (asArgument) return;
                Types t = null;               

                if (usingFunction is Function && usingFunction.attributes.Where(qt => ((_Attribute)qt).GetName(true) == "Obsolete").Count() > 0)
                    Interpreter.semanticError.Add(new Error("#704 Function " + name.Value + " is Obsolete", Interpreter.ErrorType.ERROR, name));

                Dictionary<string, Types> genericArgsTypes = new Dictionary<string, Types>();                
                if (usingFunction is Function _f)
                {
                    if (_f.assignTo != "")
                    {
                        Types q = block.SymbolTable.Get(_f.assignTo);
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
                        }
                    }
                }

                plist.Semantic();

                if (block.Parent?.Parent == null)
                    Interpreter.semanticError.Add(new Error("#000 Expecting a top level declaration", Interpreter.ErrorType.ERROR, name));
                if (block.assingBlock != null && !block.assingBlock.SymbolTable.Find(name.Value))
                {
                    string nwnam = name.Value.Split('.')[0];
                    if (name.Value.Split('.')[0] == "this")
                        nwnam = string.Join(".", name.Value.Split('.').Skip(1));
                    t = block.assingBlock.SymbolTable.Get(nwnam);
                    if (t is Class || t is Interface)
                    {
                        if (t is Class && ((Class)t).isDynamic) { return; }
                        if (t is Interface && ((Interface)t).isDynamic) { return; }
                    }
                    if(t is Error)
                        Interpreter.semanticError.Add(new Error("#707 Function with name " + name.Value + " not found", Interpreter.ErrorType.ERROR, name));
                }

                plist.GenericTUsage = genericArgsTypes;

                List<Types> allf = block.SymbolTable.GetAll(name.Value);
                Types tt = null;
                string possible = "";
                foreach(KeyValuePair<string, Types> kvp in genericArgsTypes)
                {
                    if(kvp.Value is Class)
                        possible = "\n\t" +kvp.Key + " as " + ((Class)kvp.Value).Name.Value;
                }
                if (allf != null && allf.Count > 1)
                {
                    foreach (Types q in allf)
                    {
                        ParameterList p = null;
                        if (q is Function)
                        {
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
                        Interpreter.semanticError.Add(new Error("#705 Function with name " + name.Value + "("+plist.List()+") has been found but parameters is wrong. Here is possible solutions:" + possible, Interpreter.ErrorType.ERROR, name));
                    }
                }
                else
                {
                    t = block.SymbolTable.Get(name.Value);
                    if(t is Function tf)
                    {
                        possible = "";
                        foreach (KeyValuePair<string, Types> kvp in genericArgsTypes)
                        {
                            if (kvp.Value is Class)
                                possible = "\n\t" + kvp.Key + " as " + ((Class)kvp.Value).Name.Value;
                        }
                        if (!tf.ParameterList.Compare(plist))
                        {
                            ParameterList p = null;
                            
                            if (t is Function)
                            {
                                p = ((Function)t).ParameterList;
                                Function qf = ((Function)t);
                                possible += "\n\t" + qf.RealName + "(" + qf.ParameterList.List() + ")";
                            }
                            if (t is Lambda)
                            {
                                p = ((Lambda)t).ParameterList;
                                Lambda ql = (Lambda)t;
                                possible += "\n\t" + ql.RealName + "(" + ql.ParameterList.List() + ")";
                            }
                            Interpreter.semanticError.Add(new Error("#706 Function with name " + name.Value + "("+plist.List()+") has been found but parameters is wrong. Here is possible solution:" + possible, Interpreter.ErrorType.ERROR, name));
                        }
                    }
                }
                if (plist != null && !asArgument) plist.Semantic();
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

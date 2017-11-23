using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class ParameterList : Types
    {
        public List<Types> parameters = new List<Types>();
        bool declare = false;
        public bool cantdefault = false;
        public Token token;
        public bool allowMultipel = false;
        public Token allowMultipelName = null;
        public bool cantDefaultThenNormal = false;
        Dictionary<string, Types> genericTusage = new Dictionary<string, Types>();
        public Dictionary<string, Types> defaultCustom = new Dictionary<string, Types>();

        public ParameterList(bool declare)
        {
            this.declare = declare;
        }

        public Dictionary<string, Types> GenericTUsage
        {
            get { return genericTusage; }
            set { genericTusage = value; }
        }

        public override Token getToken() { return token; }

        public Variable Find(string name)
        {
            foreach(Types par in parameters)
            {
                Variable va = null;
                if (par is Assign)
                    va = (Variable)(((Assign)par).Left);
                else if (par is Variable)
                    va = (Variable)par;
                else if (par is Lambda)
                    va = ((Lambda)par).TryVariable();
                if (va.Value == name)
                    return va;
            }
            return null;
        }

        public string List()
        {
            string ret = "";
            foreach (Types par in parameters)
            {
                if (ret != "") ret += ", ";
                if (par is Variable)
                    ret += ((Variable)par).Type + " " + ((Variable)par).Value;
                else if (par is Assign ap)
                    ret += ap.GetType() + " " + ap.Left.TryVariable().Value + " = " + ap.Right.TryVariable().Value;
                else {
                    Variable v = par.TryVariable();
                    ret += v.Type + " " + v.Value;
                }
            }
            return ret;
        }

        public List<string> ToList()
        {
            List<string> list = new List<string>();
            foreach (var p in parameters)
            {
                if(p is Assign pa)
                {
                    list.Add(pa.Left.TryVariable().Value);
                }
                else if(p is Variable pv)
                {
                    list.Add(pv.Value);
                }
                else if(p is Lambda pl)
                {
                    list.Add(pl.RealName);
                }
            }
            return list;
        }

        public override string Compile(int tabs = 0)
        {
            return Compile(tabs, null);
        }

        public string Compile(int tabs = 0, ParameterList plist = null, ParameterList myList = null)
        {
            string ret = "";
            Dictionary<string, bool> argDefined = new Dictionary<string, bool>();
            List<string> argNamed = plist?.ToList();
            int i = 0;
            bool startne = false;
            if(allowMultipel && plist == null && assingBlock != null && !assingBlock.variables.ContainsKey(allowMultipelName.Value))
            {
                new Assign(new Variable(new Token(Token.Type.ID, allowMultipelName.Value), assingBlock) { isArray = true }, new Token(Token.Type.ASIGN, '='), new Null());
            }
            foreach (Types par in parameters)
            {
                if(argNamed != null && i >= argNamed.Count && !startne)
                {
                    ret += "[";
                    startne = true;
                }
                if(argNamed != null && argNamed.Count > i)
                    argDefined[argNamed[i]] = true;

                par.endit = false;
                if (par is Variable && assingBlock != null && !assingBlock.variables.ContainsKey(((Variable)par).Value))
                {
                    assingBlock.variables.Add(((Variable)par).Value, new Assign(((Variable)par), new Token(Token.Type.ASIGN, '='), new Null()));                    
                }
                else if(par is Assign && assingBlock != null && !assingBlock.variables.ContainsKey(((Assign)par).Left.TryVariable().Value))
                {
                    assingBlock.variables.Add(((Assign)par).Left.TryVariable().Value, (Assign)par);
                }
                if (ret != "" && ret != "[") ret += ", ";
                if (declare)
                {
                    if (par is Assign)
                        ret += ((Assign)par).Left.Compile();
                    else
                        ret += par.Compile(0);
                }
                else ret += par.Compile(0);
                i++;
            }
            if (startne)
            {                
                ret += "]";
            }
            if (myList != null)
            {
                foreach (Types par in plist.Parameters)
                {
                    if (par is Assign para)
                    {
                        if (!argDefined.ContainsKey(para.Left.TryVariable().Value))
                        {
                            ret += (ret != "" ? ", " : "") + "undefined";
                        }
                    }
                    else if(par is Variable parv)
                    {
                        if (!argDefined.ContainsKey(parv.Value))
                        {
                            ret += (ret != "" ? ", " : "") + "undefined";
                        }
                    }
                }
            }
            if(defaultCustom.Count != 0 && plist != null)
            {
                i = 0;
                foreach(var p in plist.parameters)
                {
                    i++;
                    bool found = false;
                    if (i-1 < parameters.Count)
                        continue;                    
                    if (p is Assign pa)
                    {
                        foreach (var q in defaultCustom)
                        {
                            if (pa.Left.TryVariable().Value == q.Key)
                            {
                                ret += ", " + q.Value.Compile();
                                found = true;
                            }
                        }
                    }
                    if (!found)
                    {
                        ret += (ret != "" ? ", " : "") + "undefined";
                    }
                }
            }
            if(allowMultipel && myList == null)
            {
                ret += (ret != "" ? ", " : "") + allowMultipelName.Value;
            }
            assingBlock = null;
            return ret;
        }

        public List<Types> Parameters { get { return parameters; } }

        public bool Compare(ParameterList p)
        {
            if (p == null && !allowMultipel) return false;
            int i = 0;
            bool haveDefault = false;
            foreach(Types t in parameters)
            {
                bool def = false;
                bool isGeneric = false;
                string dtype = null;
                if (t is Variable)
                {
                    dtype = ((Variable)t).Type;
                    if (((Variable)t).Block.SymbolTable.Get(dtype) is Generic)
                    {
                        isGeneric = true;
                        if (p.genericTusage.ContainsKey(dtype) && p.genericTusage[dtype] is Class __c)
                        {
                            dtype = __c.Name.Value;
                            isGeneric = false;
                        }
                    }
                    else if (assingBlock?.SymbolTable.Get(dtype) is Generic)
                    {
                        isGeneric = true;
                        if (p.genericTusage.ContainsKey(dtype) && p.genericTusage[dtype] is Class __c)
                        {
                            dtype = __c.Name.Value;
                            isGeneric = false;
                        }
                    }
                    if (i < p.parameters.Count && p.parameters[i] is Variable && ((Variable)p.parameters[i]).Block.SymbolTable.Get(p.parameters[i].TryVariable().Type) is Generic)
                        isGeneric = true;
                    if (i < p.parameters.Count && p.parameters[i] is Variable && p.parameters[i].TryVariable().Type == "object")
                        isGeneric = true; // Actualy is a object xD
                }
                if(t is Assign)
                {
                    dtype = ((Variable)((Assign)t).Left).Type;
                    def = true;
                }
                if (t is Lambda)
                    dtype = "lambda";
                if (i >= p.parameters.Count && !allowMultipel && !def)
                    return false;
                if (i >= p.parameters.Count && def)
                {
                    haveDefault = true;
                    break;
                }
                if (p.parameters[i] is Variable)
                {
                    ((Variable)p.parameters[i]).Check();
                }
                if (!def && dtype != p.parameters[i].TryVariable().Type && !isGeneric)
                    return false;                
                else if (def)
                {
                    haveDefault = true;
                    if (i < p.parameters.Count)
                    {
                        if (dtype != p.parameters[i].TryVariable().Type)
                            return false;
                    }
                    else
                    {
                        break;
                    }
                }
                i++;
            }
            if(parameters.Count != p.Parameters.Count)
            {
                if (!allowMultipel && !haveDefault)
                    return false;
            }
            return true;
        }

        public bool Equal(ParameterList b)
        {
            if (this is null && b is null) return true;
            if (b is null) return false;
            if (this.parameters.Count != b.parameters.Count)
                return false;
            int index = 0;
            foreach (Types t in this.parameters)
            {
                Variable v1 = (Variable)t;
                Variable v2 = (Variable)b.parameters[index];
                if (v1.getDateType().Value != v2.getDateType().Value)
                    return false;
                index++;
            }
            return true;
        }

        static public bool operator ==(ParameterList a, ParameterList b)
        {
            if (a is null && b is null) return true;
            if (a is null) return false;
            return a.Equal(b);
        }
        static public bool operator !=(ParameterList a, ParameterList b)
        {
            if (a is null && !(b is null)) return true;
            if (a is null) return false;
            return !a.Equal(b);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return List().GetHashCode();
        }

        public override void Semantic()
        {
            Semantic(null, "");
        }
        public void Semantic(ParameterList plist = null, string fname = "")
        {
            if(defaultCustom.Count != 0 && plist != null)
            {
                foreach (var q in defaultCustom)
                {
                    bool found = false;
                    foreach (var p in plist.parameters)
                    {
                        if (p is Assign pa)
                            if(pa.Left.TryVariable().Value == q.Key)
                                found = true;
                    }
                    if(!found)
                    {
                        Interpreter.semanticError.Add(new Error("#1xx Parameter "+q.Key+" not found in function "+fname, Interpreter.ErrorType.ERROR, token));
                    }
                }
            }
            if(cantDefaultThenNormal)
                Interpreter.semanticError.Add(new Error("#1xx When you define default you can't put normal", Interpreter.ErrorType.ERROR, token));
            if (cantdefault)
                Interpreter.semanticError.Add(new Error("#113 Optional parameters must follow all required parameters", Interpreter.ErrorType.ERROR, token));
            foreach (Types par in parameters)
            {
                if (par.assingBlock == null)
                    par.assingBlock = assingBlock;
                par.Semantic();
            }
        }

        public override int Visit()
        {
            return 0;
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

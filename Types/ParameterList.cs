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

        public ParameterList(bool declare)
        {
            this.declare = declare;
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

        public override string Compile(int tabs = 0)
        {
            string ret = "";            
            foreach(Types par in parameters)
            {
                par.endit = false;
                if (par is Variable && assingBlock != null)
                {
                    assingBlock.variables.Add(((Variable)par).Value, new Assign(((Variable)par), new Token(Token.Type.ASIGN, '='), new Null()));                    
                }
                else if(par is Assign && assingBlock != null)
                {
                    assingBlock.variables.Add(((Assign)par).Left.TryVariable().Value, (Assign)par);
                }
                if (ret != "") ret += ", ";
                if (declare)
                {
                    if (par is Assign)
                        ret += ((Assign)par).Left.Compile();
                    else
                        ret += par.Compile(0);
                }
                else ret += par.Compile(0);                
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
                        isGeneric = true;
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
            if (cantdefault)
            {
                Interpreter.semanticError.Add(new Error("Optional parameters must follow all required parameters", Interpreter.ErrorType.ERROR, token));
            }
            foreach (Types par in parameters)
            {
                par.Semantic();
            }
        }

        public override int Visit()
        {
            return 0;
        }
    }
}

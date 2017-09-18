﻿using System;
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
                Variable va = (Variable)par;
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
            if (a is null && !(b is null)) return false;
            if (a is null) return true;
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

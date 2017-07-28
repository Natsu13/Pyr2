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

        public ParameterList(bool declare)
        {
            this.declare = declare;
        }

        public override Token getToken() { return null; }

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
                if (ret != "") ret += ", ";                
                ret += par.Compile(0);                
            }
            assingBlock = null;
            return ret;
        }

        public override void Semantic()
        {
            
        }

        public override int Visit()
        {
            return 0;
        }
    }
}

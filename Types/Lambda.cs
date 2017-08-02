using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class Lambda : Types
    {
        Variable name;
        Types expresion;
        ParameterList plist;
        //bool isDeclare = false;

        public Lambda(Variable name, Types expresion, ParameterList plist)
        {
            this.name = name;
            this.expresion = expresion;
            this.plist = plist;

            if (!(name).Block.variables.ContainsKey(name.Value))
            {
                //isDeclare = true;
                (name).Block.variables[name.Value] = new Assign(name, new Token(Token.Type.LAMBDA, "lambda"), this);
            }
        }

        public override string Compile(int tabs = 0)
        {            
            return DoTabs(tabs) + "var lambda$" + name.Value + " = function("+plist.Compile()+"){ return " + expresion.Compile() + "; };";
        }

        public override Token getToken()
        {
            return name.getToken();
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

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
        public bool isInArgumentList = false;
        public bool isCallInArgument = false;
        //bool isDeclare = false;

        public Lambda(Variable name, Types expresion, ParameterList plist)
        {
            this.name = name;
            this.expresion = expresion;
            this.plist = plist;
            if (name != null)
            {
                if (!(name).Block.variables.ContainsKey(name.Value))
                {
                    //isDeclare = true;
                    (name).Block.variables[name.Value] = new Assign(name, new Token(Token.Type.LAMBDA, "lambda"), this);
                }
            }
        }

        public ParameterList ParameterList { get { return plist; } }

        public override string Compile(int tabs = 0)
        {            
            if(isInArgumentList)
                return "lambda$" + name.Value;
            if (isCallInArgument)
            {
                return "function("+plist.Compile()+"){ return "+ expresion.Compile() + "; }";
            }
            return DoTabs(tabs) + "var lambda$" + name.Value + " = function("+plist.Compile()+"){ return " + expresion.Compile() + "; };";
        }

        public string RealName { get { return getToken()?.Value; } }

        public override Token getToken()
        {
            return name?.getToken();
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

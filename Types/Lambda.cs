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
        public bool isNormalLambda = false;
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

        public Lambda(ParameterList plist, Types block)
        {
            this.plist = plist;
            this.expresion = block;
            isNormalLambda = true;
        }

        public ParameterList ParameterList { get { return plist; } }

        public override string Compile(int tabs = 0)
        {
            if (isNormalLambda)
            {
                string tbs = DoTabs(tabs);
                string ret = "";
                ret += "function(" + plist.Compile() + ")";
                if (expresion is Block)
                {
                    ret += "{";
                    ret += "\n" + expresion.Compile(tabs + 2);
                    ret +=  tbs + "  }";
                }
                else
                    ret += "{ return " + expresion.Compile() + "; }";
                return ret;
            }
            else
            {
                if (isInArgumentList)
                    return "lambda$" + name.Value;
                if (isCallInArgument)
                {
                    return "function(" + plist.Compile() + "){ return " + expresion.Compile() + "; }";
                }
                return DoTabs(tabs) + "var lambda$" + name.Value + " = function(" + plist.Compile() + "){ return " + expresion.Compile() + "; };";
            }
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

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

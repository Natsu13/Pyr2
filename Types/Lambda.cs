using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    class Lambda : Types
    {
        public Variable name;
        public Types expresion;
        public ParameterList plist;
        public Types predicate = null;
        public bool isInArgumentList = false;
        public bool isCallInArgument = false;
        public bool isNormalLambda = false;
        public string replaceThis = null;
        //bool isDeclare = false;

        /*Serialization to JSON object for export*/
        [JsonParam] public Variable Variable => name;
        [JsonParam] public Types Exrpesion => expresion;
        [JsonParam] public ParameterList ParameterList => plist;
        [JsonParam] public bool IsInArgumentList => isInArgumentList;
        [JsonParam] public bool IsCallInArgument => isCallInArgument;
        [JsonParam] public bool IsNormalLambda => isNormalLambda;

        public override void FromJson(JObject o)
        {
            name = JsonParam.FromJson<Variable>(o["Variable"]);
            expresion = JsonParam.FromJson<Types>(o["Exrpesion"]);
            plist = JsonParam.FromJson<ParameterList>(o["ParameterList"]);
            isInArgumentList = (bool) o["IsInArgumentList"];
            isCallInArgument = (bool) o["IsCallInArgument"];
            isNormalLambda = (bool) o["IsNormalLambda"];
        }
        public Lambda() { }

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

        public override string Compile(int tabs = 0)
        {
            if (isNormalLambda)
            {
                string tbs = DoTabs(tabs);
                string ret = "";
                if (plist.assingToType == null)
                    plist.assingToType = predicate;
                if (plist.assingBlock == null)
                    plist.assingBlock = assingBlock;
                if (plist.assingToToken == null)
                    plist.assingToToken = assingToToken;
                ret += "function(" + plist.Compile() + ")";                
                expresion.assingToType = this;                               

                if (expresion is Block block)
                {
                    foreach (var v in plist.Parameters)
                    {
                        if (v is Variable va)
                        {
                            va.setType(new Token(Token.Type.CLASS, "object"));
                            block.SymbolTable.Add(va.Value, va);
                        }
                    }
                    ret += "{";
                    ret += "\n" + expresion.Compile(tabs + 2);
                    ret +=  tbs + "}";
                }
                else
                {
                    if (replaceThis != null && expresion is UnaryOp uoe)
                    {
                        uoe.replaceThis = replaceThis;
                    }
                    var res = expresion.Compile();
                    ret += "{ return " + res + (res[res.Length - 1] == ';' ? "" : ";") + " }";
                }

                return ret;
            }
            else
            {
                foreach (var v in plist.Parameters)
                    if (v is Variable va)
                        va.setType(new Token(Token.Type.CLASS, "object"));

                if (isInArgumentList)
                    return "lambda$" + name.Value;
                if (isCallInArgument)
                {
                    return "function(" + plist.Compile() + "){ return " + expresion.Compile() + "; }";
                }
                if (name.Value.Contains("."))
                    return DoTabs(tabs) + "var " + string.Join(".", name.Value.Split('.').Take(name.Value.Split('.').Length - 1)) + ".lambda$" + name.Value.Split('.').Skip(name.Value.Split('.').Length - 1) + " = function(" + plist.Compile() + "){ return " + expresion.Compile() + "; };";
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

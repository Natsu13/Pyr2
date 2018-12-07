using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class Interface:Types
    {
        Token name;
        Block block;
        List<Token> parents;
        public bool isExternal = false;
        public Token _external;
        public bool isDynamic = false;
        public Token _dynamic;
        public string JSName = "";
        public List<_Attribute> attributes;
        List<string> genericArguments = new List<string>();

        /*Serialization to JSON object for export*/
        [JsonParam] public Token Name => name;
        [JsonParam] public Block Block => block;
        [JsonParam] public List<string> GenericArguments => genericArguments;
        [JsonParam] public List<Token> Parens => parents;
        [JsonParam] public List<_Attribute> Attributes => attributes;   
        [JsonParam] public bool IsDynamic => isDynamic;
        [JsonParam] public bool IsExternal => isExternal;
        
        public override void FromJson(JObject o)
        {
            throw new NotImplementedException();
        }
        public Interface() { }

        public Interface(Token name, Block block, List<Token> parents)
        {
            this.name = name;
            this.block = block;
            if (block != null)
            {
                this.block.blockAssignTo = name.Value;
                this.block.blockClassTo = name.Value;
                this.block.assingToType = this;
            }
            this.assingBlock = block;
            this.parents = parents;
        }

        public void AddGenericArg(string name)
        {
            genericArguments.Add(name);
        }
        public void SetGenericArgs(List<string> list)
        {
            genericArguments = list;
        }
        
        public override string Compile(int tabs = 0)
        {
            if (!isExternal)
            {
                string tbs = DoTabs(tabs);
                string ret = tbs + "var " + getName() + " = function(){";
                if (block.variables.Count != 0 || parents.Count != 0) ret += "\n";
                foreach (Token parent in parents)
                {
                    ret += tbs + "  " + parent.Value + ".call(this);\n";
                }
                foreach (KeyValuePair<string, Assign> var in block.variables)
                {
                    //var.Key + " => ["+var.Value.GetType()+"] " + var.Value.GetVal()
                    if(var.Value.Right is Null)
                        ret += tbs + "  this." + var.Key + " = '';\n";
                    else if (var.Value.GetType() == "string")
                        ret += tbs + "  this." + var.Key + " = " + var.Value.Compile() + ";\n";
                    else
                        ret += tbs + "  this." + var.Key + " = " + var.Value.GetVal() + ";\n";
                }
                ret += tbs + "}\n";

                if (Interpreter._DEBUG)
                {
                    ret += tbs + "var " + getName() + "$META = function(){\n";
                    ret += tbs + "  return {";
                    ret += "\n" + tbs + "    type: 'interface'" + (attributes.Count > 0 ? ", " : "");
                    if (attributes.Count > 0)
                    {
                        ret += "\n" + tbs + "    attributes: {";
                        int i = 0;
                        foreach (_Attribute a in attributes)
                        {
                            ret += "\n" + tbs + "      " + a.GetName() + ": " + a.Compile() + ((attributes.Count - 1) == i ? "" : ", ");
                            i++;
                        }

                        ret += "\n" + tbs + "    },";
                    }

                    ret += "\n" + tbs + "  };\n";
                    ret += tbs + "};\n";
                }

                ret += block.Compile(tabs);
                return ret;
            }
            return "";
        }

        string _hash = "";
        public string getHash()
        {
            if (assingBlock == null) assingBlock = block;
            if (_hash == "")
                _hash = string.Format("{0:X8}", (name.Value + genericArguments.GetHashCode() + block?.GetHashCode()).GetHashCode());
            return _hash;
        }

        public string getName() {
            if (assingBlock.SymbolTable.GetAll(name.Value).Count > 1)
                return name.Value + "_" + getHash();
            if (JSName == null || JSName == "") return name.Value; else return JSName; 
        }        
        public override Token getToken() { return new Token(Token.Type.STRING, "interface"); }

        public bool haveParent(string name)
        {
            if (name == this.name.Value)
                return true;
            if (parents == null) 
                return false;
            foreach (Token p in parents)
            {
                if (p.Value == name) return true;
                Types to = block.SymbolTable.Get(p.Value);
                if (to is Class && ((Class)to).haveParent("IIterable"))
                    return true;
                if (to is Interface && ((Interface)to).haveParent("IIterable"))
                    return true;
            }
            return false;
        }

        public override void Semantic()
        {
            foreach (KeyValuePair<string, Assign> var in block?.variables)
            {
                var.Value.Semantic();
            }
            block?.Semantic();
        }

        public override int Visit()
        {
            return 0;
        }

        public Token OutputType(string op, object a, object b)
        {
            var t = block.SymbolTable.Get("operator " + op);
            if (t is Function f)
            {                
                return f.Returnt;
            }
            return new Token(Token.Type.VOID, "void");
        }
        public bool SupportOp(string op)
        {
            var t = block.SymbolTable.Get("operator " + op);
            return t is Function;
        }
        public bool SupportSecond(string op, object second, object secondAsVariable)
        {
            var t = block.SymbolTable.Get("operator " + op);
            return t is Function;
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

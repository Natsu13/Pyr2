using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    class _Enum:Types
    {
        public Dictionary<Token, Types> values = new Dictionary<Token, Types>();
        public Token name;
        public Block block;

        /*Serialization to JSON object for export*/
        [JsonParam] public Token Name => name;
        [JsonParam] public Dictionary<JObject, JObject> Values => values.ToDictionary(x => JsonParam.ToJson(x.Key), x => JsonParam.ToJson(x.Value));       
        //[JsonParam] public Block AssignBlock => assingBlock;

        public override void FromJson(JObject o)
        {
            throw new NotImplementedException();
        }
        public _Enum() { }

        public _Enum(Token name, Dictionary<Token, Types> values, Interpreter interpreter, Block block)
        {
            this.name = name;
            this.values = values;
            this.block = block;
            assingBlock = new Block(interpreter) { BlockParent = block };
            var i = 0;
            foreach (KeyValuePair<Token, Types> v in values)
            {
                if (v.Value != null)                
                    i = v.Value.Visit();                 
                var ad = new Assign(new Variable(v.Key, assingBlock), new Token(Token.Type.ASIGN, '='), new Number(new Token(Token.Type.INTEGER, i)) { assingBlock = this.assingBlock }, isVal: true);
                //new Number(new Token(Token.Type.INTEGER, i)) { assingBlock = this.assingBlock }
                this.assingBlock.SymbolTable.Add(v.Key.Value, ad);
                i++;
            }
        }

        public string getName()
        {
            return name.Value;
        }

        public override string Compile(int tabs = 0)
        {
            string tbs = DoTabs(tabs);
            StringBuilder ret = new StringBuilder();

            var nm = (block.assignTo == "" ? name.Value : block.assignTo + "." + name.Value);

            if (block.assignTo == "")
                ret.Append("var " + nm + " = {\n");
            else
                ret.Append(nm + " = {\n");
            var i = 0;
            var f = true;
            foreach(KeyValuePair<Token, Types> v in values)
            {
                if (f) { f = false; } else { ret.Append(",\n"); }
                if (v.Value != null)
                {
                    i = v.Value.Visit();
                    ret.Append(tbs + Types.TABS + v.Key.Value + ": " + i);                                        
                }
                else
                    ret.Append(tbs + Types.TABS + v.Key.Value + ": " + i);                
                i++;
            }
            ret.Append("\n" + tbs + "}\n");
            ret.Append(tbs + "Object.freeze(" + nm + ");\n");

            ret.Append(tbs + (block.assignTo == "" ? "var ":"") + nm + "$META = function(){\n");
            ret.Append(tbs + "  return {");
            ret.Append("\n" + tbs + "    type: 'enum'");
            ret.Append("\n" + tbs + "  };\n");
            ret.Append(tbs + "};\n");

            return ret.ToString();
        }

        public override Token getToken() { return name; }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }

        public override void Semantic()
        {
            foreach(KeyValuePair<Token, Types> v in values)
            {
                if (v.Value != null)
                    v.Value.Semantic();
            }
        }

        public override int Visit()
        {
            return 0;
        }
    }
}

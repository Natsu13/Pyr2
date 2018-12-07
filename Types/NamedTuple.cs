using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class NamedTuple:Types
    {
        public Dictionary<Token, Types> _list;
        public List<Types> _listNoName;
        public bool _isNamed = false;

        /*Serialization to JSON object for export*/
        [JsonParam] public Dictionary<JObject, JObject> List => _list.ToDictionary(x => JsonParam.ToJson(x.Key), x => JsonParam.ToJson(x.Value));
        
        public override void FromJson(JObject o)
        {
            throw new NotImplementedException();
        }
        public NamedTuple() { }

        public NamedTuple(Dictionary<Token, Types> list)
        {
            _list = list;
            _isNamed = true;
        }

        public NamedTuple(List<Types> list)
        {
            _listNoName = list;
            _isNamed = false;
        }

        public override string Compile(int tabs = 0)
        {
            string tbs = DoTabs(tabs);
            /*
            int tmpc = assingBlock.Interpret.tmpcount++;            
            if (!(assingBlock.SymbolTable.Get("List") is Class list))
                return "";
            if (!(list.assingBlock.SymbolTable.Get("constructor List") is Function cnstrctr))
                return "";

            var namedTuple = assingBlock.SymbolTable.Get("NamedTuple") as Class;
            var namedConstruct = namedTuple.assingBlock.SymbolTable.Get("constructor NamedTuple") as Function;
            */
            var ret = "";       
            //string ret = "var namedtuplelist$" + tmpc + " = GetModule(\"System.Generic.List\")." + list.getName() + "." + cnstrctr.Name + "(undefined, 'String');\n";
            /*                 
            foreach (var typese in _list)
            {
                ret += tbs + "namedtuplelist$" + tmpc + ".Add('" + typese.Key.Value + "');\n";
            }*/
            if(_isNamed)
                ret += tbs + "return {"+string.Join(", ", _list.Select(x => x.Key.Value + ": " +x.Value.Compile()))+"};";
            else
                ret += tbs + "return new Array("+string.Join(", ", _listNoName.Select(x => x.Compile()))+");";
            //[" + string.Join(", ", _list.Select(x => "'"+x.Value.TryVariable().GetDateType().Value+"'")) +"]);
            return ret;
        }

        public override Token getToken()
        {
            return new Token(Token.Type.NAMEDTUPLE, "", _list.First().Key.Pos, _list.Last().Key.Pos + _list.Last().Key.Value.Length);
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

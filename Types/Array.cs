using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class Array:Types
    {
        public List<Types> list;
        public Dictionary<Token, Types> _object;
        public bool isObject = false;

        /*Serialization to JSON object for export*/
        [JsonParam] public List<Types> List => list;
        [JsonParam] public Dictionary<JObject, JObject> Object => _object.ToDictionary(x => JsonParam.ToJson(x.Key), x => JsonParam.ToJson(x.Value));

        public override void FromJson(JObject o)
        {
            throw new NotImplementedException();
        }
        public Array() { }

        public Array(List<Types> list)
        {
            this.list = list;
        }
        public Array(Dictionary<Token, Types> _object)
        {
            this._object = _object;
            isObject = true;
        }

        public override Token getToken()
        {
            return Token.Combine(list.First().getToken(), list.Last().getToken());
        }

        public override int Visit()
        {
            throw new NotImplementedException();
        }

        public override string Compile(int tabs = 0)
        {
            if (isObject)
                return "{" + string.Join(", ", _object.Select(x => x.Key.Value + ": " + x.Value.Compile())) + "}";
            return "[" + string.Join(", ", list.Select(x => x.Compile())) + "]";
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }

        public override void Semantic()
        {
            if (isObject)
                _object.Select(x => x.Value).ToList().ForEach(x => x.Semantic());
            else
                list.ForEach(x => x.Semantic());
        }
    }
}

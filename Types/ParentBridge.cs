using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class ParentBridge : Block
    {
        private Token _token = null;

        public ParentBridge(Token token, Interpreter interpreter):base(interpreter, false, token)
        {
            _token = token;
        }

        public ParentBridge(string token)
        {
            _token = new Token(Token.Type.STRING, token);
        }

        public override Token getToken()
        {
            return _token;
        }

        public override int Visit()
        {
            throw new NotImplementedException();
        }

        public override string Compile(int tabs = 0)
        {
            throw new NotImplementedException();
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }

        public override void Semantic()
        {
            throw new NotImplementedException();
        }

        public override void FromJson(JObject o)
        {
            throw new NotImplementedException();
        }
    }
}

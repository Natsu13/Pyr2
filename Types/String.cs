using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class CString : Types
    {
        Token token;
        string value;
        public CString(Token token)
        {
            this.token = token;
            value = token.Value;
        }
        public string Value { get { return value; } }
        public Token.Type Type { get { return token.type; } }
        public Token Token { get { return token; } }

        public override string Compile(int tabs = 0)
        {
            return "'"+value.ToString()+"'";
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

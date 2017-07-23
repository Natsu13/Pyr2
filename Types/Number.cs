using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Number: Types
    {
        Token token;
        int value;
        public Number(Token token)
        {
            this.token = token;
            value = Int32.Parse(token.Value);
        }
        public int Value { get { return value; } }
        public Token.Type Type { get { return token.type; } }
        public Token Token { get { return token; } }

        public override string Compile(int tabs = 0)
        {
            return value.ToString();
        }

        public override void Semantic()
        {
            
        }

        public override int Visit()
        {
            return value;
        }
    }
}

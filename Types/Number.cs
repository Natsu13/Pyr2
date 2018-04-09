using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Number: Types
    {
        Token token;
        int value;
        private float fvalue;
        public bool isReal = false;
        public Number(Token token, bool isReal = false)
        {
            this.isReal = isReal;
            this.token = token;
            if (isReal)
                fvalue = float.Parse(token.Value, CultureInfo.InvariantCulture);
            else
                value = Int32.Parse(token.Value);
        }
        public int Value { get { return value; } }
        public float FValue { get { return fvalue;} }
        public Token.Type Type { get { return token.type; } }
        public override Token getToken() { return token; }

        public override string Compile(int tabs = 0)
        {
            if(isReal)
                return fvalue.ToString(CultureInfo.InvariantCulture);
            return value.ToString();
        }

        public override void Semantic()
        {
            
        }

        public override int Visit()
        {
            return value;
        }

        public override string InterpetSelf()
        {
            return "new Number(" + token.InterpetSelf() + ", " + (isReal ? "true" : "false") + ");";
        }
    }
}

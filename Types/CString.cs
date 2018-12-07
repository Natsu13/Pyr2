using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class CString : Types
    {
        Token token;
        public string value;

        /*Serialization to JSON object for export*/
        [JsonParam] public Token Token => token;    
        
        public override void FromJson(JObject o)
        {
            token = Token.FromJson(o["Token"]);
        }
        public CString() { }

        public CString(Token token)
        {
            this.token = token;
            value = token.Value;
        }
        public string Value { get { return value; } }
        public Token.Type Type { get { return token.type; } }
        public override Token getToken() { return token; }

        public override string Compile(int tabs = 0)
        {
            string o = "", consume = "";
            int state = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (state == 1 && value[i] == '}')
                {
                    if(consume.Substring(consume.Length-1, 1) == "?")
                        o += "\' + ( " + consume.Substring(0, consume.Length - 1) + " === null ? '' : " + consume.Substring(0, consume.Length - 1) + " ) + \'";
                    else
                        o += "\' + " + consume + " + \'";
                    consume = "";
                    state = 0;                    
                }
                else if (state == 1)
                    consume += value[i];
                else if (i + 1 < value.Length && value[i] == '{' && value[i + 1] == '$')
                {
                    state = 1;
                    i++;
                }
                else
                {
                    o += value[i];
                }
            }
            return "'"+o+"'";
        }        

        public override void Semantic()
        {
            string consume = "";
            int state = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (state == 1 && value[i] == '}')
                {
                    if(assingBlock?.SymbolTable.Get(consume) is Error)
                    {
                        Interpreter.semanticError.Add(new Error("#112 Variable " + consume + " not exist!", Interpreter.ErrorType.ERROR, token));
                    }
                    consume = "";
                    state = 0;
                }
                else if (state == 1)
                    consume += value[i];
                else if (i + 1 < value.Length && value[i] == '{' && value[i + 1] == '$')
                {
                    state = 1;
                    i++;
                }
            }
        }

        public override int Visit()
        {
            return 0;
        }

        public override string InterpetSelf()
        {
            return "new CString("+token.InterpetSelf()+ ");";
        }
    }
}

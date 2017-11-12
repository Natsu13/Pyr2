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
                    if(assingBlock?.SymbolTable.Find(consume) == null)
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

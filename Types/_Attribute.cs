using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class _Attribute:Types
    {
        public Types _class;
        public Token nclass;
        public ParameterList plist;
        public UnaryOp uop;
        static int attcount = 0;

        /*Serialization to JSON object for export*/
        [JsonParam] public Token _Class => nclass;
        [JsonParam] public ParameterList ParameterList => plist;

        public override void FromJson(JObject o)
        {
            throw new NotImplementedException();
        }
        public _Attribute() { }


        public _Attribute(Token _class, ParameterList plist)
        {
            this.plist = plist;
            this.nclass = _class;
        }

        public override Token getToken() { return _class.getToken(); }
        public string newName = "";
        public string GetName(bool real = false) {
            if (!real)
            {
                if (newName == "")
                {
                    attcount++;
                    newName = (_class == null ? "attribute$" + attcount : "attribute$" + attcount + "$" + _class.TryVariable().Value);
                }
                return newName;
            }
            if(_class == null)
                _class = assingBlock.SymbolTable.Get(nclass.Value);
            return _class?.TryVariable().Value;
        }

        public override string Compile(int tabs = 0)
        {
            _class = assingBlock.SymbolTable.Get(nclass.Value);
            if (_class is Error)
                return "";            
            if (uop == null)
                uop = new UnaryOp(new Token(Token.Type.NEW, -1), nclass, plist) { assingBlock = assingBlock };
            return uop.Compile();
        }

        public override void Semantic()
        {
            if(_class == null)
                _class = assingBlock.SymbolTable.Get(nclass.Value);
            if (_class == null || _class is Error)
                Interpreter.semanticError.Add(new Error("#600 Attribute "+ nclass.Value+" not found!", Interpreter.ErrorType.ERROR, nclass));
            else if (!((Class)_class).haveParent("Attribute"))
                Interpreter.semanticError.Add(new Error("#601 Class " + nclass.Value + " must be Attribute!", Interpreter.ErrorType.ERROR, nclass));
            uop?.Semantic();
            plist?.Semantic();
        }

        public override int Visit() { return 0; }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

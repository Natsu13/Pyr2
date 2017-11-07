using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class _Attribute:Types
    {
        Types _class;
        Token nclass;
        ParameterList plist;
        UnaryOp uop;
        static int attcount = 0;
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
            return _class.TryVariable().Value;
        }

        public override string Compile(int tabs = 0)
        {
            if (!assingBlock.SymbolTable.Find(nclass.Value))
                return "";
            _class = assingBlock.SymbolTable.Get(nclass.Value);
            if (uop == null)
                uop = new UnaryOp(new Token(Token.Type.NEW, -1), nclass, plist) { assingBlock = assingBlock };
            return uop.Compile();
        }

        public override void Semantic()
        {
            if (_class == null)
                Interpreter.semanticError.Add(new Error("Class "+ nclass.Value+" not found!", Interpreter.ErrorType.ERROR, nclass));
            else if (!((Class)_class).haveParent("Attribute"))
                Interpreter.semanticError.Add(new Error("Class " + nclass.Value + " must be Attribute!", Interpreter.ErrorType.ERROR, nclass));
            uop.Semantic();
            plist?.Semantic();
        }

        public override int Visit() { return 0; }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

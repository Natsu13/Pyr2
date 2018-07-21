using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class NamedTuple:Types
    {
        private Dictionary<Token, Types> _list;

        public NamedTuple(Dictionary<Token, Types> list)
        {
            _list = list;
        }

        public override string Compile(int tabs = 0)
        {
            int tmpc = assingBlock.Interpret.tmpcount++;
            string tbs = DoTabs(tabs);
            if (!(assingBlock.SymbolTable.Get("List") is Class list))
                return "";
            if (!(list.assingBlock.SymbolTable.Get("constructor List") is Function cnstrctr))
                return "";

            var namedTuple = assingBlock.SymbolTable.Get("NamedTuple") as Class;
            var namedConstruct = namedTuple.assingBlock.SymbolTable.Get("constructor NamedTuple") as Function;

            string ret = "var namedtuplelist$" + tmpc + " = GetModule(\"System.Generic.List\")." + list.getName() + "." + cnstrctr.Name + "(undefined, 'String');\n";

            foreach (var typese in _list)
            {
                ret += tbs + "namedtuplelist$" + tmpc + ".Add('" + typese.Key.Value + "');\n";
            }

            ret += tbs + "return "+namedTuple.getName()+"."+namedConstruct.Name+"("+string.Join(", ", _list.Select(x => x.Value.Compile()))+", namedtuplelist$" + tmpc + ", " + string.Join(", ", _list.Select(x => "'"+x.Value.TryVariable().GetDateType().Value+"'")) +");";
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

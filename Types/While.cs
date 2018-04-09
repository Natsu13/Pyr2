using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class While : Types
    {
        Types expr;
        Block block;

        public While(Types expr, Block block)
        {
            this.expr = expr;
            this.block = block;
        }

        public override string Compile(int tabs = 0)
        {
            string ret = "";

            string tab = DoTabs(tabs + 1);
            expr.endit = false;
            ret = tab + "while(" + expr.Compile() + "){\n";
            ret += block.Compile(tabs + 3);
            ret += tab + "  }";
            return ret;
        }

        public override Token getToken()
        {
            return expr.getToken();
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }

        public override void Semantic()
        {
            expr.Semantic();
            block.Semantic();
        }

        public override int Visit()
        {
            return 0;
        }
    }
}
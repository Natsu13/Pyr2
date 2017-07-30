using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class For : Types
    {
        Variable variable;
        Token source;
        Block block;

        public For(Variable variable, Token source, Block block)
        {
            this.variable = variable;
            this.source = source;
            this.block = block;
        }

        public override string Compile(int tabs = 0)
        {
            string ret = "";
            if (block.SymbolTable.Find(source.Value))
            {
                bool isIterable = false;
                Types t = block.SymbolTable.Get(source.Value);
                if(t is Assign)
                {
                    Types q = ((Assign)block.SymbolTable.Get(source.Value)).Left;
                    if (q is Variable v)
                    {
                        //Type tp = block.SymbolTable.GetType(v.getDateType().Value);
                        dynamic to = block.SymbolTable.Get(v.getDateType().Value);
                        if(to.haveParent("IIterable"))
                        {
                            isIterable = true;
                        }
                    }
                }

                if (isIterable)
                {

                }
            }

            return ret;
        }

        public override Token getToken()
        {
            return source;
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

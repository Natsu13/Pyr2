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
        Types source;
        Block block;
        bool isIterable = false;
        string className = "";

        public For(Variable variable, Types source, Block block)
        {
            this.variable = variable;
            this.source = source;
            this.block = block;
        }

        public override string Compile(int tabs = 0)
        {
            string ret = "";     
            if(source is Variable)
            {
                ((Variable)source).Check();

                Types to = block.SymbolTable.Get(((Variable)source).getDateType().Value);
                if (((Class)to).haveParent("IIterable"))
                {
                    isIterable = true;
                }
                className = ((Class)to).Name.Value;
            }
            if(source is UnaryOp uop && ((UnaryOp)source).Op == "new")
            {
                Types to = block.SymbolTable.Get(uop.Name.Value);
                if (((Class)to).haveParent("IIterable"))
                {
                    isIterable = true;
                }
                className = ((Class)to).Name.Value;
            }

            if (isIterable)
            {
                int tmpc = block.Interpret.tmpcount++;
                string tab = DoTabs(tabs + 1);
                ret = tab + "var $tmp" + tmpc + " = " + source.Compile(0) + ".iterator();\n";
                ret += tab + "  while($tmp" + tmpc + ".hasNext()){\n";
                ret += tab + "    var " + variable.Value + " = $tmp" + tmpc + ".next();\n";
                ret += block.Compile(tabs + 3);
                ret += tab + "  }";
            }

            return ret;
        }

        public override Token getToken()
        {
            return source.getToken();
        }

        public override void Semantic()
        {
            if (!isIterable)
            {
                Interpreter.semanticError.Add(new Error(source.TryVariable().Value + " with class '"+ className + "' is not Iterable", Interpreter.ErrorType.ERROR, source.getToken()));
            }
        }

        public override int Visit()
        {
            return 0;
        }
    }
}

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
        bool isIterable = false;
        string className = "";

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
                Types t = block.SymbolTable.Get(source.Value);
                if(t is Assign)
                {
                    Types q = ((Assign)block.SymbolTable.Get(source.Value)).Left;
                    if (q is Variable v)
                    {
                        //Type tp = block.SymbolTable.GetType(v.getDateType().Value);
                        Types to = block.SymbolTable.Get(v.getDateType().Value);
                        if(((Class)to).haveParent("IIterable"))
                        {
                            isIterable = true;
                        }
                        className = ((Class)to).Name.Value;
                    }
                }

                if (isIterable)
                {
                    int tmpc = block.Interpret.tmpcount++;
                    string tab = DoTabs(tabs+1);
                    ret  = tab + "var $tmp" + tmpc + " = " + source.Value + ".iterator();\n";
                    ret += tab + "  while($tmp" + tmpc + ".hasNext()){\n";
                    ret += tab + "    var " + variable.Value + " = $tmp" + tmpc + ".next();\n";
                    ret += block.Compile(tabs + 3);
                    ret += tab + "  }";
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
            if (!isIterable)
            {
                Interpreter.semanticError.Add(new Error("Variable " + source.Value + " with class '"+ className + "' is not Iterable", Interpreter.ErrorType.ERROR, source));
            }
        }

        public override int Visit()
        {
            return 0;
        }
    }
}

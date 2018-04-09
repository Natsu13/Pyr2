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

                Types to = block.SymbolTable.Get(((Variable)source).GetDateType().Value);
                if (to is Class && ((Class) to).haveParent("IIterable"))
                {
                    isIterable = true;
                    className = ((Class)to).Name.Value;
                }
                if (to is Interface && ((Interface) to).haveParent("IIterable"))
                {
                    isIterable = true;
                    className = ((Interface)to).Name.Value;
                }

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
            if (source is UnaryOp uoq && ((UnaryOp)source).Op == "call")
            {
                Types t1 = block.SymbolTable.Get(uoq.Name.Value);
                Types to = block.SymbolTable.Get(((Function)t1).Returnt.Value);
                if (((Class)to).haveParent("IIterable"))
                {
                    isIterable = true;
                }
                className = ((Class)to).Name.Value;
            }

            if (source is UnaryOp uor && ((UnaryOp)source).Op == "..")
            {
                Types to = block.SymbolTable.Get("Range");
                if (((Class)to).haveParent("IIterable"))
                {
                    isIterable = true;
                }
                className = ((Class)to).Name.Value;
            }

            if (isIterable)
            {
                int tmpc = block.Interpret.tmpcount++;
                source.endit = false;
                string tab = DoTabs(tabs);
                var s = source.Compile(0).Replace("\n", "");
                if (s.Substring(s.Length - 1, 1) == ";")
                    s = s.Substring(0, s.Length - 1);
                ret = tab + "var $tmp" + tmpc + " = " + s + ".iterator();\n";
                ret += tab + "  while($tmp" + tmpc + ".hasNext()){\n";
                ret += tab + "    var " + variable.Value + " = $tmp" + tmpc + ".next();\n";
                ret += block.Compile(tabs + 2);
                ret += tab + "  }";
            }

            return ret;
        }

        public override Token getToken()
        {
            return source.getToken();
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }

        public override void Semantic()
        {
            if (!isIterable)
            {
                Interpreter.semanticError.Add(new Error("#500 "+source.TryVariable().Value + " with class '"+ className + "' is not Iterable", Interpreter.ErrorType.ERROR, source.getToken()));
            }
            if (source is UnaryOp uoq && ((UnaryOp)source).Op == "call")
            {
                uoq.Semantic();
            }
        }

        public override int Visit()
        {
            return 0;
        }
    }
}

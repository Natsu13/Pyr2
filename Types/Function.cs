using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class Function:Types
    {
        Token name;
        Block block;
        ParameterList paraml;
        Token returnt;
        public bool isStatic = false;

        public Function(Token name, Types _block, ParameterList paraml, Token returnt, Interpreter interpret)
        {
            this.name = name;
            if(!(_block is Block))
            {
                block = new Block(interpret);
                block.children.Add(_block);
            }
            else
            {
                block = (Block)_block;
            }
            block.assignTo = name.Value;
            //this.block.blockAssignTo = name.Value;
            this.paraml = paraml;
            this.returnt = returnt;
        }
        public ParameterList ParameterList { get { return paraml; } }

        public override string Compile(int tabs = 0)
        {
            string tbs = DoTabs(tabs);
            string ret = "";
            if (assignTo == "")
                ret += tbs + "function " + name.Value + "(" + paraml.Compile(0) + "){\n";
            else
            {
                if (isStatic)
                    ret += tbs + assignTo + "." + name.Value + " = function(" + paraml.Compile(0) + "){\n";
                else
                    ret += tbs + assignTo + ".prototype." + name.Value + " = function(" + paraml.Compile(0) + "){\n";
            }
            ret += block.Compile(tabs + 1);
            ret += tbs + "}\n";          
            return ret;
        }
        public override int Visit()
        {
            return 0;
        }

        public override void Semantic()
        {
            if (assignTo == "" && isStatic)
                Interpreter.semanticError.Add(new Error("Static modifier outside class is useless", Interpreter.ErrorType.WARNING));
            block.Semantic();
            block.CheckReturnType(returnt.Value);
        }
    }
}

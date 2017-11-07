using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Properties : Types
    {
        public Types variable;
        Types getter, setter;
        
        public Properties(Types variable, Types getter = null, Types setter = null, Block current_block = null)
        {
            this.variable = variable;
            this.setter = setter;
            this.getter = getter;
            current_block.SymbolTable.Add(variable.TryVariable().Value, this);
        }

        public Types Getter { get { return getter; } }
        public Types Setter { get { return setter; } }

        public override string Compile(int tabs = 0)
        {
            if (assingBlock.Type != Block.BlockType.CLASS)
                return "";

            if (variable is Variable)
                ((Variable)variable).Check();

            if(this.setter is Block)
            {
                ((Block)this.setter).variables.Add("value", new Assign(new Variable(new Token(Token.Type.STRING, "value"), assingBlock, ((Variable)variable).getDateType()), new Token(Token.Type.ASIGN, "="), new Null(), assingBlock));
            }

            string var_name = variable.TryVariable().Value;
            string tab = DoTabs(tabs + 1);
            string ret = "";
            ret += assingBlock.getClass() + ".prototype.Property$" + var_name + " = {\n";
            if (this.getter != null)
            {
                ret += "  get: function(){\n";
                ret += getter.Compile(tabs + 2);
                ret += "  }"+(this.setter != null?",":"")+"\n";
            }
            if (this.setter != null)
            {
                ret += "  set: function(value){\n";
                ret += setter.Compile(tabs + 2);
                ret += "  }\n";
            }
            ret += "}";
            return ret;
        }

        public override Token getToken()
        {
            return variable.getToken();
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }

        public override void Semantic()
        {
            if(assingBlock.Type != Block.BlockType.CLASS)
                Interpreter.semanticError.Add(new Error("#602 Properties can be used only in Class!", Interpreter.ErrorType.ERROR, getToken()));
            variable.Semantic();
            setter.Semantic();
            getter.Semantic();
        }

        public override int Visit()
        {
            return 0;
        }
    }
}

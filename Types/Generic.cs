using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Generic : Types
    {
        bool isClassed = false;
        bool isFunctio = false;
        Class classAss = null;
        Function functionAss = null;
        string name = "";
        Block block;
        TypeObject bt = new TypeObject();

        public Generic(Class c, Block block, string name)
        {
            this.block = block;
            this.name = name;
            setClass(c);
        }
        public Generic(Function f, Block block, string name)
        {
            this.block = block;
            this.name = name;
            setFunction(f);
        }

        public string Name { get { return name; } }

        public void setClass(Class classs)
        {
            classAss = classs;
            isClassed = true;
            isFunctio = false;
        }
        public void setFunction(Function functi)
        {
            functionAss = functi;
            isClassed = false;
            isFunctio = true;
        }

        public override string Compile(int tabs = 0)
        {
            return "#GENERICVAR#";
        }

        public override Token getToken()
        {
            return null;
        }

        public override void Semantic()
        {            
        }

        public override int Visit()
        {
            return 0;
        }        

        public Token OutputType(string op, object a, object b)
        {            
            return bt.OutputType(op, a, b);
        }
        public bool SupportOp(string op)
        {            
            return bt.SupportOp(op);
        }
        public bool SupportSecond(string op, object second, object secondAsVariable)
        {            
            return bt.SupportSecond(second, secondAsVariable);
        }
    }
}

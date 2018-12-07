using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class Generic : Types
    {
        bool isClassed = false;
        bool isFunctio = false;
        bool isDelegat = false;
        Class classAss = null;
        Function functionAss = null;
        Delegate delegateAss = null;
        string name = "";
        Block block;
        TypeObject bt = new TypeObject();     
        
        /*Serialization to JSON object for export*/
        [JsonParam] public Class Class => classAss;
        [JsonParam] public Function Function => functionAss;
        [JsonParam] public Delegate Delegate => delegateAss;
        //[JsonParam] public Block Block => block;
        [JsonParam] public string Name => name;

        public override void FromJson(JObject o)
        {
            throw new NotImplementedException();
        }
        public Generic() { }

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
        public Generic(Delegate p, Block block, string name)
        {
            this.block = block;
            this.name = name;
            setDelegate(p);
        }  

        public void setClass(Class classs)
        {
            classAss = classs;
            isClassed = true;
            isFunctio = false;
            isDelegat = false;
        }
        public void setFunction(Function functi)
        {
            functionAss = functi;
            isClassed = false;
            isFunctio = true;
            isDelegat = false;
        }
        public void setDelegate(Delegate delagat)
        {
            delegateAss = delagat;
            isClassed = false;
            isFunctio = false;
            isDelegat = true;
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

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

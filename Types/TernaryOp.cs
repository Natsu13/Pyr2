using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class TernaryOp:Types
    {
        public Types left;
        public Types right;
        public Types condition;
        Block block;

        /*Serialization to JSON object for export*/
        [JsonParam] public Types Condition => condition;
        [JsonParam] public Types Left => left;
        [JsonParam] public Types Right => right;

        public override void FromJson(JObject o)
        {
            throw new NotImplementedException();
        }
        public TernaryOp() { }

        public TernaryOp(Types condition, Types left, Types right, Block block)
        {
            this.condition = condition;
            this.left = left;
            this.right = right;
            this.block = this.assingBlock = block;
        }
        
        public override Token getToken() { return Token.Combine(this.condition.getToken(), this.right.getToken()); }

        public override string Compile(int tabs = 0)
        {
            condition.endit = false;
            return "(" + condition.Compile() + " ? " + left.Compile() + ": " + right.Compile() + ")";
        }

        public override void Semantic()
        {
            condition.Semantic();
            left.Semantic();
            right.Semantic();
        }

        public override int Visit() { return 0; }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

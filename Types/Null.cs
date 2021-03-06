﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class Null:Types
    {
        public Null()
        {

        }
        public int Value { get { return 0; } }
        public Token.Type Type { get { return Token.Type.NULL; } }
        public override Token getToken() { return new Token(Token.Type.NULL, "null"); }

        public override string Compile(int tabs = 0)
        {            
            return "null";
        }

        public override void Semantic()
        {

        }

        public override void FromJson(JObject o)
        {
            
        }

        public override int Visit()
        {
            return 0;
        }

        public override string InterpetSelf()
        {
            return "new Null();";
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class NoOp:Types
    {
        public NoOp()
        {

        }

        public override string Compile(int tabs = 0)
        {
            return "";
        }

        public override void Semantic()
        {
            
        }

        public override void FromJson(JObject o)
        {
            return;
        }

        public override int Visit()
        {
            return 0;
        }

        public override Token getToken() { return null; }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

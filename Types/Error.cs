using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Error: Types
    {
        string error = "";
        Interpreter.ErrorType errorType;

        public Error(string error)
        {
            this.error = error;
        }

        public Error(string error, Interpreter.ErrorType errorType)
        {
            this.error = error;
            this.errorType = errorType;
        }

        public string Message { get { return error; } }
        public Interpreter.ErrorType Type { get { return errorType; } }

        public override string Compile(int tabs = 0)
        {
            throw new NotImplementedException();
        }

        public override void Semantic()
        {
            throw new NotImplementedException();
        }

        public override int Visit()
        {
            throw new NotImplementedException();
        }
    }
}

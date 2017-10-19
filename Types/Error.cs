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
        Token token;
        int line, position, lenght, rpos;
        string message;

        public Error(string error, Interpreter.ErrorType errorType = Interpreter.ErrorType.INFO, Token token = null)
        {
            this.error = error;
            this.errorType = errorType;
            this.token = token;

            if (token == null || token.Pos == -1 || token.File == "") { }
            else
            {
                int pos = token.Pos + token.Value.Length;
                rpos = token.Pos;
                string rerr = error;
                string[] splt = Interpreter.fileList[token.File].Split('\n');
                rerr += "\n";
                int startl = Interpreter.fileList[token.File].Substring(0, pos).Count(t => t == '\n');
                rerr += " " + splt[startl].TrimStart();
                rerr += "\n";
                int alltl = 0;
                for (int q = 0; q < startl; q++) { alltl += splt[q].Length + 1; }
                for (int q = -1 + alltl + (splt[startl].TakeWhile(Char.IsWhiteSpace).Count()); q < pos - token.Value.Length; q++) rerr += " ";
                for (int q = 0; q < token.Value.Length; q++) rerr += "^";
                rerr += "\n";
                rerr += "Found at " + (startl + 1) + ":" + ((pos - token.Value.Length) - (0 + alltl));
                line = startl + 1;
                position = ((pos - token.Value.Length) - (0 + alltl));
                lenght = token.Value.Length;
                message = token.File + "(" + line + ":" + position + ")";
            }
        }        

        public int RPos { get { return rpos; } }
        public int Line { get { return line; } }
        public int Position { get { return position; } }
        public int Lenght { get { return lenght; } }
        public string Place { get { return message; } }         
        public string Message { get { return error; } }
        public Interpreter.ErrorType Type { get { return errorType; } }
        public override Token getToken() { return null; }

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

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

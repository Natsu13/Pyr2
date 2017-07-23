using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Token
    {
        public enum Type
        {
            NONE, ERROR, EOF,
            INTEGER, STRING, DYNAMIC, REAL,
            COMMA, SEMI, DOT, COLON, 
            PLUS, MINUS, MUL, DIV, ASIGN, NEW, RETURN,
            CLASS, ID,
            NEWCLASS, NEWFUNCTION,
            LPAREN, RPAREN, BEGIN, END, VAR, DEFINERETURN,
            STATIC
        };
        public static Dictionary<string, Token> Reserved = new Dictionary<string, Token>()
        {
            { "var", new Token(Type.VAR, "var") },
            { "class", new Token(Type.NEWCLASS, "class") },
            { "new", new Token(Type.NEW, "new") },
            { "function", new Token(Type.NEWFUNCTION, "function") },
            { "return", new Token(Type.RETURN, "return") },
            { "static", new Token(Type.STATIC, "static") }
        };

        public Type type;
        string value = "";

        public Token(Type type, string value)
        {
            this.type = type;
            this.value = value;
        }
        public Token(Type type, char value): this(type, value.ToString())
        {

        }
        public Token(Type type, int value) : this(type, value.ToString())
        {

        }

        public string Value
        {
            get { return value; }
        }

        public override string ToString()
        {
            return "Token("+type+", "+value+")";
        }
    }
}

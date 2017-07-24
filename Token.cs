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
            NONE, ERROR, EOF, NULL, 
            INTEGER, STRING, DYNAMIC, REAL,
            COMMA, SEMI, DOT, COLON, 
            PLUS, MINUS, MUL, DIV, ASIGN, NEW, RETURN,
            CLASS, ID, FUNCTION, 
            NEWCLASS, NEWFUNCTION,
            LPAREN, RPAREN, BEGIN, END, VAR, DEFINERETURN, CALL, 
            STATIC, VOID, EXTERNAL
        };
        public static Dictionary<string, Token> Reserved = new Dictionary<string, Token>()
        {
            { "var",        new Token(Type.VAR, "var") },
            { "class",      new Token(Type.NEWCLASS, "class") },
            { "new",        new Token(Type.NEW, "new") },
            { "function",   new Token(Type.NEWFUNCTION, "function") },
            { "return",     new Token(Type.RETURN, "return") },
            { "static",     new Token(Type.STATIC, "static") },
            { "external",   new Token(Type.EXTERNAL, "external") }, 
            { "void",       new Token(Type.VOID, "void") }    
        };

        public Type type;
        string value = "";
        int pos;
        string file;

        public Token(Type type, string value, int pos = -1, string file = "")
        {
            this.type = type;
            this.value = value;
            this.pos = pos;
            this.file = file;
        }
        public Token(Type type, char value, int pos = -1, string file = ""): this(type, value.ToString(), pos, file)
        {

        }
        public Token(Type type, int value, int pos = -1, string file = "") : this(type, value.ToString(), pos, file)
        {

        }
        public Token(Token token, int pos = -1, string file = "") : this(token.type, token.value.ToString(), pos, file)
        {

        }

        public int Pos { get { return pos; } }
        public string File { get { return file; } }
        public string Value { get { return value; } }

        public override string ToString()
        {
            return "Token("+type+", "+value+")";
        }
    }
}

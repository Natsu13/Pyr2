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
            INTEGER, STRING, DYNAMIC, REAL, BOOL, AUTO,
            COMMA, SEMI, DOT, COLON,
            PLUS, MINUS, MUL, DIV, ASIGN, NEW, RETURN,
            CLASS, ID, FUNCTION, INTERFACE, LAMBDA, 
            NEWCLASS, NEWFUNCTION, NEWINTERFACE, NEWLAMBDA,
            LPAREN, RPAREN, BEGIN, END, VAR, DEFINERETURN, CALL,
            STATIC, VOID, EXTERNAL,
            IF, ELSE, ELSEIF,
            EQUAL, NOTEQUAL, AND, OR,
            TRUE, FALSE            
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
            { "void",       new Token(Type.VOID, "void") },
            { "if",         new Token(Type.IF, "if") },
            { "elseif",     new Token(Type.ELSEIF, "elseif") },
            { "else",       new Token(Type.ELSE, "else") },
            { "true",       new Token(Type.TRUE, "true") },
            { "false",      new Token(Type.FALSE, "false") },
            { "interface",  new Token(Type.NEWINTERFACE, "interface") },
            { "null",       new Token(Type.NULL, "null") },
            { "lambda",     new Token(Type.NEWLAMBDA, "lambda") }
        };

        public Type type;
        string value = "";
        int pos;
        string file;
        int endpos = -1;

        public Token(Type type, string value, int pos = -1, string file = "")
        {
            this.type = type;
            this.value = value;
            this.pos = pos;
            this.file = file;
        }
        public Token(Type type, char value, int pos = -1, string file = "") : this(type, value.ToString(), pos, file)
        {

        }
        public Token(Type type, int value, int pos = -1, string file = "") : this(type, value.ToString(), pos, file)
        {

        }
        public Token(Token token, int pos = -1, string file = "") : this(token.type, token.value.ToString(), pos, file)
        {

        }
        public Token(Type type, string value, int pos, int endpos, string file = "") : this(type, value, pos, file)
        {
            this.endpos = endpos;
        }

        public static Token Combine(Token t1, Token t2)
        {
            return new Token(t1.type, t1.value + t2.value, t1.pos, t2.pos+t2.value.Length, t1.file);
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

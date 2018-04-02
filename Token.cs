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
            INTEGER, STRING, REAL, BOOL, AUTO,
            COMMA, SEMI, DOT, COLON, IS, THREEDOT, AS,
            PLUS, MINUS, MUL, DIV, ASIGN, NEW, RETURN, INC, DEC,
            CLASS, ID, FUNCTION, INTERFACE, LAMBDA, 
            NEWCLASS, NEWFUNCTION, NEWINTERFACE, NEWLAMBDA,
            LPAREN, RPAREN, BEGIN, END, VAR, DEFINERETURN, CALL, LSQUARE, RSQUARE, 
            STATIC, VOID, EXTERNAL, OPERATOR, DYNAMIC, DECLARE,
            IF, ELSE, ELSEIF, FOR, WHILE,
            EQUAL, NOTEQUAL, AND, OR, MORE, LESS, 
            TRUE, FALSE,
            IN, GET,
            IMPORT, SET,
            DELEGATE,
            PROPERTIES,
            TWODOT,
            RANGE
        };
        public static readonly Dictionary<string, Token> Reserved = new Dictionary<string, Token>()
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
            { "lambda",     new Token(Type.NEWLAMBDA, "lambda") },
            { "for",        new Token(Type.FOR, "for") },
            { "while",      new Token(Type.WHILE, "while") },
            { "in",         new Token(Type.IN, "in") },
            { "operator",   new Token(Type.OPERATOR, "operator") },
            { "is",         new Token(Type.IS, "is") },
            { "as",         new Token(Type.AS, "as") },
            { "dynamic",    new Token(Type.DYNAMIC, "dynamic") },
            { "declare",    new Token(Type.DECLARE, "declare") },
            { "import",     new Token(Type.IMPORT, "import") },
            { "delegate",   new Token(Type.DELEGATE, "delegate") }
        };        

        public Type type;
        string      value = "";
        int         pos;
        string      file;
        int         endpos = -1;

        public int      Pos     => pos;
        public string   File    => file;
        public string   Value   => value;        

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

        public string InterpetSelf()
        {
            return "new Token("+Enum.GetName(typeof(Type), type)+", "+value+", "+pos+", "+endpos+", "+file+")";
        }

        public static Token Combine(Token t1, Token t2)
        {
            if (t2 == null) return t1;
            return new Token(t1.type, t1.value + t2.value, t1.pos, t2.pos+t2.value.Length, t1.file);
        }        

        public override string ToString()
        {
            return "Token("+type+", "+value+")";
        }
    }
}

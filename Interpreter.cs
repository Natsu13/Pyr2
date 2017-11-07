using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Compilator
{    
    public class Interpreter
    {
        string              text;
        int                 pos = 0;
        Token               current_token;
        Token               previous_token;
        List<Token>         current_modifer = new List<Token>();
        char                current_char;
        Block               current_block;
        Block               current_block_with_variable;
        int                 current_token_pos;
        Block.BlockType     current_block_type = Block.BlockType.NONE;
        Block.BlockType     main_block_type = Block.BlockType.NONE;
        string              current_file = "";
        public bool         isConsole = false;
        public bool         brekall = false;
        public int          tmpcount = 0;

        public static List<Error> semanticError = new List<Compilator.Error>();
        public enum ErrorType { INFO, WARNING, ERROR };
        public static Dictionary<string, string> fileList = new Dictionary<string, string>();
        public Dictionary<string, Import> imports = new Dictionary<string, Import>();

        /// Interpret settings
        public static bool _REDECLARATION =     false;      //If you enable redeclaration
        public static bool _WAITFORPAGELOAD =   true;       //If you want call main in window.onload
        public static bool _WRITEDEBUG =        false;      //If you want with faul write the output to file

        /// Language settings
        public enum     LANGUAGES { JAVASCRIPT, CSHARP, PYTHON, PHP, CPP };
        public static   LANGUAGES _LANGUAGE = LANGUAGES.JAVASCRIPT;
        public static bool isForExport = false;
        public Interpreter parent = null;

        public Interpreter(string text, string filename = "", Interpreter parent = null)
        {
            this.text = text;
            this.parent = parent;
            if(!fileList.ContainsKey(filename))
                fileList.Add(filename, text);
            imports = new Dictionary<string, Import>();
            pos = 0;
            current_char = text[pos];
            current_file = filename;
            current_block = new Block(this, true);
            current_block.SymbolTable.initialize();
            current_token = GetNextToken();            
        }

        public bool FindImport(string name)
        {
            if (imports.ContainsKey(name))
                return true;
            else if (parent != null)
                return parent.FindImport(name);
            return false;
        }

        public Import GetImport(string name)
        {
            if (imports.ContainsKey(name))
                return imports[name];
            else if (parent != null)
                return parent.GetImport(name);
            return null;
        }

        public void Error(string error = "Error parsing input")
        {
            if (brekall) return;
            string rerr = error;
            string[] splt = text.Split('\n');
            rerr += "\n";
            if (pos >= text.Length) pos = text.Length - 1;
            int startl = text.Substring(0, pos).Count(t => t == '\n');
            rerr += " "+splt[startl].TrimStart();
            rerr += "\n";
            int alltl = 0;
            for (int q = 0; q < startl; q++) { alltl += splt[q].Length+1; }
            for (int q = -1+alltl+(splt[startl].TakeWhile(Char.IsWhiteSpace).Count()); q < pos - current_token.Value.Length; q++) rerr += " ";
            for (int q = 0; q < current_token.Value.Length; q++) rerr += "^";
            rerr += "\n";
            rerr += "Found at " + startl + ":" + ((pos - current_token.Value.Length) - (-1 + alltl + (splt[startl].TakeWhile(Char.IsWhiteSpace).Count())));
            //Console.Write(rerr);
            //throw new Exception(rerr);
            //pos = text.Length+1;
            if (isConsole)
            {
                Console.Write(rerr);
                current_token = new Token(Token.Type.EOF, "EOF");
                Console.WriteLine();
                Console.ReadKey();
                System.Environment.Exit(1);
            }
            else
            {
                current_char = '\0';
                pos = text.Length + 1;
                Error e = new Error(error, ErrorType.ERROR, current_token);
                semanticError.Add(e);
            }
            brekall = true;
        }

        public Token GetNextToken()
        {
            while (current_char != '\0')
            {
                if (current_char == ' ' || current_char == '\t' || current_char == '\r' || current_char == '\n')
                {
                    SkipWhiteSpace();
                    continue;
                }

                if (current_char == '/' && Peek() == '*')
                {
                    Advance(); Advance();
                    SkipComments();
                    continue;
                }

                if(current_char == '/' && Peek() == '/')
                {
                    while (current_char != '\n' && current_char != '\0')
                        Advance();
                    continue;
                }

                current_token_pos = pos;

                if (Char.IsDigit(current_char))
                    return Number();

                if (current_char == '"' || current_char == '\'')
                {
                    return String(current_char);
                }

                if (Char.IsLetterOrDigit(current_char) || current_char == '_' || current_char == '$')
                    return Id();

                if (current_char == '.' && Peek() == '.' && Peek(2) == '.')
                {
                    Advance(); Advance(); Advance();
                    return new Token(Token.Type.THREEDOT, "...", current_token_pos, current_file);
                }
                if (current_char == '-' && Peek() == '>') {
                    Advance(); Advance();
                    return new Token(Token.Type.DEFINERETURN, "->", current_token_pos, current_file);
                }
                if(current_char == '=' && Peek() == '>')
                {
                    Advance(); Advance();
                    return new Token(Token.Type.SET, "=>", current_token_pos, current_file);
                }
                if (current_char == '=' && Peek() == '=') {
                    Advance(); Advance();
                    return new Token(Token.Type.EQUAL, "==", current_token_pos, current_file);
                }
                if (current_char == '!' && Peek() == '=')
                {
                    Advance(); Advance();
                    return new Token(Token.Type.NOTEQUAL, "!=", current_token_pos, current_file);
                }
                if (current_char == '&' && Peek() == '&')
                {
                    Advance(); Advance();
                    return new Token(Token.Type.AND, "&&", current_token_pos, current_file);
                }
                if (current_char == '|' && Peek() == '|')
                {
                    Advance(); Advance();
                    return new Token(Token.Type.OR, "||", current_token_pos, current_file);
                }
                if (current_char == '+' && Peek() == '+') {
                    Advance(); Advance();
                    return new Token(Token.Type.INC, "++", current_token_pos, current_file);
                }
                if (current_char == '-' && Peek() == '-')
                {
                    Advance(); Advance();
                    return new Token(Token.Type.DEC, "--", current_token_pos, current_file);
                }
                if (current_char == '>') { Advance(); return new Token(Token.Type.MORE,     '>', current_token_pos, current_file); }
                if (current_char == '<') { Advance(); return new Token(Token.Type.LESS,     '<', current_token_pos, current_file); }
                if (current_char == '=') { Advance(); return new Token(Token.Type.ASIGN,    '=', current_token_pos, current_file); }
                if (current_char == ';') { Advance(); return new Token(Token.Type.SEMI,     ';', current_token_pos, current_file); }
                if (current_char == ':') { Advance(); return new Token(Token.Type.COLON,    ':', current_token_pos, current_file); }
                if (current_char == ',') { Advance(); return new Token(Token.Type.COMMA,    ',', current_token_pos, current_file); }
                if (current_char == '.') { Advance(); return new Token(Token.Type.DOT,      '.', current_token_pos, current_file); }
                if (current_char == '+') { Advance(); return new Token(Token.Type.PLUS,     '+', current_token_pos, current_file); }
                if (current_char == '-') { Advance(); return new Token(Token.Type.MINUS,    '-', current_token_pos, current_file); }
                if (current_char == '*') { Advance(); return new Token(Token.Type.MUL,      '*', current_token_pos, current_file); }
                if (current_char == '/') { Advance(); return new Token(Token.Type.DIV,      '/', current_token_pos, current_file); }
                if (current_char == '(') { Advance(); return new Token(Token.Type.LPAREN,   '(', current_token_pos, current_file); }
                if (current_char == ')') { Advance(); return new Token(Token.Type.RPAREN,   ')', current_token_pos, current_file); }
                if (current_char == '{') { Advance(); return new Token(Token.Type.BEGIN,    '{', current_token_pos, current_file); }
                if (current_char == '}') { Advance(); return new Token(Token.Type.END,      '}', current_token_pos, current_file); }
                if (current_char == '[') { Advance(); return new Token(Token.Type.LSQUARE,  '[', current_token_pos, current_file); }
                if (current_char == ']') { Advance(); return new Token(Token.Type.RSQUARE,  ']', current_token_pos, current_file); }

                Error("Unexpeced token found");
                return null;
            }            
            return new Token(Token.Type.EOF, "" , current_token_pos, current_file);
        }

        public void Eat(Token.Type tokenType, string errorMessage = "")
        {
            previous_token = current_token;
            if (current_token.type == tokenType)
                current_token = GetNextToken();
            else
            {
                if (errorMessage != "")
                    Error(errorMessage);
                Error("A " + tokenType + " token is expected, but the " + current_token.type + " token was found");
                return;
            }
        }

        public void Advance()
        {
            pos++;
            if (pos >= text.Length)
                current_char = '\0';
            else
                current_char = text[pos];
        }

        public char Peek(int howmuch = 1)
        {
            int peek_pos = pos + howmuch;
            if(peek_pos >= text.Length)
                return '\0';
            else
                return text[peek_pos];
        }

        public Token String(char end)
        {
            Advance();
            string result = "";
            while (current_char != '\0' && current_char != end)
            {
                if ((current_char == '"' && end == '\'') || (current_char == '\'' && end == '\"'))
                    result += '\\';
                result += current_char;
                Advance();
            }
            Advance();
            return new Token(Token.Type.STRING, result, current_token_pos, current_file);
        }

        public Token Id()
        {
            string result = "";
            while(current_char != '\0' && (Char.IsLetterOrDigit(current_char) || current_char == '_' || current_char == '.' || current_char == '$'))
            {
                result += current_char;
                Advance();
            }

            if (Token.Reserved.ContainsKey(result))
                return new Token(Token.Reserved[result], current_token_pos, current_file);
            else if (current_block.SymbolTable.Find(result))
            {
                Types tp = current_block.SymbolTable.Get(result);
                if(tp is Assign)
                {
                    if (((Assign)tp).Right is Null)
                    {
                        if (((Assign)tp).Left is Variable)
                            return new Token(Token.Type.ID, result, current_token_pos, current_file);
                        else
                            return new Token(((Variable)((Assign)tp).Left).getToken().type, result, current_token_pos, current_file);
                    }
                    else if (((Assign)tp).Right is Lambda)
                        return new Token(Token.Type.LAMBDA, result, current_token_pos, current_file);
                    else if (((Assign)tp).Right is UnaryOp)
                        return new Token(Token.Type.ID, result, current_token_pos, current_file);
                    else
                        return new Token(Token.Type.ID, result, current_token_pos, current_file);
                }
                if(tp is Function)
                    return new Token(Token.Type.FUNCTION, result, current_token_pos, current_file);
                if(tp is Interface)
                    return new Token(Token.Type.INTERFACE, result, current_token_pos, current_file);
                return new Token(Token.Type.CLASS, result, current_token_pos, current_file);
            }
            else
                return new Token(Token.Type.ID, result, current_token_pos, current_file);
        }
        
        public void SkipWhiteSpace()
        {
            while (current_char != '\0' && (current_char == ' ' || current_char == '\t' || current_char == '\r' || current_char == '\n'))
                Advance();
        }

        public void SkipComments()
        {
            while (!(current_char == '*' && Peek() == '/'))
                Advance();
            Advance();
            Advance();
        }

        public Token Number()
        {
            string result = "";
            while (current_char != '\0' && Char.IsDigit(current_char))
            {
                result += current_char;
                Advance();
            }
            if(current_char == '.')
            {
                result += current_char;
                Advance();

                while (current_char != '\0' && Char.IsDigit(current_char))
                {
                    result += current_char;
                    Advance();
                }
                return new Token(Token.Type.REAL, result, current_token_pos, current_file);
            }
            else
            {
                return new Token(Token.Type.INTEGER, result, current_token_pos, current_file);
            }
        }

        public ParameterList Parameters(bool declare = false)
        {
            ParameterList plist = new ParameterList(declare);
            bool defaultstart = false;
            plist.token = current_token;
            Eat(Token.Type.LPAREN);
            while(current_token.type != Token.Type.RPAREN && current_token.type != Token.Type.EOF && !brekall)
            {
                Token vtype = null;
                if (declare)
                {
                    if (plist.allowMultipel)
                        Error("Multiple arguments can be only at end of the arguments list");
                    vtype = current_token;
                    if (current_token.type == Token.Type.ID)
                        Eat(Token.Type.ID);
                    else if (current_token.type == Token.Type.THREEDOT)
                    {
                        Eat(Token.Type.THREEDOT);
                        plist.allowMultipel = true;
                        continue;
                    }
                    else if (current_token.type == Token.Type.LPAREN)
                    {
                        ParameterList list = Parameters(true);
                        Eat(Token.Type.DEFINERETURN);
                        Token argName = current_token;
                        Eat(Token.Type.ID);
                        plist.parameters.Add(new Lambda(new Variable(argName, current_block), null, list) { isInArgumentList = true });
                        continue;
                    }
                    else
                        Eat(Token.Type.CLASS);
                    Token vname = current_token;
                    Eat(Token.Type.ID);
                    if (current_token.type == Token.Type.ASIGN)
                    {
                        Token assign = current_token;
                        Eat(Token.Type.ASIGN);
                        Types _default = Expr();
                        plist.parameters.Add(new Assign(new Variable(vname, current_block, vtype), assign, _default, current_block));
                        defaultstart = true;
                    }
                    else
                    {
                        if (defaultstart) { plist.cantdefault = true; }
                        plist.parameters.Add(new Variable(vname, current_block, vtype));
                    }
                }
                else
                {
                    Types vname = Expr();
                    plist.parameters.Add(vname);
                }
                if (current_token.type == Token.Type.COMMA)
                    Eat(Token.Type.COMMA);
            }
            Eat(Token.Type.RPAREN);            
            return plist;
        }

        public Types Factor()
        {
            Token token = current_token;
            if (token.type == Token.Type.NEW)
            {
                Eat(Token.Type.NEW);
                Token className = current_token;
                if (current_token.type == Token.Type.ID)
                    Eat(Token.Type.ID);
                else
                    Eat(Token.Type.CLASS);

                List<string> garg = new List<string>();
                if (current_token.type == Token.Type.LESS)
                {
                    Eat(Token.Type.LESS);
                    while (current_token.type != Token.Type.MORE)
                    {
                        if (current_token.type == Token.Type.COMMA)
                        {
                            garg.Add(previous_token.Value);
                            Eat(Token.Type.COMMA);
                        }
                        else Eat(current_token.type);
                    }
                    if (previous_token.type != Token.Type.LESS)
                        garg.Add(previous_token.Value);
                    else
                        Error("You need specify generic arguments!");
                    Eat(Token.Type.MORE);
                }

                int size = -1;
                Types arraySizeVariable = null;
                if (current_token.type == Token.Type.LSQUARE)
                {
                    Eat(Token.Type.LSQUARE);
                    if (current_token.type == Token.Type.INTEGER)
                    {
                        size = Int32.Parse(current_token.Value);
                        Eat(Token.Type.INTEGER);
                    }
                    else
                    {
                        arraySizeVariable = Expr();
                    }
                    Eat(Token.Type.RSQUARE);
                }

                if (current_token.type == Token.Type.SEMI)
                {
                    UnaryOp up = new UnaryOp(token, className, null, current_block);
                    up.genericArgments = garg;
                    if(size > -1)
                        up.MadeArray(size);
                    if (arraySizeVariable != null)
                        up.MadeArray(arraySizeVariable);
                    up.assingBlock = current_block;
                    return up;
                }
                else
                {
                    ParameterList p = Parameters();
                    UnaryOp up = new UnaryOp(token, className, p, current_block);
                    up.genericArgments = garg;
                    if (size > -1)
                        up.MadeArray(size);
                    if (arraySizeVariable != null)
                        up.MadeArray(arraySizeVariable);
                    up.assingBlock = current_block;
                    return up;
                }
            }            
            else if (token.type == Token.Type.PLUS)
            {
                Eat(Token.Type.PLUS);
                return new UnaryOp(token, Factor());
            }
            else if (token.type == Token.Type.MINUS)
            {
                Eat(Token.Type.MINUS);
                return new UnaryOp(token, Factor());
            }
            else if (token.type == Token.Type.INC)
            {
                Eat(Token.Type.INC);
                return new UnaryOp(token, Factor());
            }
            else if (token.type == Token.Type.DEC)
            {
                Eat(Token.Type.DEC);
                return new UnaryOp(token, Factor());
            }
            else if (token.type == Token.Type.INTEGER)
            {
                Eat(Token.Type.INTEGER);
                return new Number(token);
            }
            else if (token.type == Token.Type.STRING)
            {
                Eat(Token.Type.STRING);
                return new CString(token);
            }
            else if (token.type == Token.Type.NULL)
            {
                Eat(Token.Type.NULL);
                return new Null();
            }
            else if(token.type == Token.Type.LPAREN)
            {
                Eat(Token.Type.LPAREN);
                Types result = Expr();
                result.inParen = true;
                Eat(Token.Type.RPAREN);
                return result;
            }
            else if (token.type == Token.Type.FUNCTION)
                return FunctionCatch();
            else if (token.type == Token.Type.LAMBDA)
                return FunctionCatch(null, true);
            else
            {
                Types result = Variable();
                return result;
            }
        }      

        public Types Term()
        {
            Types result = Factor();
            Token token = null;
            while (current_token.type == Token.Type.MUL || current_token.type == Token.Type.DIV)
            {
                token = current_token;
                if(token.type == Token.Type.MUL)
                {
                    Eat(Token.Type.MUL);
                }
                if (token.type == Token.Type.DIV)
                {
                    Eat(Token.Type.DIV);
                }
                result = new BinOp(result, token, Factor(), current_block);
                result.assingBlock = current_block;
            }
            return result;
        }

        public Types Math()
        {
            Types result = Term();
            Token token = null;
            while (current_token.type == Token.Type.PLUS || current_token.type == Token.Type.MINUS)
            {
                token = current_token;
                if (token.type == Token.Type.PLUS)
                {
                    Eat(Token.Type.PLUS);
                }
                if (token.type == Token.Type.MINUS)
                {
                    Eat(Token.Type.MINUS);
                }
                result = new BinOp(result, token, Term(), current_block);
                result.assingBlock = current_block;
            }
            return result;
        }

        public Types Comp()
        {
            Types result = Math();
            Token token = null;
            while (current_token.type == Token.Type.EQUAL || current_token.type == Token.Type.NOTEQUAL || current_token.type == Token.Type.MORE || current_token.type == Token.Type.LESS)
            {
                token = current_token;
                if (token.type == Token.Type.EQUAL)
                {
                    Eat(Token.Type.EQUAL);
                }
                else if (token.type == Token.Type.NOTEQUAL)
                {
                    Eat(Token.Type.NOTEQUAL);
                }
                else if (token.type == Token.Type.MORE)
                {
                    Eat(Token.Type.MORE);
                }
                else if (token.type == Token.Type.LESS)
                {
                    Eat(Token.Type.LESS);
                }
                result = new BinOp(result, token, Math(), current_block);
                result.assingBlock = current_block;
            }
            return result;
        }

        public Types Expr()
        {
            Types result = Comp();
            Token token = null;
            while (current_token.type == Token.Type.AND || current_token.type == Token.Type.OR)
            {
                token = current_token;
                if (token.type == Token.Type.AND)
                {
                    Eat(Token.Type.AND);
                }
                if (token.type == Token.Type.OR)
                {
                    Eat(Token.Type.OR);
                }
                result = new BinOp(result, token, Comp(), current_block);
                result.assingBlock = current_block;                
            }
            return result; 
        }

        public Types Parse()
        {
            brekall = false;
            tmpcount = 0;
            Types node = Program();
            if (current_token.type != Token.Type.EOF)
            {
                Error("Expected EOF, but found " + current_token.type);
                return null;
            }
            return node;
        }

        public Types Interpret()
        {
            Types tree = Parse();
            return tree;
        }

        public Types Program()
        {
            Types node = CompoundStatement();            
            return node;
        }

        public Types CompoundStatement(string assginTo = "")
        {
            Block save_block = current_block;
            Block root = new Block(this);
            if (current_block_with_variable != null)
            {
                foreach (KeyValuePair<string, Assign> v in current_block_with_variable.variables)
                {
                    if(v.Value.Right is Lambda)
                        root.variables[v.Key] = v.Value;
                }
                current_block_with_variable = null;
            }
            root.assignTo = assginTo;
            root.Parent = current_block;
            current_block = root;
            List<Types> nodes = StatementList();
            current_block = save_block;
            if (nodes == null) return null;
            foreach (Types node in nodes)
            {
                root.children.Add(node);
            }            
            return root;
        }

        public List<Types> StatementList()
        {
            Types statement = Statement();
            List<Types> result = new List<Types>();
            result.Add(statement);

            while((current_token.type == Token.Type.SEMI || (previous_token?.type == Token.Type.END && current_token.type != Token.Type.EOF && current_token.type != Token.Type.END)) && !brekall)
            {
                if(current_token.type == Token.Type.SEMI)
                    Eat(Token.Type.SEMI);
                result.Add(Statement());
            }

            if (current_token.type == Token.Type.ID)
            {
                Error("Found unexpected token " + current_token.Value);
                return null;
            }
            return result;
        }

        public Types Statement(bool eatEnd = true)
        {
            if (current_token.type == Token.Type.BEGIN)
            {
                Eat(Token.Type.BEGIN);
                Types node = CompoundStatement();
                if (eatEnd)
                    Eat(Token.Type.END);
                return node;
            }
            else if (current_token.type == Token.Type.LSQUARE)
            {
                Types t = AttributeCatch();
                attributes.Clear();
                return t;
            }
            else if (current_token.type == Token.Type.IMPORT)
                return Import();
            else if (current_token.type == Token.Type.IF)
                return ConditionCatch();
            else if (current_token.type == Token.Type.ID)
                return AssignmentStatement();
            else if (current_token.type == Token.Type.CLASS)
                return DeclareVariable();
            else if (current_token.type == Token.Type.FOR)
                return DeclareFor();
            else if (current_token.type == Token.Type.WHILE)
                return DeclareWhile();
            else if (current_token.type == Token.Type.INTERFACE)
                return DeclareVariable();
            else if (current_token.type == Token.Type.FUNCTION)
                return FunctionCatch();
            else if (current_token.type == Token.Type.NEWCLASS)
                return DeclareClass();
            else if (current_token.type == Token.Type.NEWINTERFACE)
                return DeclareInterface();
            else if (current_token.type == Token.Type.NEWFUNCTION)
                return DeclareFunction();
            else if (current_token.type == Token.Type.NEWLAMBDA)
                return DeclareLambda();
            else if (current_token.type == Token.Type.LAMBDA)
                return FunctionCatch(null, true);
            else if (current_token.type == Token.Type.INC)
            {
                Eat(Token.Type.INC);
                return new UnaryOp(current_token, Factor());
            }
            else if (current_token.type == Token.Type.DEC)
            {
                Eat(Token.Type.DEC);
                return new UnaryOp(current_token, Factor());
            }
            else if (current_token.type == Token.Type.RETURN)
            {
                if (main_block_type != Block.BlockType.FUNCTION)
                {
                    Error("return can be used only inside function block");
                    return null;
                }
                Token token = current_token;
                Eat(Token.Type.RETURN);
                Types returnv = null;
                if (current_token.type != Token.Type.SEMI)
                    returnv = Expr();
                return new UnaryOp(token, returnv, current_block);
            }
            else if (current_token.type == Token.Type.DYNAMIC)
            {
                Token modifer = current_token;
                current_modifer.Add(current_token);
                Eat(Token.Type.DYNAMIC);
                if (current_token.type == Token.Type.NEWCLASS ||
                    current_token.type == Token.Type.NEWFUNCTION ||
                    current_token.type == Token.Type.OPERATOR ||
                    current_token.type == Token.Type.ID)
                {
                    Types type = Statement();
                    current_modifer.Remove(modifer);
                    return type;
                }
                Error("The dynamic modifer can modify only class, function or variable");
                return null;
            }
            else if (current_token.type == Token.Type.STATIC)
            {
                Token modifer = current_token;
                current_modifer.Add(current_token);
                Eat(Token.Type.STATIC);
                if (current_token.type == Token.Type.NEWCLASS ||
                    current_token.type == Token.Type.NEWFUNCTION ||
                    current_token.type == Token.Type.OPERATOR ||
                    current_token.type == Token.Type.DYNAMIC ||
                    current_token.type == Token.Type.ID)
                {
                    Types type = Statement();
                    current_modifer.Remove(modifer);
                    return type;
                }
                Error("The static modifer can modify only class, function or variable and must be before dynamic modifer");
                return null;
            }
            else if (current_token.type == Token.Type.EXTERNAL)
            {
                Token modifer = current_token;
                current_modifer.Add(current_token);
                Eat(Token.Type.EXTERNAL);
                if (current_token.type == Token.Type.STATIC ||
                    current_token.type == Token.Type.DYNAMIC ||
                    current_token.type == Token.Type.OPERATOR ||
                    current_token.type == Token.Type.NEWCLASS ||
                    current_token.type == Token.Type.NEWFUNCTION ||
                    current_token.type == Token.Type.ID ||
                    current_token.type == Token.Type.NEWINTERFACE)
                {
                    Types type = Statement();
                    current_modifer.Remove(modifer);
                    return type;
                }
                Error("The external modifer can modify only class, interface, function or variable and must be before static and dynamic modifer");
                return null;
            }
            else if (current_token.type == Token.Type.OPERATOR)
            {
                Token modifer = current_token;
                current_modifer.Add(current_token);
                Eat(Token.Type.OPERATOR);
                if (current_token.type == Token.Type.NEWFUNCTION)
                {
                    Types type = Statement();
                    current_modifer.Remove(modifer);
                    return type;
                }
                Error("The operator modifer can modify only function");
                return null;
            }
            return new NoOp();
        }

        public Types Import()
        {
            Token im = null;
            Eat(Token.Type.IMPORT);
            Token result = current_token;
            string _as = "";
            Eat(Token.Type.ID);
            if(current_token.type == Token.Type.AS)
            {
                Eat(Token.Type.AS);
                _as = current_token.Value;                
                Eat(current_token.type);
            }
            im = new Token(Token.Type.STRING, result.Value, result.Pos, current_file);
            
            Import i = null;
            if(_as != "")
            {
                i = new Import(im, current_block, this, _as);
                imports.Add(_as, i);
            }
            else
            {
                i = new Import(im, current_block, this);
                imports.Add(i.GetName(), i);
            }            

            return i;
        }

        List<_Attribute> attributes = new List<_Attribute>();
        public Types AttributeCatch()
        {            
            Eat(Token.Type.LSQUARE);
            ParameterList plist = null;
            Token token = current_token;
            if (current_token.type == Token.Type.ID)
                Eat(Token.Type.ID);
            else
                Eat(Token.Type.CLASS);
            if(current_token.type == Token.Type.LPAREN)
            {
                plist = Parameters();                
            }
            Eat(Token.Type.RSQUARE);

            attributes.Add(new _Attribute(token, plist) { assingBlock = current_block });

            if (current_token.type == Token.Type.LSQUARE)
                return AttributeCatch();
            else if (current_token.type == Token.Type.NEWFUNCTION)
                return DeclareFunction();
            else if (current_token.type == Token.Type.NEWCLASS)
                return DeclareClass();
            else if (current_token.type == Token.Type.NEWINTERFACE)
                return DeclareInterface();
            return DeclareVariable();
        }

        public Types DeclareWhile()
        {
            Eat(Token.Type.WHILE);
            Eat(Token.Type.LPAREN);
            Types f = Expr();
            Eat(Token.Type.RPAREN);
            Block block = CatchBlock(Block.BlockType.WHILE);
            return new While(f, block);
        }

        public Types DeclareFor()
        {
            Eat(Token.Type.FOR);
            Eat(Token.Type.LPAREN);
            if(current_token.type == Token.Type.CLASS || current_token.type == Token.Type.INTERFACE)
            {
                Token c = current_token;
                if (current_token.type == Token.Type.INTERFACE) Eat(Token.Type.INTERFACE);
                else Eat(Token.Type.CLASS);
                Token n = current_token;
                Eat(Token.Type.ID);
                Eat(Token.Type.IN);
                Types f = Expr();
                Eat(Token.Type.RPAREN);
                Block block = CatchBlock(Block.BlockType.FOR);
                return new For(new Variable(n, current_block, c), f, block);
            }
            Eat(Token.Type.RPAREN);
            return null;
        }

        public Types DeclareLambda()
        {
            Eat(Token.Type.NEWLAMBDA);
            Variable id = new Variable(current_token, current_block, new Token(Token.Type.LAMBDA, "lambda"));
            Eat(Token.Type.ID);
            Eat(Token.Type.ASIGN);
            bool inbegin = false;
            if (current_token.type == Token.Type.BEGIN)
            {
                inbegin = true;
                Eat(Token.Type.BEGIN);
            }
            ParameterList p = Parameters(true);
            Eat(Token.Type.DEFINERETURN);
            Types expresion = Expr();
            if (inbegin)
                Eat(Token.Type.END);
            Lambda l =  new Lambda(id, expresion, p);

            return l;
        }

        public Block CatchBlock(Block.BlockType btype, bool eatEnd = true, Block ablock = null)
        {
            Block _bloc;
            current_block_with_variable = ablock;
            Block.BlockType last_block_type = current_block_type;
            current_block_type = btype;
            if(btype != Block.BlockType.CONDITION && btype != Block.BlockType.FOR)
                main_block_type = btype;
            Block save_block = current_block;
            Types block = Statement(eatEnd);
            if (!(block is Block))
            {
                _bloc = new Block(this);
                _bloc.children.Add(block);
            }
            else _bloc = (Block)block;
            _bloc.Parent = save_block;
            if (block == null || block is NoOp) return null;
            _bloc.Type = current_block_type;
            current_block_type = last_block_type;
            return _bloc;
        }

        public Types ConditionCatch()
        {
            Eat(Token.Type.IF);
            Eat(Token.Type.LPAREN);
            Types condition = Expr();
            Eat(Token.Type.RPAREN);

            Block block = CatchBlock(Block.BlockType.CONDITION);
            if (block == null) return null;            

            Dictionary<Types, Block> conditions = new Dictionary<Types, Block>();
            conditions.Add(condition, block);
            
            while (current_token.type == Token.Type.ELSEIF)
            {
                Eat(Token.Type.LPAREN);
                condition = Expr();
                Eat(Token.Type.RPAREN);

                block = CatchBlock(Block.BlockType.CONDITION);
                if (block == null) return null;

                conditions.Add(condition, block);
            }
            
            if(current_token.type == Token.Type.ELSE)
            {
                Eat(Token.Type.ELSE);                

                block = CatchBlock(Block.BlockType.CONDITION);
                if (block == null) return null;

                conditions.Add(new NoOp(), block);
            }

            return new If(conditions);
        }

        public Types FunctionCatch(Token fname = null, bool isLambda = false)
        {
            if (fname == null)
            {
                fname = current_token;
                if (!isLambda)
                    Eat(Token.Type.FUNCTION);
                else
                    Eat(Token.Type.LAMBDA);
            }      
            if(current_token.type == Token.Type.RPAREN || current_token.type == Token.Type.COMMA)
            {
                UnaryOp up = new UnaryOp(new Token(Token.Type.CALL, "call", current_token_pos, current_file), fname, null, current_block, false);
                up.asArgument = true;
                return up;
            }
            else if (current_token.type == Token.Type.SEMI)
            { 
                return new UnaryOp(new Token(Token.Type.CALL, "call", current_token_pos, current_file), fname, null, current_block, true);
            }
            else
            {
                ParameterList p = Parameters();
                return new UnaryOp(new Token(Token.Type.CALL, "call", current_token_pos, current_file), fname, p, current_block, true);
            }
        }

        public Types DeclareFunction()
        {
            List<_Attribute> att = new List<_Attribute>(attributes);
            Eat(Token.Type.NEWFUNCTION);
            Token name = current_token;
            if (current_token.type == Token.Type.FUNCTION)
                Eat(Token.Type.FUNCTION);
            else
                Eat(Token.Type.ID);
            if (brekall) return null;
            Block sb = current_block;
            current_block = new Block(this);
            current_block.Parent = sb;
            ParameterList p = Parameters(true);
            Token returnt = null;
            bool returnrArray = false;
            if(current_token.type == Token.Type.DEFINERETURN)
            {
                Eat(Token.Type.DEFINERETURN);
                returnt = current_token;
                if (current_token.type == Token.Type.ID)
                    Eat(Token.Type.ID);
                else if (current_token.type == Token.Type.VOID)
                    Eat(Token.Type.VOID);
                else if (current_token.type == Token.Type.INTERFACE)
                    Eat(Token.Type.INTERFACE);
                else
                    Eat(Token.Type.CLASS);

                if(current_token.type == Token.Type.LSQUARE)
                {
                    Eat(Token.Type.LSQUARE);
                    returnrArray = true;
                    Eat(Token.Type.RSQUARE);
                }
            }
            Block sendb = current_block;
            current_block = sb;
            Block _bloc = CatchBlock(Block.BlockType.FUNCTION, false, sendb);
            if (brekall) return null;
            string sname = name.Value;
            if (isModifer(Token.Type.OPERATOR))
            {                
                name = new Token(Token.Type.ID, "operator_" + name.Value, name.Pos, name.File);
                sname = "operator " + sname;
            }
            Function func = new Function(name, _bloc, p, returnt, this);
            func.returnAsArray = returnrArray;
            func.assingBlock = current_block;
            func.assignTo = current_block.assignTo;
            func.attributes = att;
            if (isModifer(Token.Type.STATIC))
            {
                func.isStatic = true;
                func._static = getModifer(Token.Type.STATIC);
            }
            if (isModifer(Token.Type.EXTERNAL))
            {
                func.isExternal = true;
                func._external = getModifer(Token.Type.EXTERNAL);
            }
            if (isModifer(Token.Type.DYNAMIC))
            {
                func.isDynamic = true;
                func._dynamic = getModifer(Token.Type.DYNAMIC);
            }
            if (isModifer(Token.Type.OPERATOR))
            {
                func.isOperator = true;
            }
            if (func.isConstructor)
            {
                sname = "constructor "+sname;
                func._constuctor = name;
            }
            current_block.SymbolTable.Add(sname, func);
            if(current_token.type == Token.Type.END)
                Eat(Token.Type.END);
            return func;
        }

        public bool isModifer(Token.Type type)
        {
            foreach (Token t in current_modifer)
                if (t.type == type)
                    return true;
            return false;
        }

        public Token getModifer(Token.Type type)
        {
            foreach (Token t in current_modifer)
                if (t.type == type)
                    return t;
            return null;
        }

        public Types DeclareInterface()
        {
            List<_Attribute> att = new List<_Attribute>(attributes);
            Eat(Token.Type.NEWINTERFACE);
            Token name = current_token;
            Eat(Token.Type.ID);
            List<Token> parents = new List<Token>();
            if (current_token.type == Token.Type.COLON)
            {
                Eat(Token.Type.COLON);
                Token parent = current_token;
                parents.Add(parent);
                if(current_token.type == Token.Type.CLASS)
                    Eat(Token.Type.CLASS, "class or interface for inheritance not found");
                else
                    Eat(Token.Type.INTERFACE, "class or interface for inheritance not found");
                while (current_token.type == Token.Type.COMMA)
                {
                    Eat(Token.Type.COMMA);
                    parent = current_token;
                    parents.Add(parent);
                    if (current_token.type == Token.Type.CLASS)
                        Eat(Token.Type.CLASS, "class or interface for inheritance not found");
                    else
                        Eat(Token.Type.INTERFACE, "class or interface for inheritance not found");
                }
            }
            Eat(Token.Type.BEGIN);

            Block.BlockType last_block_type = current_block_type;
            current_block_type = Block.BlockType.INTERFACE;
            Block block = (Block)CompoundStatement();
            if (brekall) return null;
            block.Type = Block.BlockType.INTERFACE;
            block.assignTo = name.Value;
            current_block_type = last_block_type;

            Interface c = new Interface(name, block, parents);
            c.assingBlock = block;
            c.attributes = att;

            if (isModifer(Token.Type.EXTERNAL))
            {
                c.isExternal = true;
                c._external = getModifer(Token.Type.EXTERNAL);
            }
            if (isModifer(Token.Type.DYNAMIC))
            {
                c.isDynamic = true;
                c._dynamic = getModifer(Token.Type.DYNAMIC);
            }

            current_block.SymbolTable.Add(name.Value, c);
            Eat(Token.Type.END);
            return c;
        }

        public Types DeclareClass()
        {
            List<_Attribute> att = new List<_Attribute>(attributes);
            Eat(Token.Type.NEWCLASS);
            Token name = current_token;
            Eat(Token.Type.ID);

            List<string> garg = new List<string>();
            if (current_token.type == Token.Type.LESS)
            {
                Eat(Token.Type.LESS);
                while (current_token.type != Token.Type.MORE)
                {
                    if (current_token.type == Token.Type.COMMA)
                    {
                        garg.Add(previous_token.Value);
                        Eat(Token.Type.COMMA);
                    }
                    else Eat(Token.Type.ID);
                }
                if (previous_token.type != Token.Type.LESS)
                    garg.Add(previous_token.Value);
                else
                    Error("You need specify generic arguments!");
                Eat(Token.Type.MORE);
            }

            List<Token> parents = new List<Token>();
            if(current_token.type == Token.Type.COLON)
            {
                Eat(Token.Type.COLON);
                Token parent = current_token;
                parents.Add(parent);
                if (current_token.type == Token.Type.CLASS)
                    Eat(Token.Type.CLASS, "class or interface for inheritance not found");
                else
                    Eat(Token.Type.INTERFACE, "class or interface for inheritance not found");
                while (current_token.type == Token.Type.COMMA)
                {
                    Eat(Token.Type.COMMA);
                    parent = current_token;
                    parents.Add(parent);
                    if (current_token.type == Token.Type.CLASS)
                        Eat(Token.Type.CLASS, "class or interface for inheritance not found");
                    else
                        Eat(Token.Type.INTERFACE, "class or interface for inheritance not found");
                }
            }            

            Eat(Token.Type.BEGIN);

            Block.BlockType last_block_type = current_block_type;
            current_block_type = Block.BlockType.CLASS;
            Block block = (Block)CompoundStatement(assginTo: name.Value);
            if (block == null) return null;
            block.Type = Block.BlockType.CLASS;
            current_block_type = last_block_type;

            Class c = new Class(name, block, parents);
            c.assingBlock = block;
            c.SetGenericArgs(garg);
            c.attributes = att;

            if (isModifer(Token.Type.EXTERNAL))
            {
                c.isExternal = true;
                c._external = getModifer(Token.Type.EXTERNAL);
            }
            if (isModifer(Token.Type.DYNAMIC))
            {
                c.isDynamic = true;
                c._dynamic = getModifer(Token.Type.DYNAMIC);
            }

            current_block.SymbolTable.Add(name.Value, c);
            Eat(Token.Type.END);
            return c;
        }

        public Types DeclareVariable(Token sDateType = null, Types have = null)
        {
            List<_Attribute> att = new List<_Attribute>(attributes);
            List<string> garg = new List<string>();
            Token dateType = null;
            //int size = -1;
            bool isArray = false;
            if (sDateType == null)
            {
                dateType = current_token;
                if (!current_block.SymbolTable.Find(dateType.Value))
                {
                    Error("The date type " + dateType.Value + " not a found!");
                    return null;
                }
                if (current_token.type == Token.Type.INTERFACE)
                    Eat(Token.Type.INTERFACE);
                else
                    Eat(Token.Type.CLASS);

                if (current_token.type == Token.Type.LESS)
                {
                    Eat(Token.Type.LESS);
                    while (current_token.type != Token.Type.MORE)
                    {
                        if (current_token.type == Token.Type.COMMA)
                        {
                            garg.Add(previous_token.Value);
                            Eat(Token.Type.COMMA);
                        }
                        else Eat(current_token.type);
                    }
                    if (previous_token.type != Token.Type.LESS)
                        garg.Add(previous_token.Value);
                    else
                        Error("You need specify generic arguments!");
                    Eat(Token.Type.MORE);
                }
                if (current_token.type == Token.Type.LSQUARE)
                {
                    Eat(Token.Type.LSQUARE);
                    /*if(current_token.type == Token.Type.INTEGER)
                    {
                        size = Int32.Parse(current_token.Value);
                        Eat(Token.Type.INTEGER);
                    }*/
                    isArray = true;
                    Eat(Token.Type.RSQUARE);
                }
            }
            else
            {                
                dateType = sDateType;
                if(have != null)
                {
                    isArray = ((Variable)have).isArray;
                }
            }

            Types left = Variable(dateType);
            if(left is Variable)
            {
                (left as Variable).genericArgs = garg;
                if (isArray)
                    (left as Variable).MadeArray(true);
            }     
            if(current_token.type == Token.Type.SET)
            {
                Eat(Token.Type.SET);
                Eat(Token.Type.BEGIN);

                Types get_block = null, set_block = null;
                string type = "";

                while(current_token.Value == "get" || current_token.Value == "set")
                {
                    type = current_token.Value;
                    Eat(current_token.type);
                    Types b = Statement();
                    if (type == "set") set_block = b;
                    else get_block = b;
                }

                Eat(Token.Type.END);

                Properties p = new Properties(left, get_block, set_block, current_block);
                p.assingBlock = current_block;
                if (get_block is Block bg)
                    bg.Type = Block.BlockType.PROPERTIES;
                if (set_block is Block bs)
                    bs.Type = Block.BlockType.PROPERTIES;
                return p;
            }
            if(current_token.type == Token.Type.ASIGN)
            {
                Token token = current_token;
                Eat(Token.Type.ASIGN);
                Types right = Expr();
                Types node = new Assign(left, token, right, current_block) { attributes = att };               
                node.assingBlock = current_block;
                return node;
            }
            else if (current_token.type == Token.Type.SEMI)
            {
                Types node = new Assign(left, new Token(Token.Type.ASIGN, "="), new Null(), current_block) { attributes = att };
                node.assingBlock = current_block;
                return node;
            }
            else
            {
                return left;
            }
        }

        public Types AssignmentStatement()
        {
            Token saveToken = current_token;
            Types left = Variable();
            if (left is UnaryOp)
                return left;
            if(current_token.type == Token.Type.ID)
            {
                return DeclareVariable(saveToken, left);
            }
            Token token = current_token;
            if (current_token.type == Token.Type.LPAREN)
            {
                return FunctionCatch(((Variable)left).getToken());
            }
            if (current_token.type == Token.Type.INC)
            {                
                UnaryOp p = new UnaryOp(current_token, left);
                p.post = true;
                Eat(Token.Type.INC);
                return p;
            }
            else if (current_token.type == Token.Type.DEC)
            {                
                UnaryOp p = new UnaryOp(current_token, left);
                p.post = true;
                Eat(Token.Type.DEC);
                return p;
            }
            Eat(Token.Type.ASIGN); 
            Types right = Expr();
            Types node = new Assign(left, token, right, current_block);
            node.assingBlock = current_block;
            return node;
        }

        public Types Variable(Token dateType = null)
        {            
            Types node = new Variable(current_token, current_block, dateType);
            node.assingBlock = current_block;
            if (current_token.type == Token.Type.TRUE)
                Eat(Token.Type.TRUE);
            else if (current_token.type == Token.Type.FALSE)
                Eat(Token.Type.FALSE);
            else if (current_token.type == Token.Type.CLASS)
                Eat(Token.Type.CLASS);
            else
                Eat(Token.Type.ID);           

            if(current_token.type == Token.Type.LPAREN)
            {
                return FunctionCatch(((Variable)node).getToken());
            }
            if(current_token.type == Token.Type.DEFINERETURN)
            {
                ParameterList plist = new ParameterList(false);
                plist.Parameters.Add(node);
                Eat(Token.Type.DEFINERETURN);
                Lambda exp = new Lambda(null, Expr(), plist);
                exp.isCallInArgument = true;
                return exp;
            }
            if(current_token.type == Token.Type.AS)
            {
                Eat(Token.Type.AS);
                ((Variable)node).AsDateType = current_token;
                if (current_token.type == Token.Type.ID)
                    Eat(Token.Type.ID);
                else if (current_token.type == Token.Type.INTERFACE)
                    Eat(Token.Type.INTERFACE);
                else
                    Eat(Token.Type.CLASS);
                return node;
            }
            if (current_token.type == Token.Type.IS)
            {
                Token tokis = current_token;
                Eat(Token.Type.IS);
                Token t = current_token;
                if (current_token.type == Token.Type.INTERFACE)
                    Eat(Token.Type.INTERFACE);
                else if (current_token.type == Token.Type.ID)
                    Eat(Token.Type.ID);
                else
                    Eat(Token.Type.CLASS);
                return new BinOp(node, tokis, t, current_block);
            }
            if (current_token.type == Token.Type.LSQUARE)
            {
                Eat(Token.Type.LSQUARE);
                if(current_token.type == Token.Type.RSQUARE)
                {
                    Eat(Token.Type.RSQUARE);
                    if (current_token.type == Token.Type.ID)
                    {
                        ((Variable)node).isArray = true;
                        ((Variable)node).isDateType = true;
                        return node;
                    }
                }
                ((Variable)node).setKey(Expr());
                Eat(Token.Type.RSQUARE);
            }
            if (current_token.type == Token.Type.INC)
            {
                UnaryOp p = new UnaryOp(current_token, node);
                p.post = true;
                Eat(Token.Type.INC);
                return p;
            }
            else if (current_token.type == Token.Type.DEC)
            {
                UnaryOp p = new UnaryOp(current_token, node);
                p.post = true;
                Eat(Token.Type.DEC);
                return p;
            }
            return node;
        }        
    }
}

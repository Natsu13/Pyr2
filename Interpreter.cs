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
        string text;
        int pos = 0;
        Token current_token;
        Token previous_token;
        List<Token> current_modifer = new List<Token>();
        char current_char;
        Block current_block;
        int current_token_pos;
        Block.BlockType current_block_type = Block.BlockType.NONE;
        string current_file = "";
        //public SymbolTable symbolTable;
        public static List<Error> semanticError = new List<Compilator.Error>();
        public enum ErrorType { INFO, WARNING, ERROR };
        public static Dictionary<string, string> fileList = new Dictionary<string, string>();
        public bool isConsole = false;
        public bool brekall = false;

        /// Interpret settings
        public static bool _REDECLARATION = false;
        public static bool _WAITFORPAGELOAD = true;

        public Interpreter(string text, string filename = "")
        {
            this.text = text;
            fileList.Add(filename, text);
            pos = 0;
            current_char = text[pos];
            current_file = filename;
            current_block = new Block(this, true);
            current_token = GetNextToken();                       
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

                current_token_pos = pos;

                if (Char.IsDigit(current_char))
                    return Number();

                if (current_char == '"' || current_char == '\'')
                {
                    return String(current_char);
                }

                if (Char.IsLetterOrDigit(current_char) || current_char == '_')
                    return Id();

                if (current_char == '-' && Peek() == '>') {
                    Advance(); Advance();
                    return new Token(Token.Type.DEFINERETURN, "->", current_token_pos, current_file);
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
                if (current_char == '=') { Advance(); return new Token(Token.Type.ASIGN, '=', current_token_pos, current_file); }
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

        public char Peek()
        {
            int peek_pos = pos + 1;
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
            while(current_char != '\0' && (Char.IsLetterOrDigit(current_char) || current_char == '_' || current_char == '.'))
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
                    if(((Assign)tp).Right is Null)
                    {
                        if(((Assign)tp).Left is Variable)
                            return new Token(Token.Type.ID, result, current_token_pos, current_file);
                        else
                            return new Token(((Variable)((Assign)tp).Left).getToken().type, result, current_token_pos, current_file);
                    }
                    else if(((Assign)tp).Right is UnaryOp)
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
            while (current_char != '*' && Peek() != '/')
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
            Eat(Token.Type.LPAREN);
            while(current_token.type != Token.Type.RPAREN && current_token.type != Token.Type.EOF && !brekall)
            {
                Token vtype = null;
                if (declare)
                {
                    vtype = current_token;
                    Eat(Token.Type.CLASS);
                    Token vname = current_token;
                    Eat(Token.Type.ID);
                    plist.parameters.Add(new Variable(vname, current_block, vtype));
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
                {
                    Error("Class with name " + current_token.Value + " not found!");
                    return null;
                }
                Eat(Token.Type.CLASS);
                if (current_token.type == Token.Type.SEMI)
                {
                    return new UnaryOp(token, className);
                }
                else
                {
                    ParameterList p = Parameters();
                    return new UnaryOp(token, className, p);
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
                Eat(Token.Type.RPAREN);
                return result;
            }
            else if (token.type == Token.Type.FUNCTION)
                return FunctionCatch();
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
            }
            return result;
        }

        public Types Comp()
        {
            Types result = Math();
            Token token = null;
            while (current_token.type == Token.Type.EQUAL || current_token.type == Token.Type.NOTEQUAL)
            {
                token = current_token;
                if (token.type == Token.Type.EQUAL)
                {
                    Eat(Token.Type.EQUAL);
                }
                if (token.type == Token.Type.NOTEQUAL)
                {
                    Eat(Token.Type.NOTEQUAL);
                }
                result = new BinOp(result, token, Math(), current_block);
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
            }
            return result; 
        }

        public Types Parse()
        {
            brekall = false;
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

        public Types CompoundStatement()
        {
            Block save_block = current_block;
            Block root = new Block(this);
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
                if(eatEnd)
                    Eat(Token.Type.END);
                return node;
            }        
            else if(current_token.type == Token.Type.IF)
                return ConditionCatch();
            else if (current_token.type == Token.Type.ID)
                return AssignmentStatement();
            else if (current_token.type == Token.Type.CLASS)
                return DeclareVariable();
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
            else if (current_token.type == Token.Type.RETURN)
            {
                if (current_block_type != Block.BlockType.FUNCTION)
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
            else if(current_token.type == Token.Type.STATIC)
            {
                Token modifer = current_token;
                current_modifer.Add(current_token);
                Eat(Token.Type.STATIC);
                if (current_token.type == Token.Type.NEWCLASS || current_token.type == Token.Type.NEWFUNCTION || current_token.type == Token.Type.ID)
                {
                    Types type = Statement();
                    current_modifer.Remove(modifer);
                    return type;
                }
                Error("The static modifer can modify only class, function or variable");
                return null;
            }
            else if (current_token.type == Token.Type.EXTERNAL)
            {
                Token modifer = current_token;
                current_modifer.Add(current_token);
                Eat(Token.Type.EXTERNAL);
                if (current_token.type == Token.Type.STATIC || 
                    current_token.type == Token.Type.NEWCLASS || 
                    current_token.type == Token.Type.NEWFUNCTION || 
                    current_token.type == Token.Type.ID || 
                    current_token.type == Token.Type.NEWINTERFACE)
                {
                    Types type = Statement();
                    current_modifer.Remove(modifer);
                    return type;
                }
                Error("The external modifer can modify only class, interface, function or variable and must be before static modifer");
                return null;
            }
            return new NoOp();
        }

        public Block CatchBlock(Block.BlockType btype, bool eatEnd = true)
        {
            Block _bloc;
            Block.BlockType last_block_type = current_block_type;
            current_block_type = btype;
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

        public Types FunctionCatch(Token fname = null)
        {
            if (fname == null)
            {
                fname = current_token;
                Eat(Token.Type.FUNCTION);
            }
            if (current_token.type == Token.Type.SEMI)
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
            Eat(Token.Type.NEWFUNCTION);
            Token name = current_token;
            Eat(Token.Type.ID);
            if (brekall) return null;
            ParameterList p = Parameters(true);
            Token returnt = null;
            if(current_token.type == Token.Type.DEFINERETURN)
            {
                Eat(Token.Type.DEFINERETURN);
                returnt = current_token;
                if (current_token.type == Token.Type.ID)
                {
                    Error("Date type " + current_token.Value + " is unkown!");
                    return null;
                }
                if (current_token.type == Token.Type.VOID)
                {
                    Eat(Token.Type.VOID);
                }
                else if (current_token.type == Token.Type.INTERFACE)
                    Eat(Token.Type.INTERFACE);
                else
                    Eat(Token.Type.CLASS);
            }

            Block _bloc = CatchBlock(Block.BlockType.FUNCTION, false);
            if (brekall) return null;            

            Function func = new Function(name, _bloc, p, returnt, this);
            func.assingBlock = current_block;
            func.assignTo = current_block.assignTo;
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
            current_block.SymbolTable.Add(name.Value, func);
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

            if (isModifer(Token.Type.EXTERNAL))
            {
                c.isExternal = true;
                c._external = getModifer(Token.Type.EXTERNAL);
            }

            current_block.SymbolTable.Add(name.Value, c);
            Eat(Token.Type.END);
            return c;
        }

        public Types DeclareClass()
        {
            Eat(Token.Type.NEWCLASS);
            Token name = current_token;
            Eat(Token.Type.ID);
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
            Block block = (Block)CompoundStatement();
            if (block == null) return null;
            block.Type = Block.BlockType.CLASS;
            current_block_type = last_block_type;

            Class c = new Class(name, block, parents);
            c.assingBlock = block;

            if (isModifer(Token.Type.EXTERNAL))
            {
                c.isExternal = true;
                c._external = getModifer(Token.Type.EXTERNAL);
            }

            current_block.SymbolTable.Add(name.Value, c);
            Eat(Token.Type.END);
            return c;
        }

        public Types DeclareVariable()
        {
            Token dateType = current_token;
            if (!current_block.SymbolTable.Find(dateType.Value))
            {
                Error("The date type "+ dateType.Value+" not a found!");
                return null;
            }
            if (current_token.type == Token.Type.INTERFACE)
                Eat(Token.Type.INTERFACE);
            else
                Eat(Token.Type.CLASS);
            Types left = Variable(dateType);            
            if(current_token.type == Token.Type.ASIGN)
            {
                Token token = current_token;
                Eat(Token.Type.ASIGN);
                Types right = Expr();
                Types node = new Assign(left, token, right, current_block);
                node.assingBlock = current_block;
                return node;
            }
            else if (current_token.type == Token.Type.SEMI)
            {
                Types node = new Assign(left, new Token(Token.Type.ASIGN, "="), new Null(), current_block);
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
            Types left = Variable();
            Token token = current_token;
            if (current_token.type == Token.Type.LPAREN)
            {
                return FunctionCatch(((Variable)left).getToken());
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
            if (current_token.type == Token.Type.TRUE)
                Eat(Token.Type.TRUE);
            else if (current_token.type == Token.Type.FALSE)
                Eat(Token.Type.FALSE);
            else
                Eat(Token.Type.ID);
            return node;
        }        
    }
}

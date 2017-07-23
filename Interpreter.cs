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
        Token current_modifer;
        char current_char;
        Block current_block;
        Block.BlockType current_block_type = Block.BlockType.NONE;
        //public SymbolTable symbolTable;
        public static List<Error> semanticError = new List<Compilator.Error>();
        public enum ErrorType { INFO, WARNING, ERROR };

        public Interpreter(string text)
        {
            this.text = text;           
            pos = 0;
            current_char = text[pos];
            current_token = GetNextToken();
            current_block = new Block(this);               
        }
        
        public void Error(string error = "Error parsing input")
        {
            string rerr = error;
            string[] splt = text.Split('\n');
            rerr += "\n";
            int startl = text.Substring(0, pos).Count(t => t == '\n');
            rerr += " "+splt[startl].TrimStart();
            rerr += "\n";
            int alltl = 0;
            for (int q = 0; q < startl; q++) { alltl += splt[q].Length+1; }
            for (int q = -1+alltl+(splt[startl].TakeWhile(Char.IsWhiteSpace).Count()); q < pos - current_token.Value.Length; q++) rerr += " ";
            for (int q = 0; q < current_token.Value.Length; q++) rerr += "^";
            rerr += "\n";
            rerr += "Found at " + startl + ":" + ((pos - current_token.Value.Length) - (-1 + alltl + (splt[startl].TakeWhile(Char.IsWhiteSpace).Count())));
            Console.Write(rerr);
            //throw new Exception(rerr);
            //pos = text.Length+1;
            current_token = new Token(Token.Type.EOF, "EOF");
            Console.WriteLine();
            Console.ReadKey();
            System.Environment.Exit(1);
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
                }

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
                    return new Token(Token.Type.DEFINERETURN, "->");
                }
                if (current_char == '=') { Advance(); return new Token(Token.Type.ASIGN, '='); }
                if (current_char == ';') { Advance(); return new Token(Token.Type.SEMI, ';'); }
                if (current_char == ':') { Advance(); return new Token(Token.Type.COLON, ':'); }
                if (current_char == ',') { Advance(); return new Token(Token.Type.COMMA, ','); }
                if (current_char == '.') { Advance(); return new Token(Token.Type.DOT, '.'); }
                if (current_char == '+') { Advance(); return new Token(Token.Type.PLUS, '+'); }
                if (current_char == '-') { Advance(); return new Token(Token.Type.MINUS, '-'); }
                if (current_char == '*') { Advance(); return new Token(Token.Type.MUL, '*'); }
                if (current_char == '/') { Advance(); return new Token(Token.Type.DIV, '/'); }
                if (current_char == '(') { Advance(); return new Token(Token.Type.LPAREN, '('); }
                if (current_char == ')') { Advance(); return new Token(Token.Type.RPAREN, ')'); }
                if (current_char == '{') { Advance(); return new Token(Token.Type.BEGIN, '{'); }
                if (current_char == '}') { Advance(); return new Token(Token.Type.END, '}'); }               

                Error("Unexpeced token found");
            }            
            return new Token(Token.Type.EOF, "");
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
                result += current_char;
                Advance();
            }
            Advance();
            return new Token(Token.Type.STRING, result);
        }

        public Token Id()
        {
            string result = "";
            while(current_char != '\0' && (Char.IsLetterOrDigit(current_char) || current_char == '_'))
            {
                result += current_char;
                Advance();
            }

            if (Token.Reserved.ContainsKey(result))
                return Token.Reserved[result];
            else if (current_block.SymbolTable.Find(result))
                return new Token(Token.Type.CLASS, result);
            else
                return new Token(Token.Type.ID, result);
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
                return new Token(Token.Type.REAL, result);
            }
            else
            {
                return new Token(Token.Type.INTEGER, result);
            }
        }

        public ParameterList Parameters(bool declare = false)
        {
            ParameterList plist = new ParameterList(declare);
            Eat(Token.Type.LPAREN);
            while(current_token.type != Token.Type.RPAREN)
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
                    Error("Class with name "+current_token.Value+" not found!");
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
            else if(token.type == Token.Type.LPAREN)
            {
                Eat(Token.Type.LPAREN);
                Types result = Expr();
                Eat(Token.Type.RPAREN);
                return result;
            }
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

        public Types Expr()
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

        public Types Parse()
        {
            Types node = Program();
            if (current_token.type != Token.Type.EOF)
                Error("Expected EOF, but found "+current_token.type);
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

            while(current_token.type == Token.Type.SEMI || (previous_token?.type == Token.Type.END && current_token.type != Token.Type.EOF && current_token.type != Token.Type.END))
            {
                if(current_token.type == Token.Type.SEMI)
                    Eat(Token.Type.SEMI);
                result.Add(Statement());
            }

            if (current_token.type == Token.Type.ID)
                Error("Found unexpected token "+ current_token.Value);
            return result;
        }

        public Types Statement()
        {
            if (current_token.type == Token.Type.BEGIN)
            {
                Eat(Token.Type.BEGIN);
                Types node = CompoundStatement();
                Eat(Token.Type.END);
                return node;
            }
            else if (current_token.type == Token.Type.ID)
                return AssignmentStatement();
            else if (current_token.type == Token.Type.CLASS)
                return DeclareVariable();
            else if (current_token.type == Token.Type.NEWCLASS)
                return DeclareClass();
            else if (current_token.type == Token.Type.NEWFUNCTION)
                return DeclareFunction();
            else if (current_token.type == Token.Type.RETURN)
            {
                if (current_block_type != Block.BlockType.FUNCTION)
                    Error("return can be used only inside function block");
                Token token = current_token;
                Eat(Token.Type.RETURN);
                Types returnv = Expr();
                return new UnaryOp(token, returnv, current_block);
            }
            else if(current_token.type == Token.Type.STATIC)
            {
                current_modifer = current_token;
                Eat(Token.Type.STATIC);
                if (current_token.type == Token.Type.NEWCLASS || current_token.type == Token.Type.NEWFUNCTION || current_token.type == Token.Type.ID)
                {
                    Types type = Statement();
                    current_modifer = null;
                    return type;
                }
                Error("The static modifer can modify only class, function or variable");
            }
            return new NoOp();
        }

        public Types DeclareFunction()
        {
            Eat(Token.Type.NEWFUNCTION);
            Token name = current_token;
            Eat(Token.Type.ID);
            ParameterList p = Parameters(true);
            Token returnt = null;
            if(current_token.type == Token.Type.DEFINERETURN)
            {
                Eat(Token.Type.DEFINERETURN);
                returnt = current_token;
                if (current_token.type == Token.Type.ID)
                    Error("Date type " + current_token.Value + " is unkown!");
                Eat(Token.Type.CLASS);
            }

            Block.BlockType last_block_type = current_block_type;
            current_block_type = Block.BlockType.FUNCTION;
            Types block = Statement();
            ((Block)block).Type = Block.BlockType.FUNCTION;
            current_block_type = last_block_type;

            Function func = new Function(name, block, p, returnt, this);
            if (current_modifer?.type == Token.Type.STATIC)
                func.isStatic = true;
            current_block.SymbolTable.Add(name.Value, func);
            return func;
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
                Eat(Token.Type.CLASS, "class for inheritance not found");
                while(current_token.type == Token.Type.COMMA)
                {
                    Eat(Token.Type.COMMA);
                    parent = current_token;
                    parents.Add(parent);
                    Eat(Token.Type.CLASS, "class for inheritance not found");
                }
            }
            Eat(Token.Type.BEGIN);

            Block.BlockType last_block_type = current_block_type;
            current_block_type = Block.BlockType.CLASS;
            Block block = (Block)CompoundStatement();
            block.Type = Block.BlockType.CLASS;
            current_block_type = last_block_type;

            Class c = new Class(name, block, parents);
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
            Eat(Token.Type.CLASS);
            Types left = Variable(dateType);
            if(current_token.type == Token.Type.ASIGN)
            {
                Token token = current_token;
                Eat(Token.Type.ASIGN);
                Types right = Expr();
                Types node = new Assign(left, token, right);
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
            Eat(Token.Type.ASIGN);
            Types right = Expr();
            Types node = new Assign(left, token, right);
            return node;
        }

        public Types Variable(Token dateType = null)
        {            
            Types node = new Variable(current_token, current_block, dateType);
            Eat(Token.Type.ID);
            return node;
        }        
    }
}

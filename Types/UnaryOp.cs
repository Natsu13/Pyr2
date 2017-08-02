using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class UnaryOp:Types
    {
        Token token, op, name;
        Types expr;
        ParameterList plist;
        Block block;
        public bool post = false;

        public UnaryOp(Token op, Types expr, Block block = null)
        {
            this.op = this.token = op;
            this.expr = expr;
            this.block = block;
        }
        public UnaryOp(Token op, Token name, ParameterList plist = null, Block block = null, bool endit = false)
        {
            this.op = this.token = op;
            this.name = name;
            this.plist = plist;
            this.block = block;
            this.endit = endit;
        }
        
        public override Token getToken() { return token; }
        public String Op { get { return Variable.GetOperatorStatic(op.type); } }
        public Types  Expr { get { return expr; } }
        public Token Name { get { return name; } }

        public override string Compile(int tabs = 0)
        {
            if(expr != null)
                expr.assingBlock = assingBlock;
            string tbs = DoTabs(tabs);
            string o = Variable.GetOperatorStatic(op.type);
            if(o == "call")
            {
                if (!block.SymbolTable.Find(name.Value))
                {                    
                    return "";
                }
                Types t = block.SymbolTable.Get(name.Value);
                if(t is Assign && ((Assign)t).Right is Lambda)
                {
                    if (plist == null)
                        return tbs + "lambda$" + name.Value + "()" + (endit ? ";" : "");
                    return tbs + "lambda$" + name.Value + "(" + plist.Compile() + ")" + (endit ? ";" : "");
                }
                if (name.Value == "js")
                {
                    string pl = plist.Compile();
                    return pl.Substring(1, pl.Length - 2).Replace("\\", "") + (pl.Substring(pl.Length - 2, 1) != ";"?";":"");
                }
                if (name.Value.Contains("."))
                {
                    string[] nnaml = name.Value.Split('.');
                    string nname = string.Join(".", nnaml.Take(nnaml.Length - 1)) + "." + ((Function)t).Name;
                    if (plist == null)
                        return tbs + nname + "()" + (endit ? ";" : "");
                    return tbs + nname + "(" + plist.Compile() + ")" + (endit ? ";" : "");
                }
                else
                {
                    if (plist == null)
                        return tbs + name.Value + "()" + (endit ? ";" : "");
                    return tbs + name.Value + "(" + plist.Compile() + ")" + (endit ? ";" : "");
                }
            }
            if (o == "new")
            {
                Types t = assingBlock.SymbolTable.Get(name.Value);
                string rt;
                if(((Class)t).assingBlock.SymbolTable.Find("constructor " + name.Value))
                {
                    Function f = (Function)(((Class)t).assingBlock.SymbolTable.Get("constructor " + name.Value));
                    rt = tbs + ((Class)t).Name.Value + "." + f.Name + "(" + plist?.Compile() + ")";
                }
                else
                    rt = tbs + "new " + name.Value + "(" + plist?.Compile() + ")";
                return rt;
            }
            if(o == "return")
            {
                if (expr == null)
                    return tbs + "return;";
                return tbs + "return " + expr.Compile() + ";";
            }
            return tbs + Variable.GetOperatorStatic(op.type) + expr.Compile() + (endit ? ";" : "");
        }

        public override void Semantic()
        {
            string o = Variable.GetOperatorStatic(op.type);
            if (o == "call")
            {
                if(block.Parent?.Parent == null)
                    Interpreter.semanticError.Add(new Error("Expecting a top level declaration", Interpreter.ErrorType.ERROR, name));
                if (block.assingBlock != null && !block.assingBlock.SymbolTable.Find(name.Value))
                    Interpreter.semanticError.Add(new Error("Function with name " + name.Value + " not found", Interpreter.ErrorType.ERROR, name));
            }
        }

        public override int Visit()
        {
            if (op.type == Token.Type.PLUS)
                return +expr.Visit();
            else if (op.type == Token.Type.MINUS)
                return -expr.Visit();
            return 0;
        }
    }
}

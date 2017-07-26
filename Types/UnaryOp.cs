﻿using System;
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
        bool endit;

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

        public Token Token { get { return token; } }
        public String Op { get { return Variable.GetOperatorStatic(op.type); } }
        public Types  Expr { get { return expr; } }
        public Token Name { get { return name; } }

        public override string Compile(int tabs = 0)
        {
            string tbs = DoTabs(tabs);
            string o = Variable.GetOperatorStatic(op.type);
            if(o == "call")
            {                
                if (!block.SymbolTable.Find(name.Value))
                {                    
                    return "";
                }
                if (name.Value == "js")
                {
                    string pl = plist.Compile();
                    return pl.Substring(1, pl.Length - 2).Replace("\\", "") + (pl.Substring(pl.Length - 2, 1) != ";"?";":"");
                }
                if (plist == null)
                    return tbs + name.Value + "()" + (endit ? ";" : "");
                return tbs + name.Value + "(" + plist.Compile() + ")" + (endit ? ";" : "");
            }
            if (o == "new")
            {
                return tbs+"new " + name.Value + "(" + plist?.Compile() + ")";
            }
            if(o == "return")
            {
                if (expr == null)
                    return tbs + "return;";
                return tbs + "return " + expr.Compile() + ";";
            }
            return tbs + Variable.GetOperatorStatic(op.type) + expr.Compile();
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

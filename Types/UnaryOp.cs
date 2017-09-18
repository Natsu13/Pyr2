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
        public bool post = false;
        public List<string> genericArgments = new List<string>();
        bool isArray = false;
        int arraySize = -1;
        public bool asArgument = false;

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
        public void MadeArray(int size) { isArray = true; arraySize = size; }

        public override string Compile(int tabs = 0)
        {
            if(expr != null)
                expr.assingBlock = assingBlock;
            string tbs = DoTabs(tabs);
            string o = Variable.GetOperatorStatic(op.type);
            if(o == "call")
            {
                bool isDynamic = false;
                if (!block.SymbolTable.Find(name.Value))
                {                    
                    Types q = block.SymbolTable.Get(name.Value.Split('.')[0]);
                    if (q is Class && ((Class)q).isDynamic) { isDynamic = true; }
                    if (q is Interface && ((Interface)q).isDynamic) { isDynamic = true; }
                    if(!isDynamic)
                        return "";
                }
                Types t = block.SymbolTable.Get(name.Value);
                if(t is Assign && ((Assign)t).Right is Lambda)
                {
                    if (plist == null)
                        return tbs + (inParen ? "(" : "") + "lambda$" + name.Value + "()" + (inParen ? ")" : "") + (endit ? ";" : "");
                    return tbs + (inParen ? "(" : "") + "lambda$" + name.Value + "(" + plist.Compile() + ")" + (inParen ? ")" : "") + (endit ? ";" : "");
                }
                if (name.Value == "js")
                {
                    string pl = plist.Compile();
                    return pl.Substring(1, pl.Length - 2).Replace("\\", "") + (pl.Substring(pl.Length - 2, 1) != ";"?";":"");
                }
                if (name.Value.Contains("."))
                {
                    string[] nnaml = name.Value.Split('.');
                    string nname = "";
                    if (isDynamic)
                        nname = name.Value;
                    else
                        nname = string.Join(".", nnaml.Take(nnaml.Length - 1)) + "." + ((Function)t).Name;
                    if (plist == null)
                        return tbs + (inParen ? "(" : "") + nname + "()" + (inParen ? ")" : "") + (endit ? ";" : "");
                    return tbs + (inParen ? "(" : "") + nname + "(" + plist.Compile() + ")" + (inParen ? ")" : "") + (endit ? ";" : "");
                }
                else
                {
                    if(asArgument)
                        return tbs + (inParen ? "(" : "") + name.Value + (inParen ? ")" : "") + (endit ? ";" : "");
                    if (plist == null)
                        return tbs + (inParen ? "(" : "") + name.Value + "()" + (inParen ? ")" : "") + (endit ? ";" : "");
                    return tbs + (inParen ? "(" : "") + name.Value + "(" + plist.Compile() + ")" + (inParen ? ")" : "") + (endit ? ";" : "");
                }
            }
            if (o == "new")
            {
                Types t = assingBlock.SymbolTable.Get(name.Value);
                if (t is Error) return "";
                string rt;

                string _name = "";
                if(t is Class)
                {
                    if (((Class)t).JSName != "") _name = ((Class)t).JSName;
                    else _name = ((Class)t).Name.Value;
                }

                if (((Class)t).assingBlock.SymbolTable.Find("constructor " + name.Value))
                {
                    Function f = (Function)(((Class)t).assingBlock.SymbolTable.Get("constructor " + name.Value));
                    if (isArray)
                        rt = tbs + "new Array(" + arraySize + ").fill(" + _name + "." + f.Name + "(" + plist?.Compile() + "))";                    
                    else
                        rt = tbs + _name + "." + f.Name + "(" + plist?.Compile() + ")";
                }
                else
                {
                    if (isArray)
                        rt = tbs + "new Array(" + arraySize + ").fill(new " + _name + "(" + plist?.Compile() + "))";
                    else
                        rt = tbs + "new " + _name + "(" + plist?.Compile() + ")";
                }
                return (inParen ? "(" : "") + rt + (inParen ? ")" : "");
            }
            if(o == "return")
            {
                if (expr == null)
                    return tbs + "return;";
                return tbs + "return " + expr.Compile() + ";";
            }
            if(post)
                return tbs + (inParen ? "(" : "") + expr.Compile() + Variable.GetOperatorStatic(op.type) + (inParen ? ")" : "") + (endit ? ";" : "");
            else
                return tbs + (inParen ? "(" : "") + Variable.GetOperatorStatic(op.type) + expr.Compile() + (inParen ? ")" : "") + (endit ? ";" : "");
        }

        public override void Semantic()
        {
            string o = Variable.GetOperatorStatic(op.type);
            if (o == "call")
            {
                if(block.Parent?.Parent == null)
                    Interpreter.semanticError.Add(new Error("Expecting a top level declaration", Interpreter.ErrorType.ERROR, name));
                if (block.assingBlock != null && !block.assingBlock.SymbolTable.Find(name.Value))
                {
                    Types t = block.assingBlock.SymbolTable.Get(name.Value.Split('.')[0]);
                    if (t is Class || t is Interface)
                    {
                        if (t is Class && ((Class)t).isDynamic) { return; }
                        if (t is Interface && ((Interface)t).isDynamic) { return; }
                    }
                    Interpreter.semanticError.Add(new Error("Function with name " + name.Value + " not found", Interpreter.ErrorType.ERROR, name));
                }
            }
            if(o == "new")
            {
                if (plist != null) plist.Semantic();
                if (!assingBlock.SymbolTable.Find(name.Value))
                {
                    Interpreter.semanticError.Add(new Error("Class '" + name.Value + "' not found", Interpreter.ErrorType.ERROR, name));
                }
                else
                {
                    Types t = assingBlock.SymbolTable.Get(name.Value);
                    if (((Class)t).GenericArguments.Count > 0)
                    {
                        if(((Class)t).GenericArguments.Count != genericArgments.Count)
                        {
                            Interpreter.semanticError.Add(new Error("You must specify all generic types when creating instance of class '" + name.Value + "'", Interpreter.ErrorType.ERROR, name));
                        }
                    }
                }
            }
            if (o == "return")
            {
                if (expr != null)
                    expr.Semantic();
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

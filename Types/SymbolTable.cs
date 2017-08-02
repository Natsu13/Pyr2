﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class SymbolTable
    {
        Dictionary<string, Types> table = new Dictionary<string, Types>();
        Dictionary<string, int> tableCounter = new Dictionary<string, int>();
        Dictionary<string, Type> tableType = new Dictionary<string, Type>();
        Interpreter interpret;
        Block assigment_block;        

        public SymbolTable(Interpreter interpret, Block assigment_block, bool first = false)
        {
            this.interpret = interpret;
            this.assigment_block = assigment_block;
            if (first)
            {
                Token TokenIIterable = new Token(Token.Type.ID, "IIterable");
                Block BlockIIterable = new Block(interpret) { Parent = assigment_block };
                Interface IIterable = new Interface(TokenIIterable, BlockIIterable, null)
                {
                    isExternal = true
                };
                Add("IIterable", IIterable);

                /// Date type string implicit
                // Add("string",   typeof(TypeString), new List<Token> { TokenIIterable });
                Token TokenString = new Token(Token.Type.ID, "string");
                Block BlockString = new Block(interpret) { Parent = assigment_block };
                Class String = new Class(TokenString, BlockString, new List<Token> { TokenIIterable })
                {
                    isExternal = true,
                    JSName = "String"
                };
                Add("string", String);
                /// Date type int implicit
                //Add("int",      typeof(TypeInt));
                Token TokenInt = new Token(Token.Type.ID, "int");
                Block BlockInt = new Block(interpret) { Parent = assigment_block };
                Class Int = new Class(TokenInt, BlockInt, null)
                {
                    isExternal = true,
                    JSName = "Number"
                };
                Add("int", Int);

                Add("bool",     typeof(TypeBool));                
            }
        }

        public static bool initialized = false;
        public void initialize()
        {
            if (initialized) return;
            initialized = true;
            /// Initialize String Class
            Block BlockString = ((Class)Get("string")).assingBlock;
            //Operator Equal
            new Assign(
                    new Variable(new Token(Token.Type.ID, "length"), BlockString, new Token(Token.Type.CLASS, "int")),
                    new Token(Token.Type.ASIGN, "="),
                    new Null(),
                BlockString);
            Token FunctionStringOperatorEqualName = new Token(Token.Type.ID, "operator equal");
            ParameterList plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockString, new Token(Token.Type.CLASS, "string")));
            Function FunctionStringOperatorEqual = new Function(FunctionStringOperatorEqualName, null, plist, new Token(Token.Type.CLASS, "bool"), interpret) { isOperator = true };
            BlockString.SymbolTable.Add("operator equal", FunctionStringOperatorEqual);
            //Operator Plus
            Token FunctionStringOperatorPlusName = new Token(Token.Type.ID, "operator plus");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockString, new Token(Token.Type.CLASS, "string")));
            Function FunctionStringOperatorPlus = new Function(FunctionStringOperatorPlusName, null, plist, new Token(Token.Type.CLASS, "string"), interpret) { isOperator = true };
            BlockString.SymbolTable.Add("operator plus", FunctionStringOperatorPlus);
            //Operator get for key [key]
            Token FunctionStringOperatorGetName = new Token(Token.Type.ID, "operator get");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "key"), BlockString, new Token(Token.Type.CLASS, "int")));
            Function FunctionStringOperatorGet = new Function(FunctionStringOperatorGetName, null, plist, new Token(Token.Type.CLASS, "string"), interpret) { isOperator = true };
            BlockString.SymbolTable.Add("operator get", FunctionStringOperatorGet);
            
            /// Initial Int Class
            Block BlockInt = ((Class)Get("int")).assingBlock;
            //Operator Equal
            Token FunctionIntOperatorEqualName = new Token(Token.Type.ID, "operator equal");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockInt, new Token(Token.Type.CLASS, "int")));
            Function FunctionIntOperatorEqual = new Function(FunctionStringOperatorEqualName, null, plist, new Token(Token.Type.CLASS, "bool"), interpret) { isOperator = true };
            BlockInt.SymbolTable.Add("operator equal", FunctionIntOperatorEqual);
            //Operator More
            Token FunctionIntOperatorMoreName = new Token(Token.Type.ID, "operator compareTo");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockInt, new Token(Token.Type.CLASS, "int")));
            Function FunctionIntOperatorMore = new Function(FunctionIntOperatorMoreName, null, plist, new Token(Token.Type.CLASS, "bool"), interpret) { isOperator = true };
            BlockInt.SymbolTable.Add("operator compareTo", FunctionIntOperatorMore);          
            //Operator Plus
            Token FunctionIntOperatorPlusName = new Token(Token.Type.ID, "operator plus");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockInt, new Token(Token.Type.CLASS, "int")));
            Function FunctionIntOperatorPlus = new Function(FunctionIntOperatorPlusName, null, plist, new Token(Token.Type.CLASS, "int"), interpret) { isOperator = true };
            BlockInt.SymbolTable.Add("operator plus", FunctionIntOperatorPlus);
            //Operator Minus
            Token FunctionIntOperatorMinusName = new Token(Token.Type.ID, "operator minus");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockInt, new Token(Token.Type.CLASS, "int")));
            Function FunctionIntOperatorMinus = new Function(FunctionIntOperatorMinusName, null, plist, new Token(Token.Type.CLASS, "int"), interpret) { isOperator = true };
            BlockInt.SymbolTable.Add("operator minus", FunctionIntOperatorMinus);
            //Operator Inc
            Token FunctionIntOperatorIncrementName = new Token(Token.Type.ID, "operator inc");
            Function FunctionIntOperatorIncrement = new Function(FunctionIntOperatorIncrementName, null, new ParameterList(true), new Token(Token.Type.CLASS, "int"), interpret) { isOperator = true };
            BlockInt.SymbolTable.Add("operator inc", FunctionIntOperatorIncrement);
        }

        public Dictionary<string, Types> Table { get { return table; } }
        public void Add(string name, Type type, List<Token> parent = null)
        {
            table.Add(name, (Types)Activator.CreateInstance(typeof(Class<>).MakeGenericType(type), this.interpret, this.assigment_block, name, parent));
            tableType.Add(name, type);
        }

        public void Add(string name, Types type)
        {
            if (!tableCounter.ContainsKey(name))
                tableCounter.Add(name, 1);
            else
                tableCounter[name] += 1;
            if (tableCounter[name] != 1)
            {                
                table.Add(name + " " + (tableCounter[name]), type);
            }
            else
                table.Add(name, type);
        }

        public List<Types> GetAll(string name)
        {
            List<Types> types = new List<Types>();
            int index = 1;
            if (!Find(name))
                return null;
            Types rt = Get(name);
            types.Add(rt);

            while (Find(name + " " + index))
            {
                types.Add(rt.assingBlock.SymbolTable.Get(name + " " + index));
            }

            return types;
        }

        public bool Find(string name)
        {
            if (name.Contains('.'))
            {
                string[] nams = name.Split('.');
                if (Find(nams[0]))
                {
                    Types found = Get(nams[0]);
                    if (found is Assign)
                    {
                        //Variable vr = (Variable)((Assign)assigment_block.variables[nams[0]]).Left;
                        Variable vr = (Variable)((Assign)found).Left;
                        if (vr.Type != "auto")
                        {
                            return Find(vr.Type + "." + string.Join(".", nams.Skip(1)));
                        }
                        if (((Assign)assigment_block.variables[nams[0]]).Right is UnaryOp uop)
                        {
                            if (uop.Op == "new")
                            {
                                return Find(uop.Name.Value + "." + string.Join(".", nams.Skip(1)));
                            }
                        }
                    }
                    if (found is Function)
                        return ((Function)found).assingBlock.SymbolTable.Find(string.Join(".", nams.Skip(1)));
                    else if(found is Class)
                        return ((Class)found).assingBlock.SymbolTable.Find(string.Join(".", nams.Skip(1)));
                    else if (found is Interface)
                        return ((Interface)found).Block.SymbolTable.Find(string.Join(".", nams.Skip(1)));
                    return Find(string.Join(".", nams.Skip(1)));
                }
                else if(assigment_block.variables.ContainsKey(nams[0]))
                {
                    Variable vr = (Variable)((Assign)assigment_block.variables[nams[0]]).Left;
                    if (((Assign)assigment_block.variables[nams[0]]).Right is UnaryOp uop)
                    {
                        if (uop.Op == "new")
                        {
                            return Find(uop.Name.Value + "." + string.Join(".", nams.Skip(1)));
                        }
                    }
                    return vr.assingBlock.SymbolTable.Find(string.Join(".", nams.Skip(1)));
                }
            }
            if (table.ContainsKey(name))
                return true;
            else
            {
                if (assigment_block.variables.ContainsKey(name))
                    return true;
                if (assigment_block.Parent != null)
                    return assigment_block.Parent.SymbolTable.Find(name);                
                return false;
            }
        }

        public bool FindInternal(string name)
        {
            if (tableType.ContainsKey(name))
                return true;
            else
            {
                if (assigment_block.Parent != null)
                    return assigment_block.Parent.SymbolTable.FindInternal(name);
                return false;
            }
        }

        public Types Get(string name)
        {
            if (name.Contains('.'))
            {
                string[] nams = name.Split('.');
                if (Find(nams[0]))
                {
                    Types found = Get(nams[0]);
                    if(found is Assign)
                    {
                        Variable vr = (Variable)((Assign)found).Left;
                        if(vr.Type != "auto")
                        {
                            return Get(vr.Type + "." + string.Join(".", nams.Skip(1)));
                        }
                        if (((Assign)assigment_block.variables[nams[0]]).Right is UnaryOp uop)
                        {
                            if (uop.Op == "new")
                            {
                                return Get(uop.Name.Value + "." + string.Join(".", nams.Skip(1)));
                            }
                        }
                    }
                    else if (found is Function)
                        return ((Function)found).assingBlock.SymbolTable.Get(string.Join(".", nams.Skip(1)));
                    else if (found is Class)
                        return ((Class)found).assingBlock.SymbolTable.Get(string.Join(".", nams.Skip(1)));
                    else if (found is Interface)
                        return ((Interface)found).Block.SymbolTable.Get(string.Join(".", nams.Skip(1)));
                    return Get(string.Join(".", nams.Skip(1)));
                }
                else if (assigment_block.variables.ContainsKey(nams[0]))
                {
                    Variable vr = (Variable)((Assign)assigment_block.variables[nams[0]]).Left;
                    if (((Assign)assigment_block.variables[nams[0]]).Right is UnaryOp uop)
                    {
                        if (uop.Op == "new")
                        {
                            return Get(uop.Name.Value + "." + string.Join(".", nams.Skip(1)));
                        }
                    }
                    return vr.assingBlock.SymbolTable.Get(string.Join(".", nams.Skip(1)));
                }
            }
            if (table.ContainsKey(name))
                return table[name];
            else
            {
                if (assigment_block.variables.ContainsKey(name))
                    return assigment_block.variables[name];
                if (assigment_block.Parent != null)
                    return assigment_block.Parent.SymbolTable.Get(name);
            }
            /*
            if(table.ContainsKey(name))
                return table[name];
            else
            {
                if (assigment_block.Parent != null)
                    return assigment_block.Parent.SymbolTable.Get(name);
            }
            */
            return new Error("Internal error #100");
        }
        public Type GetType(string name)
        {
            if (tableType.ContainsKey(name))
                return tableType[name];
            else
            {
                if (assigment_block.Parent != null)
                    return assigment_block.Parent.SymbolTable.GetType(name);
            }
            Interpreter.semanticError.Add(new Error("Internal error #101", Interpreter.ErrorType.ERROR));
            return null;            
        }
    }
}

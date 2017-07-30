using System;
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

                Add("int",      typeof(TypeInt));
                Add("bool",     typeof(TypeBool));                
            }
        }

        public void initialize()
        {
            /// Initialize String Class
            Block BlockString = ((Class)Get("string")).assingBlock;
            //Operator Equal
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
                        Variable vr = (Variable)((Assign)assigment_block.variables[nams[0]]).Left;
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
                        Variable vr = (Variable)((Assign)assigment_block.variables[nams[0]]).Left;
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

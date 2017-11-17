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
        Dictionary<string, bool> tableIsCopy = new Dictionary<string, bool>();
        Dictionary<string, int> tableCounter = new Dictionary<string, int>();
        Dictionary<string, Type> tableType = new Dictionary<string, Type>();
        Interpreter interpret;
        public Block assigment_block;        

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

                Token TokenIterator = new Token(Token.Type.ID, "Iterator");
                Block BlockIterator = new Block(interpret) { Parent = assigment_block };
                Interface Iterator = new Interface(TokenIterator, BlockIterator, null)
                {
                    isExternal = true
                };
                Add("Iterator", Iterator);

                Token TokenAttribute = new Token(Token.Type.ID, "Attribute");
                Block BlockAttribute = new Block(interpret) { Parent = assigment_block };
                Interface Attribute = new Interface(TokenAttribute, BlockAttribute, null)
                {
                    isExternal = true
                };
                Add("Attribute", Attribute);

                Token TokenDebug = new Token(Token.Type.ID, "Debug");
                Block BlockDebug = new Block(interpret) { Parent = assigment_block };
                Class Debug = new Class(TokenDebug, BlockDebug, new List<Token> { TokenAttribute })
                {
                    isExternal = true
                };
                Add("Debug", Debug);

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

        public void Copy(string from, string to)
        {
            if (table.ContainsKey(from) && from != to)
            {
                table.Add(to, table[from]);
                tableIsCopy[to] = true;
            }
        }
        
        public void initialize()
        {
            //Function js
            Token Function_js = new Token(Token.Type.ID, "js");
            ParameterList plist_js = new ParameterList(true);
            Block Block_js = new Block(interpret) { Parent = assigment_block };
            plist_js.parameters.Add(new Variable(new Token(Token.Type.ID, "code"), Block_js, new Token(Token.Type.CLASS, "string")));
            Function js = new Function(Function_js, Block_js, plist_js, new Token(Token.Type.VOID, "void"), interpret) { isExternal = true, isConstructor = false, isOperator = false, isStatic = false };
            Add("js", js);
            //Function _default
            Token Function__default = new Token(Token.Type.ID, "_default");
            ParameterList plist_default = new ParameterList(true);
            Block Block__default = new Block(interpret) { Parent = assigment_block };
            plist_default.parameters.Add(new Variable(new Token(Token.Type.ID, "type"), Block_js, new Token(Token.Type.CLASS, "Type")));
            Function _default = new Function(Function__default, Block__default, plist_default, new Token(Token.Type.CLASS, "object"), interpret) { isExternal = true, isConstructor = false, isOperator = false, isStatic = false };
            Add("_default", _default);
            //Function alert
            Token Function_alert = new Token(Token.Type.ID, "alert");
            ParameterList plist_alert = new ParameterList(true);
            Block Block_alert = new Block(interpret) { Parent = assigment_block };
            plist_alert.parameters.Add(new Variable(new Token(Token.Type.ID, "message"), Block_alert, new Token(Token.Type.CLASS, "string")));
            Function alert = new Function(Function_alert, Block_alert, plist_alert, new Token(Token.Type.VOID, "void"), interpret) { isExternal = true, isConstructor = false, isOperator = false, isStatic = false };
            Add("alert", alert);


            /// Initialize Iterator interface
            Block BlockIterator = ((Interface)Get("Iterator")).assingBlock;
            Token FunctionIteratorNextName = new Token(Token.Type.ID, "next");
            ParameterList plist = new ParameterList(true);
            Function FunctionIteratorNext = new Function(FunctionIteratorNextName, null, plist, new Token(Token.Type.CLASS, "string"), interpret);
            BlockIterator.SymbolTable.Add("next", FunctionIteratorNext);
            Token FunctionIteratorHasNextName = new Token(Token.Type.ID, "hasNext");
            plist = new ParameterList(true);
            Function FunctionIteratorHasNext = new Function(FunctionIteratorHasNextName, null, plist, new Token(Token.Type.CLASS, "string"), interpret);
            BlockIterator.SymbolTable.Add("hasNext", FunctionIteratorHasNext);

            /// Initialize IIterable interface
            Block BlockIIterable = ((Interface)Get("IIterable")).assingBlock;
            Token FunctionIIterableIteratorName = new Token(Token.Type.ID, "iterator");
            plist = new ParameterList(true);
            Function FunctionIIterableIterator = new Function(FunctionIIterableIteratorName, null, plist, new Token(Token.Type.CLASS, "Iterator"), interpret);
            BlockIIterable.SymbolTable.Add("iterator", FunctionIIterableIterator);

            /// Initialize String Class
            Block BlockString = ((Class)Get("string")).assingBlock;
            //Operator Equal
            new Assign(
                    new Variable(new Token(Token.Type.ID, "length"), BlockString, new Token(Token.Type.CLASS, "int")),
                    new Token(Token.Type.ASIGN, "="),
                    new Null(),
                BlockString);
            Token FunctionStringOperatorEqualName = new Token(Token.Type.ID, "operator equal");
            plist = new ParameterList(true);
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
            Function FunctionStringOperatorGet = new Function(FunctionStringOperatorGetName, null, plist, new Token(Token.Type.CLASS, "string"), interpret) { isOperator = true, isExternal = true };
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
        public Dictionary<string, bool> TableIsCopy { get { return tableIsCopy; } }
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
            int index = 2;
            if (!Find(name))
                return null;
            Types rt = Get(name);
            types.Add(rt);

            while (Find(name + " " + index))
            {
                types.Add(Get(name + " " + index));
                index++;
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
                    else if (found is Class)
                        return ((Class)found).assingBlock.SymbolTable.Find(string.Join(".", nams.Skip(1)));
                    else if (found is Interface)
                        return ((Interface)found).Block.SymbolTable.Find(string.Join(".", nams.Skip(1)));
                    else if (found is Import)
                        return ((Import)found).Block.SymbolTable.Find(string.Join(".", nams.Skip(1)));
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
                if (interpret.FindImport(name))
                    return interpret.GetImport(name).Block.SymbolTable.Find(name);
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
        
        public Types Get(string name, ParameterList plist)
        {
            Types r = Get(name);
            if (!(r is Function) && !(r is Lambda))
                return r;

            bool found = false;
            List<Types> allf = GetAll(name);
            Types t = null;
            if (allf != null && allf.Count > 1)
            {
                foreach (Types q in allf)
                {
                    ParameterList p = null;
                    if (q is Function)
                        p = ((Function)q).ParameterList;
                    if (q is Lambda)
                        p = ((Lambda)q).ParameterList;

                    if (p.Compare(plist))
                    {
                        t = q;
                        found = true;
                    }
                }

            }
            else
            {
                ParameterList p = null;
                if (r is Function)
                    p = ((Function)r).ParameterList;
                if (r is Lambda)
                    p = ((Lambda)r).ParameterList;

                if (p.Compare(plist))
                {
                    t = r;
                    found = true;
                }
            }
            if (t != null) r = t;
            if (!found) return new Error("Found but arguments are bad!");
            return r;
        }

        public Types Get(string name, bool noConstrucotr = false, bool getImport = false)
        {
            if (name.Split('.')[0] == "this")
                name = string.Join(".", name.Split('.').Skip(1));
            if (name.Contains('.'))
            {
                string[] nams = name.Split('.');
                if (Find(nams[0]))
                {
                    Types found = Get(nams[0], noConstrucotr, getImport);
                    if (found is Assign)
                    {
                        Variable vr = (Variable)((Assign)found).Left;
                        if (vr.Type != "auto")
                        {
                            return Get(vr.Type + "." + string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                        }
                        if (((Assign)assigment_block.variables[nams[0]]).Right is UnaryOp uop)
                        {
                            if (uop.Op == "new")
                            {
                                return Get(uop.Name.Value + "." + string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                            }
                        }
                    }
                    else if (found is Function)
                        return ((Function)found).assingBlock.SymbolTable.Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                    else if (found is Class)
                        return ((Class)found).assingBlock.SymbolTable.Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                    else if (found is Interface)
                        return ((Interface)found).Block.SymbolTable.Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                    else if (found is Import)
                    {
                        if (getImport)
                            return found;
                        Types ttt = ((Import)found).Block.SymbolTable.Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                        if(ttt is Error)
                        {
                            if (((Import)found).Block.SymbolTable.Find(name))
                            {
                                return ((Import)found).Block.SymbolTable.Get(name, noConstrucotr, getImport);
                            }
                        }
                        return ttt;
                    }
                    return Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                }
                else if (assigment_block.variables.ContainsKey(nams[0]))
                {
                    Variable vr = (Variable)((Assign)assigment_block.variables[nams[0]]).Left;
                    if (((Assign)assigment_block.variables[nams[0]]).Right is UnaryOp uop)
                    {
                        if (uop.Op == "new")
                        {
                            return Get(uop.Name.Value + "." + string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                        }
                    }
                    return vr.assingBlock.SymbolTable.Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                }
            }
            /*if (table.Where(t => t.Key.Replace("constructor ", "") == name).Count() > 0 && !noConstrucotr)
                return table.Where(t => t.Key.Replace("constructor ", "") == name).First().Value;
            else*/
            if (table.Where(t => t.Key == name).Count() > 0)
            {
                Types ttt = table.Where(t => t.Key == name).First().Value;
                if (ttt is Import)
                {
                    if (getImport)
                        return ttt;
                    if (((Import)ttt).Block.SymbolTable.Find(name))
                    {
                        return ((Import)ttt).Block.SymbolTable.Get(name, noConstrucotr, getImport);
                    }
                }
                return table.Where(t => t.Key == name).First().Value;
            }
            else
            {
                if (assigment_block.variables.Where(t => t.Key.Split(' ')[0] == name).Count() > 0)
                    return assigment_block.variables[name];
                if (assigment_block.Parent != null)
                    return assigment_block.Parent.SymbolTable.Get(name, noConstrucotr, getImport);
            }
            if (interpret.FindImport(name))
            {
                return interpret.GetImport(name).Block.SymbolTable.Get(name, noConstrucotr, getImport);
            }
            return new Error("#100 Internal error");
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
            //Interpreter.semanticError.Add(new Error("Internal error #101", Interpreter.ErrorType.ERROR));
            return null;            
        }
    }
}

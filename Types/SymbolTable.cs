using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        public bool isForImport = false;

        static bool _initializedMain = false;
        static SymbolTable MainSymbolTable = null;

        public SymbolTable(Interpreter interpret, Block assigment_block, bool first = false)
        {
            this.interpret = interpret;
            this.assigment_block = assigment_block;

            if (_initializedMain)
                return;

            MainSymbolTable = this;

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
                Class Debug = new Class(TokenDebug, BlockDebug, new List<Types> { new UnaryOp(new Token(Token.Type.NEW, "new"), TokenAttribute) })
                {
                    isExternal = true
                };
                Add("Debug", Debug);

                /// Date type bool implicit
                // Add("bool", typeof(TypeBool));
                Token TokenBool = new Token(Token.Type.ID, "bool");
                Block BlockBool = new Block(interpret) { Parent = assigment_block };
                Class Bool = new Class(TokenBool, BlockBool, new List<Types> { })
                {
                    isExternal = true,
                    JSName = "Boolean"
                };
                Add("bool", Bool);
                /// Date type string implicit
                // Add("string", typeof(TypeString), new List<Token> { TokenIIterable });
                Token TokenString = new Token(Token.Type.ID, "string");
                Block BlockString = new Block(interpret) { Parent = assigment_block };
                Class String = new Class(TokenString, BlockString, new List<Types> { new UnaryOp(new Token(Token.Type.NEW, "new"), TokenIIterable) })
                {
                    isExternal = true,
                    JSName = "String"
                };
                Add("string", String);
                /// Date type int implicit
                //Add("int", typeof(TypeInt));
                Token TokenInt = new Token(Token.Type.ID, "int");
                Block BlockInt = new Block(interpret) { Parent = assigment_block };
                Class Int = new Class(TokenInt, BlockInt, null)
                {
                    isExternal = true,
                    JSName = "Number"
                };
                Add("int", Int);
                /// Date type float implicit
                //Add("int", typeof(TypeInt));
                Token TokenFloat = new Token(Token.Type.ID, "float");
                Block BlockFloat = new Block(interpret) { Parent = assigment_block };
                Class Float = new Class(TokenFloat, BlockFloat, null)
                {
                    isExternal = true,
                    JSName = "Number"
                };
                Add("float", Int);
            }
        }

        public override string ToString()
        {
            if (table.Count == 0)
                return "[Empty]";
            return "(" + table.Count + ")[" + string.Join(", ", table.Keys.ToList()) + "]";
        }

        public void Copy(string from, string to)
        {
            if (table.ContainsKey(from) && from != to)
            {
                table.Add(to, table[from]);
                tableIsCopy[to] = true;
            }
        }

        public void Delete(string name)
        {
            if (Find(name))
            {
                SymbolTable sb = GetSymbolTable(name);
                sb?.table.Remove(name);
            }
        }        

        public void Initialize()
        {
            if (_initializedMain)
                return;
            _initializedMain = true;

            //Function js
            Token Function_js = new Token(Token.Type.ID, "js");
            ParameterList plist_js = new ParameterList(true);
            Block Block_js = new Block(interpret) { Parent = assigment_block };
            plist_js.parameters.Add(new Variable(new Token(Token.Type.ID, "code"), Block_js, new Token(Token.Type.CLASS, "string")));
            Function js = new Function(Function_js, Block_js, plist_js, new Token(Token.Type.VOID, "void"), interpret) { isExternal = true, isConstructor = false, isOperator = false, isStatic = false };
            Add("js", js);
            /*
            //Function _default
            Token Function__default = new Token(Token.Type.ID, "_default");
            ParameterList plist_default = new ParameterList(true);
            Block Block__default = new Block(interpret) { Parent = assigment_block };
            plist_default.parameters.Add(new Variable(new Token(Token.Type.ID, "type"), Block_js, new Token(Token.Type.CLASS, "Type")));
            Function _default = new Function(Function__default, Block__default, plist_default, new Token(Token.Type.CLASS, "object"), interpret) { isExternal = true, isConstructor = false, isOperator = false, isStatic = false };
            Add("_default", _default);*/
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


            /// Initialize Bool Class
            Block BlockBool = ((Class)Get("bool")).assingBlock;
            //Operator Equal
            Token FunctionBoolOperatorEqualName = new Token(Token.Type.ID, "operator equal");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockBool, new Token(Token.Type.CLASS, "bool")));
            Function FunctionBoolOperatorEqual = new Function(FunctionBoolOperatorEqualName, null, plist, new Token(Token.Type.CLASS, "bool"), interpret) { isOperator = true };
            BlockBool.SymbolTable.Add("operator equal", FunctionBoolOperatorEqual);
            //Operator And
            Token FunctionBoolOperatorAndName = new Token(Token.Type.ID, "operator and");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockBool, new Token(Token.Type.CLASS, "bool")));
            Function FunctionBoolOperatorAnd = new Function(FunctionBoolOperatorAndName, null, plist, new Token(Token.Type.CLASS, "bool"), interpret) { isOperator = true };
            BlockBool.SymbolTable.Add("operator and", FunctionBoolOperatorAnd);
            //Operator Or
            Token FunctionBoolOperatorOrName = new Token(Token.Type.ID, "operator or");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockBool, new Token(Token.Type.CLASS, "bool")));
            Function FunctionBoolOperatorOr = new Function(FunctionBoolOperatorOrName, null, plist, new Token(Token.Type.CLASS, "bool"), interpret) { isOperator = true };
            BlockBool.SymbolTable.Add("operator or", FunctionBoolOperatorOr);

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
            Function FunctionIntOperatorEqual = new Function(FunctionIntOperatorEqualName, null, plist, new Token(Token.Type.CLASS, "bool"), interpret) { isOperator = true };
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
            //Operator Get
            Token FunctionIntOperatorGetName = new Token(Token.Type.ID, "operator get");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "key"), BlockInt, new Token(Token.Type.CLASS, "int")));
            Function FunctionIntOperatorGet = new Function(FunctionIntOperatorGetName, null, plist, new Token(Token.Type.CLASS, "int"), interpret) { isOperator = true, isExternal = true };
            BlockInt.SymbolTable.Add("operator get", FunctionIntOperatorGet);
            //Operator Multiple
            Token FunctionIntOperatorMultipleName = new Token(Token.Type.ID, "operator multiple");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockInt, new Token(Token.Type.CLASS, "int")));
            Function FunctionIntMultipleGet = new Function(FunctionIntOperatorMultipleName, null, plist, new Token(Token.Type.CLASS, "int"), interpret) { isOperator = true, isExternal = true };
            BlockInt.SymbolTable.Add("operator multiple", FunctionIntMultipleGet);

            /// Initial Float Class
            Block BlockFloat = ((Class)Get("float")).assingBlock;
            //Operator Equal
            Token FunctionFloatOperatorEqualName = new Token(Token.Type.ID, "operator equal");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockFloat, new Token(Token.Type.CLASS, "int")));
            Function FunctionFloatOperatorEqual = new Function(FunctionFloatOperatorEqualName, null, plist, new Token(Token.Type.CLASS, "bool"), interpret) { isOperator = true };
            BlockFloat.SymbolTable.Add("operator equal", FunctionFloatOperatorEqual);
            //Operator More
            Token FunctionFloatOperatorMoreName = new Token(Token.Type.ID, "operator compareTo");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockFloat, new Token(Token.Type.CLASS, "int")));
            Function FunctionFloatOperatorMore = new Function(FunctionFloatOperatorMoreName, null, plist, new Token(Token.Type.CLASS, "bool"), interpret) { isOperator = true };
            BlockFloat.SymbolTable.Add("operator compareTo", FunctionFloatOperatorMore);
            //Operator Plus
            Token FunctionFloatOperatorPlusName = new Token(Token.Type.ID, "operator plus");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockFloat, new Token(Token.Type.CLASS, "int")));
            Function FunctionFloatOperatorPlus = new Function(FunctionFloatOperatorPlusName, null, plist, new Token(Token.Type.CLASS, "int"), interpret) { isOperator = true };
            BlockFloat.SymbolTable.Add("operator plus", FunctionFloatOperatorPlus);
            //Operator Minus
            Token FunctionFloatOperatorMinusName = new Token(Token.Type.ID, "operator minus");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockFloat, new Token(Token.Type.CLASS, "int")));
            Function FunctionFloatOperatorMinus = new Function(FunctionFloatOperatorMinusName, null, plist, new Token(Token.Type.CLASS, "int"), interpret) { isOperator = true };
            BlockFloat.SymbolTable.Add("operator minus", FunctionFloatOperatorMinus);
            //Operator Inc
            Token FunctionFloatOperatorIncrementName = new Token(Token.Type.ID, "operator inc");
            Function FunctionFloatOperatorIncrement = new Function(FunctionFloatOperatorIncrementName, null, new ParameterList(true), new Token(Token.Type.CLASS, "int"), interpret) { isOperator = true };
            BlockFloat.SymbolTable.Add("operator inc", FunctionFloatOperatorIncrement);
            //Operator Get
            Token FunctionFloatOperatorGetName = new Token(Token.Type.ID, "operator get");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "key"), BlockFloat, new Token(Token.Type.CLASS, "int")));
            Function FunctionFloatOperatorGet = new Function(FunctionFloatOperatorGetName, null, plist, new Token(Token.Type.CLASS, "int"), interpret) { isOperator = true, isExternal = true };
            BlockFloat.SymbolTable.Add("operator get", FunctionFloatOperatorGet);
            //Operator Multiple
            Token FunctionFloatOperatorMultipleName = new Token(Token.Type.ID, "operator multiple");
            plist = new ParameterList(true);
            plist.parameters.Add(new Variable(new Token(Token.Type.ID, "a"), BlockFloat, new Token(Token.Type.CLASS, "int")));
            Function FunctionFloatMultipleGet = new Function(FunctionFloatOperatorMultipleName, null, plist, new Token(Token.Type.CLASS, "int"), interpret) { isOperator = true, isExternal = true };
            BlockFloat.SymbolTable.Add("operator multiple", FunctionFloatMultipleGet);
        }

        public Dictionary<string, Types> Table { get { return table; } }
        public Dictionary<string, bool> TableIsCopy { get { return tableIsCopy; } }
        public Dictionary<string, int> TableCounter { get { return tableCounter; } }

        public void Add(string name, Type type, List<Token> parent = null)
        {
            table.Add(name, (Types)Activator.CreateInstance(typeof(Class<>).MakeGenericType(type), this.interpret, this.assigment_block, name, parent));
            tableType.Add(name, type);
        }

        public void Add(Block block)
        {
            if (block == null)
                return;
            foreach (var typese in block.SymbolTable.Table)
            {
                Add(typese.Key, typese.Value);
            }
        }

        public void Add(string name, Types type, bool isForImport = false)
        {
            if (!tableCounter.ContainsKey(name))
            {
                if (Find(name))
                {
                    Types t = Get(name);
                    if (isForImport)
                        return;
                    if (t.assingBlock?.Interpret.UID != interpret.UID)
                        tableCounter.Add(name, 1);
                    else if (t.assingBlock != null && t.assingBlock.Parent.SymbolTable.tableCounter.ContainsKey(name))
                        tableCounter.Add(name, Get(name).assingBlock.Parent.SymbolTable.tableCounter[name] + 1);
                    else
                        tableCounter.Add(name, 1);
                } else
                    tableCounter.Add(name, 1);
            }
            else
                tableCounter[name] += 1;
            if (tableCounter[name] != 1)
            {
                table.Add(name + " " + (tableCounter[name]), type);
            }
            else
                table.Add(name, type);
        }

        public List<Types> GetAll(string name, bool getImport = false)
        {
            List<Types> types = new List<Types>();
            int index = 2;
            if (!Find(name))
                return null;
            Types rt = Get(name, getImport: getImport);
            types.Add(rt);

            while (Find(name + " " + index))
            {
                types.Add(Get(name + " " + index, getImport: getImport));
                index++;
            }

            return types;
        }

        public bool Find(string name, List<string> prevClass = null)
        {
            while (true)
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
                            Variable vr = (Variable) ((Assign) found).Left;
                            if (vr.Type != "auto")
                            {
                                name = vr.Type + "." + string.Join(".", nams.Skip(1));
                                prevClass = null;
                                continue;
                            }

                            if (((Assign) assigment_block.variables[nams[0]]).Right is UnaryOp uop)
                            {
                                if (uop.Op == "new")
                                {
                                    name = uop.Name.Value + "." + string.Join(".", nams.Skip(1));
                                    prevClass = null;
                                    continue;
                                }
                            }
                        }

                        if (found is Function)
                            return ((Function) found).assingBlock.SymbolTable.Find(string.Join(".", nams.Skip(1)));
                        else if (found is Class)
                            return ((Class) found).assingBlock.SymbolTable.Find(string.Join(".", nams.Skip(1)));
                        else if (found is Interface)
                            return ((Interface) found).Block.SymbolTable.Find(string.Join(".", nams.Skip(1)));
                        else if (found is Import) return ((Import) found).Block.SymbolTable.Find(string.Join(".", nams.Skip(1)));
                        name = string.Join(".", nams.Skip(1));
                        prevClass = null;
                        continue;
                    }
                    else if (assigment_block.variables.ContainsKey(nams[0]))
                    {
                        Variable vr = (Variable) ((Assign) assigment_block.variables[nams[0]]).Left;
                        if (((Assign) assigment_block.variables[nams[0]]).Right is UnaryOp uop)
                        {
                            if (uop.Op == "new")
                            {
                                name = uop.Name.Value + "." + string.Join(".", nams.Skip(1));
                                prevClass = null;
                                continue;
                            }
                        }

                        return vr.assingBlock.SymbolTable.Find(string.Join(".", nams.Skip(1)));
                    }
                }

                if (table.ContainsKey(name))
                    return true;
                else
                {
                    if (assigment_block.variables.ContainsKey(name)) return true;
                    if (assigment_block.Parent != null && (!isForImport || assigment_block.Parent.Parent != null))
                    {
                        bool ret = assigment_block.Parent.SymbolTable.Find(name, prevClass);
                        if (ret) return true;
                    }

                    if (interpret.FindImport(name))
                    {
                        var import = interpret.GetImport(name);
                        var nameLast = name.Split('.').Last();
                        if (nameLast == name) return true;
                        return import.Block.SymbolTable.Find(name.Split('.').Last());
                    }

                    foreach (KeyValuePair<string, Types> type in table)
                    {
                        if (type.Value is Import)
                        {
                            bool t = ((Import) type.Value).Block.SymbolTable.Find(name);
                            if (t) return true;
                        }
                        else if (type.Value is Class && ((Class) type.Value).isForImport && ((prevClass != null && !prevClass.Contains(type.Key)) || prevClass == null))
                        {
                            if (prevClass == null) prevClass = new List<string>();
                            prevClass.Add(type.Key);
                            bool t = ((Class) type.Value).Block.SymbolTable.Find(name, prevClass);
                            if (t) return true;
                        }
                    }

                    return (bool) interpret?.Find(name);                    
                }

                break;
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

        public static Stopwatch stopwatch = new Stopwatch();
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

        public SymbolTable GetSymbolTable(string name, bool noConstrucotr = false, bool getImport = false)
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
                            return GetSymbolTable(vr.Type + "." + string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                        }
                        if (((Assign)assigment_block.variables[nams[0]]).Right is UnaryOp uop)
                        {
                            if (uop.Op == "new")
                            {
                                return GetSymbolTable(uop.Name.Value + "." + string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                            }
                        }
                    }
                    else if (found is Function)
                        return ((Function)found).assingBlock.SymbolTable.GetSymbolTable(string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                    else if (found is Class)
                        return ((Class)found).assingBlock.SymbolTable.GetSymbolTable(string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                    else if (found is Interface)
                        return ((Interface)found).Block.SymbolTable.GetSymbolTable(string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                    else if (found is Import)
                    {
                        if (getImport)
                            return found.assingBlock.SymbolTable;
                        Types ttt = ((Import)found).Block.SymbolTable.Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                        if (ttt is Error)
                        {
                            if (((Import)found).Block.SymbolTable.Find(name))
                            {
                                return ((Import)found).Block.SymbolTable.GetSymbolTable(name, noConstrucotr, getImport);
                            }
                        }
                        return ttt.assingBlock.Parent?.SymbolTable;
                    }
                    return GetSymbolTable(string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                }
                else if (assigment_block.variables.ContainsKey(nams[0]))
                {
                    Variable vr = (Variable)((Assign)assigment_block.variables[nams[0]]).Left;
                    if (((Assign)assigment_block.variables[nams[0]]).Right is UnaryOp uop)
                    {
                        if (uop.Op == "new")
                        {
                            return GetSymbolTable(uop.Name.Value + "." + string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                        }
                    }
                    return vr.assingBlock.SymbolTable.GetSymbolTable(string.Join(".", nams.Skip(1)), noConstrucotr, getImport);
                }
            }
            if (table.Where(t => t.Key == name).Count() > 0)
            {
                Types ttt = table.Where(t => t.Key == name).First().Value;
                if (ttt is Import)
                {
                    if (getImport)
                        return ttt.assingBlock.Parent?.SymbolTable;
                    if (((Import)ttt).Block.SymbolTable.Find(name))
                    {
                        return ((Import)ttt).Block.SymbolTable.GetSymbolTable(name, noConstrucotr, getImport);
                    }
                }
                return table.Where(t => t.Key == name).First().Value.assingBlock.Parent?.SymbolTable;
            }
            else
            {
                if (assigment_block.variables.Where(t => t.Key.Split(' ')[0] == name).Count() > 0)
                    return assigment_block.variables[name].assingBlock.Parent?.SymbolTable;
                if (assigment_block.Parent != null)
                    return assigment_block.Parent.SymbolTable.GetSymbolTable(name, noConstrucotr, getImport);
            }
            if (interpret.FindImport(name))
            {
                return interpret.GetImport(name).Block.SymbolTable.GetSymbolTable(name, noConstrucotr, getImport);
            }
            return null;
        }

        public Types Get(string name, string type)
        {
            stopwatch.Start();
            Types t = __Get(name, false, false, -1, type);
            stopwatch.Stop();
            return t;
        }

        public Types Get(string name, bool noConstrucotr = false, bool getImport = false, int genericArgs = -1, string type = "", bool ignoreOnce = false)
        {
            stopwatch.Start();
            Types t = __Get(name, noConstrucotr, getImport, genericArgs, type, ignoreOnce);
            stopwatch.Stop();
            return t;
        }

        private Types __Get(string name, bool noConstrucotr = false, bool getImport = false, int genericArgs = -1, string stype = "", bool ignoreOnce = false)
        {                        
            if(genericArgs > -1)
            {
                var tttt = GetAll(name);
                foreach(var q in tttt)
                {
                    if(q is Interface qi)
                    {
                        if (qi.GenericArguments.Count == genericArgs)
                            return qi;
                    }
                    else if(q is Class qc)
                    {
                        if (qc.GenericArguments.Count == genericArgs)
                            return qc;
                    }
                    else if(q is Delegate qd)
                    {
                        if (qd.GenericArguments.Count == genericArgs)
                            return qd;
                    }
                    else if (q is Variable qv)
                    {
                        if (qv.Type == stype)
                            return qv;
                    }
                }
                if(tttt.Count > 0)
                    return tttt[0];
            }
            if (name.Split('.')[0] == "this")
                name = string.Join(".", name.Split('.').Skip(1));
            if (name.Contains('.'))
            {
                string[] nams = name.Split('.');
                if (Find(nams[0]))
                {
                    Types found = Get(nams[0], noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                    if (found is Assign)
                    {
                        Variable vr = (Variable)((Assign)found).Left;
                        if (vr.Type != "auto")
                        {
                            return Get(vr.Type + "." + string.Join(".", nams.Skip(1)), noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                        }
                        if (((Assign)assigment_block.variables[nams[0]]).Right is UnaryOp uop)
                        {
                            if (uop.Op == "new")
                            {
                                return Get(uop.Name.Value + "." + string.Join(".", nams.Skip(1)), noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                            }
                        }
                    }
                    else if (found is Function)
                        return ((Function)found).assingBlock.SymbolTable.Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                    else if (found is Class)
                        return ((Class)found).assingBlock.SymbolTable.Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                    else if (found is Interface)
                        return ((Interface)found).Block.SymbolTable.Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                    else if (found is Import)
                    {
                        if (getImport)
                            return found;
                        Types ttt = ((Import)found).Block.SymbolTable.Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                        if(ttt is Error)
                        {
                            if (((Import)found).Block.SymbolTable.Find(name))
                            {
                                return ((Import)found).Block.SymbolTable.Get(name, noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                            }
                        }
                        return ttt;
                    }
                    return Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                }
                else if (assigment_block.variables.ContainsKey(nams[0]))
                {
                    Variable vr = (Variable)((Assign)assigment_block.variables[nams[0]]).Left;
                    if (((Assign)assigment_block.variables[nams[0]]).Right is UnaryOp uop)
                    {
                        if (uop.Op == "new")
                        {
                            return Get(uop.Name.Value + "." + string.Join(".", nams.Skip(1)), noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                        }
                    }
                    if(vr.Type == stype || stype == "")
                        return vr.assingBlock.SymbolTable.Get(string.Join(".", nams.Skip(1)), noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                }
            }

            if (table.Any(t => t.Key == name))
            {
                Types ttt = table.First(t => t.Key == name).Value;
                if (ttt is Import)
                {
                    if (getImport)
                        return ttt;
                    if (((Import)ttt).Block.SymbolTable.Find(name))
                    {
                        return ((Import)ttt).Block.SymbolTable.Get(name, noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                    }
                }
                return table.First(t => t.Key == name).Value;
            }

            if (assigment_block.variables.Any(t => t.Key.Split(' ')[0] == name))
                return assigment_block.variables[name];

            if (assigment_block.Parent != null && !isForImport)
            {
                Types ret = assigment_block.Parent.SymbolTable.Get(name, noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                if(!(ret is Error))
                {
                    return ret;
                }                    
            }
            else if (assigment_block.Parent != null && isForImport && !ignoreOnce)
            {
                Types ret = assigment_block.Parent.SymbolTable.Get(name, noConstrucotr, getImport, genericArgs, stype, true);
                if(!(ret is Error))
                {
                    return ret;
                }
            }

            if (interpret.FindImport(name))
            {
                var import = interpret.GetImport(name);
                var importLast = name.Split('.').Last();
                if (name == importLast)
                    return import;
                return import.Block.SymbolTable.Get(name.Split('.').Last(), noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
            }

            foreach(var type in table)
            {
                if(type.Value is Import import)
                {
                    var t = import.Block.SymbolTable.Get(name, noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                    if (!(t is Error))
                        return t;
                }
                else if(type.Value is Class && ((Class)type.Value).isForImport)
                {
                    var t = ((Class)type.Value).Block.SymbolTable.Get(name, noConstrucotr, getImport, genericArgs, stype, ignoreOnce);
                    if (!(t is Error))
                        return t;
                }
            }

            var find = interpret?.Get(name);
            if (!(find is Error))
                return find;

            return new Error("#100 Internal error what can't normaly occured ups...");
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

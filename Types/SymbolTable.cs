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
        Dictionary<string, Type> tableType = new Dictionary<string, Type>();
        Interpreter interpret;
        Block assigment_block;

        public SymbolTable(Interpreter interpret, Block assigment_block, bool first = false)
        {
            this.interpret = interpret;
            this.assigment_block = assigment_block;
            if (first)
            {
                Add("null",     typeof(TypeNull));
                Add("int",      typeof(TypeInt));
                Add("string",   typeof(TypeString));
                Add("bool",     typeof(TypeBool));
            }
        }
        public Dictionary<string, Types> Table { get { return table; } }
        public void Add(string name, Type type)
        {
            table.Add(name, (Types)Activator.CreateInstance(typeof(Class<>).MakeGenericType(type), name));
            tableType.Add(name, type);
        }

        public void Add(string name, Types type)
        {
            table.Add(name, type);
        }

        public bool Find(string name)
        {
            if (name.Contains('.'))
            {
                string[] nams = name.Split('.');
                if (Find(nams[0]))
                {
                    Types found = Get(nams[0]);
                    if(found is Function)
                        return ((Function)found).assingBlock.SymbolTable.Find(string.Join(".", nams.Skip(1)));
                    else if(found is Class)
                        return ((Class)found).assingBlock.SymbolTable.Find(string.Join(".", nams.Skip(1)));
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
                    if (found is Function)
                        return ((Function)found).assingBlock.SymbolTable.Get(string.Join(".", nams.Skip(1)));
                    else if (found is Class)
                        return ((Class)found).assingBlock.SymbolTable.Get(string.Join(".", nams.Skip(1)));
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

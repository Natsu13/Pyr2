using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Class : Types
    {
        Token name;
        public Block block;
        List<Token> parents;
        public bool isExternal = false;
        public Token _external;
        public bool isDynamic = false;
        public Token _dynamic;
        public string JSName = "";
        List<string> genericArguments = new List<string>();
        public List<_Attribute> attributes = new List<_Attribute>();

        public Class(Token name, Block block, List<Token> parents)
        {
            this.name = name;
            this.block = block;
            this.block.blockAssignTo = name.Value;
            this.block.blockClassTo = name.Value;
            this.assingBlock = block;
            this.parents = parents;
            if (this.parents == null)
                this.parents = new List<Token>();
        }

        public void AddGenericArg(string name)
        {
            genericArguments.Add(name);
        }
        public void SetGenericArgs(List<string> list)
        {
            genericArguments = list;
        }
        public List<string> GenericArguments { get { return genericArguments; } }

        public Token Name { get { return name; } }
        public string getName() { if (JSName == null || JSName == "") return name.Value; else return JSName; }
        public override Token getToken() { return null; }

        public bool haveParent(string name)
        {
            if (parents == null) return false;
            foreach (Token p in parents)
            {
                if (p.Value == name) return true;
                Types to = block.SymbolTable.Get(p.Value);
                if (to is Class && ((Class)to).haveParent(name))
                    return true;
                else if (to is Interface && ((Interface)to).haveParent(name))
                    return true;
            }
            return false;
        }        

        public override string Compile(int tabs = 0)
        {
            if (!isExternal)
            {                
                string tbs = DoTabs(tabs);
                string ret = tbs + "var " + name.Value + " = function(){";
                if (block.variables.Count != 0 || parents.Count != 0) ret += "\n";
                foreach (string generic in genericArguments)
                {
                    block.SymbolTable.Add(generic, new Generic(this, block, generic));
                    ret += tbs+"  this.generic$" + generic + " = null;\n";
                }
                foreach (Token parent in parents)
                {
                    ret += tbs + "  " + parent.Value + ".call(this);\n";
                }
                foreach (KeyValuePair<string, Assign> var in block.variables)
                {
                    if (var.Value.Right.getToken().type == Token.Type.NULL)
                        ret += tbs + "  this." + var.Key + " = null;\n";
                    else
                        ret += tbs + "  this." + var.Key + " = " + var.Value.Right.Compile() + ";\n";
                }
                foreach (Types type in block.children)
                {
                    if(type is Properties prop)
                    {
                        ret += tbs + "  this.Property$" + prop.variable.TryVariable().Value + ".$self = this;\n";
                    }
                }
                ret += tbs + "}\n";
                
                ret += tbs + "var " + name.Value + "$META = function(){\n";                    
                ret += tbs + "  return {";
                ret += "\n" + tbs + "    type: 'class'" + (attributes.Count > 0 ? ", " : "");
                if (attributes.Count > 0)
                {
                    ret += "\n" + tbs + "    attributes: {";
                    int i = 0;
                    foreach (_Attribute a in attributes)
                    {
                        ret += "\n" + tbs + "      " + a.GetName() + ": " + a.Compile() + ((attributes.Count - 1) == i ? "" : ", ");
                        i++;
                    }
                    ret += "\n" + tbs + "    },";
                }
                ret += "\n" + tbs + "  };\n";            
                ret += tbs + "};\n";                
                ret += block.Compile(tabs, true);
                return ret;
            }
            return "";
        }        

        public override void Semantic()
        {
            foreach (KeyValuePair<string, Assign> var in block.variables)
            {
                var.Value.Semantic();
            }
            foreach (_Attribute a in attributes)
            {
                a.Semantic();
            }
            block.Semantic();
        }

        public override int Visit()
        {
            return 0;
        }

        public Token OutputType(string op, object a, object b)
        {
            if(block.SymbolTable.Find("operator " + op))
            {
                Types t = block.SymbolTable.Get("operator " + op);
                if(t is Function f)
                {
                    return f.Returnt;
                }
            }
            return new Token(Token.Type.VOID, "void");
        }
        public bool SupportOp(string op)
        {
            if (block.SymbolTable.Find("operator " + op))
            {
                Types t = block.SymbolTable.Get("operator " + op);
                if (t is Function f)
                {
                    return true;
                }
            }
            return false;
        }
        public bool SupportSecond(string op, object second, object secondAsVariable)
        {
            if (block.SymbolTable.Find("operator " + op))
            {
                Types t = block.SymbolTable.Get("operator " + op);
                if (t is Function f)
                {
                    return true;
                }
            }
            return false;
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }

    [Obsolete("Please use not Generic Class instead")]
    public class Class<T>:Types where T:new()
    {
        string name;
        List<Token> parents;
        T intr;
        Block block;        

        public Class(Interpreter interpreter, Block block, string name, List<Token> parents)
        {
            this.name = name;
            this.parents = parents;
            this.intr = new T();
            this.block = new Block(interpreter);
            this.block.Parent = block;
        }

        public bool haveParent(string name)
        {
            foreach(Token t in parents)
            {
                if (t.Value == name)
                    return true;
            }
            return false;
        }

        public T Inter { get { return intr; } }
        public override Token getToken() { return null; }

        public override string Compile(int tabs = 0)
        {
            return "";
        }

        public override void Semantic()
        {
            
        }

        public override int Visit()
        {
            return 0;
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }
    }
}

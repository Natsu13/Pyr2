using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class Class : Types
    {
        Token name;
        public Block block;
        public List<Types> parents;
        public bool isExternal = false;
        public Token _external;
        public bool isDynamic = false;
        public Token _dynamic;
        public string JSName = "";
        public List<string> genericArguments = new List<string>();
        public List<_Attribute> attributes = new List<_Attribute>();
        public bool isForImport = false;
        
        /*Serialization to JSON object for export*/
        [JsonParam] public Token Name => name;
        [JsonParam] public Block Block => block;
        [JsonParam] public List<Types> Parents => parents;

        public override void FromJson(JObject o)
        {
            name = Token.FromJson(o["Name"]);
            block = JsonParam.FromJson<Block>(o["Block"]);
            parents = JsonParam.FromJsonArray<Types>((JArray) o["Parents"]);
            this.assingBlock = block;
        }
        public Class() { }

        public Class(Token name, Block block, List<Types> parents)
        {
            this.name = name;
            this.block = block;
            this.block.assingToType = this;
            this.block.blockAssignTo = name.Value;
            this.block.blockClassTo = name.Value;
            this.assingBlock = block;
            this.parents = parents;
            if (this.parents == null)
                this.parents = new List<Types>();
            if(!this.parents.Any(x => x is UnaryOp && ((UnaryOp)x).Name.Value == "object") && name.Value != "object")
            {
                this.parents.Add(new UnaryOp(new Token(Token.Type.NEW, "new"), new Token(Token.Type.ID, "object"), null, block));
            }
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
        
        string _hash = "";
        public string getHash()
        {
            if (assingBlock == null) assingBlock = block;
            if (_hash == "")
                _hash = string.Format("{0:X8}", (name.Value + genericArguments.GetHashCode() + block?.GetHashCode()).GetHashCode());
            return _hash;
        }

        public string getName(bool force = false) {
            if (assingBlock.SymbolTable.GetAll(name.Value, true).Count > 1)
                return name.Value + "_" + getHash();
            if (force)
                return name.Value;
            if (JSName == null || JSName == "") return name.Value; else return JSName; 
        }
        public override Token getToken() { return null; }

        public override string ToString()
        {
            return "<Class\""+getName()+"\">";
        }

        public Class GetParent()
        {
            if (parents.FirstOrDefault() != null)
            {
                if (parents.FirstOrDefault() is UnaryOp puo)
                {
                    var parentClass = puo.Name.Value;
                    var parent = assingBlock.SymbolTable.Get(parentClass) as Class;                    
                    return parent;
                }
            }

            return null;
        }
        
        public bool haveParent(string name)
        {
            if (name == this.name.Value)
                return true;            
            if (parents == null) 
                return false;
            foreach (UnaryOp p in parents)
            {
                var pp = assingBlock.SymbolTable.Get(p.Name.Value, genericArgs: p.genericArgments.Count);
                if(pp is Class pc)
                {
                    if (pc.Name.Value == name) return true;
                    if (pc.haveParent(name)) return true;
                }
                else if(pp is Interface pi)
                {
                    if (pi.Name.Value == name) return true;
                    if (pi.haveParent(name)) return true;
                }
            }
            return false;
        }

        public Types Get(string name, bool import = true)
        {
            var fn = assingBlock.SymbolTable.Get(name);
            if (!(fn is Error))
            {
                if(fn is Import && import)
                    return fn;
            }
            if (parents == null) return null;
            foreach (UnaryOp p in parents)
            {
                var pp = assingBlock.SymbolTable.Get(p.Name.Value, genericArgs: p.genericArgments.Count);
                var rt = pp.assingBlock.SymbolTable.Get(name);
                if (rt != null && !(rt is Error))
                    return rt;
            }
            return null;
        }

        public override string Compile(int tabs = 0)
        {
            if (!isExternal && !isForImport)
            {
                if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                {
                    string tbs = DoTabs(tabs);
                    string ret = tbs + "var " + getName() + " = function(){";
                    if (block.variables.Count != 0 || parents.Count != 0 || genericArguments.Count != 0) ret += "\n";
                    foreach (string generic in genericArguments)
                    {
                        block.SymbolTable.Add(generic, new Generic(this, block, generic));
                        ret += tbs + "  this.generic$" + generic + " = null;\n";
                    }
                    foreach (UnaryOp parent in parents)
                    {
                        var fnd = assingBlock.SymbolTable.Get(parent.Name.Value, genericArgs: parent.genericArgments.Count);
                        if (!(fnd is Error))
                        {
                            Types inname = fnd;
                            if (inname is Interface)
                            {
                                ret += tbs + "  CloneObjectFunction("+Name.Value+", " + ((Interface) inname).getName() + ");\n";
                            }
                            else if (inname is Class ic && ic.JSName != "Object")
                                ret += tbs + "  CloneObjectFunction(this, " + ((Class) inname).getName() + ");\n";
                        }
                    }
                    foreach (KeyValuePair<string, Assign> var in block.variables)
                    {
                        if (var.Value.isStatic) continue;
                        if (var.Value.Right.getToken().type == Token.Type.NULL)
                        {
                            if (var.Value.Left is Variable vari)
                            {
                                if (block.SymbolTable.Get(vari.Type) is Delegate)
                                    ret += tbs + "  this.delegate$" + var.Key + " = null;\n";
                                else
                                    ret += tbs + "  this." + var.Key + " = null;\n";
                            }
                            else
                                ret += tbs + "  this." + var.Key + " = null;\n";
                        }
                        else
                        {
                            if (var.Value.Left is Variable vari)
                            {
                                if (block.SymbolTable.Get(vari.Type) is Delegate)
                                    ret += tbs + "  this.delegate$" + var.Key + " = " + var.Value.Right.Compile() + ";\n";
                                else
                                    ret += tbs + "  this." + var.Key + " = " + var.Value.Right.Compile() + ";\n";
                            }
                            else
                                ret += tbs + "  this." + var.Key + " = " + var.Value.Right.Compile() + ";\n";
                        }
                    }
                    foreach (Types type in block.children)
                    {
                        if (type is Properties prop)
                        {
                            ret += tbs + "  this.Property$" + prop.variable.TryVariable().Value + ".$self = this;\n";
                        }
                    }
                    ret += tbs + "}\n";


                    foreach (KeyValuePair<string, Assign> var in block.variables)
                    {
                        if (!var.Value.isStatic) continue;
                        if (var.Value.Right.getToken().type == Token.Type.NULL)
                            ret += tbs + "" + getName() + "." + var.Key + " = null;\n";
                        else
                            ret += tbs + "" + getName() + "." + var.Key + " = " + var.Value.Right.Compile() + ";\n";
                    }
                    ret += tbs + "\n";

                    if (Interpreter._DEBUG)
                    {
                        ret += tbs + "var " + getName() + "$META = function(){\n";
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
                    }

                    foreach(var b in block.SymbolTable.Table)
                    {
                        if (b.Value is Function bf)
                        {
                            if(string.IsNullOrEmpty(bf.assingBlock.blockClassTo))
                                bf.assingBlock.blockClassTo = Name.Value;
                        }
                    }
                    ret += block.Compile(tabs, true);
                    return ret;
                }
                else if(Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                {
                    string tbs = DoTabs(tabs-1);
                    string ret = tbs + "class " + getName();
                    bool frst = true;
                    foreach (UnaryOp parent in parents)
                    {
                        if(frst)
                        {
                            ret += ", ";
                            frst = false;
                        }
                        if (!(assingBlock.SymbolTable.Get(parent.Name.Value) is Error))
                        {
                            Types inname = assingBlock.SymbolTable.Get(parent.Name.Value, genericArgs: parent.genericArgments.Count);
                            if (inname is Interface)
                                ret += ((Interface)inname).getName();
                            else if (inname is Class)
                                ret += ((Class)inname).getName();
                        }
                    }
                    ret += ":";
                    if (block.variables.Count != 0 || parents.Count != 0 || genericArguments.Count != 0) ret += "\n";
                    ret += tbs + "  def __init__(self):\n";
                    foreach (string generic in genericArguments)
                    {
                        block.SymbolTable.Add(generic, new Generic(this, block, generic));
                        ret += tbs + "    self.generic__" + generic + " = None;\n";
                    }
                    
                    foreach (KeyValuePair<string, Assign> var in block.variables)
                    {
                        if (var.Value.isStatic) continue;
                        if (var.Value.Right.getToken().type == Token.Type.NULL)
                        {
                            if (var.Value.Left is Variable vari)
                            {
                                if (block.SymbolTable.Get(vari.Type) is Delegate)
                                    ret += tbs + "    self.delegate__" + var.Key + " = None;\n";
                                else
                                    ret += tbs + "    self." + var.Key + " = None;\n";
                            }
                            else
                                ret += tbs + "    self." + var.Key + " = None;\n";
                        }
                        else
                        {
                            if (var.Value.Left is Variable vari)
                            {
                                if (block.SymbolTable.Get(vari.Type) is Delegate)
                                    ret += tbs + "    self.delegate__" + var.Key + " = " + var.Value.Right.Compile() + ";\n";
                                else
                                    ret += tbs + "    self." + var.Key + " = " + var.Value.Right.Compile() + ";\n";
                            }
                            else
                                ret += tbs + "    self." + var.Key + " = " + var.Value.Right.Compile() + ";\n";
                        }
                    }
                    foreach (Types type in block.children)
                    {
                        if (type is Properties prop)
                        {
                            ret += tbs + "    self.Property__" + prop.variable.TryVariable().Value + ".__self = self;\n";
                        }
                    }
                    ret += tbs + "\n";


                    foreach (KeyValuePair<string, Assign> var in block.variables)
                    {
                        if (!var.Value.isStatic) continue;
                        if (var.Value.Right.getToken().type == Token.Type.NULL)
                            ret += tbs + "  " + var.Key + " = None;\n";
                        else
                            ret += tbs + "  " + var.Key + " = " + var.Value.Right.Compile() + ";\n";
                    }
                    ret += tbs + "\n";
                    
                    ret += block.Compile(tabs+1, true);
                    return ret;
                }
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
            if (op == "dot" || op == ".")
                return Name;
            var t = block.SymbolTable.Get("operator " + op);
            if (t is Function f)
            {
                return f.Returnt;
            }
            return new Token(Token.Type.VOID, "void");
        }
        public bool SupportOp(string op)
        {
            if (op == "dot" || op == ".")
                return true;
            var t = block.SymbolTable.Get(block.blockAssignTo + "::operator " + op);
            return t is Function;
        }
        public bool SupportSecond(string op, object second, object secondAsVariable)
        {
            if (op == "dot" || op == ".")
                return true;
            var t = block.SymbolTable.Get(block.blockAssignTo + "::operator " + op);
            return t is Function;
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
            this.block.BlockParent = block;
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

        public override void FromJson(JObject o)
        {
            throw new NotImplementedException();
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

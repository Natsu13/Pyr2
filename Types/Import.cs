using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Import:Types
    {
        Token import;
        bool found = false;
        Interpreter interpret;
        Block block, __block;
        string _as = "";
        Types _ihaveit = null;

        public Import(Token whatimpot, Block _block, Interpreter inter, string _as = null)
        {
            __block = _block;
            this._as = _as;
            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            this.import = whatimpot;

            assingBlock = _block;
            var module = GetModule();
            
            if (_block.SymbolTable.Find(module))
            {
                _ihaveit = _block.SymbolTable.Get(module);   
            }

            if (Interpreter.Imports.ContainsKey(module))
            {
                _ihaveit = Interpreter.Imports[module];
            }
            else
            {
                Interpreter.Imports.Add(module, this);
            }

            if (inter != null && _ihaveit == null && inter.parent != null)
            {
                var find = inter.parent.MainBlock.SymbolTable.Find(module);
                if(find)
                    _ihaveit = inter.parent.MainBlock.SymbolTable.Get(module);   
            }

            if (inter != null && _ihaveit == null && inter.parent != null)
            {
                var mod = inter.parent.FindImport(module);
                if (mod)
                {
                    _ihaveit = inter.parent.GetImport(module);
                }
            }

            string path = whatimpot.Value.Replace('.', '\\');
            if (File.Exists(dir + "\\" + Program.projectFolder + @"\" + path + ".p"))
            {
                found = true;
                if (_ihaveit == null)
                {
                    //Console.WriteLine("[ADD]"+inter.File + "\tAdding " + GetName() + ", module: " + GetModule());
                    string code = File.ReadAllText(dir + "\\" + Program.projectFolder + @"\" + path + ".p");                    
                    interpret = new Interpreter(code, "" + path + ".p", inter);
                    interpret.isConsole = inter.isConsole;
                    block = (Block)interpret.Interpret();
                    block.import = this;
                    if (_as == null)
                    {
                        Block beforeblock = null;
                        Block ___block = _block;
                        foreach (var part in GetName().Split('.').Take(GetName().Split('.').Count()))
                        {
                            if (part == "") continue;
                            if (___block.SymbolTable.Find(part))
                            {
                                ___block = ___block.SymbolTable.Get(part).assingBlock;
                                beforeblock = ___block;
                            }
                            else
                            {
                                //Block b = new Block(_block.Interpret);
                                Block b = block;
                                b.Parent = ___block;
                                Class c = new Class(new Token(Token.Type.ID, part), b, null) { isForImport = true };
                                c.assignTo = part;
                                c.block.SymbolTable.isForImport = true;
                                beforeblock?.SymbolTable.Add(part, c, true);
                                if (beforeblock != ___block)
                                {                                    
                                    ___block.SymbolTable.Add(part, c);
                                    ___block.children.Add(c);
                                }
                                beforeblock = c.block;
                                ___block = b;
                            }
                        }
                        if (!___block.SymbolTable.Find(module))
                        {
                            ___block.SymbolTable.Add(module, this, true);
                            //block.SymbolTable.Copy(whatimpot.Value.Split('.').Last(), _as);
                        }
                    }
                    else
                    {
                        _block.SymbolTable.Add(_as, this);
                        block.SymbolTable.Copy(whatimpot.Value.Split('.').Last(), _as);
                        this._as = string.Join(".", whatimpot.Value.Split('.').Take(whatimpot.Value.Split('.').Length - 1));
                    }
                }
                else
                {
                    //Console.WriteLine("[HAS]"+inter.File + "\tAdding " + GetName() + ", module: " + GetModule());
                    if (_as == null)
                    {
                        //_block.SymbolTable.Get("Array.Clear");
                        if (!_block.SymbolTable.Find(GetName()))
                        {                            
                            if(_ihaveit.assingBlock?.Parent.import == null || _ihaveit is Import)
                                _block.SymbolTable.Add(GetName(), _ihaveit);
                            else
                                _block.SymbolTable.Add(GetName(), _ihaveit.assingBlock?.Parent.import);
                        }
                    }
                    else
                    {
                        _block.SymbolTable.Add(_as, _ihaveit.assingBlock.Parent.import);
                        //Block block = new Block(interpret);
                        //block.SymbolTable.Copy(whatimpot.Value.Split('.').Last(), _as);
                        this._as = string.Join(".", whatimpot.Value.Split('.').Take(whatimpot.Value.Split('.').Length - 1));
                    }
                }
            }
        }

        public string As { 
            get { return _as; } 
            set
            {
                if (value == null && _as != null)
                {
                    //__block.SymbolTable.Delete(import.Value.Split('.').Last());
                    __block.SymbolTable.Table.Remove(import.Value.Split('.').Last());
                    __block.SymbolTable.TableCounter.Remove(import.Value.Split('.').Last());
                    __block.SymbolTable.Add(import.Value, this);
                    _as = null;
                }
                else if (_as != null)
                    throw new FieldAccessException("You can set only null value!");
            }
        }
        public Block Block { get { return (block??__block); } }
        public override Token getToken() { return import; }
        public string GetName()
        {
            var name = string.Join(".", import.Value.Split('.').Take(import.Value.Split('.').Length - 1));
            if (string.IsNullOrEmpty(name))
                return GetModule();
            return name;
        }
        public string GetModule() { return import.Value.Split('.').Last(); }

        public override string Compile(int tabs = 0)
        {
            if (!found || _ihaveit != null) return "";

            string compiled = block.Compile();
            compiled = compiled.Replace("\n", "\n  ");

            string n = GetName();
            string tbs = DoTabs(tabs);

            string outcom = "";
            if (n == "")
            {
                outcom = "\n" + tbs + "//Imported " + GetModule() + "\n";
                outcom += "  " + compiled.Substring(0, compiled.Length) + "\n";
            }
            else
            {
                outcom = "\n" + tbs + "//Imported " + import.Value + "\n";
                if (n.Count(c => c == '.') > 0)
                    outcom += tbs + n + " = function (_, __){\n  'use strict';\n";
                else
                    outcom += tbs + "var " + n + " = function (_, __){\n  'use strict';\n";
                outcom += "  " + compiled.Substring(0, compiled.Length) + "\n";
                List<string> exposed = new List<string>();
                foreach (KeyValuePair<string, Types> t in block.SymbolTable.Table)
                {
                    if (t.Value == null || t.Key.Trim() == "") continue;
                    if (block.SymbolTable.TableIsCopy.ContainsKey(t.Key) && block.SymbolTable.TableIsCopy[t.Key])
                        continue;
                    if (t.Key == "int" || t.Key == "string" || t.Key == "null")
                        continue;
                    if (t.Value is Function && (((Function)t.Value).isExternal || ((Function)t.Value).isExtending))
                        continue;
                    if (t.Value is Class && ((Class)t.Value).isExternal)
                        continue;
                    if (t.Value is Interface && ((Interface)t.Value).isExternal)
                        continue;
                    if (t.Value is Delegate)
                        continue;
                    if (t.Value is Generic)
                        continue;
                    if (t.Value is Function tf)
                    {
                        outcom += "  _." + tf.Name + " = " + tf.Name + ";\n";
                        outcom += "  _." + tf.Name + "$META = " + tf.Name + "$META;\n";
                    }
                    else if (t.Value is Class tc)
                    {
                        if (!tc.isForImport)
                        {
                            outcom += "  _." + tc.getName() + " = " + tc.getName() + ";\n";
                            outcom += "  _." + tc.getName() + "$META = " + tc.getName() + "$META;\n";
                        }
                        if (tc.isForImport)
                        {
                            outcom += Program.DrawClassInside(tc, tc.getName(), exposed, import.Value);
                        }
                    }
                    else if (t.Value is Interface ti)
                    {
                        outcom += "  _." + ti.getName() + " = " + ti.getName() + ";\n";
                        outcom += "  _." + ti.getName() + "$META = " + ti.getName() + "$META;\n";
                    }
                    else if (t.Value is Import im)
                    {
                        var split = im.GetName().Split('.');
                        var namem = string.Join(".", split.Take(split.Length - 1));
                        if (namem == "") namem = split[0];
                        if(namem == import.Value)
                            continue;
                       
                        if (t.Key.Contains("."))
                        {
                            var skl = "";
                            foreach (string p in t.Key.Split('.').Take(t.Key.Split('.').Length - 1))
                            {
                                skl += (skl == "" ? "" : ".") + p;
                                if (!Program.importClass.Contains(skl))
                                {
                                    Program.importClass.Add(skl);
                                    outcom += "  _." + skl + " = {};\n";
                                }
                            }
                        }
                        if (im.GetName() == "")
                        {
                            outcom += "  _." + t.Key + " = " + im.GetModule() + ";\n";
                            if (im.As != "")
                                outcom += "  var " + t.Key + " = " + im.GetModule() + ";\n";
                        }
                        else
                        {
                            var name = "module_" + im.GetName().Replace('.', '_') + "_" + im.GetModule();
                            outcom += "  var "+name+" = GetModule(\"" + im.GetName() + "." + im.GetModule() + "\");\n";
                            //outcom += "  _." + t.Key + " = " + name + ";\n";
                            if (im.As != "")
                                outcom += "  var " + t.Key + " = " + name + ";\n";
                        }
                        if(!exposed.Contains(t.Key))
                            exposed.Add(t.Key);
                        foreach (KeyValuePair<string, Types> qq in im.Block.SymbolTable.Table)
                        {
                            if (qq.Value is Class && !exposed.Contains(qq.Key) && !((Class)qq.Value).isExternal)
                            {
                                if (im.GetName() != "")
                                {
                                    outcom += "  var " + qq.Key + " = " + im.GetName() + "." + ((Class)qq.Value).getName() + ";\n";
                                    exposed.Add(qq.Key);
                                }
                            }
                        }
                    }
                    else
                        outcom += tbs + "  _." + t.Key + " = " + t.Key + ";\n";
                }

                if(import.Value.Contains("."))
                    outcom += "\n"+tbs+"  DefineModule('"+GetName()+"."+GetModule()+"', _);\n";
                else
                    outcom += "\n"+tbs+"  DefineModule('"+GetModule()+"', _);\n";

                outcom += tbs + "\n  return _;\n";
                outcom += tbs + "}(typeof " + n + " === 'undefined' ? {} : " + n + ", this);\n";
            }
            return outcom;
        }

        public override void Semantic()
        {
            if (!found)
                Interpreter.semanticError.Add(new Error("#900 Imported class " + import.Value + " not found!", Interpreter.ErrorType.ERROR, import));
            else if(block != null)
                block.Semantic();
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class Import:Types
    {
        public Token import;
        public bool found = false;
        public Interpreter interpret;
        public Block block;
        Block __block;
        string _as = "";
        public Types _ihaveit = null;
        public string _code = "";
        private bool _precompiled = false;
        private SymbolTable symbolTable = null;

        /*Serialization to JSON object for export*/
        [JsonParam("Import")]
        public string _Import => import.Value;
        
        public override void FromJson(JObject o)
        {
            //TODO: not finished O.O
            import = Token.FromJson(o["Import"]);
        }
        public Import() { }

        public Import(Token whatimpot, Block _block, Interpreter inter, string _as = null)
        {
            __block = _block;
            this._as = _as;
            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            this.import = whatimpot;
            symbolTable = inter.SymbolTable;

            assingBlock = _block;
            var module = GetModule();
            
            if (!(_block.SymbolTable.Get(module) is Error))
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
                var find = inter.parent.MainBlock.SymbolTable.Get(module);
                if(!(find is Error))
                    _ihaveit = find;   
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
                    _code = File.ReadAllText(dir + "\\" + Program.projectFolder + @"\" + path + ".p");                    
                    interpret = new Interpreter(_code, "" + path + ".p", inter, symbolTable: symbolTable);
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
                            var find = ___block.SymbolTable.Get(part);
                            if (!(find is Error))
                            {
                                ___block = find.assingBlock;
                                beforeblock = ___block;
                            }
                            else
                            {
                                //Block b = new Block(_block.Interpret);
                                Block b = block;
                                b.BlockParent = ___block;
                                Class c = new Class(new Token(Token.Type.ID, part), b, null) { isForImport = true };
                                c.assignTo = part;
                                c.block.SymbolTable.isForImport = true;
                                beforeblock?.SymbolTable.Add(part, c, true);
                                if (beforeblock != ___block)
                                {                                    
                                    ___block.SymbolTable.Add(part, c);
                                    ___block.children.Add(c);
                                    c.parent = ___block;
                                }
                                beforeblock = c.block;
                                ___block = b;
                            }
                        }
                        if (___block.SymbolTable.Get(module) is Error)
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
                        if (_block.SymbolTable.Get(GetName()) is Error)
                        {                            
                            if(_ihaveit.assingBlock?.BlockParent.import == null || _ihaveit is Import)
                                _block.SymbolTable.Add(GetName(), _ihaveit);
                            else
                                _block.SymbolTable.Add(GetName(), _ihaveit.assingBlock?.BlockParent.import);
                        }
                    }
                    else
                    {
                        _block.SymbolTable.Add(_as, _ihaveit.assingBlock.BlockParent.import);
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

            //Console.WriteLine(">" +import.Value + ": " + DateTime.Now.ToShortTimeString());
            string compiled = block.Compile();

            /*!!! CACHING !!!*/
            var hash = _code.GetHashCode();
            var path = import.Value.Replace('.', '\\');            
            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            var _compiled = true;
            Interpreter.CurrentStaticInterpreter = interpret;
            if (File.Exists(dir + "\\" + Program.projectFolder + @"\" + path + ".p.h"))
            {
                var fcache = File.ReadAllText(dir + "\\" + Program.projectFolder + @"\" + path + ".p.h");
                JObject jobject = JsonConvert.DeserializeObject<JObject>(fcache);
                if (!Interpreter._RECOMPILE && (int)jobject["hash"] == hash)
                {
                    _compiled = false;
                    //It's same soo we alerady cached it OwO
                    //TODO: 
                    //var cachecode = (JObject)jobject["content"];
                    //var builded = JsonParam.FromJson(cachecode);
                }
            }

            if (_compiled)
            {
                var json = JsonParam.ToJson(block);
                var fname = dir + "\\" + Program.projectFolder + @"\" + path + ".p";
                var rdir = new FileInfo(fname).Directory.FullName;
                JObject fl = new JObject();
                fl["hash"] = hash;
                fl["content"] = json;
                File.WriteAllText(rdir + "\\" + GetModule() + ".p.h", fl.ToString());
                var regex = new Regex(@"\/\/(.*)}\(typeof(.*)\,[ ]this\);", RegexOptions.Multiline | RegexOptions.Singleline);
                var newcompile = regex.Replace(compiled, "");
                File.WriteAllText(rdir + "\\" + GetModule() + ".p.c", newcompile.Trim());
            }

            string atttab = "";
            for(var i = 0; i <  5 - (import.Value.Length / 5); i++) { atttab+="\t"; }

            var now = DateTime.Now;            
            Console.Write((now.Hour < 10 ? "0": "") + now.ToShortTimeString() + ": " + import.Value + atttab + " -> [");
            Console.ForegroundColor = ConsoleColor.White;
            if (_compiled)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.Write("Compiled");
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.Write(" Cached ");
            }
            Console.ResetColor();
            Console.Write("] ");            
            if (interpret.stopwatch.Elapsed.Seconds > 0)
                Console.ForegroundColor = ConsoleColor.DarkRed;            
            else if (interpret.stopwatch.Elapsed.Milliseconds > 400)
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            else
                Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(interpret.stopwatch.Elapsed.Seconds + "." + interpret.stopwatch.Elapsed.Milliseconds.ToString("D3") + " sec");
            Console.ResetColor();
            compiled = compiled.Replace("\n", "\n  ");

            string n = GetName();
            string tbs = DoTabs(tabs);

            var outcom = new StringBuilder();
            if (n == "")
            {
                outcom.Append("\n" + tbs + "//Imported " + GetModule() + "\n");
                outcom.Append(compiled.Substring(0, compiled.Length) + "\n");
            }
            else
            {
                outcom.Append("\n" + tbs + "//Imported " + import.Value + "\n");
                //if (n.Count(c => c == '.') > 0)
                //    outcom.Append(tbs + n + " = function (_, __){\n  'use strict';\n");
                //else
                //    outcom.Append(tbs + "var " + n + " = function (_, __){\n  'use strict';\n");
                outcom.Append(compiled.Substring(0, compiled.Length) + "\n");
                /*
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
                        //outcom.Append("  _." + tf.Name + " = " + tf.Name + ";\n");
                        //if (Interpreter._DEBUG)
                        //    outcom.Append("  _." + tf.Name + "$META = " + tf.Name + "$META;\n");
                    }
                    else if (t.Value is Class tc)
                    {
                        if (!tc.isForImport)
                        {
                            //outcom.Append("  _." + tc.getName() + " = " + tc.getName() + ";\n");
                            //if (Interpreter._DEBUG)
                            //    outcom.Append("  _." + tc.getName() + "$META = " + tc.getName() + "$META;\n");
                        }
                        if (tc.isForImport)
                        {
                            outcom.Append(Program.DrawClassInside(tc, tc.getName(), exposed, import.Value));
                        }
                    }
                    else if (t.Value is Interface ti)
                    {
                        //outcom.Append("  _." + ti.getName() + " = " + ti.getName() + ";\n");
                        //if (Interpreter._DEBUG)
                        //    outcom.Append("  _." + ti.getName() + "$META = " + ti.getName() + "$META;\n");
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
                                    //outcom.Append("  _." + skl + " = {};\n");
                                }
                            }
                        }
                        if (im.GetName() == "")
                        {
                            //outcom.Append("  _." + t.Key + " = " + im.GetModule() + ";\n");
                            //if (im.As != "")
                            //    outcom.Append("  var " + t.Key + " = " + im.GetModule() + ";\n");
                        }
                        else
                        {
                            var name = "";
                            if (im.GetName().Replace('.', '_') != im.GetModule())
                            {
                                //name = "module_" + im.GetName().Replace('.', '_') + "_" + im.GetModule();
                                //outcom.Append("  var " + name + " = GetModule(\"" + im.GetName() + "." + im.GetModule() + "\");\n");
                            }
                            else
                            {
                                //name = "module_" + im.GetName().Replace('.', '_');
                                //outcom.Append("  var " + name + " = GetModule(\"" + im.GetName() + "\");\n");
                            }

                            //outcom += "  _." + t.Key + " = " + name + ";\n";
                            //if (im.As != "")
                            //    outcom.Append("  var " + t.Key + " = " + name + ";\n");
                        }
                        if(!exposed.Contains(t.Key))
                            exposed.Add(t.Key);
                        foreach (KeyValuePair<string, Types> qq in im.Block.SymbolTable.Table)
                        {
                            if (qq.Value is Class && !exposed.Contains(qq.Key) && !((Class)qq.Value).isExternal)
                            {
                                if (im.GetName() != "")
                                {
                                    //outcom.Append("  var " + qq.Key + " = " + im.GetName() + "." + ((Class)qq.Value).getName() + ";\n");
                                    exposed.Add(qq.Key);
                                }
                            }
                        }
                    }
                    //else
                    //    outcom.Append(tbs + "  _." + t.Key + " = " + t.Key + ";\n");
                }

                if(import.Value.Contains("."))
                    outcom.Append("\n"+tbs+"  DefineModule('"+GetName()+"."+GetModule()+"', _);\n");
                else
                    outcom.Append("\n"+tbs+"  DefineModule('"+GetModule()+"', _);\n");

                outcom.Append(tbs + "\n  return _;\n");
                outcom.Append(tbs + "}(typeof " + n + " === 'undefined' ? {} : " + n + ", this);\n");
                */
            }
            return outcom.ToString();
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

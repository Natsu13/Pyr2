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
            string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            this.import = whatimpot;
            
            if (_block.SymbolTable.Find(GetModule()))
            {
                _ihaveit = _block.SymbolTable.Get(GetModule());   
            }
            string path = whatimpot.Value.Replace('.', '\\');
            if (File.Exists(dir + @"\" + path + ".p"))
            {
                found = true;
                if (_ihaveit == null)
                {
                    string code = File.ReadAllText(dir + @"\" + path + ".p");
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
                            if (___block.SymbolTable.Find(part))
                            {
                                ___block = ___block.SymbolTable.Get(part).assingBlock;
                                beforeblock = ___block;
                            }
                            else
                            {
                                Block b = new Block(_block.Interpret);
                                b.Parent = ___block;
                                Class c = new Class(new Token(Token.Type.ID, part), b, null) { isForImport = true };
                                c.assignTo = part;
                                c.block.SymbolTable.isForImport = true;
                                if(beforeblock != null)
                                {
                                    beforeblock.SymbolTable.Add(part, c);
                                }
                                if (beforeblock != ___block)
                                {                                    
                                    ___block.SymbolTable.Add(part, c);
                                    ___block.children.Add(c);
                                }
                                beforeblock = c.block;
                                ___block = b;
                            }
                        }
                        ___block.SymbolTable.Add(GetModule(), this, true);
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
                    if (_as == null)
                        _block.SymbolTable.Add(GetName(), _ihaveit.assingBlock.Parent.import);
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
        public string GetName() { return string.Join(".", import.Value.Split('.').Take(import.Value.Split('.').Length - 1)); }
        public string GetModule() { return import.Value.Split('.').Last(); }

        public override string Compile(int tabs = 0)
        {
            if (!found || _ihaveit != null) return "";

            string compiled = block.Compile();
            compiled = compiled.Replace("\n", "\n  ");

            string n = GetName();
            string tbs = DoTabs(tabs);

            string outcom = "\n"+tbs + "//Imported "+import.Value+"\n";
            if(n.Where(c => c == '.').Count() > 0)
                outcom += tbs + n + " = function (_, __){\n  'use strict';\n";
            else
                outcom += tbs + "var " +n+" = function (_, __){\n  'use strict';\n";
            outcom += "  " + compiled.Substring(0, compiled.Length) + "\n";
            foreach (KeyValuePair<string, Types> t in block.SymbolTable.Table)
            {
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
                if (t.Value is Function tf)
                {
                    outcom += "  _." + tf.Name + " = " + tf.Name + ";\n";
                    outcom += "  _." + tf.Name + "$META = " + tf.Name + "$META;\n";
                }
                else if (t.Value is Class tc)
                {
                    outcom += "  _." + tc.getName() + " = " + tc.getName() + ";\n";
                    outcom += "  _." + tc.getName() + "$META = " + tc.getName() + "$META;\n";
                }
                else if (t.Value is Interface ti)
                {
                    outcom += "  _." + ti.getName() + " = " + ti.getName() + ";\n";
                    outcom += "  _." + ti.getName() + "$META = " + ti.getName() + "$META;\n";
                }
                else if (t.Value is Import im)
                {
                    outcom += tbs + "  _." + t.Key + " = " + im.As + "." + im.GetModule() + ";\n";
                    outcom += tbs + "  var " + t.Key + " = " + im.As + "." + im.GetModule() + ";\n";
                }
                else
                    outcom += tbs + "  _." + t.Key + " = " + t.Key + ";\n";
            }
            outcom += tbs + "\n  return _;\n";
            outcom += tbs + "}(typeof " + n + " === 'undefined' ? {} : " + n + ", this);\n";

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

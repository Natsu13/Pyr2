using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Compilator
{  
    class Program
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DrawClassInside(Class c, string add, List<string> exposed, string addMeMain = "")
        {
            return DrawClassInside(c.Block, add, exposed, addMeMain);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string DrawClassInside(Block block, string add, List<string> exposed, string addMeMain = "")
        {
            StringBuilder outcom = new StringBuilder();
            foreach (KeyValuePair<string, Types> t in block.SymbolTable.Table)
            {
                if (t.Value == null || t.Key.Trim() == "") continue;
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
                        if (addMeMain != "")
                        {
                            if(tc.assignTo == "")
                                continue;                            
                            //outcom.Append("  _." + tc.getName() + " = " + addMeMain + "." + tc.getName() + ";\n");
                            //outcom.Append("  _." + tc.getName() + "$META = " + addMeMain + "." + tc.getName() + "$META;\n");
                        }
                        else
                        {
                            if (tc.assignTo != "")
                            {
                                //outcom.Append("  _." + tc.getName() + " = " + tc.assignTo + "." + tc.getName() + ";\n");
                                //outcom.Append("  _." + tc.getName() + "$META = " + tc.assignTo + "." + tc.getName() + "$META;\n");
                            }
                            else
                            {
                                //outcom.Append("  _." + tc.getName() + " = " + tc.getName() + ";\n");
                                //outcom.Append("  _." + tc.getName() + "$META = " + tc.getName() + "$META;\n");
                            }
                        }
                    }
                    //outcom.Append("  var " + t.Key + " = " + add + "." + tc.Name.Value + ";\n");
                    //if(tc.block.Parent.import.getToken().Value != t.Key)
                    //    outcom.Append("  var " + t.Key + " = GetModule(\"" + tc.block.Parent.import.getToken().Value + "\")." + t.Key + ";\n");
                    //else
                    //    outcom.Append("  var " + t.Key + " = GetModule(\"" + tc.block.Parent.import.getToken().Value + "\");\n");

                    if (tc.isForImport)
                    {
                        if(tc.block.BlockParent.import.getToken().Value != t.Key)
                            outcom.Append(DrawClassInside(tc, add + "." + tc.Name.Value, new List<string>(), add + "." + tc.Name.Value));
                        else
                            outcom.Append(DrawClassInside(tc, add, new List<string>(), add));
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
                    if(namem == addMeMain)
                        continue;

                    if (t.Key.Contains("."))
                    {
                        var skl = "";
                        foreach (string p in t.Key.Split('.').Take(t.Key.Split('.').Length - 1))
                        {
                            skl += (skl == "" ? "" : ".") + p;
                            if (!importClass.Contains(skl))
                            {
                                importClass.Add(skl);
                                //outcom.Append("  _." + skl + " = {};\n");
                            }
                        }
                    }
                    if (im.GetName() == "")
                    {
                        //outcom.Append("  _." + t.Key + " = " + im.GetModule() + ";\n");
                        //if(im.As != "")
                        //    outcom.Append("  var " + t.Key + " = " + im.GetModule() + ";\n");
                    }
                    else
                    {
                        if (add == "")
                        {
                            //outcom.Append("  _." + t.Key + " = " + im.GetName() + "." + im.GetModule() + ";\n");
                            //if (im.As != "")
                            //    outcom.Append("  var " + t.Key + " = " + im.GetName() + "." + im.GetModule() + ";\n");
                        }
                        else
                        {
                            //outcom.Append("  _." + t.Key + " = " + add + "." + im.GetModule() + ";\n");
                            //if (!string.IsNullOrEmpty(im.As))
                            //{
                                //outcom.Append("  var " + t.Key + " = " + add + "." + im.GetModule() + ";\n");
                                //outcom.Append("  var " + t.Key + " = GetModule(\"" + im.getToken().Value + "\")." + t.Key + ";\n");
                            //}
                        }
                        exposed.Add(t.Key);
                    }
                    foreach(KeyValuePair<string, Types> qq in im.Block.SymbolTable.Table)
                    {
                        if(qq.Value is Class cc && !exposed.Contains(qq.Key) && !((Class)qq.Value).isExternal && !cc.isForImport)
                        {
                            if (im.GetName() != "")
                            {
                                //outcom.Append("  var " + qq.Key + " = " + im.GetName() + "." + ((Class)qq.Value).getName() + ";\n");
                                exposed.Add(qq.Key);
                            }
                        }
                    }
                }
                else
                    outcom.Append("  _." + t.Key + " = " + t.Key + ";\n");
            }
            return outcom.ToString();
        }

        public static void RunServer(int port = 13000)
        {
            Console.WriteLine("Server runing on port " + port);
            var wssv = new WebSocketServer("ws://127.0.0.1:" + port);
            wssv.AddWebSocketService<Service>("/pyr");
            wssv.Start();
            Console.ReadKey (true);
            wssv.Stop();
        }

        public static List<string> importClass = new List<string>();
        public static string projectFolder = "";
        private static bool _runServer = false;

        private static bool _wait = false;
        static void Main(string[] args)
        {
            projectFolder = "Project";

            if (args.Length > 0 && args[0] == "-wait")
                _wait = true;
            else if (args.Length < 1 || args[0] == "")
            {
                
            }
            else
            {
                projectFolder = args[0];
            }

            if (args.Length < 2 || args[1] == "")
            {
            }
            else
            {
                if (args[1] == "JS")
                    Interpreter._LANGUAGE = Interpreter.LANGUAGES.JAVASCRIPT;
                if (args[1] == "PY")
                    Interpreter._LANGUAGE = Interpreter.LANGUAGES.PYTHON;
            }

            if (args.Length > 1)
            {
                if (args[2] == "server")
                    _runServer = true;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            string text = File.ReadAllText(projectFolder + @"/code.p");
            Interpreter.semanticError.Clear();
            Interpreter interpret = new Interpreter(text, "/" + projectFolder + "/code.p", null, projectFolder);
            interpret.isConsole = true;

            var stopw = new Stopwatch();
            stopw.Start();
            Block block = (Block) interpret.Interpret();
            stopw.Stop();
            StringBuilder compiled = new StringBuilder();
            var stopwc = new Stopwatch();
            stopwc.Start();
            if (block != null)
            {
                var compiler = new Compiler();
                //compiled.Append(block.Compile());
                compiled = compiler.Compile(block, addBl: false);
                //compiled = compiler.StartCompile(block);
            }
            stopwc.Stop();
            
            StringBuilder outcom = new StringBuilder("");
            if (block != null)
            {
                if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                {
                    compiled = compiled.Replace("\n", "\n  ");
                    outcom = new StringBuilder("var module = function (_){\n  'use strict';\n");
                    outcom.Append("  " + compiled.ToString() + "\n");
                    importClass = new List<string>();
                    List<string> exposed = new List<string>();
                    foreach (KeyValuePair<string, Types> t in block.SymbolTable.Table)
                    {
                        if (t.Value == null || t.Key.Trim() == "") continue;
                        if (t.Key == "int" || t.Key == "string" || t.Key == "null")
                            continue;
                        if (t.Value is Function && (((Function) t.Value).isExternal || ((Function) t.Value).isExtending))
                            continue;
                        if (t.Value is Class && ((Class) t.Value).isExternal)
                            continue;
                        if (t.Value is Interface && ((Interface) t.Value).isExternal)
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
                                outcom.Append(DrawClassInside(tc, tc.getName(), exposed));
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
                            if (t.Key.Contains("."))
                            {
                                var skl = "";
                                foreach (string p in t.Key.Split('.').Take(t.Key.Split('.').Length - 1))
                                {
                                    skl += (skl == "" ? "" : ".") + p;
                                    if (!importClass.Contains(skl))
                                    {
                                        importClass.Add(skl);
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
                                //outcom.Append("  _." + t.Key + " = " + im.GetName() + "." + im.GetModule() + ";\n");
                                //if (im.As != "")
                                //    outcom.Append("  var " + t.Key + " = " + im.GetName() + "." + im.GetModule() + ";\n");
                            }

                            if (!exposed.Contains(t.Key))
                                exposed.Add(t.Key);
                            foreach (KeyValuePair<string, Types> qq in im.Block.SymbolTable.Table)
                            {
                                if (qq.Value is Class && !exposed.Contains(qq.Key) && !((Class) qq.Value).isExternal)
                                {
                                    if (im.GetName() != "")
                                    {
                                        //outcom.Append("  var " + qq.Key + " = " + im.GetName() + "." + ((Class) qq.Value).getName() + ";\n");
                                        exposed.Add(qq.Key);
                                    }
                                }
                            }
                        }
                        //else
                        //    outcom.Append("  _." + t.Key + " = " + t.Key + ";\n");
                    }

                    foreach (var import in interpret.imports)
                    {
                        if (import.Key.Length - import.Key.Replace(".", "").Length > 1)
                        {
                            var module = import.Value.GetModule();
                            //outcom.Append("  var " + module + " = GetModule(\"" + import.Key + "\")." + module + ";\n");
                        }
                    }

                    outcom.Append("\n  DefineModule('module', _);\n");

                    var fnd = block.SymbolTable.Get("main");
                    if (!(fnd is Error))
                    {
                        if (((Function) fnd).attributes.Any(x => x.GetName(true) == "OnPageLoad"))
                        {
                            outcom.Append("\n  window.onload = function(){ try {");
                            if (interpret.FileSource != null)
                            {
                                string file = "";
                                if (!File.Exists(projectFolder + "/" + interpret.FileSource.Value))
                                {
                                    Interpreter.semanticError.Add(new Error("#002 Source file " + interpret.FileSource.Value + " not found!", Interpreter.ErrorType.WARNING, interpret.FileSource));
                                }
                                else
                                {
                                    file = File.ReadAllText(projectFolder + "/" + interpret.FileSource.Value);
                                }

                                outcom.Append("\n    document.querySelector('#container').innerHTML = \"" + file.Replace("\"", "&uvz;").Replace("\r\n", "") + "\".replace(new RegExp(\"&uvz;\", 'g'), '\"');\n");
                                outcom.Append("    main();\n");
                                outcom.Append("  }catch(e){ catcherror(e); } };\n");
                            }
                            else
                            {
                                outcom.Append("main();");
                                outcom.Append("}catch(e){ catcherror(e); } };\n");
                            }

                        }
                        else
                            outcom.Append("\n  main();\n");
                    }

                    outcom.Append("\n  return _;\n");
                    outcom.Append("}(typeof module === 'undefined' ? {} : module);");
                }
                else if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                {
                    outcom.Append(compiled.ToString());
                }                
                block.Semantic();
            }


            stopwatch.Stop();

            bool iserror = false;
            if(Interpreter.semanticError.Count > 0)
            {
                foreach(Error error in Interpreter.semanticError)
                {
                    string place = error.Place;
                    if (place != "") place += "\t";
                    if (error.Type == Interpreter.ErrorType.ERROR) {
                        Console.WriteLine(place + "error: "+error.Message);
                        iserror = true;
                    }
                    else if (error.Type == Interpreter.ErrorType.WARNING)
                        Console.WriteLine(place + "warning: " + error.Message);
                    else if (error.Type == Interpreter.ErrorType.INFO)
                        Console.WriteLine(place + "info: " + error.Message);
                }
            }
            
            if (Interpreter.isForExport)
            {
                string lastname = "";
                Console.WriteLine("Finished in " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds + " sec");
                if (iserror)
                    Console.WriteLine("While compiling some errors was found!");
                Console.WriteLine("=== Avalible for export ===");
                PrintInSymbolTable(block.SymbolTable);                
                
                while (true)
                {
                    Console.Write("Enter name for export: ");
                    string name = Console.ReadLine();
                    if (name == "") {
                        if(lastname != "") { 
                            Console.WriteLine("Exporting: " + lastname + "...");
                            System.IO.StreamWriter file = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + @"\Export\" + lastname + ".txt");                        
                            
                            Types t = block.SymbolTable.Get(lastname);
                            if (t is Function _f)
                            {
                                string nam = _f.RealName;
                                string nax = nam.Replace(" ", "_");
                                file.WriteLine("Token Function_"+ nax + " = new Token(Token.Type.ID, \""+ nam + "\");");
                                file.WriteLine("ParameterList plist_" + nax + " = new ParameterList(true);");
                                file.WriteLine("Block Block_" + nax + " = new Block(interpret) { Parent = assigment_block };");
                                foreach (Types v in _f.ParameterList.Parameters)
                                {
                                    if (v is Variable __v)
                                        file.WriteLine("plist_" + nax + ".parameters.Add(new Variable(new Token(Token.Type.ID, \"" + __v.Value + "\"), Block_" + nax + ", new Token(Token.Type.CLASS, \"" + __v.GetDateType().Value + "\")));");
                                    else if (v is Assign __a)
                                    {
                                        string def = "";
                                        if (__a.Right is Number ___n)
                                            def = "new Number(new Token(Token.Type.INTEGER, \""+___n.Value+"\"))"; 
                                        else if(__a.Right is CString ___s)
                                            def = "new CString(new Token(Token.Type.STRING, \"" + ___s.Value + "\"))";
                                        else if(__a.Right is Variable ___v)
                                            def = "new Variable(new Token(Token.Type.ID, \"" + ___v.Value + "\"))";

                                        file.WriteLine("plist_" + nax + ".parameters.Add(new Assign(new Variable(new Token(Token.Type.ID, \"" + ((Variable)(__a.Left)).Value + "\"), Block_" + nax + ", new Token(Token.Type.CLASS, \"" + ((Variable)(__a.Left)).GetDateType().Value + "\")), new Token(Token.Type.ASIGN, \"=\"), " + def + ", Block_" + nax + "));");
                                    }
                                }
                                if(_f.Returnt == null || _f.Returnt.type == Token.Type.VOID)
                                    file.WriteLine("Function " + nax + " = new Function(Function_" + nax + ", Block_" + nax + ", plist_" + nax + ", new Token(Token.Type.VOID, \"void\"), interpret) { isExternal = "+_f.isExternal.ToString().ToLower() + ", isConstructor = "+ _f.isConstructor.ToString().ToLower() + ", isOperator = " + _f.isOperator.ToString().ToLower() + ", isStatic = " + _f.isStatic.ToString().ToLower() + " };");
                                else
                                    file.WriteLine("Function " + nax + " = new Function(Function_" + nax + ", Block_" + nax + ", plist_" + nax + ", new Token(Token.Type.CLASS, \""+_f.Returnt.Value+"\"), interpret) { isExternal = " + _f.isExternal.ToString().ToLower() + ", isConstructor = " + _f.isConstructor.ToString().ToLower() + ", isOperator = " + _f.isOperator.ToString().ToLower() + ", isStatic = " + _f.isStatic.ToString().ToLower() + " };");
                                file.WriteLine("Add(\"" + nam + "\", " + nax + ");");
                            }
                            else if(t is Class _c)
                            {

                            }

                            file.Close();
                        }
                        break;
                    }
                    if (block.SymbolTable.Get(name) is Error)
                    {
                        Console.WriteLine("Failed to get the \"" + name + "\" from SymbolTable");
                    }
                    else
                    {
                        Console.WriteLine("=========================");
                        Types t = block.SymbolTable.Get(name);
                        if (t is Class _c)
                        {                            
                            Console.WriteLine("Class: \t\t\t" + _c.getName());
                            Console.WriteLine("Generic arguments: \t" + string.Join(",", _c.GenericArguments));
                            Console.WriteLine("inParent: \t\t" + _c.inParen);
                            Console.WriteLine("isDynamic: \t\t" + _c.isDynamic);
                            Console.WriteLine("isExternal: \t\t" + _c.isExternal);
                            Console.WriteLine("=== Avalible for export ===");
                            PrintInSymbolTable(_c.block.SymbolTable);                           
                        }
                        if(t is Function _f)
                        {
                            Console.WriteLine("Function: \t" + _f.Name);
                            Console.WriteLine("isExtending: \t" + _f.isExtending);
                            if(_f.isExtending)
                                Console.WriteLine("Extending class: " + _f.extendingClass);
                            Console.WriteLine("inParen: \t" + _f.inParen);
                            Console.WriteLine("isConstructor: \t" + _f.isConstructor);
                            Console.WriteLine("isDynamic: \t" + _f.isDynamic);
                            Console.WriteLine("isExternal: \t" + _f.isExternal);
                            Console.WriteLine("isOperator: \t" + _f.isOperator);
                            Console.WriteLine("isStatic: \t" + _f.isStatic);
                            Console.WriteLine("ParameterList: \t" + _f.ParameterList.List());
                            Console.WriteLine("Return type: \t" + _f.Returnt?.Value);                            
                        }
                    }
                    lastname = name;
                }
            }
            else
            {
                var now = DateTime.Now;         
                Console.Write((now.Hour < 10 ? "0": "") + now.ToShortTimeString() + ": Main \t\t\t\t -> ");
                Console.WriteLine(interpret.stopwatch.Elapsed.Seconds + "." + interpret.stopwatch.Elapsed.Milliseconds + "sec");
                if (!iserror || Interpreter._WRITEDEBUG)
                {
                    System.IO.StreamWriter file = null;

                    if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                    {
                        outcom = new StringBuilder("//# sourceURL=PyrOutput.js\n//Compiled in " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds.ToString("D3") + " sec\n//On " + DateTime.Now.ToLocalTime() + "\n//By PYR compiler\n" + outcom.ToString());
                        file = new System.IO.StreamWriter("output.js");
                    }
                    else
                    {
                        outcom = new StringBuilder("#Compiled in " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds.ToString("D3") + " sec\n#On " + DateTime.Now.ToLocalTime() + "\n#By PYR compiler\n" + outcom.ToString());
                        file = new System.IO.StreamWriter("output.py");
                    }

                    file.WriteLine(outcom);
                    file.Close();
                    if (iserror && Interpreter._WRITEDEBUG)
                    {
                        Console.WriteLine("Compiled in " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds.ToString("D3") + " sec, with error but writed to output!");
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("----------------------------------------------------------------");
                        Console.ResetColor();
                        Console.WriteLine("Word(all symbols) " + Interpreter.fileList.Select(x => x.Value.Length).Sum());
                        Console.WriteLine("Interpretat " + stopw.Elapsed.Seconds + "." + stopw.Elapsed.Milliseconds.ToString("D3") + " sec");
                        Console.WriteLine("Symboltable " + Interpreter.SymboltableStopwatch.Elapsed.Seconds + "." + Interpreter.SymboltableStopwatch.Elapsed.Milliseconds.ToString("D3") + " sec");
                        Console.WriteLine("Compiled in " + stopwc.Elapsed.Seconds + "." + stopwc.Elapsed.Milliseconds.ToString("D3") + " sec");                        
                        Console.WriteLine("Finished in " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds.ToString("D3") + " sec");
                        if(_runServer)
                            RunServer();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Compiled in " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds + " sec, with errors.");
                    Console.ResetColor();
                    Console.ReadKey();
                }
            }
            if(_wait)
                Console.ReadKey();            
        }        

        public static void PrintInSymbolTable(SymbolTable s)
        {
            foreach (KeyValuePair<string, Types> t in s.Table)
            {
                if (t.Value is Class _c)
                { Console.WriteLine("Class \t\t>> " + _c.getName()); }
                if (t.Value is Function _f)
                {
                    string n = _f.Name;
                    if (_f.Name != _f.RealName) n = "(" + _f.RealName + ")" + _f.Name;
                    if (_f.isExtending) { Console.WriteLine("Function \t>> (Extending " + _f.extendingClass + ") " + n); }
                    else { Console.WriteLine("Function \t>> " +n); }
                }
            }
        }        
    }
}
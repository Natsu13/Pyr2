using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Compilator
{
    class Program
    {        
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            string text = File.ReadAllText(@"code.p");
            Interpreter.semanticError.Clear();
            Interpreter interpret = new Interpreter(text, "code.p");
            interpret.isConsole = true;

            Block block = (Block)interpret.Interpret();
            string compiled = block.Compile();
            compiled = compiled.Replace("\n", "\n  ");
            string outcom = "var module = function (_){\n  'use strict';\n";            
            outcom += "  "+ compiled.Substring(0, compiled.Length) + "\n";
            List<string> importClass = new List<string>();
            foreach (KeyValuePair<string, Types> t in block.SymbolTable.Table)
            {
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
                    if (t.Key.Contains("."))
                    {
                        var skl = "";
                        foreach (string p in t.Key.Split('.').Take(t.Key.Split('.').Length - 1))
                        {
                            skl += (skl == "" ? "" : ".") + p;
                            if (!importClass.Contains(skl))
                            {
                                importClass.Add(skl);
                                outcom += "  _." + skl + " = {};\n";
                            }
                        }
                    }
                    outcom += "  _." + t.Key + " = " + im.GetName() + "." + im.GetModule() + ";\n";
                    if(im.As != "")
                        outcom += "  var " + t.Key + " = " + im.GetName() + "." + im.GetModule() + ";\n";
                }
                else
                    outcom += "  _." + t.Key + " = " + t.Key + ";\n";
            }
            if (block.SymbolTable.Find("main"))
            {
                if(Interpreter._WAITFORPAGELOAD)
                    outcom += "\n  window.onload = function(){ main(); };\n";
                else
                    outcom += "\n  main();\n";
            }
            outcom += "\n  return _;\n";
            outcom += "}(typeof module === 'undefined' ? {} : module);";            
            block.Semantic();
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
            stopwatch.Stop();
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
                                        file.WriteLine("plist_" + nax + ".parameters.Add(new Variable(new Token(Token.Type.ID, \"" + __v.Value + "\"), Block_" + nax + ", new Token(Token.Type.CLASS, \"" + __v.getDateType().Value + "\")));");
                                    else if (v is Assign __a)
                                    {
                                        string def = "";
                                        if (__a.Right is Number ___n)
                                            def = "new Number(new Token(Token.Type.INTEGER, \""+___n.Value+"\"))"; 
                                        else if(__a.Right is CString ___s)
                                            def = "new CString(new Token(Token.Type.STRING, \"" + ___s.Value + "\"))";
                                        else if(__a.Right is Variable ___v)
                                            def = "new Variable(new Token(Token.Type.ID, \"" + ___v.Value + "\"))";

                                        file.WriteLine("plist_" + nax + ".parameters.Add(new Assign(new Variable(new Token(Token.Type.ID, \"" + ((Variable)(__a.Left)).Value + "\"), Block_" + nax + ", new Token(Token.Type.CLASS, \"" + ((Variable)(__a.Left)).getDateType().Value + "\")), new Token(Token.Type.ASIGN, \"=\"), " + def + ", Block_" + nax + "));");
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
                    if (!block.SymbolTable.Find(name))
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
                if (!iserror || Interpreter._WRITEDEBUG)
                {
                    System.IO.StreamWriter file = new System.IO.StreamWriter("output.js");
                    outcom = "//Compiled in " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds + " sec\n//On "+DateTime.Now.ToLocalTime()+"\n//By PYR compiler\n" + outcom;
                    file.WriteLine(outcom);
                    file.Close();
                    if (iserror && Interpreter._WRITEDEBUG)
                        Console.WriteLine("Compiled in " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds + " sec, with error but writed to output!");
                    else
                        Console.WriteLine("Compiled in " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds + " sec");
                }
                else
                {
                    Console.WriteLine("Compiled in " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds + " sec, with errors.");
                }
            }
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

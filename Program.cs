using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    class Program
    {        
        static void Main(string[] args)
        {            
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            string text = File.ReadAllText(@"code.p");
            Interpreter interpret = new Interpreter(text, "code.p");
            interpret.isConsole = true;

            Block block = (Block)interpret.Interpret();
            string compiled = block.Compile();
            compiled = compiled.Replace("\n", "\n  ");
            string outcom = "var module = function (_){\n  'use strict';\n";            
            outcom += "  "+ compiled.Substring(0, compiled.Length) + "\n";
            foreach (KeyValuePair<string, Types> t in block.SymbolTable.Table)
            {
                if (t.Key == "int" || t.Key == "string" || t.Key == "null")
                    continue;
                if (t.Value is Function && ((Function)t.Value).isExternal)
                    continue;
                if (t.Value is Class && ((Class)t.Value).isExternal)
                    continue;
                if (t.Value is Interface && ((Interface)t.Value).isExternal)
                    continue;
                outcom += "  _." + t.Key + " = " + t.Key+";\n";
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
            Interpreter.semanticError.Clear();
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
            if (!iserror)
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter("output.js");
                file.WriteLine(outcom);
                file.Close();                
                Console.WriteLine("Compiled in " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds + " sec");
            }
            else
            {
                Console.WriteLine("Compiled in " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds + " sec with Errors.");
            }
            Console.ReadKey();
        }
    }
}

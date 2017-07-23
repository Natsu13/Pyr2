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
            Interpreter interpret = new Interpreter(text);
            Block block = (Block)interpret.Interpret();
            string compiled = "'use strict';\n" + block.Compile();
            Interpreter.semanticError.Clear();
            block.Semantic();
            bool iserror = false;
            if(Interpreter.semanticError.Count > 0)
            {
                foreach(Error error in Interpreter.semanticError)
                {
                    if (error.Type == Interpreter.ErrorType.ERROR) {
                        Console.WriteLine("error: "+error.Message);
                        iserror = true;
                    }
                    else if (error.Type == Interpreter.ErrorType.WARNING)
                        Console.WriteLine("warning: " + error.Message);  
                }
            }
            if (!iserror)
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter("output.js");
                file.WriteLine(compiled);
                file.Close();
                stopwatch.Stop();
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

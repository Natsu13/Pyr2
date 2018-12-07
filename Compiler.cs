using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class Compiler
    {
        public Compiler()
        {

        }

        private const string _tabs = "  ";
        private string DoTabs(int tabs)
        {
            string r = "";
            for (int i = 0; i < tabs; i++) { r += _tabs; }
            return r;
        }

        public StringBuilder Compile(Types type, int tabs = 0, bool addBl = true)
        {
            if (type == null || type is NoOp)
                return new StringBuilder("");
            if (type is Block block)
                return CompileBlock(block, tabs, addBl: addBl);
            if (type is _Attribute attribute)
                return CompileAttribute(attribute);
            if (type is _Enum _enum)
                return CompileEnum(_enum, tabs);
            if (type is Array array)
                return CompileArray(array);
            if (type is Assign assign)
                return CompileAssign(assign, tabs);
            if (type is Class _class)
                return CompileClass(_class, tabs);
            if (type is Function function)
                return CompileFunction(function, tabs);
            if (type is ParameterList parameterList)
                return CompileParameterList(parameterList);
            if (type is Variable variable)
                return CompileVariable(variable, tabs);
            if (type is NamedTuple namedTuple)
                return CompileNamedTuple(namedTuple, tabs);
            if (type is CString cString)
                return CompileString(cString);
            if (type is UnaryOp unaryOp)
                return CompileUnaryOp(unaryOp, tabs);
            if (type is Import import)
                return CompileImport(import, tabs);
            if (type is Delegate _delegate)
                return CompileDelegate(_delegate, tabs);
            if (type is Number number)
                return CompileNumber(number);
            if (type is If _if)
                return CompileIf(_if, tabs);
            if (type is BinOp binOp)
                return CompileBinOp(binOp, tabs);
            if (type is For _for)
                return CompileFor(_for, tabs);
            if (type is Null)
                return new StringBuilder("null");
            if (type is Lambda lambda)
                return CompileLambda(lambda, tabs);
            if (type is Component component)
                return CompileComponent(component, tabs);
            if (type is TernaryOp ternaryOp)
                return CompileTernaryOp(ternaryOp, tabs);
            throw new Exception("Unknown type: " + type.GetType().Name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileTernaryOp(TernaryOp ternaryOp, int tabs = 0)
        {
            ternaryOp.condition.endit = false;
            return new StringBuilder("(" + Compile(ternaryOp.condition) + " ? " + Compile(ternaryOp.left) + ": " + Compile(ternaryOp.right) + ")");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileComponent(Component component, int tabs = 0, bool iret = false)
        {
            var tbs = DoTabs(tabs);
            string ret = "";
            if (component.assignTo != "")
            {
                var func = (Function) component.assingBlock.SymbolTable.Get(component.assignTo);
                if (func.attributes?.Where(x => x.GetName(true) == "Debug").Count() > 0)
                {
                    ret += "/*Component: " + component.Name + "*/";
                }
            }

            var _oname = component.Name;

            var txt = component.InnerText.Replace("\r\n", "").Replace("\t", "");
            Regex re = new Regex(@"\{(.*)\}");
            var replace = re.Replace(txt, x =>
            {
                var s = x.Value.Substring(1, x.Value.Length - 2);
                var find = component.assingBlock.SymbolTable.Get(s);
                if (component.assingBlock.assingToType?.assignTo != null)
                {
                    var _class = component.assingBlock.SymbolTable.Get(component.assingBlock.assingToType?.assignTo);
                    if (_class is Class cls)
                    {
                        var parent = cls.GetParent();
                        find = parent.assingBlock.SymbolTable.Get(s);
                    }
                }
                if (find is Variable)
                    return "\"+" + s + "+\"";
                if (find is Function)
                    return "fun";
                if (find is Assign fa)
                    return "\"+" + s + "+\"";
                if (find is Class)
                {
                    return "\"+" + s.Replace("this", "_this") + "+\"";
                }
                return "";
            });
            
            var pou = "";
            foreach (var p in component.Arguments)
            {
                if (p.Value is Assign pa)
                {
                    if (pa.Left is Variable pav)
                    {
                        var cml = Compile(p.Value);
                        pou += "" + p.Key + ": " + cml.Replace("this", "_this") + ", ";
                        //args += "{name: \"" + pav.Value + "\", value: " + cml + "}, ";
                    }
                }
                else if (p.Value is Variable pv)
                {
                    var cml = Compile(p.Value);
                    if (Char.IsLetterOrDigit(p.Key[0]))
                        pou += "" + p.Key + ": ";
                    else
                        pou += "\"" + p.Key + "\": ";
                    if ((p.Key.Substring(0, 2) != "on" && cml.ToString().Contains("this")))
                        pou += "function(){ return " + cml.Replace("this", "_this") + "; }, ";
                    else
                        pou += cml.Replace("this", "_this") + ", ";
                    //args += "{name: \"" + pv.Value + "\", value: " + cml + "}, ";
                }
                else if (p.Value is Lambda arl)
                {
                    arl.endit = false;
                    arl.replaceThis = "_this";
                    var cml = Compile(arl);
                    if (Char.IsLetterOrDigit(p.Key[0]))
                        pou += "" + p.Key + ": " + cml.Replace("this", "_this").Replace("__this", "_this") + ", ";
                    else
                        pou += "\"" + p.Key + "\": " + cml.Replace("this", "_this").Replace("__this", "_this") + ", ";
                }
                else
                {
                    if (Char.IsLetterOrDigit(p.Key[0]))
                        pou += "" + p.Key + ": " + Compile(p.Value).Replace("this", "_this") + ", ";
                    else
                        pou += "\"" + p.Key + "\": " + Compile(p.Value).Replace("this", "_this") + ", ";
                }
            }

            if(pou != "")
                pou = pou.Substring(0, pou.Length - 2);

            if (component.Name == "")
            {                
                var tb0 = DoTabs(iret ? 0 : tabs);
                if(component.Fun == null)
                    ret += tb0 + "\"" + replace + "\"";
                else
                {
                    if (component.Fun is UnaryOp fu && fu.Op == "call")
                    {
                        fu.endit = false;
                    }
                    ret += tb0 + "function(){ return " + Compile(component.Fun).Replace("this", "_this").Replace("__this","_this") + "; }";
                }
            }
            else if (component.InnerComponent.Count > 0)
            {
                var tb0 = DoTabs(iret ? 0 : tabs);
                var tb1 = DoTabs(iret ? tabs + 2 : tabs + 1);
                var hm0 = iret ? tabs + 2 : tabs + 1;

                if (Component._base.Contains(_oname) || _oname.ToLower() == _oname)
                {
                    var f = component.assingBlock.SymbolTable.Get(component.Name);
                    if(f is Error)
                        ret += tb0 + "new Pyr.createElement(\r\n" + tb1 + "\"" + _oname + "\"";
                    else
                        ret += tb0 + "new Pyr.createElement(\r\n" + tb1 + "" + _oname + "";
                }
                else
                    ret += tb0 + "new Pyr.createElement(\r\n" + tb1 + component.Name;
                if (pou != "")
                    ret += ",\r\n" + tb1 + "{ " + pou + " }, \r\n";
                else
                    ret += ",\r\n" + tb1 + "null, \r\n";
                component.InnerComponent.ForEach(x => { ret += CompileComponent(x) + ", \r\n"; });
                ret = ret.Substring(0, ret.Length - 4);
                ret += "\r\n" + tb0 + ")";
            }
            else
            {
                var tb0 = DoTabs(iret ? 0 : tabs);

                if (Component._base.Contains(_oname) || _oname.ToLower() == _oname)
                    ret += tb0 + "new Pyr.createElement(" + "\"" + _oname + "\"";
                else
                {
                    var fncreate = component.assingBlock.SymbolTable.Get("constructor " + component.Name);
                    if (fncreate is Error)
                    {
                        component._componentNotHaveConstructor = true;
                        if (component.assingBlock.SymbolTable.Get(component.Name) is Error)
                        {
                            component._componentNotHaveConstructor = false;
                            component._componentNotFound = true;
                        }
                        return new StringBuilder("");
                    }
                    else
                    {
                        var cls = component.assingBlock.SymbolTable.Get(component.Name);
                        if (cls is Class clss)
                        {
                            if (clss.GetParent()?.Name.Value != "Component")
                            {
                                component._componentNotHaveParent = true;
                                return new StringBuilder("");
                            }
                        }
                    }

                    if(fncreate is Function fa)
                        ret += tb0 + "new Pyr.createElement(" + component.Name + "." + fa.Name;
                    else
                        ret += tb0 + "new Pyr.createElement(" + component.Name;
                }
                if (pou != "")
                    ret += ", { " + pou + " }, ";
                else
                    ret += ", null, ";
                if (replace != "")
                    ret += "\"" + replace + "\"";
                else
                    ret += "null";
                ret += ")";
            }
            
            if (component.IsStart)
                return new StringBuilder("var _this = this;\r\n" + DoTabs(tabs + 1) + "return " + ret + ";");
            if(iret)
                return new StringBuilder("return " + ret + ";");
            return new StringBuilder(ret);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileLambda(Lambda lambda, int tabs = 0)
        {
            if (lambda.isNormalLambda)
            {
                string tbs = DoTabs(tabs);
                string ret = "";
                if (lambda.plist.assingToType == null)
                    lambda.plist.assingToType = lambda.predicate;
                if (lambda.plist.assingBlock == null)
                    lambda.plist.assingBlock = lambda.assingBlock;
                if (lambda.plist.assingToToken == null)
                    lambda.plist.assingToToken = lambda.assingToToken;
                ret += "function(" + Compile(lambda.plist) + ")";
                lambda.expresion.assingToType = lambda;                               

                if (lambda.expresion is Block block)
                {
                    foreach (var v in lambda.plist.Parameters)
                    {
                        if (v is Variable va)
                        {
                            va.setType(new Token(Token.Type.CLASS, "object"));
                            block.SymbolTable.Add(va.Value, va);
                        }
                    }
                    ret += "{";
                    ret += "\n" + Compile(lambda.expresion, tabs + 2);
                    ret +=  tbs + "}";
                }
                else
                {
                    if (lambda.replaceThis != null && lambda.expresion is UnaryOp uoe)
                    {
                        uoe.replaceThis = lambda.replaceThis;
                    }
                    var res = Compile(lambda.expresion);
                    ret += "{ return " + res + (res[res.Length - 1] == ';' ? "" : ";") + " }";
                }

                return new StringBuilder(ret);
            }
            else
            {
                foreach (var v in lambda.plist.Parameters)
                    if (v is Variable va)
                        va.setType(new Token(Token.Type.CLASS, "object"));

                if (lambda.isInArgumentList)
                    return new StringBuilder("lambda$" + lambda.name.Value);
                if (lambda.isCallInArgument)
                {
                    return new StringBuilder("function(" + Compile(lambda.plist) + "){ return " + Compile(lambda.expresion) + "; }");
                }
                if (lambda.name.Value.Contains("."))
                    return new StringBuilder(DoTabs(tabs) + "var " + string.Join(".", lambda.name.Value.Split('.').Take(lambda.name.Value.Split('.').Length - 1)) + ".lambda$" + lambda.name.Value.Split('.').Skip(lambda.name.Value.Split('.').Length - 1) + " = function(" + Compile(lambda.plist) + "){ return " + Compile(lambda.expresion) + "; };");
                return new StringBuilder(DoTabs(tabs) + "var lambda$" + lambda.name.Value + " = function(" + Compile(lambda.plist) + "){ return " + Compile(lambda.expresion) + "; };");
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileFor(For _for, int tabs = 0)
        {
            var ret = new StringBuilder();     
            if(_for.source is Variable)
            {
                ((Variable)_for.source).Check();

                Types to = _for.block.SymbolTable.Get(((Variable)_for.source).GetDateType().Value);
                if (to is Class && ((Class) to).haveParent("IIterable"))
                {
                    _for.isIterable = true;
                    _for.className = ((Class)to).Name.Value;
                }
                if (to is Interface && ((Interface) to).haveParent("IIterable"))
                {
                    _for.isIterable = true;
                    _for.className = ((Interface)to).Name.Value;
                }

            }
            if(_for.source is UnaryOp uop && ((UnaryOp)_for.source).Op == "new")
            {
                Types to = _for.block.SymbolTable.Get(uop.Name.Value);
                if (((Class)to).haveParent("IIterable"))
                {
                    _for.isIterable = true;
                }
                _for.className = ((Class)to).Name.Value;
            }
            if (_for.source is UnaryOp uoq && ((UnaryOp)_for.source).Op == "call")
            {
                Types t1 = _for.block.SymbolTable.Get(uoq.Name.Value);
                Types to = _for.block.SymbolTable.Get(((Function)t1).Returnt.Value);
                if (((Class)to).haveParent("IIterable"))
                {
                    _for.isIterable = true;
                }
                _for.className = ((Class)to).Name.Value;
            }

            if (_for.source is UnaryOp uor && ((UnaryOp)_for.source).Op == "..")
            {
                Types to = _for.block.SymbolTable.Get("Range");
                if (((Class)to).haveParent("IIterable"))
                {
                    _for.isIterable = true;
                }
                _for.className = ((Class)to).Name.Value;
            }

            if (_for.isIterable)
            {
                int tmpc = _for.block.Interpret.tmpcount++;
                _for.source.endit = false;
                string tab = DoTabs(tabs);
                var s = Compile(_for.source).Replace("\n", "");
                if (s.ToString().Substring(s.Length - 1, 1) == ";")
                    s = new StringBuilder(s.ToString().Substring(0, s.Length - 1));
                ret = new StringBuilder(tab + "var $tmp" + tmpc + " = " + s + ".iterator();\n");
                ret.Append(tab + "  while($tmp" + tmpc + ".hasNext()){\n");
                ret.Append(tab + "    var " + _for.variable.Value + " = $tmp" + tmpc + ".next();\n");
                ret.Append(Compile(_for.block, tabs + 2));
                ret.Append(tab + "  }");
            }

            return ret;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileBinOp(BinOp binOp, int tabs = 0)
        {
            if (binOp.right != null)
            {
                binOp.right.assingBlock = binOp.assingBlock;
                binOp.right.endit = false;
            }
            binOp.left.assingBlock = binOp.assingBlock;
            binOp.left.endit = false;

            if (binOp.right is Variable) ((Variable)binOp.right).Check();
            if (binOp.left is Variable) ((Variable)binOp.left).Check();                      

            Variable v = null;

            string o = Variable.GetOperatorStatic(binOp.op.type);
            if (o == "is")
            {
                binOp.right = binOp.block.SymbolTable.Get(binOp.rtok.Value);
                string vname = binOp.left.TryVariable().Value;

                string classname = binOp.right.TryVariable().Value;

                string rt = "";
                if (binOp.right is Generic)
                    rt = "(" + vname + ".constructor.name == this.generic$" + classname + " ? true : false)";
                else
                    rt = "("+vname + ".constructor.name == '" + classname + "' ? true : false)";

                binOp.outputType = new Token(Token.Type.BOOL, "bool");
                return new StringBuilder((binOp.inParen ? "(" : "") + rt + (binOp.inParen ? ")" : ""));
            }
            if (binOp.left is Number)
            {
                v = new Variable(((Number)binOp.left).getToken(), binOp.block, new Token(Token.Type.CLASS, "int"));
                var saveOut = binOp.outputType;
                binOp.outputType = v.OutputType(binOp.op.type, binOp.left, binOp.right);
                if (binOp.outputType.type == Token.Type.CLASS && binOp.outputType.Value == "int")
                    binOp.outputType = saveOut;
            }
            else if (binOp.left is CString)
            {
                v = new Variable(((CString)binOp.left).getToken(), binOp.block, new Token(Token.Type.CLASS, "string"));
                binOp.outputType = v.OutputType(binOp.op.type, binOp.left, binOp.right);
                if (binOp.right is UnaryOp ruop)
                    ruop.isInString = true;
            }
            else if(binOp.left is Variable)
            {
                v = ((Variable)binOp.left);
                if (v.GetDateType().Value == "auto")
                    v.Check();
                binOp.outputType = ((Variable)binOp.left).OutputType(binOp.op.type, binOp.left, binOp.right);
            }
            else if(binOp.left is UnaryOp leuo)
            {
                if(leuo.Op == "call")
                {
                    if (leuo.usingFunction == null)
                        Compile(leuo);
                    if (leuo.usingFunction != null)
                    {
                        Function f = leuo.usingFunction;
                        binOp.outputType = f.Returnt;
                    }
                }
                if (leuo.Op == "..")
                {
                    if (binOp.assingBlock?.SymbolTable.Get("Range") != null)
                    {
                        binOp.outputType = ((Class)binOp.assingBlock?.SymbolTable.Get("Range")).Name;
                    }                    
                }
                if(binOp.op.Value == "dot" && binOp.right is UnaryOp riuo)
                {
                    riuo.Block = binOp.assingBlock?.SymbolTable.Get(binOp.outputType.Value).assingBlock;
                }
                else if(binOp.op.Value == "dot" && binOp.right is Variable riva)
                {
                    var fnd = binOp.assingBlock?.SymbolTable.Get(leuo.OutputType.Value);
                    if(fnd != null && !(fnd is Error))
                    {
                        Types t = ((Class)fnd).Block.SymbolTable.Get(riva.Value);
                        if(t is Assign ta)
                        {
                            if (ta.Left is Variable tav)
                                riva.setType(tav.GetDateType());
                        }
                        else if(t is Variable tv)
                        {
                            riva.setType(tv.GetDateType());
                        }
                        binOp.outputType = riva.GetDateType();
                    }
                }
                v = binOp.left.TryVariable();
            }
            else if(binOp.left is BinOp)
            {
                Compile(binOp.left);
                v = binOp.left.TryVariable();
            }
            else
            {
                Compile(binOp.left);
                v = binOp.left.TryVariable();
            }
            if ((v._class != null && v.class_ == null) || (v.class_ != null && v.class_.JSName != ""))
            {
                if (binOp.op.Value == "dot")
                    return new StringBuilder((binOp.inParen ? "(" : "") + Compile(binOp.left) + Variable.GetOperatorStatic(binOp.op.type) + Compile(binOp.right) + (binOp.inParen ? ")" : ""));
                return new StringBuilder((binOp.inParen ? "(" : "") + Compile(binOp.left) + " " + Variable.GetOperatorStatic(binOp.op.type) + " " + Compile(binOp.right) + (binOp.inParen ? ")" : ""));
            }
            if(v.class_ != null)
            {                
                if(binOp.op.Value == "dot")
                {
                    return new StringBuilder((binOp.inParen ? "(" : "") + Compile(binOp.left) + "." + Compile(binOp.right) + (binOp.inParen ? ")" : ""));
                }
                Types oppq = v.class_.block.SymbolTable.Get("operator " + Variable.GetOperatorNameStatic(binOp.op.type));
                if (oppq is Error)                                    
                    return new StringBuilder("");                
                Function opp = (Function)oppq;
                if (binOp.op.type == Token.Type.NOTEQUAL)
                    return new StringBuilder((binOp.inParen ? "(" : "") + "!(" + Compile(binOp.left) + "." + opp.Name + "(" + Compile(binOp.right) + "))" + (binOp.inParen ? ")" : ""));
                if (binOp.op.type == Token.Type.MORE || binOp.op.type == Token.Type.LESS)
                {
                    if (binOp.op.type == Token.Type.MORE)
                        return new StringBuilder((binOp.inParen ? "(" : "") + Compile(binOp.left) + "." + opp.Name + "(" + Compile(binOp.right) + ") > 0" + (binOp.inParen ? ")" : ""));
                    return new StringBuilder((binOp.inParen ? "(" : "") + Compile(binOp.left) + "." + opp.Name + "(" + Compile(binOp.right) + ") < 0" + (binOp.inParen ? ")" : ""));
                }
                return new StringBuilder((binOp.inParen ? "(" : "") + Compile(binOp.left) + "." + opp.Name + "(" + Compile(binOp.right) + ")" + (binOp.inParen ? ")" : ""));
            }
            return new StringBuilder("");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileIf(If _if, int tabs = 0)
        {
            tabs++;
            string tbs = DoTabs(tabs);
            string ret = "";
            bool first = true;
            
            tabs++;
            foreach (var c in _if.conditions)
            {
                c.Value.BlockParent = _if.assingBlock;
                if(c.Key != null)
                    c.Key.endit = false;
                if (first)
                {                    
                    first = false;
                    ret += "if(" + Compile(c.Key) + ") {\n" + Compile(c.Value, tabs) + DoTabs(tabs-2) + "  }\n";
                }else if(c.Key is NoOp)
                {
                    ret += tbs + "else {\n" + Compile(c.Value, tabs) + DoTabs(tabs-2) + "  }\n";
                }
                else
                {
                    ret += tbs + "else if(" + Compile(c.Key) + ") {\n" + Compile(c.Value, tabs) + DoTabs(tabs-2) + "  }\n";
                }
            }
            return new StringBuilder(ret.Substring(0,ret.Length - 1));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileNumber(Number number)
        {
            if(number.isReal)
                return new StringBuilder(number.fvalue.ToString(CultureInfo.InvariantCulture));
            return new StringBuilder(number.value.ToString());
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileDelegate(Delegate _delegate, int tabs = 0)
        {
            int q = 0;
            string gener = "";
            foreach (string generic in _delegate.genericArguments)
            {
                if (q != 0) gener += ", ";
                gener += generic;
                q++;
            }

            foreach (string generic in _delegate.genericArguments)
            {
                _delegate.block.SymbolTable.Add(generic, new Generic(_delegate, _delegate.block, generic) { assingBlock = _delegate.block });
            }
        
            return new StringBuilder(DoTabs(tabs) + "//Delegate " + _delegate.RealName + (gener != "" ? "<" + gener + ">" : "") + "(" + _delegate.paraml.List() + ") -> " + _delegate.Returnt.Value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileImport(Import import, int tabs = 0)
        {
            if (!import.found || import._ihaveit != null) return new StringBuilder("");

            //Console.WriteLine(">" +import.Value + ": " + DateTime.Now.ToShortTimeString());
            var compiled = Compile(import.block, addBl: false);

            /*!!! CACHING !!!*/
            var hash = import._code.GetHashCode();
            var path = import.import.Value.Replace('.', '\\');            
            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            var _compiled = true;
            Interpreter.CurrentStaticInterpreter = import.interpret;
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

            //TODO:
            if (_compiled && 1 == 2)
            {
                var json = JsonParam.ToJson(import.block);
                var fname = dir + "\\" + Program.projectFolder + @"\" + path + ".p";
                var rdir = new FileInfo(fname).Directory.FullName;
                JObject fl = new JObject();
                fl["hash"] = hash;
                fl["content"] = json;
                File.WriteAllText(rdir + "\\" + import.GetModule() + ".p.h", fl.ToString());
                var regex = new Regex(@"\/\/(.*)}\(typeof(.*)\,[ ]this\);", RegexOptions.Multiline | RegexOptions.Singleline);
                var newcompile = regex.Replace(compiled.ToString(), "");
                File.WriteAllText(rdir + "\\" + import.GetModule() + ".p.c", newcompile.Trim());
            }

            string atttab = "";
            for(var i = 0; i <  4 - (import.import.Value.Length / 5); i++) { atttab+="\t"; }

            var now = DateTime.Now;            
            Console.Write((now.Hour < 10 ? "0": "") + now.ToShortTimeString() + ": " + import.import.Value + atttab + " -> [");
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
            if (import.interpret.stopwatch.Elapsed.Seconds > 0)
                Console.ForegroundColor = ConsoleColor.DarkRed;            
            else if (import.interpret.stopwatch.Elapsed.Milliseconds > 400)
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            else
                Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(import.interpret.stopwatch.Elapsed.Seconds + "." + import.interpret.stopwatch.Elapsed.Milliseconds.ToString("D3") + " sec");
            Console.ResetColor();
            //compiled = compiled.Replace("\n", "\n  ");

            string n = import.GetName();
            string tbs = DoTabs(tabs);

            var outcom = new StringBuilder();
            if (n == "")
            {
                outcom.Append("\n" + tbs + "//Imported " + import.GetModule() + "\n");
                outcom.Append(compiled.ToString().Substring(0, compiled.Length));
            }
            else
            {
                outcom.Append("\n" + tbs + "//Imported " + import.import.Value + "\n");
                //if (n.Count(c => c == '.') > 0)
                //    outcom.Append(tbs + n + " = function (_, __){\n  'use strict';\n");
                //else
                //    outcom.Append(tbs + "var " + n + " = function (_, __){\n  'use strict';\n");
                outcom.Append(compiled.ToString().Substring(0, compiled.Length));
                /*
                List<string> exposed = new List<string>();
                foreach (KeyValuePair<string, Types> t in import.block.SymbolTable.Table)
                {
                    if (t.Value == null || t.Key.Trim() == "") continue;
                    if (import.block.SymbolTable.TableIsCopy.ContainsKey(t.Key) && import.block.SymbolTable.TableIsCopy[t.Key])
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
                        outcom.Append("  _." + tf.Name + " = " + tf.Name + ";\n");
                        if (Interpreter._DEBUG)
                            outcom.Append("  _." + tf.Name + "$META = " + tf.Name + "$META;\n");
                    }
                    else if (t.Value is Class tc)
                    {
                        if (!tc.isForImport)
                        {
                            outcom.Append("  _." + tc.getName() + " = " + tc.getName() + ";\n");
                            if (Interpreter._DEBUG)
                                outcom.Append("  _." + tc.getName() + "$META = " + tc.getName() + "$META;\n");
                        }
                        if (tc.isForImport)
                        {
                            outcom.Append(Program.DrawClassInside(tc, tc.getName(), exposed, import.import.Value));
                        }
                    }
                    else if (t.Value is Interface ti)
                    {
                        outcom.Append("  _." + ti.getName() + " = " + ti.getName() + ";\n");
                        if (Interpreter._DEBUG)
                            outcom.Append("  _." + ti.getName() + "$META = " + ti.getName() + "$META;\n");
                    }
                    else if (t.Value is Import im)
                    {
                        var split = im.GetName().Split('.');
                        var namem = string.Join(".", split.Take(split.Length - 1));
                        if (namem == "") namem = split[0];
                        if(namem == import.import.Value)
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
                                    outcom.Append("  _." + skl + " = {};\n");
                                }
                            }
                        }
                        if (im.GetName() == "")
                        {
                            outcom.Append("  _." + t.Key + " = " + im.GetModule() + ";\n");
                            if (im.As != "")
                                outcom.Append("  var " + t.Key + " = " + im.GetModule() + ";\n");
                        }
                        else
                        {
                            var name = "";
                            if (im.GetName().Replace('.', '_') != im.GetModule())
                            {
                                name = "module_" + im.GetName().Replace('.', '_') + "_" + im.GetModule();
                                outcom.Append("  var " + name + " = GetModule(\"" + im.GetName() + "." + im.GetModule() + "\");\n");
                            }
                            else
                            {
                                name = "module_" + im.GetName().Replace('.', '_');
                                outcom.Append("  var " + name + " = GetModule(\"" + im.GetName() + "\");\n");
                            }

                            //outcom += "  _." + t.Key + " = " + name + ";\n";
                            if (im.As != "")
                                outcom.Append("  var " + t.Key + " = " + name + ";\n");
                        }
                        if(!exposed.Contains(t.Key))
                            exposed.Add(t.Key);
                        foreach (KeyValuePair<string, Types> qq in im.Block.SymbolTable.Table)
                        {
                            if (qq.Value is Class && !exposed.Contains(qq.Key) && !((Class)qq.Value).isExternal)
                            {
                                if (im.GetName() != "")
                                {
                                    outcom.Append("  var " + qq.Key + " = " + im.GetName() + "." + ((Class)qq.Value).getName() + ";\n");
                                    exposed.Add(qq.Key);
                                }
                            }
                        }
                    }
                    else
                        outcom.Append(tbs + "  _." + t.Key + " = " + t.Key + ";\n");
                }

                if(import.import.Value.Contains("."))
                    outcom.Append("\n"+tbs+"  DefineModule('"+import.GetName()+"."+import.GetModule()+"', _);\n");
                else
                    outcom.Append("\n"+tbs+"  DefineModule('"+import.GetModule()+"', _);\n");

                outcom.Append(tbs + "\n  return _;\n");
                outcom.Append(tbs + "}(typeof " + n + " === 'undefined' ? {} : " + n + ", this);\n");
                */
            }
            return outcom;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileUnaryOp(UnaryOp unaryOp, int tabs = 0)
        {
            if (unaryOp.expr != null)
                unaryOp.expr.assingBlock = unaryOp.assingBlock;
            string tbs = DoTabs(tabs);
            string o = Variable.GetOperatorStatic(unaryOp.op.type);
            if (o == "call")
            {
                unaryOp.FindUsingFunction();

                var t = unaryOp._myt;

                if (t == null)
                    return new StringBuilder("");

                string before = "";
                if (unaryOp.name.Value.Split('.')[0] == "this")
                {
                    if (unaryOp.block != null && unaryOp.block.isInConstructor)
                        before = "$this" + (unaryOp.name.Value == "this" ? "" : ".");
                    else if (unaryOp.replaceThis != null)
                        before = unaryOp.replaceThis + (unaryOp.name.Value == "this" ? "" : ".");
                    else
                        before = "this" + (unaryOp.name.Value == "this" ? "" : ".");
                }
                else if (unaryOp._myBefore != "")
                {
                    before = unaryOp._myBefore;
                }

                if (t is Assign && ((Assign)t).Right is Lambda)
                {
                    string args = "()";
                    if (unaryOp.plist != null)
                        args = "(" + Compile(unaryOp.plist) + ")";
                    if (unaryOp.asArgument)
                        return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + "lambda$" + unaryOp.name.Value + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
                    return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + "lambda$" + unaryOp.name.Value + args + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
                }

                if (t is Assign ta && ta.Right is UnaryOp tau)
                {
                    if (tau.Op == "new" && tau.isArray)
                    {
                        var func = unaryOp.name.Value.Split('.')[1];
                        var functio = (Function)unaryOp.assingBlock.SymbolTable.Get("Array." + func);
                        return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + before + unaryOp.newname + "." + functio.Name + "(" + CompileParameterList(unaryOp.plist, unaryOp.usingFunction?.ParameterList) + (unaryOp.plist.Parameters.Count > 0 && unaryOp.generic != "" ? ", " : "") + unaryOp.generic + ")" + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
                    }
                }
                if (t is Assign ta2 && ta2.Right is Null && unaryOp.assingBlock != null)
                {
                    var cls = unaryOp.assingBlock.SymbolTable.Get(ta2.Left.TryVariable()?.Value);
                    if (cls is Assign asig && asig.Left is Variable asv)
                    {
                        t = unaryOp.assingBlock.SymbolTable.Get(asv.Type + "." + string.Join(".", unaryOp.name.Value.Split('.').Skip(1)));
                    }
                }
                if (t is Assign && unaryOp.block?.SymbolTable.Get(((Variable)((Assign)t).Left).Type) is Delegate)
                {
                    string args = "()";
                    if (unaryOp.plist != null)
                        args = "(" + Compile(unaryOp.plist) + ")";
                    if (unaryOp.asArgument)
                        return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + "delegate$" + unaryOp.name.Value + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
                    if (unaryOp.isInString)
                        return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + "function(){ delegate$" + unaryOp.name.Value + args + "; }" + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
                    return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + before + "delegate$" + unaryOp.newname + args + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
                }
                if (unaryOp.name.Value == "js")
                {
                    string pl = Compile(unaryOp.plist).ToString();
                    return new StringBuilder(pl.Substring(1, pl.Length - 2).Replace("\\", ""));
                }
                if (unaryOp.name.Value.Contains("."))
                {
                    string[] nnaml = unaryOp.name.Value.Split('.');
                    string nname = "";
                    var vario = unaryOp.block?.SymbolTable.Get(string.Join(".", nnaml.Take(nnaml.Length - 1)));
                    if (!(vario is Error) && vario is Properties prop)
                    {
                        var type = unaryOp.block?.SymbolTable.Get(((Variable)prop.variable).Type + "." + nnaml[nnaml.Length - 1]);
                        nname = string.Join(".", nnaml.Take(nnaml.Length - 2)) + ".Property$" + nnaml[nnaml.Length - 2] + ".get()." + ((Function)type)?.Name;
                        unaryOp.founded = true;
                    }
                    else if (unaryOp.isDynamic || (t is Class tc && tc.Name.Value == "object"))
                        nname = unaryOp.name.Value;
                    else
                    {
                        nname = string.Join(".", nnaml.Take(nnaml.Length - 1)) + "." + ((Function)t)?.Name;
                    }
                    if (nname.Split('.')[0] == "this")
                        nname = string.Join(".", nname.Split('.').Skip(1));

                    if (unaryOp.usingFunction != null && unaryOp.usingFunction.isInline)
                    {
                        int tmpc = unaryOp.usingFunction.inlineId > 0 ? unaryOp.usingFunction.inlineId : (unaryOp.usingFunction.inlineId = Function.inlineIdCounter++);

                        if (unaryOp.assignToParent is Assign)
                        {
                            unaryOp.usingFunction.assigmentInlineVariable = unaryOp.assignToParent;
                        }
                        var ret = "\n";
                        var i = 0;

                        Dictionary<Assign, Types> defaultVal = new Dictionary<Assign, Types>();

                        foreach (var par in unaryOp.usingFunction.ParameterList.Parameters)
                        {
                            if (i >= unaryOp.plist.Parameters.Count)
                            {
                                if (par is Assign)
                                {

                                }
                            }
                            else
                            {
                                if (par is Assign para)
                                {
                                    defaultVal.Add(para, para.Right);
                                    para.Right = unaryOp.plist.Parameters[i];
                                }
                            }
                            i++;
                        }

                        ret += Compile(unaryOp.usingFunction.Block);

                        foreach (var typese in defaultVal)
                        {
                            typese.Key.Right = typese.Value;
                        }

                        unaryOp.usingFunction.inlineId = 0;
                        unaryOp.usingFunction.assigmentInlineVariable = null;
                        return new StringBuilder(ret);
                    }

                    if (unaryOp.plist == null)
                        return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + before + nname + "(" + unaryOp.generic + ")" + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
                    if (unaryOp.plist.assingBlock == null)
                        unaryOp.plist.assingBlock = unaryOp.block;
                    return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + before + nname + "(" + CompileParameterList(unaryOp.plist, unaryOp.usingFunction?.ParameterList) + (unaryOp.plist.Parameters.Count > 0 && unaryOp.generic != "" ? ", " : "") + unaryOp.generic + ")" + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
                }
                else if (unaryOp.usingFunction != null && unaryOp.usingFunction.isInline)
                {
                    int tmpc = unaryOp.usingFunction.inlineId > 0 ? unaryOp.usingFunction.inlineId : (unaryOp.usingFunction.inlineId = Function.inlineIdCounter++);

                    if (unaryOp.assignToParent is Assign)
                    {
                        unaryOp.usingFunction.assigmentInlineVariable = unaryOp.assignToParent;
                    }
                    var ret = "\n";
                    var i = 0;
                    foreach (var par in unaryOp.usingFunction.ParameterList.Parameters)
                    {
                        ret += tbs + "var " + unaryOp.newname + "$" + tmpc + "$" + par.TryVariable().Value + " = " + Compile(unaryOp.plist.Parameters[i]) + ";\n";
                        i++;
                    }

                    ret += Compile(unaryOp.usingFunction.Block);

                    unaryOp.usingFunction.inlineId = 0;
                    unaryOp.usingFunction.assigmentInlineVariable = null;
                    return new StringBuilder(ret);
                }
                else
                {
                    if (unaryOp.asArgument)
                        return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + before + unaryOp.newname + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
                    if (unaryOp.plist == null)
                        return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + before + unaryOp.newname + "(" + unaryOp.generic + ")" + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
                    return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + before + unaryOp.newname + "(" + CompileParameterList(unaryOp.plist, unaryOp.usingFunction?.ParameterList) + (unaryOp.plist.Parameters.Count > 0 && unaryOp.generic != "" ? ", " : "") + unaryOp.generic + ")" + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
                }
            }
            if (o == "new")
            {
                Types t = unaryOp.assingBlock.SymbolTable.Get(unaryOp.name.Value);
                if (t is Error) return new StringBuilder("");
                if (t is Import)
                {
                    t = ((Import)t).Block.SymbolTable.Get(unaryOp.name.Value);
                    if (t is Error) return new StringBuilder("");
                }
                string rt;

                string _name = "";
                if (t is Class)
                {
                    if (((Class)t).JSName != "") _name = ((Class)t).JSName;
                    else _name = ((Class)t).Name.Value;
                }

                if (t is Class && !(((Class)t).assingBlock.SymbolTable.Get("constructor " + _name) is Error))
                {
                    string generic = "";
                    bool fir = true;
                    string gname = "";
                    foreach (string g in unaryOp.genericArgments)
                    {
                        if (!fir) generic += ", ";
                        fir = false;
                        Types gf = unaryOp.block.SymbolTable.Get(g);
                        if (gf is Class) gname = "'" + ((Class)gf).getName() + "'";
                        if (gf is Interface) gname = "'" + ((Interface)gf).getName() + "'";
                        if (gf is Generic) gname = "generic$" + g;
                        generic += gname;
                    }
                    Function f = (Function)(((Class)t).assingBlock.SymbolTable.Get("constructor " + _name));
                    if (unaryOp.isArray)
                    {
                        Variable va = null;
                        if (unaryOp.arraySizeVariable != null && !(unaryOp.block.SymbolTable.Get(unaryOp.arraySizeVariable.Value) is Error))
                        {
                            va = (Variable)unaryOp.block.SymbolTable.Get(unaryOp.arraySizeVariable.Value);
                        }

                        string arrayS = "";
                        if (unaryOp.arraySizeVariable != null && va != null && unaryOp.arraySizeVariableTypes == null)
                            arrayS = va.Value;
                        else if (unaryOp.arraySizeVariableTypes != null)
                            arrayS = Compile(unaryOp.arraySizeVariableTypes).ToString();
                        else
                            arrayS = unaryOp.arraySize.ToString();

                        rt = tbs + "new Array(" + arrayS + ").fill(" + _name + "." + f.Name + "(" + Compile(unaryOp.plist);
                        if (unaryOp.plist != null && unaryOp.plist.Parameters.Count > 0)
                            rt += ", ";
                        rt += generic;
                        rt += "))";
                    }
                    else
                    {
                        if (t.assingBlock.Interpret.FindImport(string.Join(".", unaryOp.name.Value.Split('.').Take(unaryOp.name.Value.Split('.').Length - 1))))
                        {
                            Import im = t.assingBlock.Interpret.GetImport(string.Join(".", unaryOp.name.Value.Split('.').Take(unaryOp.name.Value.Split('.').Length - 1))) as Import;
                            if (im.As != null)
                                rt = tbs + im.As + "." + _name + "." + f.Name + "(" + CompileParameterList(unaryOp.plist, f.ParameterList, unaryOp.plist);
                            else if (string.Join(".", unaryOp.name.Value.Split('.').Take(unaryOp.name.Value.Split('.').Length - 1)) != unaryOp.name.Value)
                                rt = tbs + string.Join(".", unaryOp.name.Value.Split('.').Take(unaryOp.name.Value.Split('.').Length - 1)) + "." + _name + "." + f.Name + "(" + CompileParameterList(unaryOp.plist, f.ParameterList, unaryOp.plist);
                            else
                                rt = tbs + _name + "." + f.Name + "(" + CompileParameterList(unaryOp.plist, f.ParameterList, unaryOp.plist);
                        }
                        else
                            rt = tbs + _name + "." + f.Name + "(" + CompileParameterList(unaryOp.plist, f.ParameterList, unaryOp.plist);
                        if (unaryOp.plist != null && (unaryOp.plist.Parameters.Count > 0 || f.ParameterList.Parameters.Count > 0) && generic != "")
                            rt += ", ";
                        rt += generic;
                        rt += ")";
                    }
                }
                else
                {
                    if (t is Generic)
                    {
                        TypeObject obj = new TypeObject();
                        _name = obj.ClassNameForLanguage();
                        if (unaryOp.block.isInConstructor)
                            _name = "window[$this.generic$" + ((Generic)t).Name + "]";
                        else
                            _name = "window[this.generic$" + ((Generic)t).Name + "]";
                    }
                    if (unaryOp.isArray)
                    {
                        string generic = "", gname = "";
                        bool fir = true;
                        foreach (string g in unaryOp.genericArgments)
                        {
                            if (!fir) generic += ", ";
                            fir = false;
                            Types gf = unaryOp.block.SymbolTable.Get(g);
                            if (gf is Class) gname = "'" + ((Class)gf).getName() + "'";
                            if (gf is Interface) gname = "'" + ((Interface)gf).getName() + "'";
                            if (gf is Generic) gname = (unaryOp.block.isInConstructor ? "$this." : "this.") + "generic$" + g;
                            generic += gname;
                        }
                        Variable va = null;
                        if (unaryOp.arraySizeVariable != null && !(unaryOp.block.SymbolTable.Get(unaryOp.arraySizeVariable.Value) is Error))
                        {
                            va = ((Variable)((Assign)unaryOp.block.SymbolTable.Get(unaryOp.arraySizeVariable.Value)).Left);
                        }

                        string arrayS = "";
                        if (unaryOp.arraySizeVariable != null && va != null && unaryOp.arraySizeVariableTypes == null)
                            arrayS = va.Value;
                        else if (unaryOp.arraySizeVariableTypes != null)
                            arrayS = Compile(unaryOp.arraySizeVariableTypes).ToString();
                        else
                            arrayS = unaryOp.arraySize.ToString();

                        var tv = t.TryVariable();
                        if (tv.IsPrimitive)
                        {
                            var type = tv.GetDateType().Value;
                            var primitiveFill = "";
                            if (type == "string") { primitiveFill = "\"\""; }
                            if (type == "int") { primitiveFill = "0"; }
                            if (type == "float") { primitiveFill = "0.0"; }
                            if (type == "bool") { primitiveFill = "false"; }

                            rt = tbs + "new Array(" + arrayS + ").fill(" + primitiveFill + ")";
                        }
                        else
                        {
                            rt = tbs + "new Array(" + arrayS + ").fill(new " + _name + "(" + Compile(unaryOp.plist);
                            if (unaryOp.plist != null && unaryOp.plist.Parameters.Count > 0) rt += ", ";
                            rt += generic;
                            rt += "))";
                        }
                    }
                    else
                    {
                        if (t.assingBlock.Interpret.FindImport(unaryOp.name.Value.Split('.').First()))
                        {
                            rt = tbs + "new " + unaryOp.name.Value.Split('.').First() + "." + _name + "(" + Compile(unaryOp.plist) + ")";
                        }
                        else
                            rt = tbs + "new " + _name + "(" + Compile(unaryOp.plist) + ")";

                    }
                }

                return new StringBuilder((unaryOp.inParen ? "(" : "") + rt + (unaryOp.inParen ? ")" : ""));
            }
            if (o == "return")
            {
                if (unaryOp.expr != null)
                    unaryOp.expr.endit = false;

                var usingBlock = unaryOp.assingBlock ?? unaryOp.block;
                if (usingBlock != null && (usingBlock = usingBlock.GetBlock(Block.BlockType.FUNCTION, new List<Block.BlockType> { Block.BlockType.CLASS, Block.BlockType.INTERFACE })) != null)
                {
                    if (usingBlock.SymbolTable.Get(usingBlock.assignTo) is Function ass && ass.isInline && ass.inlineId > 0)
                    {
                        if (ass.assigmentInlineVariable == null)
                        {
                            if (unaryOp.expr != null)
                                return new StringBuilder(tbs + Compile(unaryOp.expr));
                        }
                        else if (ass.assigmentInlineVariable is Assign)
                        {
                            if (unaryOp.expr != null)
                                return new StringBuilder(tbs + ass.Name + "$" + ass.inlineId + "$return = " + Compile(unaryOp.expr) + ";");
                        }
                    }
                }

                if (unaryOp.expr == null)
                    return new StringBuilder(tbs + "return;");

                var compiled = Compile(unaryOp.expr);
                if (compiled.ToString().Contains("\n"))
                    return compiled;
                return new StringBuilder(tbs + "return " + Compile(unaryOp.expr) + ";");
            }

            if (o == "..")
            {
                var fun = unaryOp.block.SymbolTable.Get("Range.range") as Function;
                return new StringBuilder(fun.assignTo + "." + fun.Name + "(" + Compile(unaryOp.exptList[0]) + ", " + Compile(unaryOp.exptList[1]) + ")");
            }

            if (unaryOp.post)
                return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + Compile(unaryOp.expr) + Variable.GetOperatorStatic(unaryOp.op.type) + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
            return new StringBuilder(tbs + (unaryOp.inParen ? "(" : "") + Variable.GetOperatorStatic(unaryOp.op.type) + Compile(unaryOp.expr) + (unaryOp.inParen ? ")" : "") + (unaryOp.endit ? ";" : ""));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileString(CString cString)
        {
            string o = "", consume = "";
            int state = 0;
            for (int i = 0; i < cString.value.Length; i++)
            {
                if (state == 1 && cString.value[i] == '}')
                {
                    if (consume.Substring(consume.Length - 1, 1) == "?")
                        o += "\' + ( " + consume.Substring(0, consume.Length - 1) + " === null ? '' : " + consume.Substring(0, consume.Length - 1) + " ) + \'";
                    else
                        o += "\' + " + consume + " + \'";
                    consume = "";
                    state = 0;
                }
                else if (state == 1)
                    consume += cString.value[i];
                else if (i + 1 < cString.value.Length && cString.value[i] == '{' && cString.value[i + 1] == '$')
                {
                    state = 1;
                    i++;
                }
                else
                {
                    o += cString.value[i];
                }
            }

            return new StringBuilder("'" + o + "'");
        }
       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileNamedTuple(NamedTuple namedTuple, int tabs = 0)
        {
            string tbs = DoTabs(tabs);
            if (namedTuple._isNamed)
                return new StringBuilder(tbs + "return {" + string.Join(", ", namedTuple._list.Select(x => x.Key.Value + ": " + Compile(x.Value))) + "};");
            return new StringBuilder(tbs + "return new Array(" + string.Join(", ", namedTuple._listNoName.Select(x => Compile(x))) + ");");
        }
       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileVariable(Variable variable, int tabs = 0, Types ti = null)
        {
            if (variable.key != null)
                variable.key.endit = false;

            string vname = "";
            string overidename = "";

            string nameclass = "";
            if (variable.class_ != null)
                nameclass = (variable.class_.JSName != "" ? variable.class_.JSName : variable.class_.Name.Value);
            else if (variable.inter_ != null)
                nameclass = variable.inter_.Name.Value;
            else if (variable._class != null)
                nameclass = variable._class.Name;

            var fasign = variable.block.SymbolTable.Get(variable.value);
            if (fasign is Assign fass)
            {
                if (fass.Left is Variable fasv && fasv.GetHashCode() != GetHashCode() && !(fass.Right is Null))
                {
                    if (fasv.IsVal && fasv.IsPrimitive)
                    {
                        fass.Right.endit = variable.endit;
                        return Compile(fass.Right, tabs);
                    }
                }
            }

            var split = variable.Value.Split(new[] { '.' }, 1);
            var usingBlock = variable.assingBlock ?? variable.block;
            var _usingBlock = usingBlock?.GetBlock(Block.BlockType.FUNCTION, new List<Block.BlockType> { Block.BlockType.CLASS, Block.BlockType.INTERFACE });
            if (_usingBlock != null)
            {
                usingBlock = _usingBlock;
                if (usingBlock.SymbolTable.Get(usingBlock.assignTo) is Function ass && ass.isInline && ass.inlineId > 0)
                {
                    if (split[0] == "this")
                    {
                        overidename = split[0] + "." + ass.Name + "$" + ass.inlineId + "$" + variable.value;
                    }
                    else
                    {
                        overidename = ass.Name + "$" + ass.inlineId + "$" + variable.value;
                    }
                }
            }
            var t__ = (variable.assingBlock != null && variable.assingBlock.isInConstructor ? "$this" : "this") + (variable.Value == "this" ? "" : ".");
            if (variable.assingBlock != null && variable.assingBlock.isType(Block.BlockType.PROPERTIES))
                t__ = "this.$self" + (variable.Value == "this" ? "" : ".");
            if (variable.value.Split('.')[0] != "this")
                t__ = "";
            if (variable.value.Contains("."))
                t__ = variable.value.Split('.')[0] + ".";

            var not = variable.Value;
            var withouthis = variable.Value;
            if (variable.value.Contains("."))
                withouthis = string.Join(".", variable.value.Split('.').Skip(1));

            if (variable.Value.Split('.')[0] == "this")
            {
                var ths = variable.Value == "this" ? "this" : "this.";
                not = (t__ == "this." && usingBlock.isInConstructor ? "$" + ths : ths) + string.Join(".", variable.value.Split('.').Skip(1));
                vname = string.Join(".", variable.value.Split('.').Skip(1)) + (variable.isKey ? "[" + Compile(variable.key) + "]" : "");

                if (variable.block.isInConstructor)
                {
                    if (variable.block.SymbolTable.Get(variable.value) is Properties)
                    {
                        if (variable.asDateType != null)
                            return new StringBuilder(DoTabs(tabs) + (variable.inParen ? "(" : "") + "($this.Property$" + vname + ".get().constructor.name == '" + nameclass + "' ? $this.Property$" + vname + ".get() : alert('Variable " + vname + " is not type " + variable.asDateType.Value + "'))" + (variable.inParen ? ")" : ""));
                        return new StringBuilder(DoTabs(tabs) + (variable.inParen ? "(" : "") + "$this.Property$" + string.Join(".", variable.value.Split('.').Skip(1)) + ".get()" + (variable.isKey ? "[" + Compile(variable.key) + "]" : "") + (variable.inParen ? ")" : ""));
                    }
                    if (variable.asDateType != null)
                        return new StringBuilder(DoTabs(tabs) + (variable.inParen ? "(" : "") + "($this." + vname + ".constructor.name == '" + nameclass + "' ? $this." + vname + " : alert('Variable " + vname + " is not type " + variable.asDateType.Value + "'))" + (variable.inParen ? ")" : ""));
                    return new StringBuilder(DoTabs(tabs) + (variable.inParen ? "(" : "") + "$this." + string.Join(".", variable.value.Split('.').Skip(1)) + (variable.isKey ? "[" + Compile(variable.key) + "]" : "") + (variable.inParen ? ")" : ""));
                }
            }

            Types t = ti ?? variable.block.SymbolTable.Get(variable.value, variable.Type);
            if (t is Generic)
                vname = t__ + "generic$" + withouthis + (variable.isKey ? "[" + Compile(variable.key) + "]" : "");
            else if (t is Assign ta && ta.Left is Variable tav && tav.Type != "object")
            {
                //Datetype
                //((Assign)t).Left.assingBlock.SymbolTable.Get("T1")
                if (tav.Type == "auto")
                    tav.Check();
                Types tavt = variable.block.SymbolTable.Get(tav.Type, genericArgs: tav.genericArgs.Count);
                if (tavt is Delegate)
                {
                    if (t__ != "")
                        vname = (variable.assingBlock != null && variable.assingBlock.isInConstructor ? "$this" : "this") + ".delegate$" + withouthis + (variable.isKey ? "[" + Compile(variable.key) + "]" : "");
                    else
                        vname = "delegate$" + withouthis + (variable.isKey ? "[" + Compile(variable.key) + "]" : "");
                }
                else
                {
                    vname = (overidename != "" ? overidename : not) + (variable.isKey ? "[" + Compile(variable.key) + "]" : "");
                }
            }
            else if (t is Properties)
            {
                if (variable.value.Split('.')[0] == "this")
                    vname = t__ + ".Property$" + string.Join(".", variable.value.Split('.').Skip(1)) + ".get()" + (variable.isKey ? "[" + Compile(variable.key) + "]" : "");
                else
                    vname = t__ + ".Property$" + variable.Value + ".get()" + (variable.isKey ? "[" + Compile(variable.key) + "]" : "");
            }
            else
            {
                vname = not + (variable.isKey ? "[" + Compile(variable.key) + "]" : "");
            }
            if (variable.dateType.Value == "auto")
                variable.Check();
            if (variable.class_ != null && variable.isKey && !variable.class_.isExternal)
            {
                ParameterList plist = new ParameterList(false);
                plist.Parameters.Add(variable.key);
                Types oppq = variable.class_.block.SymbolTable.Get("operator " + Variable.GetOperatorNameStatic(Token.Type.GET), plist);
                if (!(oppq is Error))
                {
                    Function opp = (Function)oppq;
                    if (!opp.isExternal)
                        vname = not + "." + opp.Name + "(" + Compile(variable.key) + ")";
                }
                else if (oppq is Error && ((Error)oppq).Message == "Found but arguments are bad!") variable.getFoundButBadArgs = true;
            }

            if (variable.asDateType != null)
                return new StringBuilder(DoTabs(tabs) + (variable.inParen ? "(" : "") + "(" + vname + ".constructor.name == '" + nameclass + "' ? " + vname + " : alert('Variable " + vname + " is not type " + variable.asDateType.Value + "'))" + (variable.inParen ? ")" : ""));
            return new StringBuilder(DoTabs(tabs) + (variable.inParen ? "(" : "") + vname + (variable.inParen ? ")" : ""));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileParameterList(ParameterList parameterList, ParameterList plist = null, ParameterList myList = null)
        {
            var ret = new StringBuilder();
            Dictionary<string, bool> argDefined = new Dictionary<string, bool>();
            List<string> argNamed = plist?.ToList();
            int i = 0;
            bool startne = false;
            if (parameterList.allowMultipel && plist == null && parameterList.assingBlock != null && !parameterList.assingBlock.variables.ContainsKey(parameterList.allowMultipelName.Value))
            {
                new Assign(
                    new Variable(new Token(Token.Type.ID, parameterList.allowMultipelName.Value), parameterList.assingBlock) { isArray = true },
                    new Token(Token.Type.ASIGN, '='),
                    new Null(),
                    isVal: (parameterList.assingBlock != null && parameterList.assingBlock.assingToType is Function asif && asif.isInline));
            }
            foreach (Types par in parameterList.parameters)
            {
                if (argNamed != null && i >= argNamed.Count && !startne)
                {
                    ret.Append(ret.ToString() == "" ? "" : ", " + "[");
                    startne = true;
                }
                if (argNamed != null && argNamed.Count > i)
                    argDefined[argNamed[i]] = true;

                par.endit = false;
                if (parameterList.assingBlock != null && par is Variable && parameterList.assingBlock.SymbolTable.Get(((Variable)par).Value) is Error)
                {
                    par.assingBlock = parameterList.assingBlock;
                    var assign = new Assign(
                        ((Variable)par),
                        new Token(Token.Type.ASIGN, '='),
                        new Null(),
                        parameterList.assingBlock,
                        isVal: (parameterList.assingBlock != null && parameterList.assingBlock.assingToType is Function asif && asif.isInline));
                }
                else if (par is Assign && parameterList.assingBlock != null && !parameterList.assingBlock.variables.ContainsKey(((Assign)par).Left.TryVariable().Value))
                {
                    parameterList.assingBlock.variables.Add(((Assign)par).Left.TryVariable().Value, (Assign)par);
                    if (((Assign)par).Left is Variable parl)
                    {
                        parl.IsVal = (parameterList.assingBlock != null && parameterList.assingBlock.assingToType is Function asif && asif.isInline);
                    }
                }
                if (ret.ToString() != "" && ret[ret.Length - 1] != '[') ret.Append(", ");
                if (parameterList.declare)
                {
                    if (par is Variable parv)
                    {
                        parv.IsVal = (parameterList.assingBlock != null && parameterList.assingBlock.assingToType is Function asif && asif.isInline);
                    }
                    if (par is Assign)
                        ret.Append(Compile(((Assign)par).Left));
                    else if (par is Variable && ((Variable)par).Block?.SymbolTable.Get(((Variable)par).Type) is Delegate)
                    {
                        string rrr = Compile(par).ToString();
                        if (rrr.Split('$')[0] != "delegate")
                            ret.Append("delegate$" + rrr);
                        else
                            ret.Append(rrr);
                    }
                    else
                        ret.Append(Compile(par));
                }
                else
                {
                    if (plist != null && i < plist.parameters.Count && plist.parameters[i].TryVariable().Type == "Predicate" && par is Lambda lambda)
                    {
                        lambda.predicate = plist.parameters[i];
                        lambda.assingBlock = parameterList.assingBlock ?? plist.assingBlock;
                        lambda.assingToToken = parameterList.assingToToken;
                        ret.Append(Compile(lambda));
                    }
                    else if (parameterList.assingToType != null && parameterList.assingToType is Variable varia && varia.Type == parameterList.assingToType.TryVariable().Type && par is Variable variable && variable.Type == "auto")
                    {
                        var split = parameterList.assingToToken.Value.Split('.');
                        var foundvar = parameterList.assingBlock.SymbolTable.Get((split.Length > 1 ? split[split.Length - 2] : split[0]));
                        if (foundvar is Assign foundAssign)
                        {
                            var mydelegate = parameterList.assingBlock.SymbolTable.Get(varia.Type);
                            if (foundAssign.Left.TryVariable().genericArgs.Any() && mydelegate is Delegate jdelegate)
                            {
                                var leftvar = foundAssign.Left.TryVariable().genericArgs;
                                Dictionary<string, string> delegateAssign = new Dictionary<string, string>();
                                int x = 0;
                                foreach (var argument in jdelegate.GenericArguments)
                                {
                                    delegateAssign[argument] = leftvar[x++];
                                }

                                var funct = parameterList.assingBlock.SymbolTable.Get(parameterList.assingToToken.Value);
                                if (funct is Function f)
                                {
                                    var genericT = f.ParameterList.parameters[i].TryVariable().GenericList[i];
                                    if (delegateAssign.ContainsKey(genericT))
                                    {
                                        variable.setType(new Token(Token.Type.CLASS, delegateAssign[genericT]));
                                    }
                                }
                            }
                        }
                        var assignpredic = varia.Block.SymbolTable.Get(varia.Type);
                        ret.Append(CompileVariable(variable, 0, par));
                    }
                    else
                        ret.Append(Compile(par));
                }
                i++;
            }
            if (startne)
            {
                ret.Append("]");
            }
            if (myList != null)
            {
                foreach (Types par in plist.Parameters)
                {
                    if (par is Assign para)
                    {
                        if (!argDefined.ContainsKey(para.Left.TryVariable().Value))
                        {
                            ret.Append((ret.ToString() != "" ? ", " : "") + "undefined");
                        }
                    }
                    else if (par is Variable parv)
                    {
                        if (!argDefined.ContainsKey(parv.Value))
                        {
                            ret.Append((ret.ToString() != "" ? ", " : "") + "undefined");
                        }
                    }
                }
            }
            if (parameterList.defaultCustom.Count != 0 && plist != null)
            {
                i = 0;
                foreach (var p in plist.parameters)
                {
                    i++;
                    bool found = false;
                    if (i - 1 < parameterList.parameters.Count)
                        continue;
                    if (p is Assign pa)
                    {
                        foreach (var q in parameterList.defaultCustom)
                        {
                            if (pa.Left.TryVariable().Value == q.Key)
                            {
                                ret.Append(", " + Compile(q.Value));
                                found = true;
                            }
                        }
                    }
                    if (!found)
                    {
                        ret.Append((ret.ToString() != "" ? ", " : "") + "undefined");
                    }
                }
            }

            if (plist != null && plist.allowMultipel && parameterList.parameters.Count == argNamed.Count)
            {
                ret.Append((ret.ToString() != "" ? ", " : "") + "undefined");
            }
            if (parameterList.allowMultipel && myList == null)
            {
                ret.Append((ret.ToString() != "" ? ", " : "") + parameterList.allowMultipelName.Value);
            }
            parameterList.assingBlock = null;
            return ret;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileFunction(Function function, int tabs = 0)
        {
            function.functionOpname = "";
            var ret = new StringBuilder();

            string addCode = "";
            if (function.attributes?.Where(x => x.GetName(true) == "Debug").Count() > 0)
            {
                if (Interpreter._DEBUG)
                    Debugger.Break();
                addCode = "debugger;";
            }

            if (!function.isExternal)
            {
                string python_fun = "";
                Class c = null;
                Interface i_ = null;
                string fromClass = "";
                string tbs = DoTabs(tabs);
                int tmpc = 0;

                if (function.isExtending)
                {
                    var ex = function.assingBlock.SymbolTable.Get(function.extendingClass, false, true);
                    if (ex is Import import)
                    {
                        function.extendingClass = import.GetName() + "." + function.extendingClass;
                    }
                    fromClass = function.extendingClass;
                    if (function.isStatic || function.isConstructor)
                    {
                        ret.Append(tbs + function.extendingClass + "." + function.Name + " = function(" + Compile(function.paraml) + "){" + (function.block != null ? "\n" : ""));
                        function.functionOpname = function.extendingClass + "." + function.Name;
                    }
                    else
                    {
                        ret.Append(tbs + function.extendingClass + ".prototype." + function.Name + " = function(" + Compile(function.paraml) + "){" + (function.block != null ? "\n" : ""));
                        function.functionOpname = function.extendingClass + ".prototype." + function.Name;
                    }
                }
                else if (function.assignTo == "" || function.assignTo == function.Name)
                {
                    ret.Append(tbs + "function " + function.Name + "(" + Compile(function.paraml));

                    if (function.genericArguments.Count > 0)
                    {
                        int q = 0;
                        foreach (string generic in function.genericArguments)
                        {
                            if (q != 0) ret.Append(", ");
                            else if (function.paraml.Parameters.Count > 0) { ret.Append(", "); }
                            ret.Append("generic$" + generic);
                            q++;
                        }
                    }

                    ret.Append("){" + (function.block != null ? "\n" : ""));
                    function.functionOpname = function.Name;
                }
                else
                {
                    fromClass = function.assignTo;
                    string hash_name = "";
                    Types fg = function.assingBlock.assingToType;
                    if (fg is Class fgc)
                        hash_name = fgc.getName();
                    if (fg is Interface fgi)
                        hash_name = fgi.getName();

                    if (function.isStatic || function.isConstructor)
                    {
                        if (fg is Class)
                            c = (Class)fg;
                        if (fg is Interface)
                            i_ = (Interface)fg;

                        if (function.isConstructor && c.GenericArguments.Count > 0)
                        {
                            function.paraml.assingBlock = function.assingBlock;

                            ret.Append(tbs + hash_name + "." + function.Name + " = function(" + Compile(function.paraml));

                            function.functionOpname = hash_name + "." + function.Name;
                            bool f = !(function.paraml.Parameters.Count > 0);
                            foreach (string generic in c.GenericArguments)
                            {
                                if (!f) ret.Append(", ");
                                f = false;
                                ret.Append("generic$" + generic);
                            }
                            ret.Append("){" + (function.block != null ? "\n" : ""));
                        }
                        else
                        {
                            ret.Append(tbs + hash_name + "." + function.Name + " = function(" + Compile(function.paraml));

                            if (function.genericArguments.Count > 0)
                            {
                                int q = 0;
                                foreach (string generic in function.genericArguments)
                                {
                                    if (q != 0) ret.Append(", ");
                                    else if (function.paraml.Parameters.Count > 0) { ret.Append(", "); }
                                    ret.Append("generic$" + generic);
                                    q++;
                                }
                            }
                            ret.Append("){" + (function.block != null ? "\n" : ""));

                            function.functionOpname = hash_name + "." + function.Name;
                        }
                    }
                    else
                    {
                        ret.Append(tbs + hash_name + ".prototype." + function.Name + " = function(" + Compile(function.paraml));

                        if (function.genericArguments.Count > 0)
                        {
                            int q = 0;
                            foreach (string generic in function.genericArguments)
                            {
                                if (q != 0) ret.Append(", ");
                                else if (function.paraml.Parameters.Count > 0) { ret.Append(", "); }
                                ret.Append("generic$" + generic);
                                q++;
                            }
                        }
                        ret.Append("){" + (function.block != null ? "\n" : ""));
                        function.functionOpname = hash_name + ".prototype." + function.Name;
                    }
                }

                if (addCode != "")
                    ret.Append(tbs + "  " + addCode + "\n");

                //if (genericArguments.Count != 0) ret += "\n";
                foreach (var generic in function.genericArguments)
                {
                    function.block?.SymbolTable.Add(generic, new Generic(function, function.block, generic) { assingBlock = function.block });
                }

                if (function.isConstructor)
                {
                    if (function.block == null) ret.Append("\n");
                    ret.Append(tbs + "  var $this = Object.create(" + fromClass + ".prototype);\n");
                    ret.Append(tbs + "  " + fromClass + ".call($this);\n");
                    if (c != null)
                    {
                        foreach (string generic in c.GenericArguments)
                        {
                            ret.Append(tbs + "  $this.generic$" + generic + " = generic$" + generic + ";\n");
                        }
                    }

                }

                foreach (Types t in function.paraml.Parameters)
                {
                    if (t is Assign a)
                    {
                        var lcomp = Compile(a.Left);
                        ret.Append(tbs + "  if(" + lcomp + " == void 0) " + lcomp + " = " + Compile(a.Right) + ";\n");
                    }
                }

                ret.Append(CompileBlock(function.block, tabs + 1, componentSetFirst: true, addBl: false));

                if (function.isConstructor)
                {
                    ret.Append(tbs + "  return $this;\n");
                }

                ret.Append(tbs + "}\n");

                if (Interpreter._DEBUG)
                {
                    if (function.functionOpname.Split('.').Count() > 1)
                        ret.Append(tbs + function.functionOpname + "$META = function(){\n");
                    else
                        ret.Append(tbs + "var " + function.functionOpname + "$META = function(){\n");
                    ret.Append(tbs + "  return {");
                    ret.Append("\n" + tbs + "    type: '" + (function.isConstructor ? "constructor" : "function") + "'" + (function.attributes.Count > 0 ? ", " : ""));
                    if (function.attributes.Count > 0)
                    {
                        ret.Append("\n" + tbs + "    attributes: {");
                        int i = 0;
                        foreach (_Attribute a in function.attributes)
                        {
                            ret.Append("\n" + tbs + "      " + a.GetName() + ": " + Compile(a) + ((function.attributes.Count - 1) == i ? "" : ", "));
                            i++;
                        }
                        ret.Append("\n" + tbs + "    },");
                    }
                    ret.Append("\n" + tbs + "  };\n");
                    ret.Append(tbs + "};\n");
                }
            }
            else
            {
                var useBlock = function.block ?? function.assingBlock;
                foreach (var generic in function.genericArguments)
                {
                    useBlock.SymbolTable.Add(generic, new Generic(function, useBlock, generic) { assingBlock = useBlock });
                }
            }
            return ret;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileClass(Class _class, int tabs = 0)
        {
            if (!_class.isExternal && !_class.isForImport)
            {
                if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                {
                    string tbs = DoTabs(tabs);
                    var ret = new StringBuilder(tbs + "var " + _class.getName() + " = function(){");
                    if (_class.block.variables.Count != 0 || _class.parents.Count != 0 || _class.genericArguments.Count != 0) ret.Append("\n");
                    foreach (string generic in _class.genericArguments)
                    {
                        _class.block.SymbolTable.Add(generic, new Generic(_class, _class.block, generic));
                        ret.Append(tbs + "  this.generic$" + generic + " = null;\n");
                    }
                    foreach (UnaryOp parent in _class.parents)
                    {
                        var fnd = _class.assingBlock.SymbolTable.Get(parent.Name.Value, genericArgs: parent.genericArgments.Count);
                        if (!(fnd is Error))
                        {
                            Types inname = fnd;
                            if (inname is Interface @interface)
                                ret.Append(tbs + "  CloneObjectFunction(" + _class.Name.Value + ", " + @interface.getName() + ");\n");
                            else if (inname is Class ic && ic.JSName != "Object")
                                ret.Append(tbs + "  CloneObjectFunction(this, " + ic.getName() + ");\n");
                        }
                    }
                    foreach (KeyValuePair<string, Assign> var in _class.block.variables)
                    {
                        if (var.Value.isStatic) continue;
                        if (var.Value.Right.getToken().type == Token.Type.NULL)
                        {
                            if (var.Value.Left is Variable vari)
                            {
                                if (_class.block.SymbolTable.Get(vari.Type) is Delegate)
                                    ret.Append(tbs + "  this.delegate$" + var.Key + " = null;\n");
                                else
                                    ret.Append(tbs + "  this." + var.Key + " = null;\n");
                            }
                            else
                                ret.Append(tbs + "  this." + var.Key + " = null;\n");
                        }
                        else
                        {
                            if (var.Value.Left is Variable vari)
                            {
                                if (_class.block.SymbolTable.Get(vari.Type) is Delegate)
                                    ret.Append(tbs + "  this.delegate$" + var.Key + " = " + Compile(var.Value.Right) + ";\n");
                                else
                                    ret.Append(tbs + "  this." + var.Key + " = " + Compile(var.Value.Right) + ";\n");
                            }
                            else
                                ret.Append(tbs + "  this." + var.Key + " = " + Compile(var.Value.Right) + ";\n");
                        }
                    }
                    foreach (Types type in _class.block.children)
                    {
                        if (type is Properties prop)
                        {
                            ret.Append(tbs + "  this.Property$" + prop.variable.TryVariable().Value + ".$self = this;\n");
                        }
                    }
                    ret.Append(tbs + "}\n");


                    foreach (KeyValuePair<string, Assign> var in _class.block.variables)
                    {
                        if (!var.Value.isStatic) continue;
                        if (var.Value.Right.getToken().type == Token.Type.NULL)
                            ret.Append(tbs + "" + _class.getName() + "." + var.Key + " = null;\n");
                        else
                            ret.Append(tbs + "" + _class.getName() + "." + var.Key + " = " + Compile(var.Value.Right) + ";\n");
                    }
                    ret.Append(tbs + "\n");

                    if (Interpreter._DEBUG)
                    {
                        ret.Append(tbs + "var " + _class.getName() + "$META = function(){\n");
                        ret.Append(tbs + "  return {");
                        ret.Append("\n" + tbs + "    type: 'class'" + (_class.attributes.Count > 0 ? ", " : ""));
                        if (_class.attributes.Count > 0)
                        {
                            ret.Append("\n" + tbs + "    attributes: {");
                            int i = 0;
                            foreach (_Attribute a in _class.attributes)
                            {
                                ret.Append("\n" + tbs + "      " + a.GetName() + ": " + Compile(a) + ((_class.attributes.Count - 1) == i ? "" : ", "));
                                i++;
                            }

                            ret.Append("\n" + tbs + "    },");
                        }

                        ret.Append("\n" + tbs + "  };\n");
                        ret.Append(tbs + "};\n");
                    }

                    foreach (var b in _class.block.SymbolTable.Table)
                    {
                        if (b.Value is Function bf)
                        {
                            if (string.IsNullOrEmpty(bf.assingBlock.blockClassTo))
                                bf.assingBlock.blockClassTo = _class.Name.Value;
                        }
                    }

                    ret.Append(CompileBlock(_class.block, tabs, true));
                    return ret;
                }
            }
            return new StringBuilder("");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileAssign(Assign assign, int tabs = 0)
        {
            string addCode = "";
            if (assign.attributes != null && (assign.attributes?.Where(x => x.GetName(true) == "Debug")).Any())
            {
                if (Interpreter._DEBUG)
                    Debugger.Break();
                addCode = "debugger;";
            }
            if (assign.isStatic) return new StringBuilder();
            string addName = "";
            if (assign.left is Variable variable)
            {
                var fvar = assign.assingBlock.SymbolTable.Get(variable.Value);
                if (assign.assingBlock.BlockParent != null && !(fvar is Error))
                {
                    if (fvar.GetHashCode() != GetHashCode())
                        assign.isDeclare = false;
                }

                if (variable.Block.Type == Block.BlockType.CONDITION && variable.Block.BlockParent.variables.ContainsKey(variable.Value))
                    assign.isDeclare = false;
            }

            assign.right.assingToType = assign.left;

            Types maybeIs = assign.assingBlock.SymbolTable.Get(assign.left.TryVariable().Value);
            Types maybeIs2 = null;
            string rightCompiled = "";
            if (assign.right is Lambda lambda)
            {
                rightCompiled = Compile(lambda).ToString();
                addName = "lambda$";
            }
            else
                maybeIs2 = assign.assingBlock.SymbolTable.Get(assign.right.TryVariable().Value);

            if (assign.left is Variable variable1)
            {
                assign.right.assingBlock = variable1.Block;
                if (variable1.IsPrimitive && variable1.IsVal)
                    return new StringBuilder();
                //((Variable)left).Check();
            }

            //$[<code>]
            var returnAssigment = "";
            var rightcompile = "";

            if (assign.right is UnaryOp unaryOp)
            {
                unaryOp.assignToParent = assign;
                unaryOp.endit = false;
            }

            while (true)
            {
                if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.JAVASCRIPT)
                {
                    Function usingFunc = null;
                    var inlineIdUsing = 0;
                    var isInline = false;
                    if (assign.right is UnaryOp riuOp && riuOp.Op == "call")
                    {
                        usingFunc = riuOp.FindUsingFunction();
                        if (usingFunc is Function ass && ass.isInline)
                        {
                            ass.inlineId = inlineIdUsing = Function.inlineIdCounter++;
                            isInline = true;
                        }
                    }
                    rightcompile = (rightCompiled == "" ? Compile(assign.right).ToString() ?? "" : rightCompiled);

                    var ltryvar = assign.left.TryVariable();
                    var rtryvar = assign.right.TryVariable();
                    var fleft = assign.assingBlock.SymbolTable.Get(ltryvar.Value);

                    if (!(fleft is Error) && maybeIs is Properties)
                    {
                        string[] varname = ltryvar.Value.Split('.');
                        if (assign.assingBlock.isInConstructor || assign.assingBlock.isType(Block.BlockType.PROPERTIES))
                            varname[0] = "$this";
                        returnAssigment = varname[0] + ".Property$" + string.Join(".", varname.Skip(1)) + ".set($[<code>])";
                    }
                    else if (maybeIs2 != null && !(assign.assingBlock.SymbolTable.Get(rtryvar.Value) is Error) && maybeIs2 is Properties)
                    {
                        string[] varname = rtryvar.Value.Split('.');
                        if (assign.assingBlock.isInConstructor || assign.assingBlock.isType(Block.BlockType.PROPERTIES))
                            varname[0] = "$this";
                        returnAssigment = (assign.isDeclare ? "var " : "") + addName + Compile(assign.left) + " = $[<code>];";
                        rightcompile = varname[0] + ".Property$" + string.Join(".", varname.Skip(1)) + ".get()";
                    }
                    else if (assign.left is Variable)
                    {
                        if (((Variable)assign.left).Type == "auto")
                        {
                            ((Variable)assign.left).Check();
                            if (((Variable)assign.left).Type == "auto")
                            {
                                if (assign.right is Variable rvar)
                                {
                                    rvar.Check();
                                    if (rvar.Type != "auto")
                                        ((Variable)assign.left).setType(rvar.GetDateType());
                                }
                                else if (assign.right is BinOp)
                                    ((Variable)assign.left).setType(((BinOp)assign.right).OutputType);
                                else if (assign.right is CString)
                                    ((Variable)assign.left).setType(new Token(Token.Type.STRING, "string"));
                                else if (assign.right is Number rin)
                                {
                                    if (rin.isReal)
                                        ((Variable)assign.left).setType(new Token(Token.Type.REAL, "float"));
                                    else
                                        ((Variable)assign.left).setType(new Token(Token.Type.INTEGER, "int"));
                                }
                            }
                        }
                        string tbs = DoTabs(tabs);
                        //string ret = "";
                        if (addName == "lambda$")
                        {
                            string var = Compile(assign.left).ToString();
                            if (var.Contains("delegate$") || (var.IndexOf('.') == -1))
                            {
                                returnAssigment = (assign.isDeclare ? "var " : "") + addName + var + " = $[<code>];";
                            }
                            else
                            {
                                string[] spli = var.Split('.');
                                returnAssigment = (assign.isDeclare ? "var " : "") + string.Join(".", spli.Take(spli.Length - 1)) + ".delegate$" + spli.Skip(spli.Length - 1).First() + " = $[<code>];";
                            }
                        }
                        else
                        {
                            if (assign.isNull)
                            {
                                returnAssigment = (assign.isDeclare ? "var " : "") + addName + Compile(assign.left) + ";";
                                rightcompile = "";
                            }
                            else
                            {
                                returnAssigment = (assign.isDeclare ? "var " : "") + addName + Compile(assign.left) + " = $[<code>];";
                            }
                        }
                        //return ret;
                    }
                    else
                    {
                        returnAssigment = addName + Compile(assign.left) + " = $[<code>];";
                        rightcompile = (rightCompiled == "" ? Compile(assign.right).ToString() : rightCompiled);
                    }

                    if (isInline)
                    {
                        var ret = "";
                        var varname = usingFunc.Name + "$" + inlineIdUsing + "$return";
                        var rightCode = rightcompile.Substring(rightcompile.Length - 1, 1) == "\n" ? rightcompile.Substring(0, rightcompile.Length - 1) : rightcompile;
                        rightCode = (rightCode.Substring(0, 1) == "\n" ? rightCode.Substring(1) : rightCode);
                        if (usingFunc.Block.children.Count == 2 && usingFunc.ParameterList.IsAllPrimitive)
                        {
                            rightCode = (rightCode.Substring(rightCode.Length - 1, 1) == "\n" ? rightCode.Substring(0, rightCode.Length - 1) : rightCode);
                            rightCode = new StringBuilder(rightCode).Replace(varname + " = ", "").ToString();
                            ret = new StringBuilder(returnAssigment).Replace("$[<code>]", rightCode.Substring(0, rightCode.Length - 1)).ToString();
                        }
                        else
                        {
                            ret = DoTabs(tabs) + "var " + varname + " = '';\n";
                            ret += rightCode;
                            ret += new StringBuilder(returnAssigment).Replace("$[<code>]", varname);
                        }

                        return new StringBuilder(ret);
                    }
                }

                break;
            }
            return new StringBuilder(DoTabs(tabs) + returnAssigment).Replace("$[<code>]", rightcompile);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileArray(Array array)
        {
            if (array.isObject)
                return new StringBuilder("{" + string.Join(", ", array._object.Select(x => x.Key.Value + ": " + Compile(x.Value))) + "}");
            return new StringBuilder("[" + string.Join(", ", array.list.Select(x => Compile(x))) + "]");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileEnum(_Enum _enum, int tabs = 0)
        {
            string tbs = DoTabs(tabs);
            StringBuilder ret = new StringBuilder();

            var nm = (_enum.block.assignTo == "" ? _enum.name.Value : _enum.block.assignTo + "." + _enum.name.Value);

            if (_enum.block.assignTo == "")
                ret.Append("var " + nm + " = {\n");
            else
                ret.Append(nm + " = {\n");
            var i = 0;
            var f = true;
            foreach (KeyValuePair<Token, Types> v in _enum.values)
            {
                if (f) { f = false; } else { ret.Append(",\n"); }
                if (v.Value != null)
                {
                    i = v.Value.Visit();
                    ret.Append(tbs + Types.TABS + v.Key.Value + ": " + i);
                }
                else
                    ret.Append(tbs + Types.TABS + v.Key.Value + ": " + i);
                i++;
            }
            ret.Append("\n" + tbs + "}\n");
            ret.Append(tbs + "Object.freeze(" + nm + ");\n");

            if (Interpreter._DEBUG)
            {
                ret.Append(tbs + (_enum.block.assignTo == "" ? "var " : "") + nm + "$META = function(){\n");
                ret.Append(tbs + "  return {");
                ret.Append("\n" + tbs + "    type: 'enum'");
                ret.Append("\n" + tbs + "  };\n");
                ret.Append(tbs + "};\n");
            }

            return ret;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileAttribute(_Attribute attribute)
        {
            attribute._class = attribute.assingBlock.SymbolTable.Get(attribute.nclass.Value);
            if (attribute._class is Error)
                return new StringBuilder("");
            if (attribute.uop == null)
                attribute.uop = new UnaryOp(new Token(Token.Type.NEW, -1), attribute.nclass, attribute.plist) { assingBlock = attribute.assingBlock };
            return Compile(attribute.uop);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringBuilder CompileBlock(Block block, int tabs = 0, bool noAssign = false, bool componentSetFirst = false, bool addBl = true)
        {
            var tbs = DoTabs(tabs);
            var ret = new StringBuilder();
            foreach (Types child in block.children)
            {
                if (child == null) continue;
                if (noAssign && child is Assign) continue;
                child.assignTo = (block.assingBlock == null ? block.blockAssignTo : block.assingBlock.assignTo);
                if (child is Component chc && componentSetFirst)
                    chc.IsStart = true;
                /* TODO: fuckar
                if (!(child is Class) && !(child is _Enum))
                    child.assingBlock = block;
                */
                var p = Compile(child, 0);
                if (tabs != 0)
                {
                    var returntab = "";
                    if (p.Length > 0)
                    {
                        foreach (var s in p.ToString().Split('\n'))
                        {
                            returntab += (returntab == "" ? s : (s.Trim() == s ? tbs + s : s)) + "\n";
                        }

                        p = new StringBuilder(returntab.Substring(returntab.Length - 1) == "\n" ? returntab.Substring(0, returntab.Length - 1) : returntab);
                    }

                    if (p.Length > 0 && p.ToString().Substring(p.Length - 2, 1) != "\n")
                        ret.Append(tbs + p + "\n");
                    else if (p.Length > 0)
                        ret.Append(p + "\n");
                }
                else
                {
                    if (p.Length > 0 && p.ToString().Substring(p.Length - 2, 1) != "\n")
                        ret.Append(tbs + p + "\n");
                    else
                        ret.Append(p + "\n");
                }
            }

            if (block.children.Count > 1 && addBl)
            {
                var r2 = ret.Replace("\n", "\n" + Types.TABS + Types.TABS).ToString();
                ret = new StringBuilder(tbs + "{\n" + Types.TABS + Types.TABS + r2.Substring(0, r2.Length - (Types.TABS.Length * 2)) + "}");
            }

            return ret;
        }
    }
}

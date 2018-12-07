using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compilator
{
    public class Component : Types
    {
        public bool _componentNotFound = false;
        public bool _componentNotHaveConstructor = false;
        public bool _componentNotHaveParent = false;

        public Component(Block block)
        {
            assingBlock = block;
            BeforeCreate();
        }

        private void BeforeCreate()
        {
            if (_rewr.Count == 0)
            {
                _rewr.Add("input", "Input");
                _rewr.Add("div", "Div");
                _rewr.Add("span", "Span");
                _rewr.Add("label", "Label");
                _rewr.Add("ul", "Ul");
                _rewr.Add("li", "Li");
            }
        }

        /*Serialization to JSON object for export*/
        //[JsonParam] public Block AssignBlock => assingBlock;
        [JsonParam] public bool IsStart { get; set; } = false;
        [JsonParam] public string Name { set; get; } = "";
        [JsonParam] public Token Token { set; get; }
        [JsonParam] public Dictionary<string, Types> Arguments { set; get; } = new Dictionary<string, Types>();
        [JsonParam] public string InnerText { set; get; } = "";
        [JsonParam] public List<Component> InnerComponent { set; get; } = new List<Component>();
        [JsonParam] public Types Fun { set; get; } = null;

        public override void FromJson(JObject o)
        {
            BeforeCreate();
            throw new NotImplementedException();
        }
        public Component() { }

        public static readonly string[] _base = { "div", "span", "input", "select", "label", "ul", "li" };
        private static Dictionary<string, string> _rewr = new Dictionary<string, string>();

        public override string Compile(int tabs = 0) { return Compile(tabs, true); }
        public string Compile(int tabs = 0, bool iret = false)
        {
            var tbs = DoTabs(tabs);
            string ret = "";
            if (assignTo != "")
            {
                var func = (Function) assingBlock.SymbolTable.Get(assignTo);
                if (func.attributes?.Where(x => x.GetName(true) == "Debug").Count() > 0)
                {
                    ret += "/*Component: " + Name + "*/";
                }
            }

            var _oname = Name;
            /*if (_rewr.ContainsKey(_name) && (!iret || _inner.Count > 0 ))
                _name = _rewr[_name];*/

            var txt = InnerText.Replace("\r\n", "").Replace("\t", "");
            Regex re = new Regex(@"\{(.*)\}");
            var replace = re.Replace(txt, x =>
            {
                var s = x.Value.Substring(1, x.Value.Length - 2);
                var find = assingBlock.SymbolTable.Get(s);
                if (assingBlock.assingToType?.assignTo != null)
                {
                    var _class = assingBlock.SymbolTable.Get(assingBlock.assingToType?.assignTo);
                    if (_class is Class cls)
                    {
                        var parent = cls.GetParent();
                        find = parent.assingBlock.SymbolTable.Get(s);
                    }
                }
                if (find is Variable)
                    return "\"+" + s + "+\"";
                else if (find is Function)
                    return "fun";
                else if (find is Assign fa)
                    return "\"+" + s + "+\"";
                else if (find is Class)
                {
                    return "\"+" + s.Replace("this", "_this") + "+\"";
                }
                return "";
            });
            
            var pou = "";
            foreach (var p in Arguments)
            {
                if (p.Value is Assign pa)
                {
                    if (pa.Left is Variable pav)
                    {
                        var cml = p.Value.Compile();
                        pou += "" + p.Key + ": " + cml.Replace("this", "_this") + ", ";
                        //args += "{name: \"" + pav.Value + "\", value: " + cml + "}, ";
                    }
                }
                else if (p.Value is Variable pv)
                {
                    var cml = p.Value.Compile();
                    if (Char.IsLetterOrDigit(p.Key[0]))
                        pou += "" + p.Key + ": ";
                    else
                        pou += "\"" + p.Key + "\": ";
                    if ((p.Key.Substring(0, 2) != "on" && cml.Contains("this")))
                        pou += "function(){ return " + cml.Replace("this", "_this") + "; }, ";
                    else
                        pou += cml.Replace("this", "_this") + ", ";
                    //args += "{name: \"" + pv.Value + "\", value: " + cml + "}, ";
                }
                else if (p.Value is Lambda arl)
                {
                    arl.endit = false;
                    arl.replaceThis = "_this";
                    var cml = arl.Compile();
                    if (Char.IsLetterOrDigit(p.Key[0]))
                        pou += "" + p.Key + ": " + cml.Replace("this", "_this").Replace("__this", "_this") + ", ";
                    else
                        pou += "\"" + p.Key + "\": " + cml.Replace("this", "_this").Replace("__this", "_this") + ", ";
                }
                else
                {
                    if (Char.IsLetterOrDigit(p.Key[0]))
                        pou += "" + p.Key + ": " + p.Value.Compile().Replace("this", "_this") + ", ";
                    else
                        pou += "\"" + p.Key + "\": " + p.Value.Compile().Replace("this", "_this") + ", ";
                }
            }

            if(pou != "")
                pou = pou.Substring(0, pou.Length - 2);

            if (Name == "")
            {                
                var tb0 = DoTabs(iret ? 0 : tabs);
                if(Fun == null)
                    ret += tb0 + "\"" + replace + "\"";
                else
                {
                    if (Fun is UnaryOp fu && fu.Op == "call")
                    {
                        fu.endit = false;
                        //ret += tb0 + Fun.Compile().Replace("this", "_this");
                    }
                    //else
                        ret += tb0 + "function(){ return " + Fun.Compile().Replace("this", "_this").Replace("__this","_this") + "; }";
                }
            }
            else if (InnerComponent.Count > 0)
            {
                var tb0 = DoTabs(iret ? 0 : tabs);
                var tb1 = DoTabs(iret ? tabs + 2 : tabs + 1);
                var hm0 = iret ? tabs + 2 : tabs + 1;

                if (_base.Contains(_oname) || _oname.ToLower() == _oname)
                {
                    var f = assingBlock.SymbolTable.Get(Name);
                    if(f is Error)
                        ret += tb0 + "new Pyr.createElement(\r\n" + tb1 + "\"" + _oname + "\"";
                    else
                        ret += tb0 + "new Pyr.createElement(\r\n" + tb1 + "" + _oname + "";
                }
                else
                    ret += tb0 + "new Pyr.createElement(\r\n" + tb1 + Name;
                if (pou != "")
                    ret += ",\r\n" + tb1 + "{ " + pou + " }, \r\n";
                else
                    ret += ",\r\n" + tb1 + "null, \r\n";
                InnerComponent.ForEach(x => { ret += x.Compile(hm0, false) + ", \r\n"; });
                ret = ret.Substring(0, ret.Length - 4);
                ret += "\r\n" + tb0 + ")";
            }
            else
            {
                var tb0 = DoTabs(iret ? 0 : tabs);

                if (_base.Contains(_oname) || _oname.ToLower() == _oname)
                    ret += tb0 + "new Pyr.createElement(" + "\"" + _oname + "\"";
                else
                {
                    var fncreate = assingBlock.SymbolTable.Get("constructor " + Name);
                    if (fncreate is Error)
                    {
                        _componentNotHaveConstructor = true;
                        if (assingBlock.SymbolTable.Get(Name) is Error)
                        {
                            _componentNotHaveConstructor = false;
                            _componentNotFound = true;
                        }
                        return "";
                    }
                    else
                    {
                        var cls = assingBlock.SymbolTable.Get(Name);
                        if (cls is Class clss)
                        {
                            if (clss.GetParent()?.Name.Value != "Component")
                            {
                                _componentNotHaveParent = true;
                                return "";
                            }
                        }
                    }

                    if(fncreate is Function fa)
                        ret += tb0 + "new Pyr.createElement(" + Name + "." + fa.Name;
                    else
                        ret += tb0 + "new Pyr.createElement(" + Name;
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
            
            if (IsStart)
                return "var _this = this;\r\n" + DoTabs(tabs + 1) + "return " + ret + ";";
            if(iret)
                return "return " + ret + ";";
            return ret;
        }

        public override Token getToken()
        {
            throw new NotImplementedException();
        }

        public override string InterpetSelf()
        {
            throw new NotImplementedException();
        }

        public override void Semantic()
        {
            //if(Name.ToLower() == Name)
            //    Interpreter.semanticError.Add(new Error("#1001 Component class \"" + Name + "\" must start with big letter!", Interpreter.ErrorType.WARNING, Token));
            if(_componentNotFound)
                Interpreter.semanticError.Add(new Error("#1001 Component \"" + Name + "\" was not found!", Interpreter.ErrorType.ERROR, Token));
            if(_componentNotHaveConstructor)
                Interpreter.semanticError.Add(new Error("#1002 Component \"" + Name + "\" must have constructor!", Interpreter.ErrorType.ERROR, Token));
            if(_componentNotHaveParent)
                Interpreter.semanticError.Add(new Error("#1003 Component \"" + Name + "\" must inherit from class \"Component\"!", Interpreter.ErrorType.ERROR, Token));
            if(InnerComponent.Count > 0)
                InnerComponent.ForEach(x => x.Semantic());
            Fun?.Semantic();
            Arguments?.Any(x => { x.Value.Semantic(); return false; });
        }

        public override int Visit()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Delegate : Types
    {
        Token name;
        Block block;
        ParameterList paraml;
        Token returnt;
        public bool returnAsArray = false;
        public List<_Attribute> attributes = new List<_Attribute>();
        public List<string> returnGeneric = new List<string>();
        List<string> genericArguments = new List<string>();

        public Delegate(Token name, ParameterList paraml, Token returnt, Interpreter interpret, Block parent_block = null)
        {
            this.block = parent_block;
            this.block.assignTo = name.Value;
            this.block.assingToType = this;

            this.name = name;
            this.paraml = paraml;
            this.paraml.assingBlock = this.block;
            this.returnt = returnt;

            parent_block.Parent.SymbolTable.Add(name.Value, this);
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

        public ParameterList ParameterList { get { return paraml; } }
        public Token Returnt { get { return returnt; } }
        public override Token getToken(){ return name; }

        /// <summary>
        /// You will get this:
        /// <para>0 - Function is okay for Predicate</para>
        /// <para>1 - Return type is not same</para>
        /// <para>2 - Generic return is not same</para>
        /// <para>3 - Predicate is not return as Array</para>
        /// <para>4 - Predicate return as Array</para>
        /// <para>5 - Number of arguments is not same</para>
        /// <para>6 - Parameters type is not same</para>
        /// </summary>
        /// <param name="func">Function to compare with Predicate</param>
        /// <returns>Error state as int</returns>
        public int CompareTo(Variable t, Function func, ParameterList p)
        {            
            if (p.parameters[0] is Lambda lambda)
            {                                
                if (lambda.ParameterList.Parameters.Count != paraml.Parameters.Count)
                    return 5;
                if (lambda.ParameterList.declare)
                {
                    Block b = new Block(block.Interpret);
                    ParameterList plist = new ParameterList(lambda.ParameterList);
                    plist.assingBlock = b;
                    int i = 0;
                    foreach (string g in t.GenericList)
                    {
                        if (genericArguments.Count > i)
                        {
                            Types qqq = assingBlock.SymbolTable.Get(g);
                            if (!(qqq is Error))
<<<<<<< HEAD
                                plist.GenericTUsage.Add(genericArguments[i], qqq);
=======
                                plist.GenericTUsage.Add(genericArguments[i], assingBlock.SymbolTable.Get(g));
>>>>>>> 0c640203808d4ca5c25cb372dd6d91da202c18f8
                            else
                                plist.GenericTUsage.Add(genericArguments[i], p.GenericTUsage[genericArguments[i]]);
                        }
                        i++;
                    }

                    if (!paraml.Compare(plist))
                        return 6;
                }
            }
            else if (func == null)
            {
                if (p.Parameters.Count != paraml.Parameters.Count)
                    return 5;
                if (p.declare)
                {
                    Block b = new Block(block.Interpret);
                    ParameterList plist = new ParameterList(p);
                    plist.assingBlock = b;
                    int i = 0;
                    foreach (string g in t.GenericList)
                    {
                        if (genericArguments.Count > i)
                        {
                            Types qqq = assingBlock.SymbolTable.Get(g);
                            if (!(qqq is Error))
<<<<<<< HEAD
                                plist.GenericTUsage.Add(genericArguments[i], qqq);
=======
                                plist.GenericTUsage.Add(genericArguments[i], assingBlock.SymbolTable.Get(g));
>>>>>>> 0c640203808d4ca5c25cb372dd6d91da202c18f8
                            else
                                plist.GenericTUsage.Add(genericArguments[i], p.GenericTUsage[genericArguments[i]]);
                        }
                        i++;
                    }

                    if (!paraml.Compare(plist))
                        return 6;
                }
            }
            else
            {
                Block b = new Block(block.Interpret);
                ParameterList plist = new ParameterList(func.ParameterList);
                plist.assingBlock = b;
                int i = 0;
                foreach (string g in t.GenericList)
                {
                    if (genericArguments.Count > i)
                    {
                        Types qqq = assingBlock.SymbolTable.Get(g);
                        Types sss = func.Block.SymbolTable.Get(g);
                        if (!(qqq is Error))
                            plist.GenericTUsage.Add(genericArguments[i], qqq);
                        else if(!(sss is Error))
                            plist.GenericTUsage.Add(genericArguments[i], sss);
                        else
                            plist.GenericTUsage.Add(genericArguments[i], p.GenericTUsage[genericArguments[i]]);
                    }
                    i++;
                }
                if (func.ParameterList.Parameters.Count != paraml.Parameters.Count)
                    return 5;
                if (func.Returnt != null)
                {
                    bool isGenericReturnOkay = false;
                    if (func.Returnt.Value != returnt.Value)
                    {
                        if (GenericArguments.Contains(returnt.Value))
                        {
                            if (plist.GenericTUsage.ContainsKey(returnt.Value))
                            {
                                if (plist.GenericTUsage[returnt.Value] is Class)
                                {
                                    if (func.Returnt.Value == ((Class)plist.GenericTUsage[returnt.Value]).Name.Value)
                                        isGenericReturnOkay = true;
                                }
                            }
                        }
                        if (!isGenericReturnOkay)
                            return 1;
                    }
                    if (func.returnGeneric != null)
                    {
                        int x = 0;
                        foreach (string g in func.returnGeneric) { if (g != returnGeneric[x]) { return 2; } x++; }
                    }
                    if (func.returnAsArray && !returnAsArray)
                        return 3;
                    if (!func.returnAsArray && returnAsArray)
                        return 4;
                }
                if (!paraml.Compare(plist))
                    return 6;
            }
            return 0;
        }

        public string GetError(int errorCode)
        {
            if (errorCode == 1)
                return "Return type is not same";
            else if (errorCode == 2)
                return "Generic return is not same";
            else if (errorCode == 3)
                return "Predicate is not return as Array";
            else if (errorCode == 4)
                return "Predicate return as Array";
            else if (errorCode == 5)
                return "Number of arguments are not same";
            else if (errorCode == 6)
                return "Parameters types are not same";
            return "UNKOWN errorCode";
        }

        public string _hash = "";
        public string getHash()
        {
            if (assingBlock == null) assingBlock = block;
            if (assingBlock.SymbolTable.GetAll(name.Value)?.Count > 1)
            {
                if (_hash == "")
                    _hash = string.Format("{0:X8}", (name.Value + paraml.List() + block?.GetHashCode()).GetHashCode());
                return _hash;
            }
            return "";
        }
        public String Name {
            get {
                string hash = getHash();
                return name.Value + (hash != "" ? "_" + hash : "");
            }
        }
        public String RealName { get { return name.Value; } }

        public override string Compile(int tabs = 0)
        {
            int q = 0;
            string gener = "";
            foreach (string generic in genericArguments)
            {
                if (q != 0) gener += ", ";
                gener += generic;
                q++;
            }

            foreach (string generic in genericArguments)
            {
                block.SymbolTable.Add(generic, new Generic(this, block, generic) { assingBlock = block });
            }

<<<<<<< HEAD
            if (Interpreter._LANGUAGE == Interpreter.LANGUAGES.PYTHON)
                return "#Delegate " + RealName + (gener != "" ? "<" + gener + ">" : "") + "(" + paraml.List() + ") -> " + Returnt.Value;            
=======
>>>>>>> 0c640203808d4ca5c25cb372dd6d91da202c18f8
            return "//Delegate " + RealName + (gener != "" ? "<" + gener + ">" : "") + "(" + paraml.List() + ") -> " + Returnt.Value;
        }
        
        public override void Semantic()
        {
            
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

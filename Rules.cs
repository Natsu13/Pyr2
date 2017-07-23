using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Rules
    {
        Dictionary<string, string> rules = new Dictionary<string, string>();

        public void Add(string name, string value)
        {
            if (!rules.ContainsKey(name))
            {
                rules.Add(name, value);
            }
        }

        public Dictionary<string, string> Get()
        {
            return rules;
        }

        public Dictionary<string, string> Test(Dictionary<string, string> source, string test, string group, List<string> iFound = null)
        {            
            Dictionary<string, string> ret = new Dictionary<string, string>();
            Dictionary<string, List<List<string>>> rulec = new Dictionary<string, List<List<string>>>();
            foreach (KeyValuePair<string, string> rule in rules) {                
                int t = 0;
                string have = "";
                List<List<string>> ruls = new List<List<string>>();
                List<string> lokg = new List<string>();
                foreach (char rch in rule.Value)
                {
                    if (rch == '(' && t == 0)
                    {
                        t = 1;
                    }
                    else if (t == 1 && rch != ')')
                    {
                        have += rch;
                    }
                    else if(t == 1 && rch == ')')
                    {
                        t = 0;
                        char firstH = '\0', secondH = '\0';
                        bool separ = false;
                        lokg = new List<string>();
                        foreach (char ch in have)
                        {
                            if (firstH == '\0')
                            {
                                firstH = ch;
                                if (firstH == '.')
                                {
                                    lokg.Add(".");
                                    firstH = '\0';
                                }
                            }
                            else if (ch == '-') { separ = true; }
                            else if (!separ)
                            {
                                lokg.Add(firstH.ToString());
                                firstH = ch;
                            }
                            else if(separ)
                            {
                                separ = false;
                                secondH = ch;

                                string chars = "";
                                for (int x = firstH; x <= secondH; x++)
                                {
                                    chars += (char)x;
                                }
                                lokg.Add(chars);

                                secondH = '\0';
                                firstH = '\0';
                            }
                        }                        
                        if (firstH != '\0') Console.WriteLine("Error in specification range! ("+ have + ")");
                        have = "";
                        ruls.Add(lokg);
                        t = 2;
                    }
                    else if(t == 2 && (rch == '*' || rch == '?' || rch == '+' || rch == '!'))
                    {
                        lokg.Insert(0, rch.ToString());
                        t = 0;
                    }
                    else
                    {
                        t = 0;
                        lokg = new List<string>();
                        lokg.Add(rch.ToString());
                        ruls.Add(lokg);
                    }
                }
                rulec.Add(rule.Key, ruls);                
            }

            int segments = 0;
            string sgroup = "";
            string ssave = "";

            foreach (KeyValuePair<string, List<List<string>>> dta in rulec)
            {                
                int posintest = 0;
                int posinregu = 0;
                string retgh = "";
                bool error = false;
                List<List<string>> chrsa = dta.Value;
                chrsa.Add(new List<string>() { "\0" });
                string founhere = "";
                foreach (List<string> _chrsa in dta.Value)
                {
                    if (_chrsa[0][0] == '\0') break;
                    if (_chrsa.Count == 1) {
                        if(test.Length == posintest)
                        {
                            error = true;
                            break;
                        }
                        if (tryCheck(test[posintest], _chrsa))
                        {
                            retgh += test[posintest].ToString();
                            posintest++;
                            posinregu++;
                        }
                        else
                        {
                            error = true;
                            break;
                        }
                    }
                    else
                    {
                        if(_chrsa[0][0] == '?' || _chrsa[0][0] == '*')
                        {
                            if(tryCheck(test[posintest], chrsa[posinregu+1]))
                            {
                                retgh += test[posintest].ToString();
                                posintest++;
                                posinregu++;
                                break;
                            }
                        }

                        bool found = false;
                        bool cont = true;
                        founhere = "";
                        while (cont)
                        {
                            if(test.Length == posintest)
                            {
                                cont = false;
                                break;
                            }
                            if(found && _chrsa[0][0] == '?')
                            {
                                cont = false;
                                break;
                            }
                            bool foundnext = tryCheck(test[posintest], chrsa[posinregu + 1]);
                            if (tryCheck(test[posintest], _chrsa) && !foundnext)
                            {
                                if (!found) found = true;
                                retgh += test[posintest];
                                founhere += test[posintest];
                                posintest++;
                            }
                            else
                            {
                                cont = false;
                            }
                        }
                        if (_chrsa[0][0] == '+' && !found)
                        {
                            error = true;
                            break;
                        }                       
                        posinregu++;

                    }
                }
                if (!error)
                {
                    if (segments == 0 || dta.Value.Count > segments)
                    {
                        iFound.Clear();
                        if (iFound != null) iFound.Add(founhere);
                        sgroup = group + ":" + dta.Key;
                        ssave = retgh;
                        segments = dta.Value.Count;
                    }
                }
            }
            if (segments != 0)
            {
                source.Add(sgroup, ssave);
            }
            return source;
        }

        public bool tryCheck(char c, List<string> chrsa)
        {
            if (chrsa.Count == 1)
            {
                if (chrsa[0][0] == '\0')
                    return false;
                if (chrsa[0][0] != c)
                {
                    return false;
                }
                return true;
            }
            else
            {
                string rule;
                bool first = true;
                foreach (string rlchar in chrsa)
                {
                    if (first)
                    {
                        rule = rlchar[0].ToString();
                        first = false;
                    }
                    else
                    {
                        if (rlchar.Contains(c) || rlchar == ".")
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}

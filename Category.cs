using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilator
{
    public class Category
    {
        Dictionary<string, List<Rules>> category = new Dictionary<string, List<Rules>>();

        public void Add(string name, Rules rule)
        {
            if (!category.ContainsKey(name))
                category.Add(name, new List<Rules>());
            category[name].Add(rule);
        }

        public List<Rules> Get(string name)
        {
            return category[name];
        }

        public Dictionary<string, string> Test(string test, List<string> found = null)
        {            
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (KeyValuePair<string, List<Rules>> r in category)
            {
                foreach (Rules k in r.Value)
                {
                    k.Test(ret, test, r.Key, found);
                }
            }
            Console.WriteLine();
            return ret;
        }
    }
}

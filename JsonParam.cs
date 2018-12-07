using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Compilator
{    
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonParam : Attribute
    {
        public string Name { get; set; }

        public JsonParam() { }
        public JsonParam(string name)
        {
            Name = name;
        }

        public static Types FromJson(object jsomething)
        {
            JObject jObject = null;
            if (jsomething is JValue jValue && !jValue.HasValues)
                return null;
            if (jsomething is JObject _jObject)
                jObject = _jObject;

            Type t = Type.GetType(jObject["type"].ToString());
            if (t == null)
                throw new Exception("Unexisting type " + jObject["type"].ToString());
            var inst = (Types)Activator.CreateInstance(t);
            inst.FromJson((JObject)jObject["construct"]);
            return inst;
        }

        public static T FromJson<T>(object jObject) where T:Types
        {
            var result = FromJson(jObject);
            if (result == null)
                return null;
            if (!(result is T))
                throw new Exception("Object type " + result.GetType().Name + " is not " + typeof(T).Name);
            return (T) result;
        }

        public static List<T> FromJsonArray<T>(JArray jArray) where T:Types
        {
            if (jArray == null)
                return null;

            var list = new List<T>();
            foreach (var jToken in jArray)
            {
                var jobject = (JObject) jToken;
                list.Add((T)FromJson(jobject));
            }
            return list;
        }

        public static List<T> FromJsonArrayBase<T>(JArray jArray)
        {
            if (jArray == null)
                return null;

            var list = new List<T>();
            foreach (var jToken in jArray)
            {
                if (jToken is JValue jValue)
                {
                    list.Add((T)jValue.Value);
                }
                else if (jToken is JObject jObject)
                {
                    if (typeof(T).Name != typeof(KeyValuePair<,>).Name)
                        throw new CheckoutException("Can't convert " + typeof(T).Name + " to " + typeof(KeyValuePair<,>).Name);

                    var key = jObject.First.First;
                    var value = jObject.Last.First;
                    var v = (T) Convert.ChangeType(new KeyValuePair<JObject, JObject>((JObject) key, (JObject) value), typeof(T));
                    list.Add(v);
                }
            }
            return list;
        }

        public static Dictionary<T1, T2> FromJsonDictionary<T1, T2>(object obj) where T1:Types where T2:Types
        {
            var dict = new Dictionary<T1, T2>();
            if (obj is JValue jValue)
            {
                if (jValue.Value is string jString)
                {
                    var jSomething = JsonConvert.DeserializeObject<object>(jString);
                    if (jSomething is JArray jArray)
                    {
                        if (jArray.Count > 0)
                        {
                            var x = JsonParam.FromJsonArrayBase<KeyValuePair<JObject, JObject>>(jArray);
                            foreach (var keyValuePair in x)
                            {
                                dict.Add((T1) FromJson(keyValuePair.Key), (T2) FromJson(keyValuePair.Value));
                            }
                        }
                    }
                    else if (jSomething is JObject jObject)
                    {
                        if (jObject.Count > 0)
                        {
                            var x = 0;
                        }
                    }
                }
            }
            else
            {
                var x = 1;
            }
            return dict;
        }

        public static Dictionary<T1, T2> FromJsonDictionaryKeyBase<T1, T2>(object obj) where T2:Types
        {
            var dic = new Dictionary<T1, T2>();
            if (obj is JValue jValue)
            {
                if (jValue.Value is string jString)
                {
                    var jSomething = JsonConvert.DeserializeObject<object>(jString);
                    if (jSomething is JObject jObject)
                    {
                        if (jObject.Count > 0)
                        {
                            var x = 0;
                        }
                    }
                }
            }
            return dic;
        }

        public static Dictionary<T1, T2> FromJsonDictionaryBase<T1, T2>(object obj)
        {
            return null;
        }

        public static JObject ToJson<T>(T myobject)
        {
            var o = new JObject();
            var type = myobject.GetType();
            foreach (var propertyInfo in type.GetProperties())
            {
                var MyAttribute = (JsonParam) Attribute.GetCustomAttribute(propertyInfo, typeof (JsonParam));
                if (MyAttribute != null)
                {
                    var param = type.GetProperty(propertyInfo.Name)?.GetValue(myobject, null);

                    JToken t = null;
                    if (param is NoOp) continue;
                    if (param is Class pc && pc.isForImport) continue;
                    if (param == null) t = null;
                    else if (param is Types pt) t = ToJson(pt);
                    else if (param is Token pto) t = ToJson(pto);
                    else if (param is string) t = param.ToString();
                    else if (param is int pi) t = pi;
                    else if (param is float pf) t = pf;
                    else if (param is decimal pd) t = pd;
                    else if (param is bool pb) t = pb;
                    else if (param is List<Types> pl) t = JToken.FromObject(pl.Select(ToJson)); 
                    else if (param.GetType().IsGenericType && param.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        var q = param.GetType();                        
                        if (q.GenericTypeArguments[0] == typeof(JObject) && q.GenericTypeArguments[1] == typeof(JObject))
                        {
                            var c = ((Dictionary<JObject, JObject>) param).ToList();
                            t = JsonConvert.SerializeObject(c);
                        }else
                            t = JsonConvert.SerializeObject(param);
                    }
                    else t = JToken.FromObject(param);                    

                    o[MyAttribute.Name ?? propertyInfo.Name] = t;
                }   
            }

            var expr = new JObject();
            expr["type"] = type.Namespace + "." + type.Name;
            expr["construct"] = o;

            return expr;
        }
    }
}

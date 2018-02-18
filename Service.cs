using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Compilator
{
    public class Service : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            string[] data = e.Data.Split(' ');
            if (data[0] == "filelist")
            {
                List<string> list = new List<string>();
                foreach(var p in Interpreter.fileList) { list.Add(p.Key); }
                Send(JsonConvert.SerializeObject(list));
            }
            else if(data[0] == "load")
            {
                Send(Interpreter.fileList[data[1]]);
            }
        }
    }
}

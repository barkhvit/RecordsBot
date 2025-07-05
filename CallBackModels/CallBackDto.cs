using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.CallBackModels
{
    public class CallBackDto
    {
        public string Object { get; set; }
        public string Action { get; set;}
        

        public CallBackDto(string _object, string action)
        {
            Object = _object;
            Action = action;
        }
        //принимает строку типа: Object:Action
        public static CallBackDto FromString(string query)
        {
            string[] strings = query.Split(':');
            return new CallBackDto(strings[0], strings[1]);
        }

        public override string ToString()
        {
            return $"{Object}:{Action}";
        }
    }
}

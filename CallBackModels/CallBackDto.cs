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
        public string? Action { get; set;}
        public Guid? Id { get; set; }
        public DateOnly? Date {get;set;}

        public CallBackDto(string _object, string? action = null, Guid? id = null, DateOnly? date=null)
        {
            Object = _object;
            Action = action;
            Id = id;
            Date = date;
        }
        //принимает строку типа: Object:Action
        public static CallBackDto FromString(string query)
        {
            string[] strings = query.Split(':');

            switch (strings.Count())
            {
                case 3:
                    if (Guid.TryParse(strings[2], out Guid guid))
                    {
                        return new CallBackDto(strings[0], strings[1], guid);
                    } 
                    else if (DateOnly.TryParse(strings[2], out DateOnly result))
                    {
                        return new CallBackDto(strings[0], strings[1], date: result);
                    }
                    break;
                case 2: return new CallBackDto(strings[0], strings[1]);
            }
            return new CallBackDto(strings[0]);
        }

        public override string ToString()
        {
            if(Action != null && Id == null && Date == null) return $"{Object}:{Action}";
            else if (Action != null && Id != null && Date == null) return $"{Object}:{Action}:{Id}";
            else if (Action != null && Id == null && Date != null) return $"{Object}:{Action}:{Date}";
            else return $"{Object}";
        }
    }
}

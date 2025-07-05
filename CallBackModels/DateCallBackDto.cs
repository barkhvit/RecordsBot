using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.CallBackModels
{
    public class DateCallBackDto : CallBackDto
    {
        public DateOnly? dateOnly { get; set; }
        public DateCallBackDto(string _object, string action, DateOnly? _dateOnly) : base(_object, action)
        {
            dateOnly = _dateOnly;
        }

        public static new DateCallBackDto FromString(string input)
        {
            string[] strings = input.Split(':');
            DateOnly? date = null;
            if (strings.Length > 2)
            {
                if (DateOnly.TryParse(strings[2],out DateOnly result))
                {
                    date = result;
                }
            }
            return new DateCallBackDto("Date", strings[1], date);
        }

        public override string ToString() 
        {
            return $"{base.ToString()}:{dateOnly.ToString()}";
        }
    }
}

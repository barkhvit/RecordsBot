using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.CallBackModels
{
    public class FreePeriodCallBackDto : CallBackDto
    {
        public Guid? FreePeriodId { get; set; }
        public FreePeriodCallBackDto(string _object, string action, Guid? freePeriod) : base(_object, action)
        {
            FreePeriodId = freePeriod;
        }

        public static new FreePeriodCallBackDto FromString(string input)
        {
            string[] strings = input.Split(':');
            Guid? guid = null;
            if (strings.Length > 2)
            {
                if(Guid.TryParse(strings[2], out Guid result))
                {
                    guid = result;
                }
            }
            return new FreePeriodCallBackDto("FreePeriod", strings[1], guid);
        }

        public override string ToString()
        {
            return $"{base.ToString()}:{FreePeriodId.ToString()}";
        }
    }
}

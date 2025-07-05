using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.CallBackModels
{
    public class ProcedureCallBackDto : CallBackDto
    {
        public ProcedureCallBackDto(string _object, string action, Guid? procedureId) : base(_object, action)
        {
            ProcedureId = procedureId;
        }

        public Guid? ProcedureId { get; set; }

        //На вход принимает строку ввида Object:Action:ProcedureId
        public static new ProcedureCallBackDto FromString(string input)
        {
            string[] strings = input.Split(':');
            Guid? guid = null;
            if (strings.Length > 2)
            {
                if (Guid.TryParse(strings[2], out Guid result))
                {
                    guid = result;
                }
            }
            return new ProcedureCallBackDto(strings[0], strings[1], guid);
        }

        public override string ToString()
        {
            return $"{base.ToString()}:{ProcedureId.ToString()}";
        }
    }
}

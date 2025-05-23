using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Enums
{
    public enum CreateProcedure
    {
        start, // начало
        waitDescription, // запрашиваем описание
        waitPrice, // запрашиваем цену услуги
        waitDuration // запрашиваем длительность услуги
    }
}

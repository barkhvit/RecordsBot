using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.CallBackModels
{
    public class AppointmentCallBackDto : CallBackDto
    {
        public AppointmentCallBackDto(string _object, string action, Guid? appointmentId) : base(_object, action)
        {
            AppointmentId = appointmentId;
        }

        public Guid? AppointmentId { get; set; }

        //На вход принимает строку ввида Object:Action:AppointmentId
        public static new AppointmentCallBackDto FromString(string input)
        {
            string[] strings = input.Split(':');
            Guid? toDoListId = null;
            if (strings.Length > 2)
            {
                if (Guid.TryParse(strings[2], out Guid guidId))
                {
                    toDoListId = guidId;
                }
            } 
            return new AppointmentCallBackDto(strings[0], strings[1], toDoListId);
        }

        public override string ToString()
        {
            return $"{base.ToString()}:{AppointmentId.ToString()}";
        }
    }
}

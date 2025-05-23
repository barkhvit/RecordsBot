using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime dateTime { get; set; }
        public bool isConfirmed { get; set; }
        public long UserId { get; set; }
        public int ProcedureId { get; set; }
    }
}

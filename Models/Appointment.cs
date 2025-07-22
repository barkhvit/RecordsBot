using RecordBot.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Models
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public DateTime dateTime { get; set; }
        public bool isConfirmed { get; set; }
        public Guid UserId { get; set; }
        public Guid ProcedureId { get; set; }
    }
}

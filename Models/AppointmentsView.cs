using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Models
{
    public class AppointmentsView
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsConfirmed { get; set; }
        public Guid? UserId { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public Guid ProcedureId { get; set; }
    }
}

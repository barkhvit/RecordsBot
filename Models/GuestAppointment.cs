using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Models
{
    public class GuestAppointment : IAppointment
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsConfirmed { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public Guid ProcedureId { get; set; }
    }
}

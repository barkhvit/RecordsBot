using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Models
{
    public interface IAppointment
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsConfirmed { get; set; }
        public Guid ProcedureId { get; set; }
    }
}

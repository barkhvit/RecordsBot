using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.DataAccess.Model
{
    [Table("guest_appointments")]
    public class GuestAppointmentModel
    {
        [PrimaryKey]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("datetime")]
        public DateTime DateTime { get; set; }

        [Column("isconfirmed")]
        public bool IsConfirmed { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("procedureid")]
        public Guid ProcedureId { get; set; }

        [Association(ThisKey = nameof(ProcedureId), OtherKey = nameof(ProcedureModel.Id))]
        public ProcedureModel Procedure { get; set; }
    }
}

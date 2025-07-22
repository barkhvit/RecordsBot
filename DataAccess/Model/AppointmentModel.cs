using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace RecordBot.DataAccess.Model
{
    [Table("appointments")]
    public class AppointmentModel
    {
        [PrimaryKey][Column("id")]public Guid Id { get; set; }
        [Column("datetime")] public DateTime dateTime { get; set; }
        [Column("isconfirmed")] public bool isConfirmed { get; set; }
        [Column("userid")] public Guid UserId { get; set; }
        [Column("procedureid")] public Guid ProcedureId { get; set; }

        [Association(ThisKey = nameof(ProcedureId), OtherKey = nameof(ProcedureModel.Id))]
        public ProcedureModel Procedure { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(UserModel.Id))]
        public UserModel User { get; set; }
    }
}

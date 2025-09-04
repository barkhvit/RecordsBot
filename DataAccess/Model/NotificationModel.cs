using System;
using System.Collections.Generic;
using LinqToDB.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.DataAccess.Model
{
    [Table("notification")]
    public class NotificationModel
    {
        [Column("id")] public Guid Id { get; set; }
        [Column("userid")] public Guid UserId { get; set; }
        [Column("type")] public string Type { get; set; }
        [Column("text")] public string Text { get; set; }
        [Column("scheduledat")] public DateTime ScheduledAt { get; set; }
        [Column("isnotified")] public bool IsNotified { get; set; }
        [Column("notifiedat")] public DateTime? NotifiedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(UserModel.Id))]
        public UserModel User { get; set; }
    }
}

using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.DataAccess.Model
{
    [Table("freeperiods")]
    public class FreePeriodModel
    {
        //freeperiodid, date, starttime, finishtime, duration
        [PrimaryKey][Column("freeperiodid")]public Guid FreePeriodId { get; set; }
        [Column("date")] public DateOnly Date { get; set; }
        [Column("starttime")] public TimeSpan StartTime { get; set; }
        [Column("finishtime")] public TimeSpan FinishTime { get; set; }
        [Column("duration")] public int Duration { get; set; }
    }
}

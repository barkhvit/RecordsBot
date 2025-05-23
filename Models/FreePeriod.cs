using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Models
{
    public class FreePeriod
    {
        private int _duration;
        public Guid FreePeriodId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly FinishTime { get; set; }
        public int Duration
        {
            get => _duration;
            set
            {
                TimeSpan tm = FinishTime - StartTime;
                _duration = (int)(tm.TotalMinutes);
            }
        }
    }
}

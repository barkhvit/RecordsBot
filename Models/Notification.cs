using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public User User { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public DateTime ScheduledAt { get; set; }
        public bool IsNotified { get; set; }
        public DateTime? NotifiedAt { get; set; }
    }
}

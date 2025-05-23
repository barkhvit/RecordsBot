using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public long TelegramId { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public UserState CurrentState { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}

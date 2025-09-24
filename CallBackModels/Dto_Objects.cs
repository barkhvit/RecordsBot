using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.CallBackModels
{
    public static class Dto_Objects
    {
        public static string MainMenu { get; } = nameof(MainMenu);
        public static string FreePeriod { get; } = nameof(FreePeriod);
        public static string AdminMenu { get; } = nameof(AdminMenu);
        public static string Appointment { get; } = nameof(Appointment);
        public static string Date { get; } = nameof(Date);
        public static string Proc { get; } = nameof(Proc);
        public static string MessageToAdmin { get; } = nameof(MessageToAdmin);
        public static string Notif { get; } = nameof(Notif);

    }
}

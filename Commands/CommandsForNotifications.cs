using RecordBot.CallBackModels;
using RecordBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RecordBot.Commands
{
    public class CommandsForNotifications : Commands
    {
        public CommandsForNotifications(ITelegramBotClient telegramBotClient, IAppointmentService appointmentService, IProcedureService procedureService)
            : base(telegramBotClient, appointmentService, procedureService)
        {
        }

        
    }
}

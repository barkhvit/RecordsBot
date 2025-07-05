using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace RecordBot.Services
{
    public static class MenuCommandsService
    {
        public static List<BotCommand> mainMenu = new List<BotCommand>
        {
            new(){Command = "info", Description = "информация"},
            new(){Command = "message", Description = "задать вопрос"},
            new(){Command = "myrecords", Description = "мои записи"},
            new(){Command = "procedures", Description = "процедуры"},
            new(){Command = "admin", Description = "управление"}
        };
    }
}

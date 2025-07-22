using RecordBot.Enums;
using RecordBot.Interfaces;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RecordBot.Handlers
{
    public class ReplyToMessageUpdateHandler : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IProcedureService _procedureService;


        //КОНСТРУКТОР
        public ReplyToMessageUpdateHandler(ITelegramBotClient _botClient, IProcedureService _procedureService)
        {
            this._botClient = _botClient;
            this._procedureService = _procedureService;
        }
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            
        }

        

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        
    }
}

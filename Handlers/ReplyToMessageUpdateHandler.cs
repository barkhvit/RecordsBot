using RecordBot.Enums;
using RecordBot.Interfaces;
using RecordBot.Models;
using RecordBot.Scenario;
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
        private readonly ICreateProcedureService _createProcedureService;

        //для процесса создания новой услуги
        private Dictionary<Guid, Enums.CreateProcedure> _userStates; // словарь для отслеживания статуса процесса создания услуги
        private Dictionary<Guid, Procedure> _newProcedures; // словарь для хранения Услуги перед тем как добавить

        //КОНСТРУКТОР
        public ReplyToMessageUpdateHandler(ITelegramBotClient _botClient, IProcedureService _procedureService, ICreateProcedureService _createProcedureService)
        {
            this._botClient = _botClient;
            this._procedureService = _procedureService;
            this._createProcedureService = _createProcedureService;
            _userStates = new Dictionary<Guid, Enums.CreateProcedure>();
            _newProcedures = new Dictionary<Guid, Procedure>();
        }
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message == null || update.Message.Text == null) throw new NullReferenceException();
            string[] text = update.Message.ReplyToMessage.Text.Split(':', 2);
            try
            {
                switch (text[0])
                {
                    case "Создание услуги": await CreateProcedureCommand(update, cancellationToken); break;
                }


                await Task.CompletedTask;
            }
            catch(Exception)
            {
                Console.WriteLine("Ошибка в обработчике ответов на сообщения");
                throw;
            }
        }

        private async Task CreateProcedureCommand(Update update, CancellationToken cancellationToken)
        {
            var status = await _createProcedureService.GetCreateProcedureStatus(update.Message.From.Id, cancellationToken);
            switch (status)
            {
                case CreateProcedureStatus.WaitName:
                    _createProcedureService.SetNewProcedure(update.Message.From.Id, cancellationToken, text: update.Message.Text);
                    await _botClient.SendMessage(update.Message.Chat.Id, 
                        text: "Создание услуги: введите описание",
                        replyMarkup: new ForceReplyMarkup { Selective = true, InputFieldPlaceholder = "описание услуги" },
                        cancellationToken: cancellationToken);
                    break;
                case CreateProcedureStatus.WaitDescription:
                    _createProcedureService.SetNewProcedure(update.Message.From.Id, cancellationToken, text: update.Message.Text);
                    await _botClient.SendMessage(update.Message.Chat.Id,
                        text: "Создание услуги: введите стоимость",
                        replyMarkup: new ForceReplyMarkup { Selective = true, InputFieldPlaceholder = "описание услуги" },
                        cancellationToken: cancellationToken);
                    break;
                case CreateProcedureStatus.WaitPrice:
                    if(int.TryParse(update.Message.Text,out int price))
                    {
                        _createProcedureService.SetNewProcedure(update.Message.From.Id, cancellationToken, number: price);
                        await _botClient.SendMessage(update.Message.Chat.Id,
                            text: "Создание услуги: введите длительность",
                            replyMarkup: new ForceReplyMarkup { Selective = true, InputFieldPlaceholder = "описание услуги" },
                            cancellationToken: cancellationToken);
                    }
                    break;
                case CreateProcedureStatus.WaitDuration://
                    if (int.TryParse(update.Message.Text, out int duration))
                    {
                        _createProcedureService.SetNewProcedure(update.Message.From.Id, cancellationToken, number: duration);
                        var procedure = await _createProcedureService.GetProcedure(update.Message.From.Id, cancellationToken);
                        if (procedure != null)
                        {
                            await _botClient.SendMessage(update.Message.Chat.Id,
                            text: $"Название: {procedure.Name}\nОписание: {procedure.Description}\n" +
                                    $"Стоимость: {procedure.Price} рублей\nДлительность:{procedure.DurationMinutes} минут.",
                            replyMarkup: Keyboards.Keyboards.GetAddNewProcedure(),
                            cancellationToken: cancellationToken);
                        }
                    }
                    break;
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        
    }
}

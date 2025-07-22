using RecordBot.Enums;
using RecordBot.Interfaces;
using RecordBot.Keyboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RecordBot.Commands
{
    public class CommandsForMainMenu : Commands
    {
        //private readonly string MainMenuPictures = "1.gif";
        public CommandsForMainMenu(ITelegramBotClient telegramBotClient, IAppointmentService appointmentService, IProcedureService procedureService) 
            : base(telegramBotClient, appointmentService, procedureService)
        {
        }

        public async Task ShowMainMenu(Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);
            
            string FolderPath = Path.Combine("Image");
            int filesCount = Directory.GetFiles(FolderPath).Length;
            int random = Random.Shared.Next(1, filesCount+1);
            string MainMenuPictures = $"{random.ToString()}.jpg";
            string PhotoPath = Path.Combine("Image", MainMenuPictures);

            switch (update.Type)
            {
                case UpdateType.Message:
                    await _telegramBotClient.SendPhoto(chatId,
                        photo: InputFile.FromStream(File.OpenRead(PhotoPath), PhotoPath),
                        cancellationToken: ct);
                    await _telegramBotClient.SendMessage(chatId, "Добро пожаловать! Выберите действие:",
                        cancellationToken: ct,
                        replyMarkup: KeyBoardsForMainMenu.MainMenu());
                    break;
                case UpdateType.CallbackQuery:
                    await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
                    await _telegramBotClient.EditMessageText(chatId, messageId, "Выберите действие:",
                        cancellationToken: ct,
                        replyMarkup: KeyBoardsForMainMenu.MainMenu());
                    break;
            }

        }

    }
}

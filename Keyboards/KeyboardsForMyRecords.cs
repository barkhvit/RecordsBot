using RecordBot.CallBackModels;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace RecordBot.Keyboards
{
    public static class KeyboardsForMyRecords
    {

        //показываем записи пользователя, если записей нет то две кнопки: "Главное меню" и "Записаться"
        internal static InlineKeyboardMarkup? GetShowMyRecordsKeybord(IReadOnlyList<Appointment>? records)
        {
            List<IEnumerable<InlineKeyboardButton>> buttons = new();
            if (records == null || !records.Any())
            {
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData("🏠 Главное меню",new CallBackDto("MainMenu","Show").ToString()),
                    InlineKeyboardButton.WithCallbackData("📅 Записаться",new CallBackDto("Procedure","ShowAllActiveForUser").ToString()),
                });
            }
            else
            {
                foreach (var r in records)
                {
                    string textButton = $"{r.dateTime.ToString("✅ dd.MM.yyyy HH:mm")}";
                    string callBackData = new AppointmentCallBackDto("Appointment", "Show", r.Id).ToString();
                    buttons.Add(new[]
                    {
                    InlineKeyboardButton.WithCallbackData(textButton,callBackData)
                    });

                }
                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData("🏠 Главное меню",new CallBackDto("MainMenu","Show").ToString())
                });
            }
            return new InlineKeyboardMarkup(buttons);
        }
    }
}

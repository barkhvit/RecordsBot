using RecordBot.CallBackModels;
using RecordBot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace RecordBot.Keyboards
{
    public static class KeyBoardsForMainMenu
    {
        public static InlineKeyboardMarkup MainMenu()
        {
            var buttons = new List<InlineKeyboardButton[]>();

            buttons.Add(new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("✂️ Услуги ",new CallBackDto("Procedure","ShowAllActiveForUser").ToString()),
                InlineKeyboardButton.WithCallbackData("📝 Мои записи  ",new CallBackDto("Appointment","ShowAll").ToString())
            });
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("✉️ Написать администратору", new CallBackDto("MessageToAdmin","Create").ToString())
            });
            return new InlineKeyboardMarkup(buttons);
        }

        public static InlineKeyboardMarkup BackToMainMenu()
        {
            List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();
            var row1 = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Меню админа", new CallBackDto("AdminMenu", "Show").ToString()),
                InlineKeyboardButton.WithCallbackData("Меню пользователя", new CallBackDto("MainMenu", "Show").ToString())
            };
            
            buttons.Add(row1);

            return new InlineKeyboardMarkup(buttons);
        }
    }
}

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
            return new InlineKeyboardMarkup(buttons);
        }
    }
}

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
                InlineKeyboardButton.WithCallbackData("✂️ Услуги ",new CallBackDto(Dto_Objects.Proc,Dto_Action.Proc_ShowAllActiveForUser).ToString()),
                InlineKeyboardButton.WithCallbackData("📝 Мои записи  ",new CallBackDto(Dto_Objects.Appointment,Dto_Action.App_ShowAll).ToString())
            });
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("✉️ Написать администратору", new CallBackDto(Dto_Objects.MessageToAdmin,Dto_Action.MTA_Create).ToString())
            });
            return new InlineKeyboardMarkup(buttons);
        }

        public static InlineKeyboardMarkup BackToMainMenu()
        {
            List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();
            var row1 = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("Меню админа", new CallBackDto(Dto_Objects.AdminMenu,Dto_Action.AM_Show).ToString()),
                InlineKeyboardButton.WithCallbackData("Меню пользователя", new CallBackDto(Dto_Objects.MainMenu,Dto_Action.MM_Show).ToString())
            };
            
            buttons.Add(row1);

            return new InlineKeyboardMarkup(buttons);
        }
    }
}

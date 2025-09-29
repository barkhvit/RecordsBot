using Microsoft.VisualBasic;
using RecordBot.CallBackModels;
using RecordBot.Enums;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace RecordBot.Keyboards
{
    public static class Keyboards
    {
        
        //клавиатура выбора даты: "<--" "date" "-->"
        public static InlineKeyboardMarkup GetInlineDate(DateOnly date)
        {
            var buttons = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("⬅️"){CallbackData = new CallBackDto(Dto_Objects.Date,Dto_Action.Date_Show,date:date.AddDays(-1)).ToString()},
                new InlineKeyboardButton(date.ToString("dd.MM.yyyy")){CallbackData = new CallBackDto(Dto_Objects.Date,Dto_Action.Date_Select,date:date).ToString()},
                new InlineKeyboardButton("➡️"){CallbackData = new CallBackDto(Dto_Objects.Date,Dto_Action.Date_Show,date:date.AddDays(1)).ToString()}
            };
            return new InlineKeyboardMarkup(buttons);
        }

        //Администрирование дат, доступных для резервирования
        public static InlineKeyboardMarkup GetDateAdminKeybord()
        {
            List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();
            buttons.Add(new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("➕ Добавить день") { CallbackData = new CallBackDto(Dto_Objects.Date,Dto_Action.Date_Show,date:DateOnly.FromDateTime(DateTime.Today)).ToString() },//добавить дату GetInlineDate
                    new InlineKeyboardButton("➕ Добавить период") { CallbackData = new CallBackDto(Dto_Objects.FreePeriod,Dto_Action.FP_Create).ToString()} // добавить произвольный период
                    
                });

            buttons.Add(new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("👀 Посмотреть ") { CallbackData = new CallBackDto(Dto_Objects.FreePeriod,Dto_Action.FP_Show).ToString()}, // показать все даты, доступные для резервирования
                    new InlineKeyboardButton("⬅️ Назад") { CallbackData = new CallBackDto(Dto_Objects.AdminMenu,Dto_Action.AM_Show).ToString() },//назад в меню админа
                });

            return new InlineKeyboardMarkup(buttons);
        }
    }
}


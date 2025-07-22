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
                new InlineKeyboardButton("⬅️"){CallbackData = new DateCallBackDto("Date","Show",date.AddDays(-1)).ToString()},
                new InlineKeyboardButton(date.ToString("dd.MM.yyyy")){CallbackData = new DateCallBackDto("Date","Select",date).ToString()},
                new InlineKeyboardButton("➡️"){CallbackData = new DateCallBackDto("Date","Show",date.AddDays(1)).ToString()}
            };
            return new InlineKeyboardMarkup(buttons);
        }

        //Администрирование дат, доступных для резервирования
        public static InlineKeyboardMarkup GetDateAdminKeybord()
        {
            List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();
            buttons.Add(new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("➕ Добавить день") { CallbackData = new DateCallBackDto("Date","Show",DateOnly.FromDateTime(DateTime.Today)).ToString() },//добавить дату GetInlineDate
                    new InlineKeyboardButton("➕ Добавить период") { CallbackData = new CallBackDto("FreePeriod", "Create").ToString()} // добавить произвольный период
                    
                });

            buttons.Add(new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton("👀 Посмотреть ") { CallbackData = new CallBackDto("FreePeriod","Show").ToString()}, // показать все даты, доступные для резервирования
                    new InlineKeyboardButton("⬅️ Назад") { CallbackData = new CallBackDto("AdminMenu", "Show").ToString() },//назад в меню админа
                });

            return new InlineKeyboardMarkup(buttons);
        }


        //клавиатура администрирования новой услуги
        public static InlineKeyboardMarkup GetAddNewProcedure()
        {
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("добавить"){CallbackData = $"adminprocedure_addnew"},
                new InlineKeyboardButton("удалить"){CallbackData = $"adminprocedure_deletenew"}
            };
            return new InlineKeyboardMarkup(buttons);
        }

        
        //формирует клавиатуру для выбора дат, даты должны идти в строку по три
        public static InlineKeyboardMarkup GetKeybordDates(IEnumerable<DateOnly> datesForReserve, ReasonShowDates reasonShowDates)
        {
            // в зависимости от reasonShowDates готовим callBackData
            string callBackData = reasonShowDates switch
            {
                ReasonShowDates.ForReservedProcedures => "DateForReserved",
                _ => ""
            };
            // Преобразуем даты в кнопки с callback данными
            var buttons = datesForReserve.Select(date => InlineKeyboardButton.WithCallbackData(
                text: date.ToString("dd.MM.yyyy "),
                callbackData: $"{callBackData}:{date}")).ToList();

            //группируем в ряд по три
            var keyboardRows = new List<IEnumerable<InlineKeyboardButton>>();
            for (int i = 0; i < buttons.Count; i += 3)
            {
                keyboardRows.Add(buttons.Skip(i).Take(3));
            }
            // Добавляем дополнительную кнопку (например, "Назад")
            keyboardRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("Отменить", new CallBackDto("Procedure","ShowAllActiveForUser").ToString())
            });

            return new InlineKeyboardMarkup(keyboardRows);
        }

        internal static InlineKeyboardMarkup GetKeybordTimes(List<TimeOnly> timesForReserve, ReasonShowTimes reasonShowTimes, Guid procedureId)
        {
            //в зависимости от ReasonShowTimes готовим CallBackData
            string callBackData = reasonShowTimes switch
            {
                ReasonShowTimes.ForReservedProcedures => "TimeForReserved",
                _ => ""
            };
            // Преобразуем даты в кнопки с callback данными
            var buttons = timesForReserve.Select(time => InlineKeyboardButton.WithCallbackData(
                text: time.ToString("HH:mm"),
                callbackData: $"{callBackData}:{time}")).ToList();

            //группируем в ряд по три
            var keyboardRows = new List<IEnumerable<InlineKeyboardButton>>();
            for (int i = 0; i < buttons.Count; i += 3)
            {
                keyboardRows.Add(buttons.Skip(i).Take(3));
            }
            // Добавляем дополнительную кнопку (например, "Назад")
            keyboardRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("Отменить", new CallBackDto("Procedure","ShowAllActiveForUser").ToString()),
                InlineKeyboardButton.WithCallbackData("Изменить дату", $"reservedOnProcedure:{procedureId.ToString()}")
            });

            return new InlineKeyboardMarkup(keyboardRows);
        }

        //клавиатура подтверждения записи
        internal static InlineKeyboardMarkup GetKeybordConfirmReserved()
        {
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("Отменить"){CallbackData = new CallBackDto("Procedure","ShowAllActiveForUser").ToString()},
                new InlineKeyboardButton("ПОДТВЕРДИТЬ"){CallbackData = "reservedOnProcedureDone"}
            };
            return new InlineKeyboardMarkup(buttons);
        }
    }
}


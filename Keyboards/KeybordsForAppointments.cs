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
    public static class KeybordsForAppointments
    {
        public static InlineKeyboardMarkup GetAllProcedures(IReadOnlyList<Procedure> procedures, ReasonShowProcedure reasonShowProcedure)
        {
            string callBackData = reasonShowProcedure == ReasonShowProcedure.admin ? "SDFA" : "SDFU";
            List<InlineKeyboardButton[]> buttons = new();
            List<InlineKeyboardButton> row = new();
            int i = 0;
            foreach (Procedure procedure in procedures)
            {
                if (procedure.Name != null)
                {
                    row.Add(
                    new InlineKeyboardButton(procedure.Name) { CallbackData = new ProcedureCallBackDto("Procedure", callBackData, procedure.Id).ToString() }
                    );
                }
                i++;
                if (i == 2 || i == procedures.Count) // чтобы было два в ряд
                {
                    buttons.Add(row.ToArray());
                    row = new List<InlineKeyboardButton>();
                }
            }

            
           buttons.Add(new InlineKeyboardButton[]
           {
               InlineKeyboardButton.WithCallbackData("Отменить","cancel")
           });

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
                InlineKeyboardButton.WithCallbackData("Отменить", "cancel")
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
                callbackData: $"{callBackData}:{time.ToString("HH.mm")}")).ToList();

            //группируем в ряд по три
            var keyboardRows = new List<IEnumerable<InlineKeyboardButton>>();
            for (int i = 0; i < buttons.Count; i += 3)
            {
                keyboardRows.Add(buttons.Skip(i).Take(3));
            }
            // Добавляем дополнительную кнопку (например, "Назад")
            keyboardRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("Отменить", "cancel")
            });

            return new InlineKeyboardMarkup(keyboardRows);
        }
    }
}

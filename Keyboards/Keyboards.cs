using Microsoft.VisualBasic;
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
        //клавиатура после выбора команды /admin - админская клавиатура
        public static InlineKeyboardMarkup GetAdminKeybord()
        {
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("Даты работы") { CallbackData = $"admin:dates" },
                new InlineKeyboardButton("Услуги") { CallbackData = $"admin:procedures" },
                new InlineKeyboardButton("Окна") { CallbackData = $"admin:freeperiod" },
            };
            return new InlineKeyboardMarkup(buttons);
        }

        //клавиатура выбора даты: "<--" "date" "-->"
        public static InlineKeyboardMarkup GetInlineDate(DateOnly date)
        {
            var buttons = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("<--"){CallbackData = $"admindate_showdate:{date.AddDays(-1).ToString("dd.MM.yyyy")}"},
                new InlineKeyboardButton(date.ToString("dd.MM.yyyy")){CallbackData = $"admindate_selectdate:{date.ToString("dd.MM.yyyy")}"},
                new InlineKeyboardButton("-->"){CallbackData = $"admindate_showdate:{date.AddDays(1).ToString("dd.MM.yyyy")}"}
            };
            return new InlineKeyboardMarkup(buttons);
        }

        //Администрирование дат, доступных для резервирования
        public static InlineKeyboardMarkup GetDateAdminKeybord()
        {
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("добавить") { CallbackData = $"admindate_showdate:{DateTime.Today.ToString("dd.MM.yyyy")}" },//добавить дату GetInlineDate
                new InlineKeyboardButton("посмотреть") { CallbackData = $"AllDate" } // показать все даты, доступные для резервирования
            };
            return new InlineKeyboardMarkup(buttons);
        }

        //показать все даты, доступные для резервирования
        public static InlineKeyboardMarkup GetAllDateKeybord(IReadOnlyList<DateOnly> dates)
        {
            return new InlineKeyboardMarkup();
        }

        //клавиатура администрирования Услуг(Procedure), появляется после нажатия: /admin --> Услуги
        public static InlineKeyboardMarkup GetProcedureAdminKeybord()
        {
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("создать"){CallbackData = $"adminprocedure_add"},
                new InlineKeyboardButton("удалить"){CallbackData = $"adminprocedure_delete"},
                new InlineKeyboardButton("показать"){CallbackData = $"adminprocedure_show"}
            };
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
    }
}

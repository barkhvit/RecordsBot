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
    public static class KeyboardsForProcedures
    {
        //клавиатура после выбора команды /admin - админская клавиатура
        public static InlineKeyboardMarkup GetAdminKeybord()
        {
            List<InlineKeyboardButton[]> btn = new List<InlineKeyboardButton[]>();
            btn.Add(new[]
            {
                new InlineKeyboardButton("✂️ Услуги") { CallbackData = new CallBackDto("Procedure","Admin").ToString()},
                new InlineKeyboardButton("📅 Периоды работы  ") { CallbackData = new CallBackDto("FreePeriod","Admin").ToString()}
            });
            btn.Add(new[]
            {
                new InlineKeyboardButton("🗒️ Записи ") {CallbackData = new CallBackDto("Appointment","ShowAdminMenu").ToString()}
            });

            return new InlineKeyboardMarkup(btn);
        }

        //клавиатура для редактирования процедуры (кнопка Назад(показать все процедуры), кнопка сделать акт/неакт)
        internal static InlineKeyboardMarkup? GetKeybordForProcedure(Procedure procedure)
        {
            string textButton = procedure.isActive == true ? "в архив" : "сделать активной";
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("⬅️ Назад"){CallbackData = new CallBackDto("Procedure","ShowAllActiveForAdmin").ToString()},
                new InlineKeyboardButton(textButton){CallbackData = new ProcedureCallBackDto("Procedure","ChangeActive",procedure.Id).ToString()}
            };
            return new InlineKeyboardMarkup(buttons);
        }

        //клавиатура администрирования Услуг(Procedure), появляется после нажатия: /admin --> Услуги
        public static InlineKeyboardMarkup GetProcedureAdminKeybord()
        {
            var buttons = new List<InlineKeyboardButton[]>();
            InlineKeyboardButton[] row1 = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("⬅️ Назад "){CallbackData = new CallBackDto("AdminMenu", "Show").ToString()},
                new InlineKeyboardButton("➕ Cоздать "){CallbackData = new CallBackDto("Procedure", "Create").ToString()}
            };

            InlineKeyboardButton[] row2 = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("👀 Показать"){CallbackData = new CallBackDto("Procedure","ShowAllActiveForAdmin").ToString()},
                new InlineKeyboardButton("🗄 Архив"){CallbackData = new CallBackDto("Procedure","ShowAllArchiveForAdmin").ToString()}
            };
            buttons.Add(row1);
            buttons.Add(row2);

            return new InlineKeyboardMarkup(buttons);
        }

        //создание InlineKeyboard из списка услуг
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
                    new InlineKeyboardButton(procedure.Name) { CallbackData = new ProcedureCallBackDto("Procedure", callBackData,procedure.Id).ToString() }
                    );
                }
                i++;
                if (i == 2 || i == procedures.Count) // чтобы было два в ряд
                {
                    buttons.Add(row.ToArray());
                    row = new List<InlineKeyboardButton>();
                }
            }
            
            if(reasonShowProcedure == ReasonShowProcedure.reserved) //если обычный пользователь
            {
                buttons.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("🏠 Главное меню",new CallBackDto("MainMenu","Show").ToString())
                });
            }
            else // если администратор
            {
                buttons.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Меню администратора",new CallBackDto("AdminMenu","Show").ToString())
                });
            }

            return new InlineKeyboardMarkup(buttons);
        }

        //клавиатура для записи на процедуру (кнопка Назад(показать все процедуры), кнопка ЗАПИСАТЬСЯ)
        internal static InlineKeyboardMarkup? GetKeybordForReserved(Procedure procedure)
        {
            string textButton = "📅 ЗАПИСАТЬСЯ  ";
            //string callBackData = $"reservedOnProcedure:{procedure.Id}";
            string callBackData = new ProcedureCallBackDto("Procedure", "CreateAppointment", procedure.Id).ToString();
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("⬅️ Назад"){CallbackData = new CallBackDto("Procedure","ShowAllActiveForUser").ToString()},
                new InlineKeyboardButton(textButton){CallbackData = callBackData}
            };
            return new InlineKeyboardMarkup(buttons);
        }
    }
}

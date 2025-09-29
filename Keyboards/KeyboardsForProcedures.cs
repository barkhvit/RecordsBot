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
                new InlineKeyboardButton("✂️ Услуги") { CallbackData = new CallBackDto(Dto_Objects.Proc,Dto_Action.Proc_Admin).ToString()},
                new InlineKeyboardButton("📅 Периоды работы  ") { CallbackData = new CallBackDto(Dto_Objects.FreePeriod,Dto_Action.FP_Admin).ToString()}
            });
            btn.Add(new[]
            {
                new InlineKeyboardButton("🗒️ Записи ") {CallbackData = new CallBackDto(Dto_Objects.Appointment,Dto_Action.App_ShowAdminMenu).ToString()}
            });

            return new InlineKeyboardMarkup(btn);
        }

        //клавиатура для редактирования процедуры (кнопка Назад(показать все процедуры), кнопка сделать акт/неакт)
        internal static InlineKeyboardMarkup? GetKeybordForProcedure(Procedure procedure)
        {
            string textButton = procedure.isActive == true ? "в архив" : "сделать активной";
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("⬅️ Назад"){CallbackData = new CallBackDto(Dto_Objects.Proc,Dto_Action.Proc_ShowAllActiveForAdmin).ToString()},
                new InlineKeyboardButton(textButton){CallbackData = new CallBackDto(Dto_Objects.Proc,Dto_Action.Proc_ChangeActive,procedure.Id).ToString()}
            };
            return new InlineKeyboardMarkup(buttons);
        }

        //клавиатура администрирования Услуг(Proc), появляется после нажатия: /admin --> Услуги
        public static InlineKeyboardMarkup GetProcedureAdminKeybord()
        {
            var buttons = new List<InlineKeyboardButton[]>();
            InlineKeyboardButton[] row1 = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("⬅️ Назад "){CallbackData = new CallBackDto(Dto_Objects.AdminMenu,Dto_Action.AM_Show).ToString()},
                new InlineKeyboardButton("➕ Cоздать "){CallbackData = new CallBackDto(Dto_Objects.Proc, Dto_Action.Proc_Create).ToString()}
            };

            InlineKeyboardButton[] row2 = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("👀 Показать"){CallbackData = new CallBackDto(Dto_Objects.Proc,Dto_Action.Proc_ShowAllActiveForAdmin).ToString()},
                new InlineKeyboardButton("🗄 Архив"){CallbackData = new CallBackDto(Dto_Objects.Proc,Dto_Action.Proc_ShowAllArchiveForAdmin).ToString()}
            };
            buttons.Add(row1);
            buttons.Add(row2);

            return new InlineKeyboardMarkup(buttons);
        }

        //создание InlineKeyboard из списка услуг
        public static InlineKeyboardMarkup GetAllProcedures(IReadOnlyList<Procedure> procedures, ReasonShowProcedure reasonShowProcedure)
        {
            string callBackData = reasonShowProcedure == ReasonShowProcedure.admin ? nameof(Dto_Action.Proc_SA) : nameof(Dto_Action.Proc_SU);
            List<InlineKeyboardButton[]> buttons = new();
            List<InlineKeyboardButton> row = new();
            int i = 0;
            foreach (Procedure procedure in procedures)
            {
                if (procedure.Name != null)
                {
                    row.Add(
                    new InlineKeyboardButton(procedure.Name) { CallbackData = new CallBackDto(Dto_Objects.Proc, callBackData,procedure.Id).ToString() }
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
                    InlineKeyboardButton.WithCallbackData("🏠 Главное меню",new CallBackDto(Dto_Objects.MainMenu,Dto_Action.MM_Show).ToString())
                });
            }
            else // если администратор
            {
                buttons.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Меню администратора",new CallBackDto(Dto_Objects.AdminMenu,Dto_Action.AM_Show).ToString())
                });
            }

            return new InlineKeyboardMarkup(buttons);
        }

        //клавиатура для записи на процедуру (кнопка Назад(показать все процедуры), кнопка ЗАПИСАТЬСЯ)
        internal static InlineKeyboardMarkup? GetKeybordForReserved(Procedure procedure)
        {
            string textButton = "📅 ЗАПИСАТЬСЯ  ";
            //string callBackData = $"reservedOnProcedure:{procedure.Id}";
            string callBackData = new CallBackDto(Dto_Objects.Proc, Dto_Action.Proc_CreateAppointment, procedure.Id).ToString();
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[]
            {
                new InlineKeyboardButton("⬅️ Назад"){CallbackData = new CallBackDto(Dto_Objects.Proc,Dto_Action.Proc_ShowAllActiveForUser).ToString()},
                new InlineKeyboardButton(textButton){CallbackData = callBackData}
            };
            return new InlineKeyboardMarkup(buttons);
        }
    }
}

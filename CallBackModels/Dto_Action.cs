using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.CallBackModels
{
    public static class Dto_Action
    {
        //MainMenu
        public static string MM_Show { get; } = nameof(MM_Show);

        //FreePeriod
        public static string FP_Admin { get; } = nameof(FP_Admin);
        public static string FP_Show { get; } = nameof(FP_Show);
        public static string FP_Create { get; } = nameof(FP_Create);

        //AdminMenu
        public static string AM_Show { get; } = nameof(AM_Show);

        //Appointment
        public static string App_Cancel { get; } = nameof(App_Cancel);
        public static string App_ShowAll { get; } = nameof(App_ShowAll);
        public static string App_Delete { get; } = nameof(App_Delete);
        public static string App_Show { get; } = nameof(App_Show);
        public static string App_ShowAdminMenu { get; } = nameof(App_ShowAdminMenu);
        public static string App_ShowForAdminDates { get; } = nameof(App_ShowForAdminDates);
        public static string App_ShowByDate { get; } = nameof(App_ShowByDate);
        public static string App_EditAdmin { get; } = nameof(App_EditAdmin);
        public static string App_Cf { get; } = nameof(App_Cf); //подтвердить запись

        //Date
        public static string Date_Show { get; } = nameof(Date_Show);
        public static string Date_Select { get; } = nameof(Date_Select);

        //Proc
        public static string Proc_Admin { get; } = nameof(Proc_Admin);
        public static string Proc_ShowAllActiveForAdmin { get; } = nameof(Proc_ShowAllActiveForAdmin);
        public static string Proc_ShowAllActiveForUser { get; } = nameof(Proc_ShowAllActiveForUser);
        public static string Proc_ShowAllArchiveForAdmin { get; } = nameof(Proc_ShowAllArchiveForAdmin);
        public static string Proc_SA { get; } = nameof(Proc_SA);//Show Detial For Admin
        public static string Proc_SU { get; } = nameof(Proc_SU);//Show Detail For User
        public static string Proc_ChangeActive { get; } = nameof(Proc_ChangeActive);
        public static string Proc_Create { get; } = nameof(Proc_Create);
        public static string Proc_CreateAppointment { get; } = nameof(Proc_CreateAppointment);
        

        //MessageToAdmin
        public static string MTA_Create { get; } = nameof(MTA_Create);

        //Notification
        public static string Not_TA { get; } = nameof(Not_TA); //Tommorrow Appointment (завтра у вас запись)
        
    }
}

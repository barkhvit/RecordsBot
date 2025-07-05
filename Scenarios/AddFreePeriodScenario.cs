using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RecordBot.Scenarios
{
    public class AddFreePeriodScenario : IScenario
    {

        //ШАГИ:
        //null - получаем пользователя, context.Data["User"] = user, context.CurrentStep = "Дата";
        //Дата - проверяем дату, context.Data["Дата"] = DateOnly, context.CurrentStep = "Начало";
        //Начало - проверяем время, context.Data["Начало"] = TimeOnly, context.CurrentStep = "Окончание";
        //Окончание - проверяем время, context.Data["Окончание"] = TimeOnly,

        public bool CanHandle(ScenarioType scenarioType) => scenarioType == ScenarioType.AddPeriod;



        public Task<ScenarioResult> HandleScenarioAsync(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Scenarios
{
    public interface IScenarioContextRepository
    {
        Task<ScenarioContext?> GetContext(long UserId, CancellationToken ct);
        Task SetContext(long UserId, ScenarioContext context, CancellationToken ct);
        Task ResetContext(long UserId, CancellationToken ct);
    }
}

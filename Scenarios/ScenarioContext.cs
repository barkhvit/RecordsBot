using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Scenarios
{
    public class ScenarioContext
    {
        public long UserId { get; }
        public ScenarioType CurrentScenario { get; }
        public string? CurrentStep { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        public ScenarioContext(long userId, ScenarioType scenarioType)
        {
            UserId = userId;
            CurrentScenario = scenarioType;
        }
        
    }
}

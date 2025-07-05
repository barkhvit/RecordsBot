using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Scenarios
{
    public class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly ConcurrentDictionary<long, ScenarioContext> _contexts = new();

        public Task<ScenarioContext?> GetContext(long UserId, CancellationToken ct)
        {
            _contexts.TryGetValue(UserId, out var context);
            return Task.FromResult(context);
        }

        public Task ResetContext(long UserId, CancellationToken ct)
        {
            _contexts.Remove(UserId, out ScenarioContext? context);
            return Task.CompletedTask;
        }

        public Task SetContext(long UserId, ScenarioContext context, CancellationToken ct)
        {
            _contexts[UserId] = context;
            return Task.CompletedTask;
        }
    }
}

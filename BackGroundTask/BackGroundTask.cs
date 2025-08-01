using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.BackGroundTask
{
    public abstract class BackGroundTask(TimeSpan delay, string name) : IBackGroundTask
    {
        protected abstract Task Execute(CancellationToken ct);
        public async Task Start(CancellationToken ct)
        {
            while(!ct.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine($"{name}.Execute");
                    await Execute(ct);

                    Console.WriteLine($"{name}. Start delay {delay}");
                    await Task.Delay(delay, ct);
                }
                catch(OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    // нормально закрываемся при отмене
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"{name}. Error: {ex}");
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                }
            }
        }
    }
}

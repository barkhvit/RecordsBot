using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.BackGroundTask
{
    public class BackGroundTaskRunner : IDisposable
    {
        private readonly ConcurrentBag<IBackGroundTask> _tasks = new();
        private Task? _runningTasks;
        private CancellationTokenSource? _stoppingCts;

        public void AddTask(IBackGroundTask task)
        {
            if(_runningTasks is not null)
            {
                throw new InvalidOperationException("Задача уже запущена");
            }
            _tasks.Add(task);
        }

        public void StartTasks(CancellationToken ct)
        {
            if (_runningTasks is not null)
                throw new InvalidOperationException("Задача уже запущена");

            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            // Отдельная обёртка для логирования и корректной обработки отмены
            static async Task RunSafe(IBackGroundTask task, CancellationToken ct)
            {
                try
                {
                    await task.Start(ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    // нормально завершаемся при отмене
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error in {task.GetType().Name}: {ex}");
                }
            }

            // Собираем все таски в один
            _runningTasks = Task.WhenAll(_tasks.Select(t => RunSafe(t, _stoppingCts.Token)));
        }

        /// <summary>
        /// Останавливает запущенные задачи и и ожидает из завершения
        /// </summary>
        public async Task StopTasks(CancellationToken ct)
        {
            if (_runningTasks is null)
                return;

            try
            {
                _stoppingCts?.Cancel();
            }
            finally
            {
                await _runningTasks.WaitAsync(ct).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            }
        }

        public void Dispose()
        {
            _stoppingCts?.Cancel();
            _stoppingCts?.Dispose();
        }
    }
}

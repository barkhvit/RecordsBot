using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.BackGroundTask
{
    public interface IBackGroundTask
    {
        Task Start(CancellationToken ct);
    }
}

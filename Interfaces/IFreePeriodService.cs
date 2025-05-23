using RecordBot.Models;
using RecordBot.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Interfaces
{
    public interface IFreePeriodService
    {
        public Task<bool> Add(FreePeriod freePeriod, CancellationToken cancellationToken);
        public Task<IReadOnlyList<FreePeriod>> GetAllPeriods(CancellationToken cancellationToken);
    }
}

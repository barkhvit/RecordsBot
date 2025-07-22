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
        Task<bool> Add(FreePeriod freePeriod, CancellationToken cancellationToken);
        Task<IReadOnlyList<FreePeriod>> GetAllPeriods(CancellationToken cancellationToken);
        Task<IReadOnlyList<DateOnly>> GetDates(CancellationToken cancellationToken);
        Task<IReadOnlyList<DateTime>> GetDateTimeForReserved(Procedure procedure, CancellationToken cancellationToken);
        Task<FreePeriod?> GetFreePeriodForReserved(Procedure procedure, DateTime dateTimeAppointment, CancellationToken ct);
        Task<bool> SplitPeriod(FreePeriod freePeriod, DateTime dateTime, int duration, CancellationToken ct);
        Task Delete(FreePeriod freePeriod, CancellationToken ct);
        Task<FreePeriod?> GetMergePeriod(FreePeriod freePeriod, CancellationToken ct);
        Task<IReadOnlyList<FreePeriod>> GetPeriodsByDate(DateOnly dateOnly, CancellationToken ct);
    }
}

﻿using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Interfaces
{
    public interface IFreePeriodRepository
    {
        Task Add(FreePeriod freePeriod,CancellationToken cancellationToken);
        Task<IReadOnlyList<FreePeriod>> GetAllPeriods(CancellationToken cancellationToken);
        Task<IReadOnlyList<DateOnly>> GetDates(CancellationToken cancellationToken);
        Task<bool> SplitPeriod(FreePeriod freePeriod, DateTime dateTime, int duration, CancellationToken ct);
        Task<bool> Delete(FreePeriod freePeriod, CancellationToken ct);
        Task<IReadOnlyList<FreePeriod>> GetPeriodsByDate(DateOnly dateOnly, CancellationToken ct);
    }
}

using RecordBot.Interfaces;
using RecordBot.Models;
using RecordBot.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecordBot.Services
{
    public class FreePeriodService : IFreePeriodService
    {
        private readonly IFreePeriodRepository _jsonFreePeriodRepository;
        public FreePeriodService(IFreePeriodRepository _jsonFreePeriodRepository)
        {
            this._jsonFreePeriodRepository = _jsonFreePeriodRepository;
        }

        public async Task<bool> Add(FreePeriod freePeriod,CancellationToken cancellationToken)
        {
            var dates = await _jsonFreePeriodRepository.GetDates(cancellationToken);
            if (!dates.Contains(freePeriod.Date))
            {
                await _jsonFreePeriodRepository.Add(freePeriod, cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<IReadOnlyList<FreePeriod>> GetAllPeriods(CancellationToken cancellationToken)
        {
            return await _jsonFreePeriodRepository.GetAllPeriods(cancellationToken);
        }
    }
}

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

        public async Task<bool> Add(FreePeriod freePeriod,CancellationToken ct)
        {
            var periodForAdd = await MergePeriods(freePeriod, ct);
            if (periodForAdd != null)
            {
                await _jsonFreePeriodRepository.Add(periodForAdd, ct);
                return true;
            }
            return false;
        }

        public async Task Delete(FreePeriod? freePeriod, CancellationToken ct)
        {
            if(freePeriod!=null) await _jsonFreePeriodRepository.Delete(freePeriod, ct);
        }

        public async Task<IReadOnlyList<FreePeriod>> GetAllPeriods(CancellationToken cancellationToken)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            var allFreePeriods =  await _jsonFreePeriodRepository.GetAllPeriods(cancellationToken);
            return allFreePeriods.Where(p => p.Date >= today).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<DateOnly>> GetDates(CancellationToken cancellationToken)
        {
            var freePeriods = await GetAllPeriods(cancellationToken);
            var dates = freePeriods.Select(d => d.Date).ToList().AsReadOnly();
            return dates;
        }

        //выводит в формате DateTime возможные слоты для бронирования, но не ранее, чем текущее время
        public async Task<IReadOnlyList<DateTime>> GetDateTimeForReserved(Procedure procedure, CancellationToken cancellationToken)
        {
            List<DateTime> dateTimes = new List<DateTime>();
            DateTime dateTimeNow = DateTime.UtcNow;

            var freePeriods = await GetAllPeriods(cancellationToken);
            foreach (FreePeriod freePeriod in freePeriods)
            {
                DateTime currentDateTime = new DateTime(freePeriod.Date, freePeriod.StartTime);
                while (currentDateTime.AddMinutes(procedure.DurationMinutes) <= new DateTime(freePeriod.Date, freePeriod.FinishTime))
                {
                    dateTimes.Add(currentDateTime);
                    currentDateTime = currentDateTime.AddMinutes(procedure.DurationMinutes);
                }
            }
            return dateTimes.Where(d => d >= dateTimeNow).OrderBy(d => d).ToList().AsReadOnly();
        }

        public async Task<FreePeriod?> GetFreePeriodForReserved(Procedure procedure, DateTime dateTimeAppointment, CancellationToken ct)
        {
            TimeOnly appFinishTime = TimeOnly.FromDateTime(dateTimeAppointment).AddMinutes(procedure.DurationMinutes);
            var periods = await GetAllPeriods(ct);
            var period = periods.FirstOrDefault(p =>
                p.Date == DateOnly.FromDateTime(dateTimeAppointment) &&
                p.StartTime <= TimeOnly.FromDateTime(dateTimeAppointment) &&
                p.FinishTime >= appFinishTime);
            return period;
        }

        public async Task<bool> SplitPeriod(FreePeriod freePeriod, DateTime dateTime, int duration, CancellationToken ct)
        {
            return await _jsonFreePeriodRepository.SplitPeriod(freePeriod, dateTime, duration, ct);
        }

        //проверяем на пересечение и объединяем если нужно
        private async Task<FreePeriod> MergePeriods(FreePeriod freePeriod, CancellationToken ct)
        {
            //получаем все периоды той же датой что и freePeriod
            var allPeriods = await _jsonFreePeriodRepository.GetAllPeriods(ct);
            var periods = allPeriods.Where(p => p.Date == freePeriod.Date).ToList();

            //если на эту дату нет периодов то возвращаем freePeriod
            if (periods == null) return freePeriod;

            //проверяем пересечения
            var mergePeriods = periods.Where(p => 
                (freePeriod.StartTime >= p.StartTime && freePeriod.StartTime <= p.FinishTime) || (freePeriod.FinishTime >= p.StartTime && freePeriod.StartTime <= p.FinishTime))
                .ToList();
            if(mergePeriods==null || mergePeriods.Count==0) return freePeriod;

            
            //добавляем период в mergePeriods и создаем объединенный период
            mergePeriods = mergePeriods.Append(freePeriod).ToList();
            var startTime = mergePeriods.Select(p => p.StartTime).Min();
            var finishTime = mergePeriods.Select(p => p.FinishTime).Max();


            //сздаем новый период
            FreePeriod newPeriod = new FreePeriod
            {
                FreePeriodId = Guid.NewGuid(),
                Date = freePeriod.Date,
                StartTime = startTime,
                FinishTime = finishTime
            };

            //удалить периоды из mergePeriods
            foreach (var p in mergePeriods) await _jsonFreePeriodRepository.Delete(p, ct);


            return newPeriod;
        }



    }
}

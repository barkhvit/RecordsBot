using LinqToDB;
using RecordBot.DataAccess;
using RecordBot.Interfaces;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Repository
{
    public class SqlFreePeriodRepository : IFreePeriodRepository
    {
        private readonly IDataContextFactory<DBContext> _factory;
        public SqlFreePeriodRepository(IDataContextFactory<DBContext> factory)
        {
            _factory = factory;
        }
        public async Task Add(FreePeriod freePeriod, CancellationToken cancellationToken)
        {
            using var context = _factory.CreateDataContext();
            await context.InsertAsync(ModelMapper.MapToModel(freePeriod), token: cancellationToken);
        }

        public async Task<bool> Delete(FreePeriod freePeriod, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            var delete = await context.DeleteAsync(ModelMapper.MapToModel(freePeriod), token: ct);
            return delete > 0;
        }

        public async Task<IReadOnlyList<FreePeriod>> GetAllPeriods(CancellationToken cancellationToken)
        {
            using var context = _factory.CreateDataContext();
            DateOnly now = DateOnly.FromDateTime(DateTime.Now);
            return await context.freePeriodModel
                .Where(p => p.Date >= now)
                .Select(p => ModelMapper.MapFromModel(p))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<DateOnly>> GetDates(CancellationToken cancellationToken)
        {
            using var context = _factory.CreateDataContext();
            return await context.freePeriodModel
                .Select(p => p.Date)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<FreePeriod>> GetPeriodsByDate(DateOnly dateOnly, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            return await context.freePeriodModel
                .Where(p => p.Date == dateOnly)
                .Select(p=>ModelMapper.MapFromModel(p))
                .ToListAsync(ct);
        }

        public async Task<bool> SplitPeriod(FreePeriod freePeriod, DateTime dateTime, int duration, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();

            DateTime startPeriod = new DateTime(freePeriod.Date, freePeriod.StartTime);
            DateTime finishPeriod = new DateTime(freePeriod.Date, freePeriod.FinishTime);
            DateTime startAppointment = dateTime;
            DateTime finishAppointment = dateTime.AddMinutes(duration);

            FreePeriod freePeriod1 = new()
            {
                FreePeriodId = Guid.NewGuid(),
                Date = freePeriod.Date,
                StartTime = TimeOnly.FromDateTime(startPeriod),
                FinishTime = TimeOnly.FromDateTime(startAppointment)
            };

            FreePeriod freePeriod2 = new()
            {
                FreePeriodId = Guid.NewGuid(),
                Date = freePeriod.Date,
                StartTime = TimeOnly.FromDateTime(finishAppointment),
                FinishTime = TimeOnly.FromDateTime(finishPeriod)
            };

            //удаляем старый период
            var delete = await context.DeleteAsync(ModelMapper.MapToModel(freePeriod), token: ct);
            if (delete == 0) return false;

            //когда запись внутри периода
            if (startPeriod < startAppointment && finishPeriod > finishAppointment)
            {
                await context.InsertAsync(ModelMapper.MapToModel(freePeriod1), token: ct);
                await context.InsertAsync(ModelMapper.MapToModel(freePeriod2), token: ct);
            }

            // Случай, когда appointment начинается в начале периода
            else if (startPeriod == startAppointment && finishPeriod > finishAppointment)
            {
                await context.InsertAsync(ModelMapper.MapToModel(freePeriod2), token: ct);
            }

            // Случай, когда appointment заканчивается в конце периода
            else if (startPeriod < startAppointment && finishPeriod == finishAppointment)
            {
                await context.InsertAsync(ModelMapper.MapToModel(freePeriod1), token: ct);
            }

            return true;
        }

    }
}

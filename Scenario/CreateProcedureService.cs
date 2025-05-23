using RecordBot.Interfaces;
using RecordBot.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Scenario
{
    public class CreateProcedureService : ICreateProcedureService
    {
        private ConcurrentDictionary<long, CreateProcedureStatus> _dictStatus;
        private ConcurrentDictionary<long, Procedure> _dictProcedure;
        private readonly IProcedureService _procedureService;

        public CreateProcedureService(IProcedureService _procedureService)
        {
            this._procedureService = _procedureService;
            _dictStatus = new ConcurrentDictionary<long, CreateProcedureStatus>();
            _dictProcedure = new ConcurrentDictionary<long, Procedure>();
        }

        //проверяет по словарям, если Id нет то добавляем с полями по умолчанию
        public async Task<CreateProcedureStatus> GetCreateProcedureStatus(long userId, CancellationToken cancellationToken)
        {
            var status = _dictStatus.GetOrAdd(userId, CreateProcedureStatus.WaitName);
            var procedure = _dictProcedure.GetOrAdd(userId, new Procedure());
            return await Task.FromResult(status);
        }

        //заполняет поля процедуры и изменяет статус
        public async Task SetNewProcedure(long userId, CancellationToken cancellationToken, string text = "",int number = 0)
        {
            var status = await GetCreateProcedureStatus(userId, cancellationToken);

            switch (status)
            {
                case CreateProcedureStatus.WaitName:
                    _dictProcedure[userId].Name = text;
                    _dictStatus[userId] = CreateProcedureStatus.WaitDescription; break;
                case CreateProcedureStatus.WaitDescription:
                    _dictProcedure[userId].Description = text;
                    _dictStatus[userId] = CreateProcedureStatus.WaitPrice; break;
                case CreateProcedureStatus.WaitPrice:
                    _dictProcedure[userId].Price = number;
                    _dictStatus[userId] = CreateProcedureStatus.WaitDuration; break;
                case CreateProcedureStatus.WaitDuration:
                    _dictProcedure[userId].DurationMinutes = number; break;
            }
        }

        public async Task<Procedure> GetProcedure(long userId, CancellationToken cancellation)
        {
            var status = _dictStatus.GetOrAdd(userId, CreateProcedureStatus.WaitName);
            var procedure = _dictProcedure.GetOrAdd(userId, new Procedure());
            return await Task.FromResult(procedure);
        }

        public async Task GetToStart(long userId, CancellationToken cancellationToken)
        {
            var status = _dictStatus.GetOrAdd(userId, CreateProcedureStatus.WaitName);
            var procedure = _dictProcedure.GetOrAdd(userId, new Procedure());
            _dictProcedure[userId] = new Procedure();
            _dictStatus[userId] = CreateProcedureStatus.WaitName;
        }

    }
}

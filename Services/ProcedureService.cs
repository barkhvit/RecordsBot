using RecordBot.Interfaces;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Services
{
    public class ProcedureService : IProcedureService
    {
        private readonly IProcedureRepository _procedureRepository;
        public ProcedureService(IProcedureRepository procedureRepository)
        {
            _procedureRepository = procedureRepository;
        }

        public async Task AddProcedure(Procedure procedure, CancellationToken cancellationToken)
        {
            await _procedureRepository.Add(procedure, cancellationToken);
        }

        public async Task<bool> DeleteProcedure(Guid id, CancellationToken cancellationToken)
        {
            var isDelete = await _procedureRepository.Delete(id, cancellationToken);
            return isDelete;
        }

        public async Task<IReadOnlyList<Procedure>> GetAllProcedures(CancellationToken cancellationToken)
        {
            return await _procedureRepository.GetProcedures(cancellationToken);
        }

    }
}

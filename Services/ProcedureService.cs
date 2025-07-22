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

        public async Task ChangeActive(Guid procedureId, CancellationToken ct)
        {
            await _procedureRepository.ChangeActive(procedureId, ct);
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

        public async Task<Procedure?> GetProcedureByGuidId(Guid guidId,CancellationToken cancellationToken)
        {
            return await _procedureRepository.GetProcedureById(guidId, cancellationToken);
        }

        public async Task<IReadOnlyList<Procedure>> GetProceduresByActive(bool isActive,CancellationToken cancellationToken)
        {
            var procedures =  await _procedureRepository.GetProcedures(cancellationToken);
            return procedures.Where(p => p.isActive == isActive).ToList().AsReadOnly();
        }

    }
}

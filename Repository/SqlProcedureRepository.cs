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
    public class SqlProcedureRepository : IProcedureRepository
    {
        private readonly IDataContextFactory<DBContext> _factory;
        public SqlProcedureRepository(IDataContextFactory<DBContext> factory)
        {
            _factory = factory;
        }
        public async Task Add(Procedure procedure, CancellationToken cancellationToken)
        {
            using var context = _factory.CreateDataContext();
            await context.InsertAsync(ModelMapper.MapToModel(procedure), token: cancellationToken);
        }

        public async Task ChangeActive(Guid procedureId, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            await context.procedureModel
                .Where(p => p.Id == procedureId)
                .Set(p => p.isActive, p => !p.isActive)  // Инвертирует текущее значение
                .UpdateAsync(ct);
        }

        public async Task<bool> Delete(Guid procedureId, CancellationToken cancellationToken)
        {
            using var context = _factory.CreateDataContext();
            var p = context.procedureModel.Any(p => p.Id == procedureId);
            if (p)
            {
                await context.procedureModel.
                    Where(p => p.Id == procedureId).
                    DeleteAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<Procedure?> GetProcedureById(Guid procedureId, CancellationToken cancellationToken)
        {
            using var context = _factory.CreateDataContext();
            var procedure = await context.procedureModel
                .FirstOrDefaultAsync(p => p.Id == procedureId, cancellationToken);
            return procedure != null ? ModelMapper.MapFromModel(procedure) : null;
        }

        public async Task<IReadOnlyList<Procedure>> GetProcedures(CancellationToken cancellationToken)
        {
            using var context = _factory.CreateDataContext();
            var procedures = await context.procedureModel.ToListAsync(cancellationToken);
            return procedures.Select(p => ModelMapper.MapFromModel(p)).ToList();
        }
    }
}

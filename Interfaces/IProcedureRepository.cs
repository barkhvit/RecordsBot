using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Interfaces
{
    public interface IProcedureRepository
    {
        Task Add(Procedure procedure, CancellationToken cancellationToken);
        Task<bool> Delete(Guid procedureId, CancellationToken cancellationToken);
        Task<IReadOnlyList<Procedure>> GetProcedures(CancellationToken cancellationToken);
    }
}

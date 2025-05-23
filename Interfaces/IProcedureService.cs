using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Interfaces
{
    public interface IProcedureService
    {
        Task AddProcedure(Procedure procedure, CancellationToken cancellationToken);
        Task<bool> DeleteProcedure(Guid id,CancellationToken cancellationToken);
        Task<IReadOnlyList<Procedure>> GetAllProcedures(CancellationToken cancellationToken);
    }
}

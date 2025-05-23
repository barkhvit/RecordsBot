using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUser(Guid UserId, CancellationToken cancellationToken);
        Task<User?> GetUserByTelegramId(long telegramUserId, CancellationToken cancellationToken);
        Task Add(User user, CancellationToken cancellation);
    }
}

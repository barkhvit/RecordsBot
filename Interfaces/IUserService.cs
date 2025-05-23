using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterUser(Telegram.Bot.Types.Update update, CancellationToken cancellationToken);
        Task<User?> GetUser(long telegarmUserId,CancellationToken cancellation); 
    }
}

using LinqToDB;
using RecordBot.DataAccess;
using RecordBot.Interfaces;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Telegram.Bot.Types;

namespace RecordBot.Repository
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly IDataContextFactory<DBContext> _factory;

        public SqlUserRepository(IDataContextFactory<DBContext> dataContextFactory)
        {
            _factory = dataContextFactory;
        }

        public async Task Add(User user, CancellationToken cancellation)
        {
            using var dbContext = _factory.CreateDataContext();
            await dbContext.InsertAsync(ModelMapper.MapToModel(user), token: cancellation);
        }

        public async Task<User?> GetUser(Guid UserId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();
            var user = await dbContext.userModel
                .FirstOrDefaultAsync(u => u.Id == UserId, cancellationToken);
            return user != null ? ModelMapper.MapFromModel(user) : null;
        }

        public async Task<User?> GetUserByTelegramId(long telegramUserId, CancellationToken cancellationToken)
        {
            using var dbContext = _factory.CreateDataContext();
            var user = await dbContext.userModel
                .FirstOrDefaultAsync(u => u.TelegramId == telegramUserId, cancellationToken);
            return user != null ? ModelMapper.MapFromModel(user) : null;
        }
    }
}

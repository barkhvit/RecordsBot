using RecordBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RecordBot.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<Models.User?> GetUser(long telegarmUserId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetUserByTelegramId(telegarmUserId, cancellationToken);
        }

        public async Task<Models.User> RegisterUser(Update update, CancellationToken cancellationToken)
        {
            User userFrom = update.Message.From;
            var user = await GetUser(userFrom.Id, cancellationToken);
            if(user == null)
            {
                Models.User newUser = new Models.User
                {
                    Id = Guid.NewGuid(),
                    TelegramId = userFrom.Id,
                    FirstName = userFrom.FirstName,
                    LastName = userFrom.LastName,
                    CurrentState = Models.UserState.first,
                    RegistrationDate = DateTime.UtcNow
                };
                await _userRepository.Add(newUser, cancellationToken);
                return newUser;
            }
            return user;
        }
    }
}

using RecordBot.Interfaces;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecordBot.Repository
{
    public class JsonUserRepository : IUserRepository
    {
        private readonly string _userStorage;

        public JsonUserRepository(string userStorage)
        {
            _userStorage = userStorage;
            try
            {
                Directory.CreateDirectory(_userStorage);
            }
            catch(Exception)
            {
                Console.WriteLine("Ошибка при создании директория пользователей.");
                throw;
            }
        }
        public async Task Add(User user, CancellationToken cancellationToken)
        {
            string json = JsonSerializer.Serialize<User>(user, new JsonSerializerOptions { WriteIndented = true });
            string pathFile = Path.Combine(_userStorage, $"{user.Id}.json");
            await File.WriteAllTextAsync(pathFile, json, cancellationToken);
        }

        public async Task<User?> GetUser(Guid UserId, CancellationToken cancellationToken)
        {
            string pathFile = Path.Combine(_userStorage, $"{UserId}.json");
            if (File.Exists(pathFile))
            {
                string json = await File.ReadAllTextAsync($"{UserId}.json", cancellationToken);
                var user = JsonSerializer.Deserialize<User>(json);
                return user;
            }
            return null;
        }

        public async Task<User?> GetUserByTelegramId(long telegramUserId, CancellationToken cancellationToken)
        {
            var users = await GetAllUsers(cancellationToken);
            return users.FirstOrDefault(u => u.TelegramId == telegramUserId);
        }

        public async Task<IReadOnlyList<User>> GetAllUsers(CancellationToken cancellationToken)
        {
            List<User> listUsers = new();
            foreach(string file in Directory.GetFiles(_userStorage))
            {
                string json = await File.ReadAllTextAsync(file, cancellationToken);
                var user = JsonSerializer.Deserialize<User>(json);
                if (user != null) listUsers.Add(user);
            }
            return listUsers.AsReadOnly();
        }
    }
}

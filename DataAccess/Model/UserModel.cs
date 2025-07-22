using System;
using System.Collections.Generic;
using LinqToDB.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.DataAccess.Model
{
    [Table("users")]
    public class UserModel
    {
        //id, telegramid, firstname, lastname, registrationdate
        [PrimaryKey][Column("id")]public Guid Id { get; set; }
        [Column("telegramid")] public long TelegramId { get; set; }
        [Column("firstname")] public string FirstName { get; set; }
        [Column("lastname")] public string? LastName { get; set; }
        [Column("registrationdate")] public DateTime RegistrationDate { get; set; }
    }
}

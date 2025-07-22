using System;
using System.Collections.Generic;
using LinqToDB.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.DataAccess.Model
{
    [Table("procedures")]
    public class ProcedureModel
    {
        //id, name, description, price, durationminutes, isactive
        [PrimaryKey][Column("id")]public Guid Id { get; set; }
        [Column("name")] public string Name { get; set; }
        [Column("description")] public string Description { get; set; }
        [Column("price")] public decimal Price { get; set; }
        [Column("durationminutes")] public int DurationMinutes { get; set; }
        [Column("isactive")] public bool isActive { get; set; }
    }
}

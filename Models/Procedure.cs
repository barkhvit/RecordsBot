using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Models
{
    public class Procedure
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get;set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }

        //конструктор
        public Procedure(string name, string description, decimal price, int duration)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Price = price;
            DurationMinutes = duration;
        }

        //конструктор без параметров()
        public Procedure()
        {
            Id = Guid.NewGuid();
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Scenario.InfoStorage
{
    public class InfoContext
    {
        public ConcurrentDictionary<string, object> Data { get; set; } = new();
    }

}

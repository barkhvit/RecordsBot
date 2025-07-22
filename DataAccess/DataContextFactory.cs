using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.DataAccess
{
    public class DataContextFactory : IDataContextFactory<DBContext>
    {
        private readonly string _connectionString;
        public DataContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public DBContext CreateDataContext()
        {
            return new DBContext(_connectionString);
        }
    }
}

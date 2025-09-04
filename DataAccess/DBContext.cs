using LinqToDB;
using LinqToDB.Data;
using RecordBot.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.DataAccess
{
    public class DBContext : DataConnection
    {
        public DBContext(string connectionString) : base(ProviderName.PostgreSQL, connectionString) { }
        public ITable<AppointmentModel> appointmentModels => this.GetTable<AppointmentModel>();
        public ITable<FreePeriodModel> freePeriodModel => this.GetTable<FreePeriodModel>();
        public ITable<ProcedureModel> procedureModel => this.GetTable<ProcedureModel>();
        public ITable<UserModel> userModel => this.GetTable<UserModel>();
        public ITable<NotificationModel> notificationModel => this.GetTable<NotificationModel>();
    }
}

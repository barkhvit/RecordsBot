using RecordBot.DataAccess.Model;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.DataAccess
{
    internal static class ModelMapper
    {
        public static Appointment MapFromModel(AppointmentModel model)
        {
            return new Appointment
            {
                Id = model.Id,
                dateTime = model.dateTime,
                isConfirmed = model.isConfirmed,
                UserId = model.UserId,
                ProcedureId = model.ProcedureId
            };
        }
        public static AppointmentModel MapToModel(Appointment entity)
        {
            return new AppointmentModel
            {
                Id = entity.Id,
                dateTime = entity.dateTime,
                isConfirmed = entity.isConfirmed,
                UserId = entity.UserId,
                ProcedureId = entity.ProcedureId
            };
        }

        public static FreePeriod MapFromModel(FreePeriodModel model)
        {
            return new FreePeriod
            {
                FreePeriodId = model.FreePeriodId,
                Date = model.Date,
                StartTime = TimeOnly.FromTimeSpan(model.StartTime),
                FinishTime = TimeOnly.FromTimeSpan(model.FinishTime),
                Duration = model.Duration
            };
        }

        public static FreePeriodModel MapToModel(FreePeriod entity)
        {
            return new FreePeriodModel
            {
                FreePeriodId = entity.FreePeriodId,
                Date = entity.Date,
                StartTime = entity.StartTime.ToTimeSpan(),
                FinishTime = entity.FinishTime.ToTimeSpan(),
                Duration = entity.Duration
            };
        }

        public static Procedure MapFromModel(ProcedureModel model)
        {
            return new Procedure
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                DurationMinutes = model.DurationMinutes,
                isActive = model.isActive
            };
        }

        public static ProcedureModel MapToModel(Procedure entity)
        {
            return new ProcedureModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                DurationMinutes = entity.DurationMinutes,
                isActive = entity.isActive
            };
        }

        public static User MapFromModel(UserModel model)
        {
            return new User
            {
                Id = model.Id,
                TelegramId = model.TelegramId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                RegistrationDate = model.RegistrationDate
            };
        }

        public static UserModel MapToModel(User entity)
        {
            return new UserModel
            {
                Id = entity.Id,
                TelegramId = entity.TelegramId,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                RegistrationDate = entity.RegistrationDate
            };
        }

    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookCalendar.Domain.Abstract
{
    public interface ICalendarApi<T>
    {
        void Login();
        string CreateAppointment(T obj);
        void EditAppointment(T obj);
        List<String> CheckRange(DateTime startDateTime, DateTime endDateTime);
        List<string> CheckEntity(string entityId);
        void DeleteAppointment(string eventId);
    }
}

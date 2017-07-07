using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutlookCalendar.Domain.Entities;

namespace OutlookCalendar.Domain.Abstract
{
    public interface IAppointmentRepository
    {
        IEnumerable<Appointment> Appointments { get; }
        void SaveAppointment(Appointment appointment);
        void ClearAppointments();
        void DeleteAppointment(string appointmentId);
    }
}

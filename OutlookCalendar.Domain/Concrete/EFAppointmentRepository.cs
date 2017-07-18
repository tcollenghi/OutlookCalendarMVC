using OutlookCalendar.Domain.Abstract;
using OutlookCalendar.Domain.Entities;
using System.Collections.Generic;
using Z.EntityFramework.Plus;

namespace OutlookCalendar.Domain.Concrete
{
    public class EFAppointmentRepository : IAppointmentRepository
    {
        private EFDbContext context = new EFDbContext();

        public IEnumerable<Appointment> Appointments => context.Appointments;

        public void SaveAppointment(Appointment appointment)
        { 
            Appointment dbEntry = context.Appointments.Find(appointment.EventId);
            if (dbEntry != null)
            {
                dbEntry.Title = appointment.Title;
                dbEntry.Attendee = appointment.Attendee;
                dbEntry.AttendeeEmail = appointment.AttendeeEmail;
                dbEntry.Body = appointment.Body;
                dbEntry.Location = appointment.Location;
                dbEntry.EntityId = appointment.EntityId;
                dbEntry.Start = appointment.Start;
                dbEntry.End = appointment.End;
            }
            else
            {
                context.Appointments.Add(appointment);
            }
            context.SaveChanges();
        }

        public void ClearAppointments()
        {
            context.Appointments.Delete();
        }

        public void DeleteAppointment(string appointmentId)
        {
            Appointment dbEntry = context.Appointments.Find(appointmentId);
            if (dbEntry != null)
            {
                context.Appointments.Remove(dbEntry);
                context.SaveChanges();
            }
        }
    }
}

using OutlookCalendar.Domain.Entities;
using System.Data.Entity;

namespace OutlookCalendar.Domain.Concrete
{
    public class EFDbContext : DbContext
    {
        public DbSet<Appointment> Appointments { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutlookCalendar.Domain.Entities
{
    [Table("Appointments")]
    public class Appointment
    {
        [Key][MaxLength]
        public string EventId { get; set; }
        public string Attendee { get; set; }
        public string AttendeeEmail { get; set; }
        public string Title { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        [MaxLength]
        public string Body { get; set; }
        public string EntityId { get; set; }
        public string Location { get; set; }
    }
}

using System.Web.Mvc;
using OutlookCalendar.Domain.Abstract;
using OutlookCalendar.Domain.Entities;
using OutlookCalendar.WebUI.Helpers;

namespace OutlookCalendar.WebUI.Controllers
{
    public class EditController : Controller
    {
        OutlookService service = new OutlookService();
        private IAppointmentRepository repository;

        public EditController(IAppointmentRepository appointmentRepository)
        {
            repository = appointmentRepository;
        }

        public ActionResult Index(Appointment appointment)
        {
            return View("Edit", appointment);
        }

        [HttpPost]
        public ActionResult Submit(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                repository.SaveAppointment(appointment);
                service.EditAppointment(appointment);
                return RedirectToAction("Index", "Appointment");
            }

            return View("Edit", appointment);
        }

        public ActionResult Back()
        {
            return RedirectToAction("Index", "Appointment");
        }
    }
}
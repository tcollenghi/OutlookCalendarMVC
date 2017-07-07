using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OutlookCalendar.Domain.Abstract;
using OutlookCalendar.Domain.Entities;
using OutlookCalendar.WebUI.Helpers;

namespace OutlookCalendar.WebUI.Controllers
{
    public class CreateController : Controller
    {
        private IAppointmentRepository repository;
        OutlookService service = new OutlookService();

        public CreateController(IAppointmentRepository appointmentRepository)
        {
            repository = appointmentRepository;
        }

        public ActionResult Index()
        {
            return View("Create");
        }

        [HttpPost]
        public ActionResult Create(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                var appointmentId = service.CreateAppointment(appointment);
                appointment.EventId = appointmentId;
                repository.SaveAppointment(appointment);
                return RedirectToAction("Index", "Appointment");
            }
            return View("Create");
        }

        public ActionResult Back()
        {
            return RedirectToAction("Index", "Appointment");
        }
    }
}
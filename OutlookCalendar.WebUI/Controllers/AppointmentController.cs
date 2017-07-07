using System;
using System.Web.Mvc;
using OutlookCalendar.Domain.Abstract;
using OutlookCalendar.Domain.Entities;
using OutlookCalendar.WebUI.Helpers;

namespace OutlookCalendar.WebUI.Controllers
{
    public class AppointmentController : Controller
    {
        private IAppointmentRepository repository;
        OutlookService service = new OutlookService();

        public AppointmentController(IAppointmentRepository appointmentRepository)
        {
            repository = appointmentRepository;
        }

        public ActionResult Index()
        {
            return View("Appointment", repository.Appointments);
        }

        [Authorize]
        public ActionResult CheckEvents()
        {
            try
            {
                repository.ClearAppointments();
                var start = Convert.ToDateTime(Request["start"]);
                var end = Convert.ToDateTime(Request["end"]);

                var appointmentIds = service.CheckRange(start, end);

                foreach (string id in appointmentIds)
                {
                    Appointment app = service.GetAppointment(id);
                    repository.SaveAppointment(app);
                }

                return View("Appointment", repository.Appointments);
            }
            catch (Exception e)
            {
                if (e.Message == "Caller needs to authenticate.") return new EmptyResult();
                return RedirectToAction("Index", "Error", new {message = Request.RawUrl + ": " + e.Message});
            }
        }

        [Authorize]
        public ActionResult CheckEntity()
        {
            try
            {
                repository.ClearAppointments();
                var entityId = Convert.ToString(Request["entityId"]);
                var appointmentIds = service.CheckEntity(entityId);

                foreach (string id in appointmentIds)
                {
                    Appointment app = service.GetAppointment(id);
                    repository.SaveAppointment(app);
                }
                return View("Appointment", repository.Appointments);
            }
            catch (Exception e)
            {
                if (e.Message == "Caller needs to authenticate.") return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = Request.RawUrl + ": " + e.Message });
            }
        }

        [Authorize]
        public ActionResult Edit(Appointment appointment)
        {
            try
            {
                return RedirectToAction("Index", "Edit", appointment);
            }
            catch (Exception e)
            {
                if (e.Message == "Caller needs to authenticate.") return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = Request.RawUrl + ": " + e.Message });
            }
        }

        [Authorize]
        public ActionResult Create()
        {
            try
            {
                return RedirectToAction("Index", "Create");
            }
            catch (Exception e)
            {
                if (e.Message == "Caller needs to authenticate.") return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = Request.RawUrl + ": " + e.Message });
            }
            
        }

        [Authorize]
        public ActionResult Delete(Appointment appointment)
        {
            try
            {
                service.DeleteAppointment(appointment.EventId);
                repository.DeleteAppointment(appointment.EventId);
                return View("Appointment", repository.Appointments);
            }
            catch (Exception e)
            {
                if (e.Message == "Caller needs to authenticate.") return new EmptyResult();
                return RedirectToAction("Index", "Error", new {message = Request.RawUrl + ": " + e.Message});
            }
        }
    }
}
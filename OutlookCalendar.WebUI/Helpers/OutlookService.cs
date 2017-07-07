using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using OutlookCalendar.Domain.Entities;
using Microsoft.Graph;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using OutlookCalendar.Domain.Abstract;

namespace OutlookCalendar.WebUI.Helpers
{
    public class OutlookService : ICalendarApi<Appointment>
    {
        private const string ServiceEndpoint = "https://graph.microsoft.com/v1.0/";

        public void Login()
        {
        }

        public string CreateAppointment(Appointment appointment)
        {
            var graphClient = GetGraphClient();
            var entityId = appointment.EntityId;
            var primaryAttendee = appointment.Attendee;
            var attendeeEmail = appointment.AttendeeEmail;
            var outlookEvent = CreateEvent(appointment);
            var createdEvent = graphClient.Me.Calendar.Events.Request().AddAsync(outlookEvent).Result;
            var createdId = createdEvent.Id;
            AddExtension(entityId, primaryAttendee, attendeeEmail, createdEvent.Id);
            return createdId;
        }

        public void EditAppointment(Appointment appointment)
        {
            var graphClient = GetGraphClient();
            var eventToEdit = graphClient.Me.Events[appointment.EventId].Request().GetAsync().Result;

            eventToEdit.Subject = appointment.Title;
            eventToEdit.Body.Content = appointment.Body;
            eventToEdit.Location.DisplayName = appointment.Location;
            eventToEdit.Start.DateTime = Convert.ToString(appointment.Start);
            eventToEdit.Start.TimeZone = "Pacific Standard Time";
            eventToEdit.End.DateTime = Convert.ToString(appointment.End);
            eventToEdit.End.TimeZone = "Pacific Standard Time";

            graphClient.Me.Events[appointment.EventId].Request().UpdateAsync(eventToEdit);

            UpdateExtension(appointment.EntityId, appointment.Attendee, appointment.AttendeeEmail, appointment.EventId);
        }

        public List<string> CheckRange(DateTime startDateTime, DateTime endDateTime)
        {
            var eventIds = new List<string>();
            var client = GetHttpClient();
            var usersEndpoint = new Uri(ServiceEndpoint + "me/events?$filter=Start/DateTime+ge+'" + startDateTime +
                                        "'+and+End/DateTime+lt+'" + endDateTime + "'");
            var response = client.GetAsync(usersEndpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var jResult = JObject.Parse(responseContent);

                foreach (var jToken in jResult["value"])
                {
                    var calendarEvent = (JObject)jToken;
                    var eventId = (string)calendarEvent["id"];
                    eventIds.Add(eventId);
                }
            }

            return eventIds;
        }

        public List<string> CheckEntity(string entityId)
        {
            var eventIds = new List<string>();
            var client = GetHttpClient();
            var eventsEndpoint = new Uri(ServiceEndpoint + "me/events?$filter=Extensions/any(f:f/id%20eq%20" +
                                         "'Com.Xanatek.Appointment')&$expand=Extensions" +
                                         "($filter=id%20eq%20'Com.Xanatek.Appointment')");
            var response = client.GetAsync(eventsEndpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var jResult = JObject.Parse(responseContent);

                foreach (var calendarEvent in jResult["value"])
                {
                    if ((string)calendarEvent["extensions"][0]["entityId"] == entityId)
                    {
                        var eventId = (string)calendarEvent["id"];
                        eventIds.Add(eventId);
                    }
                }
            }
            else
            {
                return null;
            }
            return eventIds;
        }

        public void DeleteAppointment(string eventId)
        {
            var graphClient = GetGraphClient();
            var eventToDelete = graphClient.Me.Events[eventId].Request().GetAsync().Result;

            if (eventToDelete.Attendees.Any())
            {
                CancelAppointment(eventId);
            }
            else
            {
                graphClient.Me.Events[eventId].Request().DeleteAsync();
            }
        }

        public Appointment GetAppointment(string eventId)
        {
            var appointment = new Appointment();
            var client = GetHttpClient();
            var graphClient = GetGraphClient();
            Event outlookEvent = graphClient.Me.Calendar.Events[eventId].Request().GetAsync().Result;
            var body = ParseBody(outlookEvent.Body.Content);

            appointment.Title = outlookEvent.Subject;
            appointment.EventId = outlookEvent.Id;
            appointment.Body = body;
            appointment.Start = Convert.ToDateTime(outlookEvent.Start.DateTime);
            appointment.End = Convert.ToDateTime(outlookEvent.End.DateTime);
            appointment.Start = TimeZoneInfo.ConvertTimeFromUtc(appointment.Start, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
            appointment.End = TimeZoneInfo.ConvertTimeFromUtc(appointment.End, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
            appointment.Location = outlookEvent.Location.DisplayName;

            var extensionEndpoint = new Uri(ServiceEndpoint + "me/events/" + eventId +
                                            "/extensions/Com.Xanatek.Appointment");
            var response = client.GetAsync(extensionEndpoint).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var jResult = JObject.Parse(responseContent);

                appointment.Attendee = (string)jResult["primaryAttendee"];
                appointment.EntityId = (string)jResult["entityId"];
                appointment.AttendeeEmail = (string)jResult["attendeeEmail"];
            }

            return appointment;
        }

        private HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            var token = AuthProvider.Instance().GetUserAccessTokenAsync().Result;
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            return client;
        }

        private GraphServiceClient GetGraphClient()
        {
            var graphserviceClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer",
                            AuthProvider.Instance().GetUserAccessTokenAsync().Result);

                        return Task.FromResult(0);
                    }));

            return graphserviceClient;
        }

        private void AddExtension(string entityId, string primaryAttendee, string attendeeEmail, string eventId)
        {
            var client = GetHttpClient();
            Uri targetEventEndpoint = new Uri(ServiceEndpoint + "me/events/" + eventId + "/extensions");

            string postBody = "{'@odata.type':'Microsoft.Graph.OpenTypeExtension',"
                              + "'extensionName':'Com.Xanatek.Appointment',"
                              + "'entityId':'" + entityId + "',"
                              + "'primaryAttendee':'" + primaryAttendee + "',"
                              + "'attendeeEmail':'" + attendeeEmail + "'}";

            var createBody = new StringContent(postBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(targetEventEndpoint, createBody).Result;
        }

        private void CancelAppointment(string eventId)
        {
            var client = GetHttpClient();
            var targetEndpoint = new Uri("https://graph.microsoft.com/beta/me/events/" + eventId + "/cancel");
            var postBody = "{" +
                           "\"Comment\": \"Appointment canceled.\"" +
                           "}";
            var body = new StringContent(postBody, System.Text.Encoding.UTF8, "application/json");

            var response = client.PostAsync(targetEndpoint, body).Result;
        }

        private string ParseBody(string html)
        {
            var body = html;
            var document = new HtmlDocument();
            document.LoadHtml(body);
            try
            {
                var parsedBody = document.DocumentNode.SelectSingleNode("//body/font/span/div").InnerText;
                return parsedBody;
            }
            catch (Exception)
            {
                return "";
            }
        }

        private void UpdateExtension(string entityId, string attendee, string attendeeEmail, string eventId)
        {
            var client = GetHttpClient();
            Uri targetEventEndpoint = new Uri(ServiceEndpoint + "me/events/" + eventId + "/extensions/Com.Xanatek.Appointment");
            var method = new HttpMethod("PATCH");
            string patchBody = "{'@odata.type':'Microsoft.Graph.OpenTypeExtension',"
                               + "'extensionName':'Com.Xanatek.Appointment',"
                               + "'entityId':'" + entityId + "',"
                               + "'primaryAttendee':'" + attendee + "',"
                               + "'attendeeEmail':'" + attendeeEmail + "'}";
            var createBody = new StringContent(patchBody, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(method, targetEventEndpoint)
            {
                Content = createBody
            };

            var response = client.SendAsync(request).Result;
        }

        private Event CreateEvent(Appointment appointment)
        {
            var outlookEvent = new Event
            {
                Subject = appointment.Title,
                Body = new Microsoft.Graph.ItemBody
                {
                    Content = appointment.Body,
                    ContentType = BodyType.Text
                },
                Location = new Location
                {
                    DisplayName = appointment.Location
                },
                Start = new DateTimeTimeZone
                {
                    DateTime = appointment.Start.ToString("s"),
                    TimeZone = "Pacific Standard Time"
                },
                End = new DateTimeTimeZone
                {
                    DateTime = appointment.End.ToString("s"),
                    TimeZone = "Pacific Standard Time"
                }
            };

            return outlookEvent;
        }
    }
}
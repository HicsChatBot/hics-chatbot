using HicsChatBot.Model;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using System;

namespace HicsChatBot.Services
{
    public class AppointmentsService : BaseService
    {
        private readonly string appointmentsAddress = "appointments";
        public AppointmentsService() : base() { }

        public async Task<Appointment> GetAppointment(string clinicId, string roomNumber, string patientId, string startDatetime)
        {
            var uri_params = new Dictionary<string, string> { };
            uri_params.Add("clinicId", clinicId);
            uri_params.Add("roomNumber", roomNumber);
            uri_params.Add("patientId", patientId);
            uri_params.Add("startDatetime", startDatetime);

            string resp = await base.Get(uri: this.appointmentsAddress, uri_params: uri_params);

            JsonNode apptJson = JsonObject.Parse(resp)!["appointment"];
            return Appointment.ToEntity(apptJson);
        }

        public async Task<Appointment> GetNextUpcomingAppointment(string patientId)
        {
            var uri_params = new Dictionary<string, string> { };
            uri_params.Add("patientId", patientId);

            string resp = await base.Get(uri: this.appointmentsAddress + "/getNextUpcomingAppointment", uri_params: uri_params);

            JsonNode apptJson = JsonObject.Parse(resp)!["appointment"];
            return Appointment.ToEntity(apptJson);
        }

        public async Task<Appointment> GetMostRecentPastAppointment(string patientId)
        {
            var uri_params = new Dictionary<string, string> { };
            uri_params.Add("patientId", patientId);

            string resp = await base.Get(uri: this.appointmentsAddress + "/getMostRecentPastAppointment", uri_params: uri_params);

            JsonNode apptJson = JsonObject.Parse(resp)!["appointment"];
            return Appointment.ToEntity(apptJson);
        }

        public async Task<Appointment> CreateAppointment(Appointment appt)
        {
            object uri_params = new
            {
                appointment = appt.ToObject(),
            };

            string resp = await base.Post(uri: this.appointmentsAddress + "/createUpcomingAppointment", uri_params);

            JsonNode apptJson = JsonObject.Parse(resp)!["appointment"];
            return Appointment.ToEntity(apptJson);
        }

        public async Task<Appointment> DeleteUpcomingAppointment(Appointment appt)
        {
            object uri_params = new
            {
                clinicId = appt.ClinicId,
                roomNumber = appt.RoomNumber,
                patientId = appt.PatientId,
                startDatetime = appt.StartDatetime?.ToString("yyyy-MM-ddTHH:mm"),
            };

            string resp = await base.Delete(uri: this.appointmentsAddress + "/deleteUpcomingAppointment", uri_params);

            JsonNode apptJson = JsonObject.Parse(resp)!["appointment"];
            return Appointment.ToEntity(apptJson);
        }

        public async Task<Appointment> FindNextAvailableSubsidizedAppointment(string specialization, string apptType)
        {
            var uri_params = new Dictionary<string, string> { };
            uri_params.Add("specialization", specialization);
            uri_params.Add("apptType", apptType);

            string resp = await base.Get(uri: this.appointmentsAddress + "/findNextAvailableSubsidizedAppointment", uri_params: uri_params);

            JsonNode apptJson = JsonObject.Parse(resp)!["apptData"];
            return Appointment.ToEntity(apptJson);
        }
    }
}

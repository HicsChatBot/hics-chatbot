using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HicsChatBot.Model
{
    /**
    Models a Appointment in the database.
    */
    public record class Appointment : BaseModel<Appointment>
    {
        public int? ClinicId { get; set; }
        public int? RoomNumber { get; set; }

        public int? PatientId { get; set; }
        public DateTime? StartDatetime { get; set; }
        public string ApptStatus { get; set; }
        public int? DoctorId { get; set; }
        public string ApptType { get; set; }

        public Appointment(int? clinicId = null, int? roomNumber = null, int? patientId = null, string apptStatus = null, string startDatetime = null, int? doctorId = null, string apptType = null)
        {
            this.ClinicId = clinicId;
            this.RoomNumber = roomNumber;
            this.PatientId = patientId;
            this.StartDatetime = startDatetime != null ? DateTime.Parse(startDatetime) : null;
            this.ApptStatus = apptStatus;
            this.DoctorId = doctorId;
            this.ApptType = apptType;
        }

        public static new Appointment ToEntity(JsonNode json)
        {
            if (json == null)
            {
                return null;
            }

            return new Appointment(
                clinicId: json["clinicId"].GetValue<int>(),
                roomNumber: json["roomNumber"].GetValue<int>(),
                patientId: json["patientId"]?.GetValue<int>(),
                startDatetime: json["startDatetime"].GetValue<string>(),
                apptStatus: json["apptStatus"]?.GetValue<string>(),
                doctorId: json["doctorId"]?.GetValue<int>(),
                apptType: json["apptType"]?.GetValue<string>()
            );
        }

        public string ToJson()
        {
            object patient = ToObject();
            string json = JsonSerializer.Serialize(patient);
            return json;
        }

        public override object ToObject()
        {
            return new
            {
                clinicId = this.ClinicId,
                roomNumber = this.RoomNumber,
                patientId = this.PatientId,
                startDatetime = this.StartDatetime?.ToString("yyyy-MM-ddTHH:mm"),
                apptStatus = this.ApptStatus,
                doctorId = this.DoctorId,
                apptType = this.ApptType,
            };
        }

        public override string ToString()
        {
            return $"Appointment [ ClinicId: {this.ClinicId}, RoomNumber: {this.RoomNumber}, PatientId: {this.PatientId}, StartDatetime: {this.StartDatetime}, ApptStatus: {this.ApptStatus}, DoctorId: {this.DoctorId}, ApptType: {this.ApptType} ]\n";
        }
    }
}

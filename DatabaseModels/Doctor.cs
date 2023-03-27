using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HicsChatBot.Model
{
    /**
    Models a Doctor in the database.
    */
    public record class Doctor : BaseModel<Doctor>
    {
        public int? Id { get; set; }
        public string Nric { get; set; }
        public string Fullname { get; set; }
        public string Gender { get; set; }
        public DateTime? Dob { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int? HospitalId { get; set; }

        public Doctor(int? id = null, string nric = null, string fullname = null, string gender = null, string dob = null, string address = null, string phone = null, int? hospitalId = null)
        {
            this.Id = id;
            this.Nric = nric;
            this.Fullname = fullname;
            this.Gender = gender;
            this.Dob = dob != null ? DateTime.Parse(dob) : null;
            this.Address = address;
            this.Phone = phone;
            this.HospitalId = hospitalId;
        }

        public static new Doctor ToEntity(JsonNode json)
        {
            if (json == null)
            {
                return null;
            }

            Console.WriteLine(json);

            return new Doctor(
                id: json["id"].GetValue<int>(),
                nric: json["nric"].GetValue<string>(),
                fullname: json["fullname"].GetValue<string>(),
                gender: json["gender"].GetValue<string>(),
                dob: json["dob"].GetValue<string>(),
                address: json["address"].GetValue<string>(),
                phone: json["phone"].GetValue<string>(),
                hospitalId: json["hospitalId"].GetValue<int>()
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
                id = this.Id,
                nric = this.Nric,
                fullname = this.Fullname,
                gender = this.Gender,
                dob = this.Dob?.ToString("yyyy-MM-dd"),
                address = this.Address,
                phone = this.Phone,
                hospitalId = this.HospitalId,
            };
        }

        public override string ToString()
        {
            return $"Doctor [ id: {this.Id}, nric: {this.Nric}, fullname: {this.Fullname}, gender: {this.Gender}, dob: {this.Dob}, address: {this.Address}, phone: {this.Phone}, hospitalId: {this.HospitalId} ]\n";
        }
    }
}

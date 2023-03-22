
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HicsChatBot.Model
{
    /**
    Models a Patient in the database.
    */
    public record class Patient : BaseModel<Patient>
    {
        public int? Id { get; set; }
        public string Nric { get; set; }
        public string Fullname { get; set; }
        public string Gender { get; set; }
        public DateTime? Dob { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Title { get; set; }

        public Patient(int? id = null, string nric = null, string fullname = null, string gender = null, string dob = null, string address = null, string phone = null, string title = null)
        {
            this.Id = id;
            this.Nric = nric;
            this.Fullname = fullname;
            this.Gender = gender;
            this.Dob = dob != null ? DateTime.Parse(dob) : null;
            this.Address = address;
            this.Phone = phone;
            this.Title = title;
        }

        public static new Patient ToEntity(JsonNode json)
        {
            if (json == null)
            {
                return null;
            }

            return new Patient(
                id: json["id"].GetValue<int>(),
                nric: json["nric"].GetValue<string>(),
                fullname: json["fullname"].GetValue<string>(),
                gender: json["gender"].GetValue<string>(),
                dob: json["dob"].GetValue<string>(),
                address: json["address"].GetValue<string>(),
                phone: json["phone"].GetValue<string>(),
                title: json["title"].GetValue<string>()
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
                title = this.Title,
            };
        }

        public override string ToString()
        {
            return $"Patient [ id: {this.Id}, nric: {this.Nric}, fullname: {this.Fullname}, gender: {this.Gender}, dob: {this.Dob}, address: {this.Address}, phone: {this.Phone}, title: {this.Title} ]\n";
        }
    }
}

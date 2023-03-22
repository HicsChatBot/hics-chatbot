
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using HicsChatBot.Exceptions;

namespace HicsChatBot.Model
{
    /**
    Models a Patient in the database.
    */
    public record class Patient : BaseModel<Patient>
    {
        private int Id { get; set; }
        private string Nric { get; set; }
        private string Fullname { get; set; }
        private string Gender { get; set; }
        private DateTime Dob { get; set; }
        private string Address { get; set; }
        private string Phone { get; set; }
        private string Title { get; set; }

        public Patient(int id, string nric, string fullname, string gender, string dob, string address, string phone, string title)
        {
            this.Id = id;
            this.Nric = nric;
            this.Fullname = fullname;
            this.Gender = gender;
            this.Dob = DateTime.Parse(dob);
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
                dob = this.Dob.ToString("yyyy-MM-dd"),
                address = this.Address,
                phone = this.Phone,
                title = this.Title,
            };
        }

        public override string ToString()
        {
            return $"Patient [ id: {this.Id}, nric: {this.Nric}, fullname: {this.Fullname}, gender: {this.Gender}, dob: {this.Dob}, address: ${this.Address}, phone: {this.Phone}, title: {this.Title} ]\n";
        }
    }
}

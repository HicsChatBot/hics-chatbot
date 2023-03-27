using HicsChatBot.Model;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Nodes;

namespace HicsChatBot.Services
{
    public class DoctorsService : BaseService
    {
        private readonly string doctorsAddress = "doctors";
        public DoctorsService() : base() { }

        public async Task<Doctor> GetDoctor(string id)
        {
            var uri_params = new Dictionary<string, string> { };
            uri_params.Add("id", id);

            string resp = await base.Get(uri: this.doctorsAddress, uri_params: uri_params);

            JsonNode doctorJson = JsonObject.Parse(resp)!["doctor"];
            return Doctor.ToEntity(doctorJson);
        }

        public async Task<List<Doctor>> GetDoctorsBySpecialization(string specialization)
        {
            var uri_params = new Dictionary<string, string> { };
            uri_params.Add("specialization", specialization);

            string resp = await base.Get(uri: this.doctorsAddress + "/getDoctorsBySpecialization", uri_params: uri_params);

            JsonNode json = JsonObject.Parse(resp)!;
            JsonArray doctorsJson = json!["doctors"]!.AsArray();

            List<Doctor> doctors = new();
            foreach (JsonNode doctorJson in doctorsJson)
            {
                doctors.Add(Doctor.ToEntity(doctorJson));
            }

            return doctors;
        }

        public async Task<List<Doctor>> GetDoctorsBySpecializationAndRanking(string specialization, string ranking)
        {
            var uri_params = new Dictionary<string, string> { };
            uri_params.Add("specialization", specialization);
            uri_params.Add("ranking", ranking);

            string resp = await base.Get(uri: this.doctorsAddress + "/getDoctorsBySpecializationAndRanking", uri_params: uri_params);

            JsonNode json = JsonObject.Parse(resp)!;
            JsonArray doctorsJson = json!["doctors"]!.AsArray();

            List<Doctor> doctors = new();
            foreach (JsonNode doctorJson in doctorsJson)
            {
                doctors.Add(Doctor.ToEntity(doctorJson));
            }

            return doctors;
        }
    }
}

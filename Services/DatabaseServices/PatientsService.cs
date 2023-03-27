using HicsChatBot.Model;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Nodes;

namespace HicsChatBot.Services
{
    public class PatientsService : BaseService
    {
        private readonly string patientsAddress = "patients";
        public PatientsService() : base() { }

        public async Task<List<Patient>> GetAllPatients()
        {
            string resp = await base.Get(this.patientsAddress + "/getAllPatients");
            JsonNode json = JsonObject.Parse(resp)!;
            JsonArray patientsJson = json!["patients"]!.AsArray();

            List<Patient> patients = new();
            foreach (JsonNode patientJson in patientsJson)
            {
                patients.Add(Patient.ToEntity(patientJson));
            }

            return patients;
        }

        public async Task<Patient> GetPatientByNric(string nric)
        {
            var uri_params = new Dictionary<string, string> { };
            uri_params.Add("nric", nric);

            string resp = await base.Get(uri: this.patientsAddress + "/getPatientByNric", uri_params: uri_params);

            JsonNode patientJson = JsonObject.Parse(resp)!["patient"];
            return Patient.ToEntity(patientJson);
        }

        public async Task<Patient> CreatePatient(Patient patient)
        {
            var data = new
            {
                patient = patient.ToObject(),
            };

            string resp = await base.Post(this.patientsAddress, data);

            JsonNode patientJson = JsonObject.Parse(resp)!["patient"];
            return Patient.ToEntity(patientJson);
        }
    }
}

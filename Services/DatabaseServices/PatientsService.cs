using System.Text.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;

using HicsChatBot.Model;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using System.Net.Mime;
using System.Text;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace HicsChatBot.Services
{
    public class PatientsService : BaseService
    {
        private readonly string patientsAddress = "patients/";
        public PatientsService() : base() { }

        public async Task<List<Patient>> GetAllPatients()
        {
            string resp = await base.Get(this.patientsAddress + "getAllPatients");
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
            string resp = await base.Get(this.patientsAddress + "getPatientByNric/" + nric);
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

using HicsChatBot.Model;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Nodes;

namespace HicsChatBot.Services
{
    public class ClinicsService : BaseService
    {
        private readonly string clinicsAddress = "clinics";
        public ClinicsService() : base() { }

        public async Task<Clinic> GetClinic(string id)
        {
            var uri_params = new Dictionary<string, string> { };
            uri_params.Add("id", id);

            string resp = await base.Get(uri: this.clinicsAddress, uri_params: uri_params);

            JsonNode clinicJson = JsonObject.Parse(resp)!["clinic"];
            return Clinic.ToEntity(clinicJson);
        }
    }
}

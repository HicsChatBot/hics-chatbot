using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HicsChatBot.Model
{
    /**
    Models a Clinic in the database.
    */
    public record class Clinic : BaseModel<Clinic>
    {

        public record class Hospital : BaseModel<Hospital>
        {
            public int HospitalId { get; set; }
            public string HospitalName { get; set; }
            public string HospitalLocation { get; set; }
            public string HospitalType { get; set; }

            public Hospital(int HospitalId, string HospitalName, string HospitalLocation, string HospitalType)
            {
                this.HospitalId = HospitalId;
                this.HospitalName = HospitalName;
                this.HospitalLocation = HospitalLocation;
                this.HospitalType = HospitalType;
            }

            public override object ToObject()
            {
                return new
                {
                    hospitalId = this.HospitalId,
                    hospitalName = this.HospitalName,
                    hospitalLocation = this.HospitalLocation,
                    hospitalType = this.HospitalType,
                };
            }

            public override string ToString()
            {
                return $"Hospital [ HospitalId: {this.HospitalId}, HospitalName: {this.HospitalName}, HospitalLocation: {this.HospitalLocation}, HospitalType: {this.HospitalType} ]\n";
            }
        }

        public record class Specialization : BaseModel<Specialization>
        {
            public int SpecializationId { get; set; }
            public string SpecializationName { get; set; }

            public Specialization(int SpecializationId, string SpecializationName)
            {
                this.SpecializationId = SpecializationId;
                this.SpecializationName = SpecializationName;
            }

            public override object ToObject()
            {
                return new
                {
                    specializationId = this.SpecializationId,
                    specializationName = this.SpecializationName,
                };
            }

            public override string ToString()
            {
                return $"Specialization [ SpecializationId: {this.SpecializationId}, SpecializationName: {this.SpecializationName} ]\n";
            }
        }


        public int ClinicId { get; set; }
        public Hospital ClinicHospital { get; set; }

        public Specialization ClinicSpecialization { get; set; }

        public Clinic(int clinicId, int hospitalId, string hospitalName, string hospitalLocation, string hospitalType, int specializationId, string specializationName)
        {
            this.ClinicId = clinicId;
            this.ClinicHospital = new Hospital(hospitalId, hospitalName, hospitalLocation, hospitalType);
            this.ClinicSpecialization = new Specialization(specializationId, specializationName);
        }

        public string ToJson()
        {
            object patient = ToObject();
            string json = JsonSerializer.Serialize(patient);
            return json;
        }

        public static new Clinic ToEntity(JsonNode json)
        {
            if (json == null)
            {
                return null;
            }

            Console.WriteLine(json);

            JsonNode hospitalJson = json!["hospital"];
            JsonNode specializationJson = json!["specialization"];

            return new Clinic(
                clinicId: json["clinicId"].GetValue<int>(),
                hospitalId: hospitalJson["hospitalId"].GetValue<int>(),
                hospitalName: hospitalJson["hospitalName"].GetValue<string>(),
                hospitalLocation: hospitalJson["hospitalLocation"].GetValue<string>(),
                hospitalType: hospitalJson["hospitalType"].GetValue<string>(),
                specializationId: specializationJson["specializationId"].GetValue<int>(),
                specializationName: specializationJson["specializationName"].GetValue<string>()
            );
        }

        public override object ToObject()
        {
            return new
            {
                clinicId = this.ClinicId,
                clinicHospital = this.ClinicHospital.ToObject(),
                clinicSpecialization = this.ClinicSpecialization.ToObject(),
            };
        }

        public override string ToString()
        {
            return $"Clinic [ ClinicId: {this.ClinicId}, ClinicHospital: {this.ClinicHospital}, ClinicSpecialization: {this.ClinicSpecialization} ]\n";
        }
    }
}

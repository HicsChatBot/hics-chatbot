using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace HicsChatBot.Services.CluModelUtil
{
    public class Prediction
    {
        private Intent topIntent;
        private List<Intent> intents;
        private List<Entity> entities;

        private readonly string[] specializations = new string[] { "nephrology", "oncology", "general_doctor", "pediatrics", "neurology", "cardiology" };

        /// Maps prediction data (in JSON format) to the appropriate intents and entities objects.
        public Prediction(JsonElement prediction_data)
        {
            // Map: intents
            if (prediction_data.TryGetProperty("intents", out JsonElement predictedIntents))
            {
                this.intents = new List<Intent>();

                foreach (JsonElement intentElement in predictedIntents.EnumerateArray())
                {
                    if (!intentElement.TryGetProperty("category", out JsonElement categoryJson) ||
                        !intentElement.TryGetProperty("confidenceScore", out JsonElement confidenceScoreJson))
                    {
                        continue;
                    }
                    string category = categoryJson.GetString();
                    double confidenceScore = confidenceScoreJson.GetSingle();

                    Intent intent = new Intent(category: category, confidenceScore: confidenceScore);
                    this.intents.Add(intent);
                }
            }

            // Maps: topIntent
            if (prediction_data.TryGetProperty("topIntent", out JsonElement topIntentElement) &&
                    this.intents.Count > 0 &&
                    this.intents[0].getCategory() == topIntentElement.GetString())
            {
                this.topIntent = new Intent(this.intents[0].getCategory(), this.intents[0].getConfidenceScore());
            }

            // Maps: entities
            if (prediction_data.TryGetProperty("entities", out JsonElement predictedEntities))
            {
                this.entities = new List<Entity>();
                foreach (JsonElement ent in predictedEntities.EnumerateArray())
                {
                    if (!ent.TryGetProperty("category", out JsonElement categoryJson) ||
                        !ent.TryGetProperty("text", out JsonElement textJson) ||
                        !ent.TryGetProperty("confidenceScore", out JsonElement confidenceScoreJson))
                    {
                        continue;
                    }

                    string category = categoryJson.GetString();
                    string text = textJson.GetString();
                    double confidenceScore = confidenceScoreJson.GetSingle();

                    Entity e;
                    if (category == "Specialization")
                    {
                        if (ent.GetProperty("extraInformation").GetArrayLength() == 0 || !ent.GetProperty("extraInformation")[0].TryGetProperty("key", out JsonElement specializationJson))
                        {
                            continue;
                        }
                        e = new DoctorSpecializationEntity(category, text, confidenceScore, specializationJson.ToString());
                    }
                    else if (category == "Agreement")
                    {
                        if (ent.GetProperty("resolutions").GetArrayLength() == 0 || !ent.GetProperty("resolutions")[0].TryGetProperty("value", out JsonElement booleanJson))
                        {
                            continue;
                        }
                        e = new AgreementEntity(category, text, confidenceScore, booleanJson.GetBoolean());
                    }
                    else if (category == "DateTime")
                    {
                        if (ent.GetProperty("resolutions").GetArrayLength() == 0 || !ent.GetProperty("resolutions")[0].TryGetProperty("value", out JsonElement datetimeJson))
                        {
                            continue;
                        }
                        e = new DateTimeEntity(category, text, confidenceScore, datetimeJson.GetString());
                    }
                    else
                    {
                        e = new Entity(category, text, confidenceScore);
                    }

                    this.entities.Add(e);
                }
            }
        }

        public Intent GetTopIntent()
        {
            return this.topIntent;
        }

        public List<Intent> GetIntents()
        {
            return this.intents;
        }

        public List<Entity> GetEntities()
        {
            return this.entities;
        }

        public Entity GetNricEntity()
        {
            foreach (Entity e in this.entities)
            {
                if (e.getCategory() == "NRIC")
                {
                    return e;
                }
            }
            return null;
        }

        public Entity GetNameEntity()
        {
            foreach (Entity e in this.entities)
            {
                if (e.getCategory() == "Name")
                {
                    return e;
                }
            }
            return null;
        }

        public DoctorSpecializationEntity GetTopSpecialization()
        {
            foreach (Entity e in this.entities)
            {
                if (e.GetType() == typeof(DoctorSpecializationEntity))
                {
                    return (DoctorSpecializationEntity)e;
                }
            }
            return null;
        }

        public AgreementEntity GetAgreement()
        {
            foreach (Entity e in this.entities)
            {
                if (e.GetType() == typeof(AgreementEntity))
                {
                    return (AgreementEntity)e;
                }
            }
            return null;
        }

        public List<DateTimeEntity> GetDateTimes()
        {
            List<DateTimeEntity> datetimes = new();

            foreach (Entity e in this.entities)
            {
                if (e.GetType() == typeof(DateTimeEntity))
                {
                    datetimes.Add((DateTimeEntity)e);
                }
            }

            return datetimes;
        }

        public override string ToString()
        {
            string str = "";

            str += "#intents = " + this.intents.Count + "\n  ";
            foreach (Intent i in this.intents)
            {
                str += i.ToString() + "\n  ";
            }

            str += "\n\n#entities = " + this.entities.Count + "\n  ";
            foreach (Entity e in this.entities)
            {
                str += e.ToString() + "\n  ";
            }

            return str;
        }
    }
}

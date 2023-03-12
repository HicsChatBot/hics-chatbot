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
            JsonElement predictedIntents;
            if (prediction_data.TryGetProperty("intents", out predictedIntents))
            {
                this.intents = new List<Intent>();

                foreach (JsonElement intentElement in predictedIntents.EnumerateArray())
                {
                    JsonElement categoryJson;
                    JsonElement confidenceScoreJson;
                    if (!intentElement.TryGetProperty("category", out categoryJson) ||
                        !intentElement.TryGetProperty("confidenceScore", out confidenceScoreJson))
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
            JsonElement topIntentElement;
            if (prediction_data.TryGetProperty("topIntent", out topIntentElement) &&
                    this.intents.Count > 0 &&
                    this.intents[0].getCategory() == topIntentElement.GetString())
            {
                this.topIntent = new Intent(this.intents[0].getCategory(), this.intents[0].getConfidenceScore());
            }

            // Maps: entities
            JsonElement predictedEntities;
            if (prediction_data.TryGetProperty("entities", out predictedEntities))
            {
                this.entities = new List<Entity>();
                foreach (JsonElement ent in predictedEntities.EnumerateArray())
                {
                    JsonElement categoryJson;
                    JsonElement textJson;
                    JsonElement confidenceScoreJson;

                    if (!ent.TryGetProperty("category", out categoryJson) ||
                        !ent.TryGetProperty("text", out textJson) ||
                        !ent.TryGetProperty("confidenceScore", out confidenceScoreJson))
                    {
                        continue;
                    }

                    string category = categoryJson.GetString();
                    string text = textJson.GetString();
                    double confidenceScore = confidenceScoreJson.GetSingle();

                    Entity e;
                    if (this.specializations.Contains(category))
                    {
                        e = new DoctorSpecializationEntity(category, text, confidenceScore);
                    }
                    else if (category == "DateTime")
                    {
                        JsonElement datetimeJson;
                        if (ent.GetProperty("resolutions").GetArrayLength() == 0 || !ent.GetProperty("resolutions")[0].TryGetProperty("value", out datetimeJson))
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

        public Intent getTopIntent()
        {
            return this.topIntent;
        }

        public List<Intent> getIntents()
        {
            return this.intents;
        }

        public List<Entity> getEntities()
        {
            return this.entities;
        }

        public DoctorSpecializationEntity getTopSpecialization()
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

        public List<DateTimeEntity> getDateTimes()
        {
            List<DateTimeEntity> datetimes = new List<DateTimeEntity>();

            foreach (Entity e in this.entities)
            {
                if (e.GetType() == typeof(DateTimeEntity))
                {
                    datetimes.Add((DateTimeEntity)e);
                }
            }

            return datetimes;
        }
    }
}

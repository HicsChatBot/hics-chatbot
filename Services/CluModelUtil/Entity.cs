using System;

namespace HicsChatBot.Services.CluModelUtil
{
    public class Entity
    {
        private string category; // name of entity as defined in the Model.
        private string text; // whatever the user said to cause model to predict this category.
        private double confidenceScore;

        public Entity(string category, string text, double confidenceScore)
        {
            this.category = category;
            this.text = text;
            this.confidenceScore = confidenceScore;
        }

        public string getCategory()
        {
            return this.category;
        }

        public string getText()
        {
            return this.text;
        }

        public double getConfidenceScore()
        {
            return this.confidenceScore;
        }

        /// Gets the appropriate value depending on the type of entity.
        /// To be overridden, if the value != text. For eg, <see cref="DateTimeEntity"> and <see cref="DoctorSpecializationEntity">.
        public virtual object getValue()
        {
            return this.text;
        }

        public override string ToString()
        {
            return $"Entity [ category: {this.category}, text: {this.text}, confidenceScore: {this.confidenceScore.ToString()} ]\n";
        }
    }

    public class DateTimeEntity : Entity
    {
        private DateTime resolution;

        public DateTimeEntity(string category, string text, double confidenceScore, string resolution) :
                base(category, text, confidenceScore)
        {
            this.resolution = DateTime.Parse(resolution);
        }

        public override object getValue()
        {
            return this.resolution;
        }

        public override string ToString()
        {
            return $"Entity [ category: {base.getCategory()}, text: {base.getText()}, confidenceScore: {base.getConfidenceScore().ToString()}, resolution: {this.resolution} ]\n";
        }
    }

    public class AgreementEntity : Entity
    {
        private bool resolution;
        public AgreementEntity(string category, string text, double confidenceScore, bool resolution) :
                base(category, text, confidenceScore)
        {
            this.resolution = resolution;
        }

        public override object getValue()
        {
            return this.resolution;
        }

        public override string ToString()
        {
            return $"Entity [ category: {base.getCategory()}, text: {base.getText()}, confidenceScore: {base.getConfidenceScore()}, resolution: {this.resolution} ]\n";
        }
    }

    public class DoctorSpecializationEntity : Entity
    {
        private string key;
        public DoctorSpecializationEntity(string category, string text, double confidenceScore, string key) :
                base(category, text, confidenceScore)
        {
            this.key = key.ToLower();
        }

        public override object getValue()
        {
            return this.key;
        }

        public override string ToString()
        {
            return $"Entity [ category: {base.getCategory()}, text: {base.getText()}, confidenceScore: {base.getConfidenceScore()}, key: {this.key} ]\n";
        }
    }
}

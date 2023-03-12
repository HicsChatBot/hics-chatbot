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

    public class DoctorSpecializationEntity : Entity
    {
        public DoctorSpecializationEntity(string category, string text, double confidenceScore) :
                base(category, text, confidenceScore)
        { }

        public override object getValue()
        {
            return base.getCategory();
        }
    }
}

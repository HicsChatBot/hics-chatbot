namespace HicsChatBot.Services.CluModelUtil
{
    public class Intent
    {
        private string category;  // name of intent as defined in the Model.
        private double confidenceScore;

        public Intent(string category, double confidenceScore)
        {
            this.category = category;
            this.confidenceScore = confidenceScore;
        }

        public string getCategory()
        {
            return this.category;
        }

        public double getConfidenceScore()
        {
            return this.confidenceScore;
        }

        public override string ToString()
        {
            return $"Intent [ category: {this.category}, confidenceScore: {this.confidenceScore.ToString()} ]\n";
        }
    }
}

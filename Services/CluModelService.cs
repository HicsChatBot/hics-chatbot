using Azure;
using Azure.AI.Language.Conversations;
using Azure.Core;
using HicsChatBot.Services.CluModelUtil;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System;

namespace HicsChatBot.Services
{
    public class CluModelService
    {
        private readonly AzureKeyCredential credential = new AzureKeyCredential(System.Environment.GetEnvironmentVariable("CLU_KEY"));
        private readonly Uri endpoint = new Uri(System.Environment.GetEnvironmentVariable("CLU_ENDPOINT"));
        private ConversationAnalysisClient client;

        private readonly String projectName = "sched-langservice";
        private readonly String deploymentName = "model-v6";

        private static CluModelService instance = new CluModelService();

        private CluModelService()
        {
            client = new ConversationAnalysisClient(endpoint, credential);
        }

        public static CluModelService inst()
        {
            return instance;
        }

        public Prediction predict(String query)
        {
            var data = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = query,
                        id = "1",
                        participantId = "1",
                    },
                },
                parameters = new
                {
                    projectName = this.projectName,
                    deploymentName = this.deploymentName,
                },
                kind = "Conversation",
            };

            Response response = this.client.AnalyzeConversation(RequestContent.Create(data));
            JsonElement result = JsonDocument.Parse(response.ContentStream).RootElement;

            Prediction prediction = new Prediction(result.GetProperty("result").GetProperty("prediction"));

            return prediction;
        }
    }
}

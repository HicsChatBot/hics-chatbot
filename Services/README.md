## Developer Documentation for Services:

### 1. Conversational Language Understanding Model Service

* Example Post response (from CLU Model):
```
{
    "kind": "ConversationResult",
    "result":
    { "query": "book appointment for next friday",
        "prediction":
        {
            "topIntent":"Book", 
            "projectKind":"Conversation",
            "intents": [
                {"category":"Book","confidenceScore":0.91114134},
                {"category":"Reschedule","confidenceScore":0.83780277},
                {"category":"Cancel","confidenceScore":0.76063},
                {"category":"Confirm","confidenceScore":0.40305954},
                {"category":"None","confidenceScore":0.27129003}
            ],
            "entities":[
                {
                    "category":"Appointment",
                    "text":"appointment",
                    "offset":5,
                    "length":11,
                    "confidenceScore":1
                },
                {
                    "category":"AppointmentDatetime",
                    "text":"next friday",
                    "offset":21,
                    "length":11,
                    "confidenceScore":1,"resolutions":  
                        [
                            {"resolutionKind":"DateTimeResolution",
                            "dateTimeSubKind":"Date",
                            "timex":"2023-03-17",
                            "value":"2023-03-17"}
                        ],
                    "extraInformation":
                        [
                            {"extraInformationKind":"EntitySubtype",
                            "value":"datetime.date"}
                        ]
                },
                {
                    "category":"DateTime",
                    "text":"next friday",
                    "offset":21,
                    "length":11,
                    "confidenceScore":1,
                    "resolutions":
                        [
                            {"resolutionKind":"DateTimeResolution",
                            "dateTimeSubKind":"Date",
                            "timex":"2023-03-17",
                            "value":"2023-03-17"}
                        ],
                    "extraInformation":
                        [
                            {"extraInformationKind":"EntitySubtype",
                            "value":"datetime.date"}
                        ]
                }
            ]
        }
    }
}
```
* For [reference/example](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/cognitivelanguage/Azure.AI.Language.Conversations/samples/Sample1_AnalyzeConversation_ConversationPrediction.md)
* For [documentation: ConversationalAnalysisClient.AnalyzeConversation](https://learn.microsoft.com/en-us/dotnet/api/azure.ai.language.conversations.conversationanalysisclient.analyzeconversation?view=azure-dotnet)


### Comments in C# Projects:
* See the [documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments) for reference
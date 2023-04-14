using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Services;
using HicsChatBot.Services.CluModelUtil;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs.UtilDialogs
{
    public class RequestSpecializationDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();

        public RequestSpecializationDialog() : base(nameof(RequestSpecializationDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                RequestSpecializationAsync,
                CheckIfNeedHelpAsync,
                CompleteAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new GetHelpDialog());

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> RequestSpecializationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text($"Are you looking for a particular type of treatment or doctor, or just a general doctor?") },
                    cancellationToken);
        }

        private static async Task<DialogTurnResult> CheckIfNeedHelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string query = (string)stepContext.Result;
            Prediction prediction = clu.predict(query);

            if (prediction.GetTopIntent().getCategory() == "Help")
            {
                return await stepContext.BeginDialogAsync(
                        nameof(GetHelpDialog),
                        query,
                        cancellationToken
                        );
            }

            return await stepContext.NextAsync(query, cancellationToken);
        }

        private static async Task<DialogTurnResult> CompleteAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string query = (string)stepContext.Result;

            Prediction prediction = clu.predict(query);

            string specialization = "";

            query = query.ToLower();

            if (query.Contains("general"))
            {
                specialization = "general";
            }
            else if (prediction.GetTopSpecialization() == null)
            {
                await stepContext.Context.SendActivityAsync("I didn't quite get that, let's try again.", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(RequestSpecializationDialog), cancellationToken: cancellationToken);
            }
            else
            {
                specialization = (string)prediction.GetTopSpecialization().getValue();
            }

            return await stepContext.EndDialogAsync(specialization, cancellationToken);
        }
    }
}

using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Services;
using HicsChatBot.Services.CluModelUtil;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs.UtilDialogs
{
    public class RequestRankingDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();

        public RequestRankingDialog() : base(nameof(RequestRankingDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                RequestRankingAsync,
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

        private static async Task<DialogTurnResult> RequestRankingAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text($"Would you like to see a consultant, specialist or senior consultant?") },
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
            if (query == null)
            {
                return await stepContext.ReplaceDialogAsync(nameof(RequestRankingDialog), cancellationToken: cancellationToken);
            }

            query = query.ToLower().Replace(".", "");

            if (query.Contains("senior consultant"))
            {
                return await stepContext.EndDialogAsync("senior consultant", cancellationToken);
            }
            else if (query.Contains("consultant"))
            {
                return await stepContext.EndDialogAsync("consultant", cancellationToken);
            }
            else if (query.Contains("specialist"))
            {
                return await stepContext.EndDialogAsync("specialist", cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("I didn't quite get that, let's try again.", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(RequestRankingDialog), cancellationToken: cancellationToken);
            }
        }
    }
}

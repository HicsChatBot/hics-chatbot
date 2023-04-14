using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Services;
using HicsChatBot.Services.CluModelUtil;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs.UtilDialogs
{
    public class RequestPrivateOrSubsidizedDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();

        public RequestPrivateOrSubsidizedDialog() : base(nameof(RequestPrivateOrSubsidizedDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                RequestPrivateOrSubsidizedAsync,
                RequestRankingAsync,
                CompleteAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> RequestPrivateOrSubsidizedAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text($"Would you like to see a private doctor or make a subsidized appointment? For subsidized appointments, you are not allowed to choose the doctor you see.") },
                    cancellationToken);
        }

        private static async Task<DialogTurnResult> RequestRankingAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string query = (string)stepContext.Result;
            query = query.ToLower().Replace(".", "");

            if (query.Contains("subsidized"))
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else if (query.Contains("private"))
            {
                return await stepContext.BeginDialogAsync(
                        nameof(RequestRankingDialog),
                        null,
                        cancellationToken
                );
            }
            else
            {
                await stepContext.Context.SendActivityAsync("I didn't quite get that, let's try again.", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(RequestPrivateOrSubsidizedDialog), cancellationToken: cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> CompleteAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string ranking = (string)stepContext.Result;
            return await stepContext.EndDialogAsync(ranking, cancellationToken);
        }
    }
}

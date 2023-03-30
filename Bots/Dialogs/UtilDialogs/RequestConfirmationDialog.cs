using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Services;
using HicsChatBot.Services.CluModelUtil;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs.UtilDialogs
{
    public class RequestConfirmationDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();
        private static string question;

        public RequestConfirmationDialog() : base(nameof(RequestConfirmationDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                RequestConfirmationAsync,
                CompleteAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> RequestConfirmationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            question = (string)stepContext.Options;

            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text(question) },
                    cancellationToken);
        }

        private static async Task<DialogTurnResult> CompleteAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string query = (string)stepContext.Result;

            Prediction prediction = clu.predict(query);

            AgreementEntity e = prediction.GetAgreement();

            if (e == null)
            {
                await stepContext.Context.SendActivityAsync("I didn't quite get that, let's try again.", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(RequestConfirmationDialog), question, cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync((bool)e.getValue(), cancellationToken);
        }
    }
}

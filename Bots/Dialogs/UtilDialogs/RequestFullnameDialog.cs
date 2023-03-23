using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Services;
using HicsChatBot.Services.CluModelUtil;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs.UtilDialogs
{
    public class RequestFullnameDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();

        public RequestFullnameDialog() : base(nameof(RequestFullnameDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                RequestFullnameAsync,
                CompleteAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> RequestFullnameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text($"What's your full name?") },
                    cancellationToken);
        }

        private static async Task<DialogTurnResult> CompleteAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string query = (string)stepContext.Result;
            Prediction prediction = clu.predict(query);

            string name = (string)prediction.GetNameEntity()?.getValue();

            if (name == null)
            {
                await stepContext.Context.SendActivityAsync("I didn't quite get that, let's try again.", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(RequestFullnameDialog), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(name, cancellationToken);
        }
    }
}

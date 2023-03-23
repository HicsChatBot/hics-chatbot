using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Model;
using HicsChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs
{
    public class FetchOrCreatePatientDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();
        private static readonly int MaxNumRetryAttempts = 2;

        private static int numRetryAttempts = MaxNumRetryAttempts;

        public FetchOrCreatePatientDialog() : base(nameof(FetchOrCreatePatientDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                IsPatientRegisteredAsync,
                GetOrCreatePatientAsync,
                ConfirmPatientAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // AddDialog(new RequestNricDialog());
            AddDialog(new CreateNewPatientDialog());
            AddDialog(new FetchPatientDialog());

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        private static async Task<DialogTurnResult> IsPatientRegisteredAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("Have you registered your patient data before?") },
                    cancellationToken);
        }

        private static async Task<DialogTurnResult> GetOrCreatePatientAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!((string)stepContext.Result).ToLower().Contains("yes"))
            {
                // not registered
                return await stepContext.BeginDialogAsync(nameof(CreateNewPatientDialog), cancellationToken: cancellationToken);
            }
            // registered
            return await stepContext.BeginDialogAsync(nameof(FetchPatientDialog), cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> ConfirmPatientAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Patient patient = (Patient)stepContext.Result;

            if (patient == null)
            {
                if (numRetryAttempts <= 0)
                {
                    return await stepContext.ReplaceDialogAsync(nameof(TransferToHumanDialog), cancellationToken: cancellationToken);
                }
                await stepContext.Context.SendActivityAsync("Let's try that again.", cancellationToken: cancellationToken);
                numRetryAttempts -= 1;

                return await stepContext.ReplaceDialogAsync(nameof(FetchOrCreatePatientDialog), null, cancellationToken);
            }

            return await stepContext.EndDialogAsync(patient, cancellationToken);
        }
    }
}

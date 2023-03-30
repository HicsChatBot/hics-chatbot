using System;
using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Dialogs.UtilDialogs;
using HicsChatBot.Model;
using HicsChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs
{
    public class FetchOrCreatePatientDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();

        private static int numRetryAttempts;

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

            AddDialog(new RequestConfirmationDialog());

            // AddDialog(new RequestNricDialog());
            AddDialog(new CreateNewPatientDialog());
            AddDialog(new FetchPatientDialog());
            AddDialog(new TransferToHumanDialog());

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        private static async Task<DialogTurnResult> IsPatientRegisteredAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            numRetryAttempts = (int)stepContext.Options;
            return await stepContext.BeginDialogAsync(nameof(RequestConfirmationDialog), "Have you registered your patient data before?", cancellationToken);
        }

        private static async Task<DialogTurnResult> GetOrCreatePatientAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool hasRegisteredBefore = (bool)stepContext.Result;
            if (!hasRegisteredBefore)
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
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
                await stepContext.Context.SendActivityAsync("Let's try that again.", cancellationToken: cancellationToken);

                return await stepContext.ReplaceDialogAsync(nameof(FetchOrCreatePatientDialog), numRetryAttempts - 1, cancellationToken);
            }

            return await stepContext.EndDialogAsync(patient, cancellationToken);
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Services;
using HicsChatBot.Dialogs.UtilDialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using HicsChatBot.Model;

namespace HicsChatBot.Dialogs
{
    public class FetchPatientDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();
        private static readonly PatientsService patientsService = new PatientsService();

        private static Patient patient = null;

        public FetchPatientDialog() : base(nameof(FetchPatientDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                RequestNricAsync,
                FetchPatientDataAsync,
                ConfirmPatientAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new RequestNricDialog());

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> RequestNricAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(RequestNricDialog), cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> FetchPatientDataAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string nric = (string)stepContext.Result;

            Patient p = await patientsService.GetPatientByNric(nric);

            if (p == null)
            {
                // If patient does not exist (in DB), change to create a new patient.
                await stepContext.Context.SendActivityAsync($"There is no patient with the NRIC {nric} in our records.", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken: cancellationToken);
            }

            patient = p;
            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text($"Are you {patient.Title} {patient.Fullname}?") },
                    cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> ConfirmPatientAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((string)stepContext.Result != "yes")
            {
                return await stepContext.ReplaceDialogAsync(nameof(FetchPatientDialog), cancellationToken: cancellationToken);
            }

            // Confirm
            return await stepContext.EndDialogAsync(patient, cancellationToken);
        }
    }
}

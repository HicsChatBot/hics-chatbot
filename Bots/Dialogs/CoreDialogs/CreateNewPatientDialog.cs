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
    public class CreateNewPatientDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();
        private static readonly PatientsService patientsService = new PatientsService();

        private static readonly Patient patient = new Patient();

        public CreateNewPatientDialog() : base(nameof(CreateNewPatientDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                RequestNricAsync,
                RequestFullnameAsync,
                GetPatientDataConfirmationAsync,
                ConfirmPatientAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new RequestNricDialog());
            AddDialog(new RequestFullnameDialog());

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> RequestNricAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Let's first register you into our system.", cancellationToken: cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(RequestNricDialog), cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> RequestFullnameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            patient.Nric ??= (string)stepContext.Result;

            return await stepContext.BeginDialogAsync(nameof(RequestFullnameDialog), cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> GetPatientDataConfirmationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            patient.Fullname ??= (string)stepContext.Result;

            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text($"Alright, so just to check: your nric is {patient.Nric}, your full name is {patient.Fullname}, etc. Can you confirm that this information is correct?") },
                    cancellationToken: cancellationToken);
        }

        // If caller confirmed "yes", create the new patient in the database.
        private static async Task<DialogTurnResult> ConfirmPatientAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string confirm = (string)stepContext.Result;
            if (!(confirm.ToLower().Contains("yes")))
            {
                return await stepContext.ReplaceDialogAsync(nameof(FetchPatientDialog), cancellationToken: cancellationToken);
            }

            await stepContext.Context.SendActivityAsync($"Please give me a moment to register your data into our system...", cancellationToken: cancellationToken);

            // TODO: add more util dialogs (and remove these hard-coded patient data)
            patient.Address = "Blk 123 Ang mo Kio Street 11";
            patient.Dob = DateTime.Parse("1980-10-09");
            patient.Gender = "M";
            patient.Phone = "+65 91231234";
            patient.Title = "Mr";

            Patient createdPatient = await patientsService.CreatePatient(patient);
            await stepContext.Context.SendActivityAsync($"Thank you for your patience, let's move on the the next step.", cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync(createdPatient, cancellationToken);
        }
    }
}

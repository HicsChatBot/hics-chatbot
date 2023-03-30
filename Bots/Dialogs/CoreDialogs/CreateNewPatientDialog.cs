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
                RequestAddressAsync,
                RequestDobAsync,
                RequestPhoneNumberAsync,
                GetPatientDataConfirmationAsync,
                ConfirmPatientAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new RequestNricDialog());
            AddDialog(new RequestFullnameDialog());
            AddDialog(new RequestAddressDialog());
            AddDialog(new RequestDobDialog());
            AddDialog(new RequestPhoneNumberDialog());

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

        private static async Task<DialogTurnResult> RequestAddressAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            patient.Fullname ??= (string)stepContext.Result;

            return await stepContext.BeginDialogAsync(nameof(RequestAddressDialog), cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> RequestDobAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            patient.Address ??= (string)stepContext.Result;

            return await stepContext.BeginDialogAsync(nameof(RequestDobDialog), cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> RequestPhoneNumberAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            patient.Dob ??= DateTime.Parse((string)stepContext.Result);  // TODO: Probably need a try catch because DateTime Parse throws an error when an invalid date is specified

            return await stepContext.BeginDialogAsync(nameof(RequestPhoneNumberDialog), cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> GetPatientDataConfirmationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            patient.Phone ??= (string)stepContext.Result;

            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text($"Alright, so just to confirm: your NRIC is {patient.Nric}, your full name is {patient.Fullname}, your address is {patient.Address}, your date of birth is {patient.Dob}, and your phone number is {patient.Phone}. Can you confirm that this information is correct?") },
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
            patient.Gender = "M";
            patient.Title = "Mr";

            Patient createdPatient = await patientsService.CreatePatient(patient);
            await stepContext.Context.SendActivityAsync($"Thank you for your patience, let's move on the the next step.", cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync(createdPatient, cancellationToken);
        }
    }
}

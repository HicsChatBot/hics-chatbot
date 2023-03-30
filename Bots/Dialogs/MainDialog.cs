using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Model;
using HicsChatBot.Services;
using HicsChatBot.Services.CluModelUtil;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();
        private static Patient patient = null;

        public MainDialog() : base(nameof(MainDialog))
        {
            // Patient patient = new Patient();

            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                // TestReqAsync,
                // TestRespAsync,
                FetchOrCreatePatientAsync,
                OfferHelpAsync,
                HandleRequestAsync,
                CompleteCallAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new FetchOrCreatePatientDialog());
            AddDialog(new BookAppointmentDialog());
            AddDialog(new TransferToHumanDialog());
            AddDialog(new CancelAppointmentDialog());

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> FetchOrCreatePatientAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(FetchOrCreatePatientDialog), 2, cancellationToken: cancellationToken);
        }


        private static async Task<DialogTurnResult> OfferHelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            patient = (Patient)stepContext.Result;

            if (patient == null)
            {
                return await stepContext.ReplaceDialogAsync(nameof(TransferToHumanDialog), cancellationToken: cancellationToken);
            }

            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("How can I help you?") },
                    cancellationToken);
        }

        private static async Task<DialogTurnResult> HandleRequestAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string query = (string)stepContext.Result;
            Prediction prediction = clu.predict(query);

            Console.WriteLine(patient);

            if (prediction.GetTopIntent()?.getCategory() == "Book")
            {
                return await stepContext.BeginDialogAsync(nameof(BookAppointmentDialog), patient, cancellationToken);
            }
            else if (prediction.GetTopIntent()?.getCategory() == "Cancel")
            {
                return await stepContext.BeginDialogAsync(nameof(CancelAppointmentDialog), patient, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("I don't understand...", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(TransferToHumanDialog), cancellationToken: cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> CompleteCallAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Thank you for your time!", cancellationToken: cancellationToken);
            return await stepContext.CancelAllDialogsAsync(cancellationToken: cancellationToken);
        }
    }
}

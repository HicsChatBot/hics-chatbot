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
        private static bool isFirstIteration = true;
        private static Patient patient = null;

        public MainDialog() : base(nameof(MainDialog))
        {
            // Patient patient = new Patient();

            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                FetchOrCreatePatientAsync,
                OfferHelpAsync,
                HandleRequestAsync,
                CheckContinueCallAsync,
                ConfirmContinueCallAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new FetchOrCreatePatientDialog());
            AddDialog(new BookAppointmentDialog());

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> FetchOrCreatePatientAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (patient != null)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            return await stepContext.BeginDialogAsync(nameof(FetchOrCreatePatientDialog), cancellationToken: cancellationToken);
        }


        private static async Task<DialogTurnResult> OfferHelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            patient ??= (Patient)stepContext.Result;

            string msg = isFirstIteration ? "How can I help you?" : "What else can I help you with?";
            isFirstIteration = false;
            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text(msg) },
                    cancellationToken);
        }

        private static async Task<DialogTurnResult> HandleRequestAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string query = (string)stepContext.Result;
            Prediction prediction = clu.predict(query);

            if (prediction.GetTopIntent()?.getCategory() == "Book")
            {
                return await stepContext.BeginDialogAsync(nameof(BookAppointmentDialog), patient, cancellationToken);
            }
            else if (prediction.GetTopIntent()?.getCategory() == "Cancel")
            {
                await stepContext.Context.SendActivityAsync("Cancelling appointment", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("I don't understand...", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), cancellationToken: cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> CheckContinueCallAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // After successful / failure to do something (intent)
            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("Do you still need anymore help?") },
                    cancellationToken);
        }

        private static async Task<DialogTurnResult> ConfirmContinueCallAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((string)stepContext.Result != "yes")
            {
                await stepContext.Context.SendActivityAsync("Thank you for your time!", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Model;
using HicsChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs
{
    public class BookAppointmentDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();
        private static Patient patient;

        public BookAppointmentDialog() : base(nameof(BookAppointmentDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                GetAppointmentTypeAsync,
                ConfirmAppointmentAsync,
                OnNotConfirmAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> GetAppointmentTypeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (patient == null)
            {
                patient = (Patient)stepContext.Options;
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Booking an appointment for {patient.Title} {patient.Fullname} ..."), cancellationToken);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Can you confirm your booking for an appointment with Dr XXX at X?X, Hospistal XXX Clinic XXX Room XXX?") }, cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> ConfirmAppointmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string confirm = (string)stepContext.Result;
            if (!confirm.ToLower().Contains("yes"))
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to book a different appointment?") }, cancellationToken: cancellationToken);
            }

            // TODO: Call DB to add appointment
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static async Task<DialogTurnResult> OnNotConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string confirm = (string)stepContext.Result;
            if (confirm.ToLower().Contains("yes"))
            {
                // Book a different appointment
                return await stepContext.ReplaceDialogAsync(nameof(BookAppointmentDialog), cancellationToken: cancellationToken);
            }

            // Terminate execution
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Services;
using HicsChatBot.Services.CluModelUtil;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs.UtilDialogs
{
    public class RequestPhoneNumberDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();

        public RequestPhoneNumberDialog() : base(nameof(RequestPhoneNumberDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                RequestPhoneNumberAsync,
                CompleteAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> RequestPhoneNumberAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text($"What is your phone number?") },
                cancellationToken);
        }

        private static async Task<DialogTurnResult> CompleteAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string query = (string) stepContext.Result;
            Prediction prediction = clu.predict(query);

            string phoneNumber = (string) prediction.GetPhoneNumberEntity()?.getValue();

            if (phoneNumber == null)
            {
                await stepContext.Context.SendActivityAsync("I didn't quite get that, let's try again.", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(RequestPhoneNumberDialog), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(phoneNumber, cancellationToken);
        }
    }
}
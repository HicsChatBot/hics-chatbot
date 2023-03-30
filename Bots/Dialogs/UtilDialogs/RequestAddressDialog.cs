using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Services;
using HicsChatBot.Services.CluModelUtil;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs.UtilDialogs
{
    public class RequestAddressDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();

        public RequestAddressDialog() : base(nameof(RequestAddressDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                RequestAddressAsync,
                CompleteAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        public static async Task<DialogTurnResult> RequestAddressAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text($"What is your address?") },
                cancellationToken);
        }

        private static async Task<DialogTurnResult> CompleteAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string address = (string)stepContext.Result;
            //Prediction prediction = clu.predict(query);  // TODO: Need to be fixed

            //string address = (string)prediction.GetAddressEntity()?.getValue();

            if (address == null)
            {
                await stepContext.Context.SendActivityAsync("I didn't quite get that, let's try again.", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(RequestAddressDialog), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(address, cancellationToken);
        }
    }
}
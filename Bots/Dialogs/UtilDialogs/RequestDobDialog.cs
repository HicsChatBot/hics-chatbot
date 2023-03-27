using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Services;
using HicsChatBot.Services.CluModelUtil;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs.UtilDialogs
{
    public class RequestDobDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();

        public RequestDobDialog() : base(nameof(RequestDobDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                RequestDobAsync,
                CompleteAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        public static async Task<DialogTurnResult> RequestDobAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text($"What is your date of birth?") },
                cancellationToken);
        }

        public static async Task<DialogTurnResult> CompleteAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string query = (string) stepContext.Result;
            Prediction prediction = clu.predict(query);

            string dob = (string) prediction.GetDobEntity()?.getValue();

            if (dob == null)
            {
                await stepContext.Context.SendActivityAsync("", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(RequestAddressDialog), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(dob, cancellationToken);
        }
    }
}
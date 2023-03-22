using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs
{
    public class TransferToHumanDialog : ComponentDialog
    {
        public TransferToHumanDialog() : base(nameof(TransferToHumanDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                TransferCallerAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> TransferCallerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Get patient data from parent dialog
            await stepContext.Context.SendActivityAsync("So sorry, let me transfer you to my colleague...", cancellationToken: cancellationToken);

            return await stepContext.CancelAllDialogsAsync(cancellationToken: cancellationToken);
        }
    }
}

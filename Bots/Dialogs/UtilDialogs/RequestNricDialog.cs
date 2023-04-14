using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Services;
using HicsChatBot.Services.CluModelUtil;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs.UtilDialogs
{
    public class RequestNricDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();

        public RequestNricDialog() : base(nameof(RequestNricDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                RequestNricAsync,
                CompleteAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> RequestNricAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text($"What's your NRIC?") },
                    cancellationToken);
        }

        private static async Task<DialogTurnResult> CompleteAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string query = (string)stepContext.Result;

            string nric = getNric(query);
            if (nric == null) // no nric found in client's response.
            {
                await stepContext.Context.SendActivityAsync("I didn't quite get that, let's try again.", cancellationToken: cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(RequestNricDialog), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(nric, cancellationToken);
        }

        private static string getNric(string text)
        {
            text = text.Replace(" ", "").Replace(".", "");
            text = text.ToUpper();

            Regex r = new Regex(@"(T|S|F|G)0\d{6}[A-Z]", RegexOptions.IgnoreCase);
            Match m = r.Match(text);

            return m.Success ? m.ToString() : null;
        }
    }
}

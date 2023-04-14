using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Dialogs.CustomDialogData;
using HicsChatBot.Services;
using HicsChatBot.Services.CluModelUtil;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs.UtilDialogs
{
    public class GetHelpDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();

        // Maps Entity.category to explanation of the 
        private static readonly Dictionary<string, string> explanations = new Dictionary<string, string>{
            {"NRIC", "Nric is the ID Document issued to Singaporean Citizens and Permanant Residents. "
                + "It starts with a T, S, F or G, followed by 7 numbers and ends with another letter."},
            {"AppointmentType", "We offer 3 different appointment types. "
                + "'First consult' is a first time consultation with a doctor or at a clinic. "
                + "'Follow up' is when a patient has already had past consults and wants to book another appointment with the same doctor or clinic. "
                + "'Review' is review."},
            {"DoctorRanking", "Private doctors can have 3 different rankings. "
                + "'Consultant' provides expert advice, typically with many years of experience. "
                + "'Specialist' has completed advanced training and has expertise in their particular field. "
                + "'Senior consultant' has achieved a high level of expertise and experience in their field. "
                + "Generally, an appointment with a senior consultant costs more than with a specialist, which costs more than with a consultant. "
                }
        };

        public GetHelpDialog() : base(nameof(GetHelpDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                ExplainAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> ExplainAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string helpRequest = (string)stepContext.Options;

            Prediction prediction = clu.predict(helpRequest);

            HashSet<string> seenCategories = new HashSet<string>();

            foreach (Entity e in prediction.GetEntities())
            {
                if (seenCategories.Contains(e.getCategory()))
                {
                    continue;
                }

                if (explanations.TryGetValue(e.getCategory(), out string explanation))
                {
                    Console.WriteLine(explanation);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(explanation), cancellationToken);
                    seenCategories.Add(e.getCategory());
                }
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

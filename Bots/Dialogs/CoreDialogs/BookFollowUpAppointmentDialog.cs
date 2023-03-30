using System;
using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Dialogs.CustomDialogData;
using HicsChatBot.Model;
using HicsChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs
{
    public class BookFollowUpAppointmentDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();
        private static readonly AppointmentsService apptsService = new AppointmentsService();
        private static readonly ClinicsService clinicsService = new ClinicsService();
        private static FollowUpAppointmentData followUpAppointmentData;

        private static Appointment newAppt;

        public BookFollowUpAppointmentDialog() : base(nameof(BookFollowUpAppointmentDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                FindAppointmentAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> FindAppointmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            followUpAppointmentData = (FollowUpAppointmentData)stepContext.Options;

            Patient p = followUpAppointmentData.patient;

            if (followUpAppointmentData.lastAppt.DoctorId == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Please give me a moment while I look for the next subsidized available appointment."), cancellationToken);

                newAppt = await apptsService.FindNextAvailableSubsidizedAppointment(
                    followUpAppointmentData.lastClinic.ClinicSpecialization.SpecializationName,
                    "follow up"
                );

                newAppt.PatientId = p.Id;
                newAppt.ApptStatus = "upcoming";

                return await stepContext.EndDialogAsync(newAppt, cancellationToken);
            }

            newAppt = await apptsService.FindNextAvailablePrivateAppointment(
                followUpAppointmentData.lastClinic.ClinicSpecialization.SpecializationName,
                "follow up",
                followUpAppointmentData.lastAppt.DoctorId
            );

            newAppt.PatientId = p.Id;
            newAppt.ApptStatus = "upcoming";

            return await stepContext.EndDialogAsync(newAppt, cancellationToken);
        }
    }
}

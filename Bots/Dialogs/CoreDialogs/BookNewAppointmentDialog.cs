using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HicsChatBot.Dialogs.CustomDialogData;
using HicsChatBot.Dialogs.UtilDialogs;
using HicsChatBot.Model;
using HicsChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace HicsChatBot.Dialogs
{
    public class BookNewAppointmentDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();
        private static readonly AppointmentsService apptsService = new AppointmentsService();
        private static readonly ClinicsService clinicsService = new ClinicsService();
        private static readonly DoctorsService doctorsService = new DoctorsService();
        private static NewAppointmentData newAppointmentData;

        private static Appointment newAppt;

        public BookNewAppointmentDialog() : base(nameof(BookNewAppointmentDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                GetSpecializationAsync,
                GetRankingAsync,
                FindAppointmentAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new RequestSpecializationDialog());
            AddDialog(new RequestRankingDialog());

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> GetSpecializationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            newAppointmentData = (NewAppointmentData)stepContext.Options;

            if (newAppointmentData.specialization != null)
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }

            return await stepContext.BeginDialogAsync(nameof(RequestSpecializationDialog), null, cancellationToken);
        }

        private static async Task<DialogTurnResult> GetRankingAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result != null)
            {
                newAppointmentData.specialization = (string)stepContext.Result;
            }

            if (newAppointmentData.ranking != null)
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }

            return await stepContext.BeginDialogAsync(nameof(RequestRankingDialog), null, cancellationToken);
        }

        private static async Task<DialogTurnResult> FindAppointmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result != null)
            {
                newAppointmentData.ranking = (string)stepContext.Result;
            }

            Patient p = newAppointmentData.patient;

            if (newAppointmentData.ranking == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Please give me a moment while I look for the next subsidized available appointment."), cancellationToken);

                newAppt = await apptsService.FindNextAvailableSubsidizedAppointment(
                    newAppointmentData.specialization,
                    "first consult"
                );

                newAppt.PatientId = p.Id;
                newAppt.ApptStatus = "upcoming";

                return await stepContext.EndDialogAsync(newAppt, cancellationToken);
            }

            List<Doctor> doctors = await doctorsService.GetDoctorsBySpecializationAndRanking(newAppointmentData.specialization, newAppointmentData.ranking);

            Doctor d = doctors[0];

            newAppt = await apptsService.FindNextAvailablePrivateAppointment(
                    newAppointmentData.specialization,
                    "first consult",
                    d.Id
                );

            newAppt.PatientId = p.Id;
            newAppt.ApptStatus = "upcoming";

            return await stepContext.EndDialogAsync(newAppt, cancellationToken);
        }
    }
}

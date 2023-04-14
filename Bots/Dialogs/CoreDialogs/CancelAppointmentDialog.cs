using System;
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
    public class CancelAppointmentDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();

        private static readonly AppointmentsService apptsService = new AppointmentsService();
        private static readonly DoctorsService doctorsService = new DoctorsService();
        private static readonly ClinicsService clinicsService = new ClinicsService();
        private static UpcomingAppointmentData upcomingAppointmentData;

        public CancelAppointmentDialog() : base(nameof(CancelAppointmentDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                GetNextAppointmentAsync,
                ConditionallyNavigateToBookAppointmentAsync,
                ConfirmCancelAppointmentAsync,
                AskBookNewAppointmentAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new RequestConfirmationDialog());

            AddDialog(new BookAppointmentDialog());

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> GetNextAppointmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            upcomingAppointmentData = new UpcomingAppointmentData((Patient)stepContext.Options);

            upcomingAppointmentData.nextAppt = await apptsService.GetNextUpcomingAppointment(upcomingAppointmentData.patient.Id.ToString());

            if (upcomingAppointmentData.nextAppt == null)
            {
                return await stepContext.BeginDialogAsync(
                    nameof(RequestConfirmationDialog),
                    "You do not have any upcoming appointments. Would you like to book an appointment instead?",
                    cancellationToken: cancellationToken);
            }

            upcomingAppointmentData.nextClinic = await clinicsService.GetClinic(upcomingAppointmentData.nextAppt.ClinicId.ToString());
            if (upcomingAppointmentData.nextAppt.DoctorId != null)
            {
                upcomingAppointmentData.nextDoctor = await doctorsService.GetDoctor(upcomingAppointmentData.nextAppt.DoctorId.ToString());
            }

            return await stepContext.BeginDialogAsync(
                nameof(RequestConfirmationDialog),
                $"Your next appointment is at {upcomingAppointmentData.nextAppt.StartDatetime}, {upcomingAppointmentData.nextClinic.ClinicHospital.HospitalName}, {upcomingAppointmentData.nextClinic.ClinicSpecialization.SpecializationName} clinic, room {upcomingAppointmentData.nextAppt.RoomNumber}" +
                (upcomingAppointmentData.nextDoctor == null ? "." : $" with doctor {upcomingAppointmentData.nextDoctor.Fullname}.") +
                "Can you confirm that you would like to cancel this appointment?",
                cancellationToken
            );
        }

        private static async Task<DialogTurnResult> ConditionallyNavigateToBookAppointmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (upcomingAppointmentData.nextAppt != null)
            {
                // Pass data to next async step
                return await stepContext.NextAsync(stepContext.Result, cancellationToken);
            }

            bool navToBookAppointment = (bool)stepContext.Result;

            if (navToBookAppointment)
            {
                return await stepContext.ReplaceDialogAsync(nameof(BookAppointmentDialog), upcomingAppointmentData.patient, cancellationToken);
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static async Task<DialogTurnResult> ConfirmCancelAppointmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool confirmCancel = (bool)stepContext.Result;

            if (confirmCancel)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Please give me a moment while I cancel your appointment."), cancellationToken);
                Appointment result = await apptsService.DeleteUpcomingAppointment(upcomingAppointmentData.nextAppt);

                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Okay, I have cancelled your appointment."), cancellationToken);
                return await stepContext.BeginDialogAsync(
                        nameof(RequestConfirmationDialog),
                        $"Would you like to book a new appointment?",
                        cancellationToken
                );
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Noted, not cancelling appointment."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> AskBookNewAppointmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool isRequestingToBookNewAppt = (bool)stepContext.Result;

            if (isRequestingToBookNewAppt)
            {
                return await stepContext.ReplaceDialogAsync(
                    nameof(BookAppointmentDialog),
                    upcomingAppointmentData.patient,
                    cancellationToken
                );
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

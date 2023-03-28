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
    public class BookAppointmentDialog : ComponentDialog
    {
        private static readonly CluModelService clu = CluModelService.inst();

        private static readonly AppointmentsService apptsService = new AppointmentsService();
        private static readonly DoctorsService doctorsService = new DoctorsService();
        private static readonly ClinicsService clinicsService = new ClinicsService();
        private static FollowUpAppointmentData followUpAppointmentData;
        private static Appointment newAppt;

        public BookAppointmentDialog() : base(nameof(BookAppointmentDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                // Add waterfall steps here
                GetAppointmentTypeAsync,
                NewOrFollowUpAppointmentAsync,
                HandleAppointmentAsync,
                ConfirmAppointmentAsync,
            };

            // Add named dialogs to DialogSet.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new BookFollowUpAppointmentDialog());

            // Initial child dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> GetAppointmentTypeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            followUpAppointmentData = new FollowUpAppointmentData((Patient)stepContext.Options);

            followUpAppointmentData.lastAppt = await apptsService.GetMostRecentPastAppointment(followUpAppointmentData.patient.Id.ToString());

            if (followUpAppointmentData.lastAppt == null)
            {
                return await stepContext.NextAsync("new", cancellationToken);
            }

            followUpAppointmentData.lastClinic = await clinicsService.GetClinic(followUpAppointmentData.lastAppt.ClinicId.ToString());

            followUpAppointmentData.lastDoctor = followUpAppointmentData.lastAppt.DoctorId != null ?
                    (await doctorsService.GetDoctor(followUpAppointmentData.lastAppt.DoctorId.ToString())) : null;

            string apptDetails = (followUpAppointmentData.lastDoctor == null) ?
                    $"another {followUpAppointmentData.lastClinic.ClinicSpecialization.SpecializationName} doctor at {followUpAppointmentData.lastClinic.ClinicHospital.HospitalName}" :
                    $"Dr. {followUpAppointmentData.lastDoctor.Fullname} at {followUpAppointmentData.lastClinic.ClinicHospital.HospitalName}";

            return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text($"Would you like to book a follow up appointment with {apptDetails} or a new appointment?") },
                    cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> NewOrFollowUpAppointmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string query = (string)stepContext.Result;
            query = query.Trim().ToLower().Replace("-", " ").Replace(".", "");

            if (query.Contains("follow up"))
            {
                return await stepContext.BeginDialogAsync(nameof(BookFollowUpAppointmentDialog), followUpAppointmentData, cancellationToken);
            }
            else if (query.Contains("new"))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Creating new appt... Done!"), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(nameof(BookAppointmentDialog), followUpAppointmentData.patient, cancellationToken: cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> HandleAppointmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            newAppt = (Appointment)stepContext.Result;
            if (newAppt == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("No Appt Data found... encountered some error :(. Retry!"), cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(BookAppointmentDialog), followUpAppointmentData.patient, cancellationToken: cancellationToken);
            }
            Clinic newClinic = await clinicsService.GetClinic(newAppt.ClinicId.ToString());

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text($"The earliest I can give you is {newAppt.StartDatetime} at {newClinic.ClinicHospital.HospitalName}, {newClinic.ClinicSpecialization.SpecializationName} Clinic, Room {newAppt.RoomNumber}, will that work for you?") },
                cancellationToken: cancellationToken);
        }

        private static async Task<DialogTurnResult> ConfirmAppointmentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string confirm = (string)stepContext.Result;
            if (confirm.ToLower().Contains("yes"))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please give me a moment while I book the appointment..."), cancellationToken);
                await apptsService.CreateAppointment(newAppt);
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

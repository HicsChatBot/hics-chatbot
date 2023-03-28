using HicsChatBot.Model;

namespace HicsChatBot.Dialogs.CustomDialogData
{
    public class NewAppointmentData
    {
        public Patient patient { get; set; }
        public string specialization { get; set; }
        public string ranking { get; set; }

        public NewAppointmentData(Patient patient, string specialization, string ranking)
        {
            this.patient = patient;
            this.specialization = specialization;
            this.ranking = ranking;
        }
    }
}

using HicsChatBot.Model;

namespace HicsChatBot.Dialogs.CustomDialogData
{
    public class FollowUpAppointmentData
    {
        public Patient patient { get; set; }
        public Appointment lastAppt { get; set; }
        public Clinic lastClinic { get; set; }
        public Doctor lastDoctor { get; set; }

        public FollowUpAppointmentData(Patient patient)
        {
            this.patient = patient;
            this.lastAppt = null;
            this.lastClinic = null;
            this.lastDoctor = null;
        }
    }
}

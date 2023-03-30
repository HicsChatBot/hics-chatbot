using HicsChatBot.Model;

namespace HicsChatBot.Dialogs.CustomDialogData
{
    public class UpcomingAppointmentData
    {
        public Patient patient { get; set; }
        public Appointment nextAppt { get; set; }
        public Clinic nextClinic { get; set; }
        public Doctor nextDoctor { get; set; }

        public UpcomingAppointmentData(Patient patient)
        {
            this.patient = patient;
            this.nextAppt = null;
            this.nextClinic = null;
            this.nextDoctor = null;
        }
    }
}

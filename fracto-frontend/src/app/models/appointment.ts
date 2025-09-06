export interface Appointment {
  id?: number;
  userId: number;
  doctorId: number;
  appointmentDate: string;
  timeSlot: string;
  status?: string;
  doctor?: any;            // optional expanded doctor object used in UI
}
